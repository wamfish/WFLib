using System.Net;
namespace WFLib;
public class Hosts
{
    private Dictionary<EndPoint, int> hostLookup = new Dictionary<EndPoint, int>();
    private List<HostData> hostList = new List<HostData>();

    private EndPoint localEndPoint;
    public Hosts(EndPoint localEndPoint)
    {
        this.localEndPoint = localEndPoint.Duplicate();
    }
    public HostData AddOrGetHost(EndPoint remoteEndPoint)
    {
        int id;
        if (remoteEndPoint.Equals(localEndPoint)) return null;
        lock (hostLookup)
        {
            if (hostLookup.TryGetValue(remoteEndPoint, out id))
            {
                return hostList[id];
            }
        }
        HostData host;
        lock (hostList)
        {
            id = hostList.Count;
            host = new HostData(localEndPoint, remoteEndPoint.Duplicate(), id);
            hostList.Add(host);
        }
        lock (hostLookup)
        {
            hostLookup.Add(host.RemoteEndPoint, id);
        }
        return host;
    }
    public HostData GetHost(EndPoint remoteEndPoint)
    {
        lock (hostLookup)
        {
            if (hostLookup.TryGetValue(remoteEndPoint, out int id))
            {
                return hostList[id];
            }
        }
        return null;
    }
}