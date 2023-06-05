using System.Net;
using System.Net.Sockets;

namespace WFLib;

public partial class WFUdpServer : UdpCommon, IDisposable
{
    public event Action OnStart;
    public event Action OnStop;

    private EndPoint serverEndPoint = null;
    public EndPoint ServerEndPoint => serverEndPoint;
    public IPEndPoint ServerIPEndPoint => serverEndPoint.IPEndPoint();
    public string ServerIPAddress => ServerIPEndPoint.Address.ToString();
    public int ServerPort => ServerIPEndPoint.Port;

    internal Hosts hosts;

    public bool PacketTestingMode = false;
    protected Random PacketDrop = new Random();

    public WFUdpServer(int serverPort = DefaultPort)
    {
        serverEndPoint = new IPEndPoint(LocalIPAddress, serverPort);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(serverEndPoint);
        Log($"Server on: {serverEndPoint.ToString()}");
        hosts = new Hosts(serverEndPoint);
    }

    const int ProcessLoops = 1;
    CancellationTokenSource processLoopCancelSource;
    CancellationToken processLoopCancelToken;
    SemaphoreSlim processLoopSlim;
    public void Start()
    {
        OnStart?.Invoke();
        Log($"Start");
        processLoopCancelSource = new CancellationTokenSource();
        processLoopCancelToken = processLoopCancelSource.Token;
        processLoopSlim = new SemaphoreSlim(0);
        for (int i = 0; i < ProcessLoops; i++)
        {
            _ = Task.Run(Dispatch);
        }
        _ = Task.Run(Receive);
    }
    public void Stop()
    {
        if (socket == null)
            return;
        processLoopCancelSource.Cancel();
        socket.Close();
        socket = null;
        OnStop?.Invoke();
    }

    Queue<Packet> receivePool = new();
    void Receive()
    {
        void ReceiveLoop()
        {
            var sender = new IPEndPoint(IPAddress.Any, 0).EndPoint();
            var buf = SerializationBuffer.Rent(PacketSize);
            while (true)
            {
                int bytesRead = socket.ReceiveFrom(buf.Data, buf.Data.Length, SocketFlags.None, ref sender);
                if (bytesRead == 0) return;
                buf.SetWriteIndex(bytesRead);
                Interlocked.Add(ref totalBytesRead, bytesRead);
                if (PacketTestingMode && PacketDrop.Next(0, 11) == 10)
                {
                    //PacketTestingMode = false;
                    buf.Clear();
                    continue;
                }
                var host = hosts.GetHost(sender);
                if (host != null) // process only host in our host list
                {
                    var packet = Packet.Rent(buf, host);
                    lock (receivePool)
                    {
                        receivePool.Enqueue(packet);
                        processLoopSlim.Release();
                    }
                }
                buf.Clear();
            }
        }
        try
        {
            ReceiveLoop();
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
            //Log($"Error Code: {ex..ErrorCode}");
            Log(ex.Message + ex.StackTrace);
        }
        finally
        {
            Stop();
        }
    }
    void Dispatch()
    {
        void DispatchLoop()
        {
            while (true)
            {
                processLoopSlim.Wait(processLoopCancelToken);
                Packet buf;
                lock (receivePool)
                {
                    buf = receivePool.Dequeue();
                }
                var channel = AssembleRequest(this, buf);
                if (channel != null)
                {
                    if (channel.reqBuffers.Count == 0) return;
                    var rbuf = channel.reqBuffers[0];
                    if (!rbuf.TryReadByte(out byte rt)) return;
                    RequestType reqType = (RequestType)rt;
                    switch (reqType)
                    {
                        case RequestType.ConnectRequest:
                            ConnectRequest(channel);
                            break;
                        case RequestType.Ping:
                            PingRequest(channel);
                            break;
                    }
                }
                buf.Return();
            }
        }
        try
        {
            DispatchLoop();
        }
        catch (OperationCanceledException)
        {
            //ignore this is the normal exit
        }
        catch (Exception ex)
        {
            LogException(ex);
            return;
        }
        finally
        {
            Log("ProcessLoop Exit");
            Stop();
        }
    }
    void ConnectRequest(ChannelData cd)
    {
        var buf = cd.reqBuffers[0];
        if (!buf.TryReadSize(out int k1)) return;
        if (!buf.TryReadSize(out int k2)) return;
        if (!buf.TryReadSize(out int k3)) return;
        buf.Clear();
        if (k1 == 321 && k2 == 123 && k3 == 67856)
        {
            buf.Write(true);
        }
        else
        {
            buf.Write(false);
        }
        SendResponse(cd, buf);
    }
    void PingRequest(ChannelData channel)
    {
        void _PingRequest(ChannelData cd)
        {
            var buf = cd.reqBuffers[0];
            var msg = buf.ReadString();
            //Log($"Ping From: {cd.Host.ClientEndPoint} Msg: {msg}");
            buf.Clear();
            buf.Write($"Hello from: {cd.Host.LocalEndPoint}");
            SendResponse(cd, buf);
        }
        try
        {
            _PingRequest(channel);
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    public void Dispose()
    {
        if (socket != null)
            Stop();
    }

    public bool SendResponse(ChannelData cd, List<SerializationBuffer> response)
    {
        try
        {
            int count = response.Count;
            for (int i = 0; i < count; i++)
            {
                CopyToBuffers(cd, response[i], i, count, cd.respBuffers);
            }
            return SendResponse(cd);
        }
        catch (Exception ex)
        {
            LogException(ex);
            return false;
        }


    }
    public bool SendResponse(ChannelData cd, SerializationBuffer response)
    {
        try
        {
            CopyToBuffers(cd, response, 0, 1, cd.respBuffers);
            return SendResponse(cd);
        }
        catch (Exception ex)
        {
            LogException(ex);
            return false;
        }
    }
    private bool SendResponse(ChannelData cd)
    {
        List<SerializationBuffer> sendBuffers = cd.respBuffers;
        var socket = GetSocket();
        var endPoint = GetEndPoint(cd.Host);
        for (int i = 0; i < sendBuffers.Count; i++)
        {
            var buf = sendBuffers[i];
            int bytesSent = socket.SendTo(buf.Data, buf.BytesUsed, SocketFlags.None, endPoint);
            Interlocked.Add(ref totalBytesSent, bytesSent);
            if (bytesSent < buf.BytesUsed) return false;
        }
        return true;
    }

    public ChannelData AssembleRequest(UdpCommon udp, Packet inPacket)
    {
        SerializationBuffer inbuf = inPacket.Buf;
        List<SerializationBuffer> buffers;
        //if (!inbuf.TryReadSize(out int hostId)) return null;
        if (!inbuf.TryReadSize(out int channelId)) return null;
        if (!inbuf.TryReadUShort(out ushort reqId)) return null;

        var cd = Channel.GetChannelData(udp, inPacket.Host, channelId);
        if (cd == null) return null;
        buffers = cd.reqBuffers;
        //We don't care about out of order request. This is
        //not the same thing as out of order packets.
        if (cd.IsReqIdOutOfOrder(reqId)) return null;
        // when the client sends a new reqId, the
        // server will not process and older reqIds 
        if (reqId != cd.ReqId)
        {
            lock (cd)
            {
                cd.ReqId = reqId;
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
            }
        }
        if (!inbuf.TryReadSize(out int bufIndex)) return null;
        if (!inbuf.TryReadSize(out int bufCount)) return null;
        if (!inbuf.TryReadSize(out int curSize)) return null;
        if (!inbuf.TryReadSize(out int outOffset)) return null;
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
        //if (outbuf.RequestSize == outbuf.RequestBytesRead)
        //{
        //    if (IsReqComplete(udp, buffers, cd, bufCount, ref outbuf)) return cd;
        //    return null;
        //}
        outbuf.BlockCopy(inbuf.Data, inbuf.ReadIndex, outOffset, inbuf.BytesToRead);
        outbuf.RequestBytesRead += inbuf.BytesToRead;
        if (IsReqComplete(udp, buffers, cd, bufCount, ref outbuf)) return cd;
        return null;
        static bool IsReqComplete(UdpCommon udp, List<SerializationBuffer> buffers, ChannelData cd, int bufCount, ref SerializationBuffer outbuf)
        {
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

            return reqComplete;
        }
    }
}
