using System.Diagnostics;
using System.Net;
namespace WFLib;

public class HostData
{
    internal int hostId;
    public int HostId => hostId;

    private EndPoint remoteEndPoint;
    public EndPoint RemoteEndPoint => remoteEndPoint;
    public IPEndPoint RemoteIPEndPoint => remoteEndPoint.IPEndPoint();
    public string RemoteIPAddress => RemoteIPEndPoint.Address.ToString();
    public int RemotePort => RemoteIPEndPoint.Port;

    // ChannelByThread is only used in WfUdpClient
    public List<int> ChannelByThread = new List<int>();
    internal List<ChannelData> ChannelData = new();

    private EndPoint localEndPoint;
    public EndPoint LocalEndPoint => localEndPoint;
    public IPEndPoint LocalIPEndPoint => localEndPoint.IPEndPoint();
    public string LocalIPAddress => LocalIPEndPoint.Address.ToString();
    public int LocalPort => LocalIPEndPoint.Port;

    internal long timeStamp;
    public long TimeStamp => timeStamp;

    //internal Request Request;

    public HostData(EndPoint localEndPoint, EndPoint remoteEndPoint, int hostId)
    {
        this.remoteEndPoint = remoteEndPoint;
        this.localEndPoint = localEndPoint;
        this.hostId = hostId;
        timeStamp = Stopwatch.GetTimestamp();
    }
}
