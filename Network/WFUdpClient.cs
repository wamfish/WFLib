using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
namespace WFLib;
public class WFUdpClient : UdpCommon, IDisposable
{
    public event Action OnStart;
    public event Action OnStop;
    Queue<Packet> receivePool = new();
    CancellationTokenSource processLoopCancelSource;
    CancellationToken processLoopCancelToken;
    SemaphoreSlim processLoopSlim = new SemaphoreSlim(0);
    private int NextChannelId = 0;

    Hosts hosts;

    internal HostData localHost;
    public HostData LocalHost => localHost;
    public string IPAddress => localHost.RemoteIPAddress;
    public int Port => localHost.RemotePort;


    public bool IsRunning { get; private set; } = false;

    public WFUdpClient(IPEndPoint serverEndPoint, Action<string> logger, LOGLEVEL logLevel)
    {
        NetLogger = logger;
        LogLevel = LOGLEVEL.ALL;
        var clientEndPoint = new IPEndPoint(LocalIPAddress, 0);
        //localHost.Request = new Request(- 1, RequestType.Ignore, localHost, this);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(clientEndPoint);
        //we use socket.LocalEndPoint to get the correct port that was assigned with socket.Bind
        hosts = new Hosts(socket.LocalEndPoint);
        localHost = hosts.AddOrGetHost(serverEndPoint);
        localHost = new HostData(socket.LocalEndPoint, serverEndPoint, -1);
        Log($"IP Address: {localHost.LocalIPAddress} Port: {localHost.LocalPort}");
    }
    public void Start()
    {
        _ = Task.Run(Process);
        _ = Task.Run(Receive);
        OnStart?.Invoke();
    }
    public void Stop()
    {
        if (socket != null)
        {
            IsRunning = false;
            socket.Close();
            socket = null;
            processLoopCancelSource.Cancel();
            //Thread.Sleep(1000);
            OnStop?.Invoke();
        }
    }
    private void Receive()
    {
        void RecieveLoop()
        {
            SerializationBuffer buf = SerializationBuffer.Rent(PacketSize);
            while (true)
            {
                buf.Clear();
                var fromEp = localHost.LocalEndPoint.Duplicate();
                int bytesRead = socket.ReceiveFrom(buf.Data, buf.Data.Length, SocketFlags.None, ref fromEp);
                if (bytesRead == 0) return;
                buf.SetWriteIndex(bytesRead);
                buf.ClientEndPoint = fromEp.Duplicate();
                Interlocked.Add(ref totalBytesRead, bytesRead);
                var packet = Packet.Rent(buf, localHost);
                lock (receivePool)
                {
                    receivePool.Enqueue(packet);
                    processLoopSlim.Release();
                }
            }
        }
        try
        {
            IsRunning = true;
            RecieveLoop();
        }
        catch (SocketException se)
        {
            if (se.ErrorCode != 10004) //normal when we close the socket
            {
                Log($"Socket Error: {se.ErrorCode}");
                LogException(se);
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
            return;
        }
        finally
        {
            Stop();
        }
    }
    private void Process()
    {
        void ProcessLoop()
        {
            processLoopSlim.Wait(processLoopCancelToken);
            Packet packet;
            lock (receivePool)
            {
                packet = receivePool.Dequeue();
            }
            var channel = AssembleResponse(this, packet);
            if (channel != null)
            {
                channel.Mre.Set();
            }
            packet.Return();
        }
        processLoopCancelSource = new CancellationTokenSource();
        processLoopCancelToken = processLoopCancelSource.Token;
        try
        {
            while (true)
            {
                ProcessLoop();
            }
        }
        catch (OperationCanceledException)
        {
            // this is the normal exit path
            Log("ProcessLoop Exit");
        }
        catch (Exception ex)
        {
            LogException(ex);
            return;
        }
        finally
        {
            Stop();
        }
    }
    public bool Connect()
    {
        var cd = Channel.ClientChannelByThread(localHost, this, ref NextChannelId);
        if (cd == null) return false;
        SerializationBuffer req = SerializationBuffer.Rent();
        req.Write((byte)RequestType.ConnectRequest);
        req.WriteSize(321);
        req.WriteSize(123);
        req.WriteSize(67856);
        var resp = SendReqAndGetResponse(cd, req);
        if (resp == null) return false;
        bool rmsg = resp.ReadBool();
        Log($"Connect:{rmsg}");
        return true;
    }
    long pingSent = 0;
    long pingRecv = 0;
    public long PingSent => pingSent;
    public long PingRecv => pingRecv;
    public void LogStats()
    {
        Log($"Bytes Sent: {totalBytesSent,10:N0}");
        Log($"Bytes Read: {totalBytesRead,10:N0}");
        Log($"Ping Sent: {PingSent,11:N0} ");
        Log($"Ping Recv: {PingRecv,11:N0} ");
    }
    public bool Ping(string msg)
    {
        var cd = Channel.ClientChannelByThread(localHost, this, ref NextChannelId);
        if (cd == null) return false;
        long timeStart = Stopwatch.GetTimestamp();
        SerializationBuffer req = SerializationBuffer.Rent();
        req.Write((byte)RequestType.Ping);
        req.Write(msg);
        Interlocked.Increment(ref pingSent);
        var resp = SendReqAndGetResponse(cd, req);
        if (resp == null) return false;
        Interlocked.Increment(ref pingRecv);
        long timeStop = Stopwatch.GetTimestamp();
        string rmsg = resp.ReadString();
        double elapsed = ((double)(timeStop - timeStart)) / Stopwatch.Frequency;
        elapsed *= 1000;
        Log($"Ping({cd.ReqId})({elapsed:0}):{cd.Id}-{rmsg}");
        return true;
    }
    public void Dispose()
    {
        if (socket != null) Stop();
    }

    public bool GetResponse(ChannelData cd, SerializationBuffer req)
    {
        int timeOutCount = 0;
        //If I stick with using my own udp library
        //this method will need to be given lots of love
        //handling dropped packets looks like a lot of work
        // to do it correctly.
        //

        while (!cd.Mre.WaitOne(50))
        {
            if (timeOutCount > 2) return false;
            timeOutCount++;
            //For now I am resending the whole packet
            //I am not putting much work into recovery of
            //dropped packets until I know for sure if I will use
            //my own library.
            SendReq(cd, req);
        }
        if (cd.Canceled)
            return false;
        return true;
    }
    ushort nextReqId = 0;

    private void InitChannelData(ChannelData cd)
    {
        lock (cd)
        {
            cd.Canceled = false;
            foreach (var buffer in cd.reqBuffers)
            {
                buffer.Return();
            }
            foreach (var buffer in cd.respBuffers)
            {
                buffer.Return();
            }
            cd.reqBuffers.Clear();
            cd.respBuffers.Clear();
            cd.Mre.Reset();
        }
    }
    private bool SendReq(ChannelData cd, List<SerializationBuffer> req)
    {
        try
        {
            InitChannelData(cd);
            int count = req.Count;
            for (int i = 0; i < count; i++)
            {
                if (!SendBuffers(cd, req[i], i, count)) return false;
                //CopyToBuffers(cd, req[i], i, count, cd.reqBuffers);
            }
            return true;
            //return SendReq(cd);
        }
        catch (Exception ex)
        {
            LogException(ex);
            return false;
        }
    }

    public SerializationBuffer SendReqAndGetResponse(ChannelData cd, SerializationBuffer req)
    {
        cd.NextRequest();
        if (!SendReq(cd, req)) return null;
        int timeOutCount = 0;
        while (!cd.Mre.WaitOne(50))
        {
            if (timeOutCount > 2) return null;
            timeOutCount++;
            var oldId = cd.ReqId;
            cd.NextRequest();
            Log($"Resend: CurId {cd.ReqId} OldId: {oldId}");
            if (!SendReq(cd, req)) return null;
        }
        if (cd.Canceled) return null;
        if (cd.respBuffers.Count < 1) return null;
        return cd.respBuffers[0];
    }
    private bool SendReq(ChannelData cd, SerializationBuffer req)
    {
        try
        {
            InitChannelData(cd);
            //CopyToBuffers(cd, req, 0, 1, cd.reqBuffers);
            return SendBuffers(cd, req, 0, 1);
            //return SendReq(cd);
        }
        catch (Exception ex)
        {
            LogException(ex);
            return false;
        }
    }

    public ChannelData AssembleResponse(UdpCommon udp, Packet inPacket)
    {
        SerializationBuffer inbuf = inPacket.Buf;
        List<SerializationBuffer> buffers;
        //int hostId = inPacket.Host.Id;
        if (!inbuf.TryReadSize(out int channelId)) return null;
        if (!inbuf.TryReadUShort(out ushort reqId)) return null;

        var cd = Channel.GetChannelData(udp, inPacket.Host, channelId);
        if (cd == null) return null;
        buffers = cd.respBuffers;
        //We don't care about out of order request. This is
        //not the same thing as out of order packets.
        if (cd.IsReqIdOutOfOrder(reqId)) return null;

        if (reqId != cd.ReqId)
        {
            // I don't think this should ever happend
            // The client should set everything before sending the request
            LogError("Does this ever happen?");
            return null;
        }

        if (!inbuf.TryReadSize(out int bufIndex)) return null;
        if (!inbuf.TryReadSize(out int bufCount)) return null;
        if (!inbuf.TryReadSize(out int curSize)) return null;
        if (!inbuf.TryReadSize(out int outOffset)) return null;
        //The problem is RequestSize and RequesBytesRead is not set correctly 
        //when we come back through on the 2nd call, I need to get this hole scope into my head a little better
        while (buffers.Count < bufCount)
        {
            var rb = SerializationBuffer.Rent(udp.PacketSize);
            rb.RequestSize = 0;
            rb.RequestBytesRead = 0;
            buffers.Add(rb);
        }
        var outbuf = buffers[bufIndex];
        if (outbuf.RequestSize == 0)
        {
            outbuf.RequestSize = curSize;
        }
        //copy buf to the request buf
        outbuf.BlockCopy(inbuf.Data, inbuf.ReadIndex, outOffset, inbuf.BytesToRead);
        outbuf.RequestBytesRead += inbuf.BytesToRead;
        bool reqComplete = true;
        for (int i = 0; i < bufCount; i++)
        {
            outbuf = buffers[i];
            if (outbuf.RequestSize != outbuf.RequestBytesRead)
            {
                reqComplete = false;
                udp.Log($"Channel Id: {cd.Id} Size: {outbuf.RequestSize} Bytes Read: {outbuf.RequestBytesRead} ");
                break;
            }
        }
        if (reqComplete) return cd;
        return null;
    }

    protected bool SendBuffers(ChannelData cd, SerializationBuffer inbuf, int i, int count)
    {
        int cursize;
        int offset;
        ushort reqId = cd.ReqId;
        cursize = inbuf.BytesUsed;
        offset = 0;
        int bytesOut = 0;
        int bytesToAdd = cursize;
        var buf = SerializationBuffer.Rent(PacketSize);
        while (bytesOut < cursize)
        {
            WriteHeader(buf);
            if (bytesToAdd > buf.BytesAvailable)
                bytesToAdd = buf.BytesAvailable;
            buf.BlockCopy(inbuf.Data, offset, buf.WriteIndex, bytesToAdd);
            offset += bytesToAdd;
            bytesOut += bytesToAdd;
            int bytesSent = socket.SendTo(buf.Data, buf.BytesUsed, SocketFlags.None, cd.Host.LocalEndPoint);
            Interlocked.Add(ref totalBytesSent, bytesSent);
            if (bytesSent < buf.BytesUsed) return false;
            buf.Clear();
        }
        return true;
        int WriteHeader(SerializationBuffer buf)
        {
            //buf.WriteSize(cd.Host.Id);
            buf.WriteSize(cd.Id); //ChannelData.Id
            buf.Write(reqId);
            buf.WriteSize(i);
            buf.WriteSize(count);
            buf.WriteSize(cursize); //size of curbuf
            buf.WriteSize(offset);  //in case we need to split curbuf
            return buf.BytesUsed;
        }
    }


}