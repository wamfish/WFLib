using System.Net;
using System.Net.Sockets;
namespace WFLib;
public enum LOGLEVEL { EXCEPTIONS, ERRORS, WARNINGS, ALL }
public enum RequestType : byte { Ignore, Ping, ConnectRequest, SendBuf, ReadRecord, ReadRecords, ReqInfo }
public abstract class UdpCommon
{
    public bool IsUdpServer => (this is WFUdpServer);
    public bool IsUdpClient => (this is WFUdpClient);

    public UdpCommon() { }
    public LOGLEVEL LogLevel = LOGLEVEL.EXCEPTIONS;
    public Action<string> NetLogger = null;
    public void Log(string message)
    {
        if (LogLevel < LOGLEVEL.ALL) return;
        Logger.Message(message, NetLogger);
    }
    public void LogException(Exception ex)
    {
        Logger.Exception(ex, NetLogger);
    }
    public void LogError(string error)
    {
        if (LogLevel < LOGLEVEL.ERRORS) return;
        Logger.Error(error);
    }
    public void LogWarning(string msg)
    {
        if (LogLevel < LOGLEVEL.WARNINGS) return;
        Logger.Warning(msg, NetLogger);
    }
    //private SemaphoreSlim LogSemaphore = new SemaphoreSlim(0);
    //private CancellationTokenSource logCancelSource;
    //private CancellationToken logCancel;
    //private ManualResetEvent mre = new ManualResetEvent(true);
    //This a background task that dispenses 
    public readonly int PacketSize = 8192;
    public SerializationBuffer RecvBuf()
    {
        return SerializationBuffer.Rent(PacketSize);
    }
    protected const int DefaultPort = 32121;
    protected const string LoopbackIP = "127.0.0.1";
    protected const string AnyIP = "0.0.0.0";
    protected const string BroadcastIP = "255.255.255.255";
    protected static IPAddress BroadcastAddress => IPAddress.Broadcast;
    protected IPAddress LocalIPAddress
    {
        get
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            Log("No network adapters with an IPv4 address in the system!");
            return null;
        }
    }

    internal long totalBytesSent = 0;
    public long TotalBytesSent => totalBytesSent;

    internal long totalBytesRead = 0;
    public long TotalBytesRead => totalBytesRead;

    internal Socket socket;
    public Socket Socket => socket;

    //public class TestRecordReader : RecordReader<TestRecord.Table, TestRecord.Context, TestRecord>
    //{

    //}
    //public readonly TestRecordReader reader = new TestRecordReader();

    internal EndPoint GetEndPoint(HostData host)
    {
        if (IsUdpServer) return host.RemoteEndPoint;
        if (IsUdpClient) return host.LocalEndPoint;
        throw new NotImplementedException();
    }
    internal Socket GetSocket()
    {
        if (this is WFUdpServer udpServer) return udpServer.Socket;
        if (this is WFUdpClient udpClient) return udpClient.Socket;
        throw new NotImplementedException();
    }


    protected bool CopyToBuffers(ChannelData cd, SerializationBuffer inbuf, int i, int count, List<SerializationBuffer> buffers)
    {
        int cursize;
        int offset;
        ushort reqId = cd.ReqId;
        cursize = inbuf.BytesUsed;
        offset = 0;
        int bytesOut = 0;
        int bytesToAdd = cursize;
        while (bytesOut < cursize)
        {
            var buf = SerializationBuffer.Rent(PacketSize);
            WriteHeader(buf);
            if (bytesToAdd > buf.BytesAvailable)
                bytesToAdd = buf.BytesAvailable;
            buf.BlockCopy(inbuf.Data, offset, buf.WriteIndex, bytesToAdd);
            offset += bytesToAdd;
            bytesOut += bytesToAdd;
            buffers.Add(buf);
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