using System.Net.Sockets;
using static WFLib.Global;
using System.Net;
using System.Threading.Channels;
using System.Diagnostics;

namespace WFLib.Network;

public abstract class UdpTunnelBase : IDisposable
{
    public event Action<UdpTunnelBase> OnPing;
    const int AckBufferCount = 10;
    #region vars
    List<SerializationBuffer> ackBuffers = new List<SerializationBuffer>();
    List<SerializationBuffer> reorderList = new List<SerializationBuffer>();
    List<SerializationBuffer> outOfOrderList = new List<SerializationBuffer>();

    SemaphoreSlim sendSemaphoreSlim;
    bool cancelSend = false;

    protected Socket socket;

    public abstract void OnReceive(SerializationBuffer sb);
    public abstract void OnSend(SerializationBuffer sb);
    public abstract void OnReceiveAck(SerializationBuffer sb);
    public abstract void OnStop();
    public abstract void OnStart();
    public abstract void OnError(string message);
    public abstract void OnException(Exception ex);
    

    IPEndPoint localEndPoint;
    public IPEndPoint LocalEndPoint => localEndPoint;
    EndPointKey localEndPointKey;
    public EndPointKey LocalEndPointKey => localEndPointKey;

    IPEndPoint remoteEndPoint;
    public IPEndPoint RemoteEndPoint => remoteEndPoint;
    EndPointKey remoteEndPointKey;
    public EndPointKey RemoteEndPointKey => remoteEndPointKey;

    long numBadEndPoint = 0;
    public long NumBadEndPoint => numBadEndPoint;

    long numInvalidPacket = 0;
    public long NumInvalidPacket => numInvalidPacket;

    long numBytesRead;
    public long NumBytesRead => numBytesRead;
        
    long numBytesSent = 0;
    public long NumBytesSent => numBytesSent;

    long numResends = 0;
    public long NumResends => numResends;

    public string Name { get; private set; }

    public int BufferSize { get; private set; }

    private int nextSeqNum = 0;


    private int recvSeqNum = -1;

    private bool TestDropPacket = false;
    private Random TestDropRandom = new Random();

    #endregion

    public void Init(string name, IPAddress localAddress, int localPort, IPAddress remoteAddress, int remotePort, int bufferSize)
    {
        BufferSize = bufferSize;
        Name = name;
        localEndPoint = new IPEndPoint(localAddress, localPort);
        localEndPointKey = new EndPointKey(localEndPoint);
        remoteEndPoint = new IPEndPoint(remoteAddress, remotePort);
        remoteEndPointKey = new EndPointKey(remoteEndPoint);
    }
    public void Start()
    {
        Stop();
        cancelSend = false;
        socket = new Socket(LocalEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(LocalEndPoint);
        socket.ReceiveBufferSize = BufferSize * AckBufferCount;
        socket.SendBufferSize = BufferSize * AckBufferCount;
        sendSemaphoreSlim = new SemaphoreSlim(1);
        Task.Run(ReceiveLoop);
        OnStart();
    }
    public void Stop()
    {
        if (socket == null)
        {
            //OnError($"{Name} already stopped");
            return;
        }
        cancelSend = true;
        sendSemaphoreSlim.Release();
        socket.Close();
        socket = null;
        OnStop();
    }
    public bool IsOpen => socket != null;
    public void SendBuf(ushort channelID, SerializationBuffer sb)
    {
        try
        {
            if (cancelSend) return;
            var sendBuf = SerializationBuffer.Rent();
            sendBuf.SeqNum =  Interlocked.Increment(ref nextSeqNum);
            sendBuf.Acked = false;
            sendBuf.ChannelID = channelID;
            sendBuf.AckCheckCount = 0;
            sendBuf.Write(PacketType.Packet);
            sendBuf.Write(sendBuf.SeqNum);
            sendBuf.Write(channelID);
            sendBuf.Append(sb.Data, 0, sb.BytesUsed);
            //OnError($"{Name} sending {sendBuf.BytesUsed} bytes on channel {channelID} Seq: {sendBuf.SeqNum}");
            Send(sendBuf,true);
        }
        catch (Exception ex)
        {
            OnException(ex);
        }
    }
    private void Send(SerializationBuffer sb, bool requestAck)
    {
        bool ready = false;
        int ackBufferCount = 0;
        while(!ready)
        {
            ready = sendSemaphoreSlim.Wait(500);
            if (!ready) Log($"{Name} Send Wait Timeout");
        }
        lock (ackBuffers)
        {
            ackBufferCount = ackBuffers.Count;
        }
        while (ackBufferCount >= AckBufferCount)
        {
            Task.Delay(2).Wait();
            lock (ackBuffers)
            {
                ackBufferCount = ackBuffers.Count;
            }
            if (ackBufferCount < AckBufferCount) break;
            ProcessAckBuffers();
        }
        if (cancelSend)
        {
            OnError($"{Name} Send canceled");
            return;
        }
            
        try
        {
            int bytesSent;

            if (requestAck)
            {
                lock (ackBuffers)
                {
                    ackBuffers.Add(sb);
                }
            }
            if (requestAck) OnSend(sb); //Not actually sent but makes more since here for debugging
            if (TestDropPacket && TestDropRandom.Next(100) < 5)
            {
                OnError($"{Name} TestDropPacket");
                return;
            }
            bytesSent = socket.SendTo(sb.Data, 0, sb.BytesUsed, SocketFlags.None, remoteEndPoint);
            if (bytesSent != sb.BytesUsed)
            {
                OnError($"SendTo failed to send all bytes {bytesSent} != {sb.BytesUsed}");
            }
            Interlocked.Add(ref numBytesSent, bytesSent);

        }
        catch (Exception ex)
        {
            OnException(ex);
        }
        finally
        {
            if (!requestAck) sb.Return();
            sendSemaphoreSlim.Release();
        }
    }
    public void Ping()
    {
        var sb = SerializationBuffer.Rent();
        sb.Write(PacketType.Ping);
        sb.Write(DateTime.UtcNow.Ticks);
        Send(sb,false);    
    }
    void ProcessPing(SerializationBuffer insb)
    {
        var sb = SerializationBuffer.Rent();
        long ticks = insb.ReadLong();
        sb.Write(PacketType.Pong);
        sb.Write(ticks);
        Send(sb,false);
    }
    void ProcessPong(SerializationBuffer insb)
    {
        long ticks = insb.ReadLong();
        Log($"{Name} Tunnel Ping: {(DateTime.UtcNow.Ticks - ticks) / 10000} ms");
        OnPing?.Invoke(this);
    }
    void ProcessPacket(SerializationBuffer sb)
    {
        var osb = SerializationBuffer.Rent();
        osb.SeqNum = sb.SeqNum = sb.ReadInt();
        if (recvSeqNum <  0 ) 
            recvSeqNum = sb.SeqNum;
        osb.ChannelID = sb.ChannelID = sb.ReadUShort();
        osb.Write(PacketType.PacketAck);
        osb.Write(sb.SeqNum);
        osb.Write(sb.ChannelID);
        Send(osb, false);
        lock (outOfOrderList)
        {
            if (recvSeqNum == sb.SeqNum)
            {
                OnReceive(sb);
                recvSeqNum++;
            }
            else
            {
                var nsb = SerializationBuffer.Rent();
                nsb.SeqNum = sb.SeqNum;
                nsb.ChannelID = sb.ChannelID;
                nsb.Append(sb.Data, 0, sb.BytesUsed);
                outOfOrderList.Add(nsb);
                bool rebuild = false;
                for (int i = 0; i < outOfOrderList.Count; i++)
                {
                    if (outOfOrderList[i].SeqNum == recvSeqNum)
                    {
                        OnReceive(outOfOrderList[i]);
                        recvSeqNum++;
                        outOfOrderList[i].Return();
                        outOfOrderList[i] = null;
                        rebuild = true;
                    }
                }
                if (rebuild)
                {
                    outOfOrderList.RemoveAll(x => x == null);
                }
            }
        }

    }
    void ProcessPacketAck(SerializationBuffer sb)
    {
        bool doProcess = false;
        lock(ackBuffers)
        {
            int seqNum = sb.ReadInt();
            int channelID = sb.ReadUShort();
            for (int i = 0; i < ackBuffers.Count; i++)
            {
                if (ackBuffers[i].SeqNum == seqNum)
                {
                    ackBuffers[i].Acked = true;
                    doProcess = true;
                    break;
                }
                else
                {
                    for(int x=0;x<ackBuffers.Count; x++)
                    {
                        if (ackBuffers[x].SeqNum == seqNum)
                        {
                            ackBuffers[x].Acked = true;
                            Debug.Assert(LogError($"{Name} out of order seq:  {seqNum} channel {channelID}"));
                            break;
                        }
                    }
                }
            }
            if (doProcess)
            {
                ProcessAckBuffers();
            }
            else
            {
                OnError($"{Name} Ack for unknown seqNum {seqNum}");
            }
        }
    }
    void ProcessAckBuffers()
    {
        lock (ackBuffers)
        {
            if (ProcessAcks())
            {
                ReorderAcktBuffers();
            }
        }
        bool ProcessAcks()
        {
            bool doReorder = false;
            for (int i = 0; i < ackBuffers.Count; i++)
            {
                var sb = ackBuffers[i];
                if (!sb.Acked)
                {
                    sb.AckCheckCount++;
                    if (sb.AckCheckCount > 2)
                    {
                        sb.AckCheckCount = 0;
                        int bytesSent = socket.SendTo(sb.Data, 0, sb.BytesUsed, SocketFlags.None, remoteEndPoint);
                        if (bytesSent != sb.BytesUsed)
                        {
                            OnError($"SendTo failed to send all bytes {bytesSent} != {sb.BytesUsed}");
                        }
                        Interlocked.Add(ref numBytesSent, bytesSent);
                    }
                    return doReorder;
                }
                OnReceiveAck(sb);
                sb.Return();
                ackBuffers[i] = null;
                doReorder = true;
            }
            return doReorder;
        }
        void ReorderAcktBuffers()
        {
            reorderList.Clear();
            foreach (var sb in ackBuffers)
            {
                if (sb != null) reorderList.Add(sb);
            }
            ackBuffers.Clear();
            ackBuffers.AddRange(reorderList);
            //LogError($"{Name} ReorderAcktBuffers {ackBuffers.Count}");
            //foreach (var sb in ackBuffers)
            //{
            //    LogError($"{Name} ackBuffer {sb.SeqNum}");
            //}
        }

    }
    private void ReceiveLoopCore()
    {
        using var sb = SerializationBuffer.Rent(BufferSize);
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        EndPointKey checkKey = new EndPointKey(remoteEndPoint);
        var buf = sb.Data.AsSpan<byte>();
        while(true)
        {
            sb.Clear();
            int bytesRead = socket.ReceiveFrom(buf, SocketFlags.None, ref remoteEndPoint);
            if (bytesRead == 0)
            {
                OnError("ReceiveFrom returned 0 bytes");
                return;
            }
            sb.SetWriteIndex(bytesRead);
            checkKey.Init(remoteEndPoint);
            if (!checkKey.Equals(remoteEndPointKey))
            {
                Debug.Assert(Log($"Invalid IP: {remoteEndPointKey}{checkKey}"));
                Interlocked.Increment(ref numBadEndPoint);
                continue;
            }
            int packetType = sb.ReadByte();
            switch (packetType)
            {
                case PacketType.Packet:
                    ProcessPacket(sb);
                    continue;
                case PacketType.PacketAck:
                    ProcessPacketAck(sb);
                    continue;
                case PacketType.Ping:
                    ProcessPing(sb);
                    continue;
                case PacketType.Pong:
                    ProcessPong(sb);
                    continue;
                case PacketType.Close:
                    Debug.Assert(LogError("Close Packet Recieved"));
                    return;
            }
            Interlocked.Increment(ref numInvalidPacket);
        }
    }
    void ReceiveLoop()
    {
        try
        {
            ReceiveLoopCore();
        }
        catch (Exception ex)
        {
            if (ex is SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Debug.Assert(LogError($"ConnectionReset {se.SocketErrorCode}"));
                }
                else
                {
                    Debug.Assert(LogError($"Socket Exception: {se.SocketErrorCode}"));
                }
            }
            else
            {
                LogException(ex);
            }
            
        }
        Stop();
    }
    public void Dispose()
    {
        try
        {
            Stop();
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
        GC.SuppressFinalize(this);
    }
}
