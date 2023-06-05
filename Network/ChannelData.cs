namespace WFLib;
public enum RecvStatus { Ok, Canceled, Timeout }
public class ChannelData : IDisposable
{
    static Queue<ChannelData> channelDataPool = new();
    private ChannelData(int id, HostData host, UdpCommon udp)
    {
        Id = id;
        Mre = null;
        if (udp.IsUdpClient)
        {
            Mre = new ManualResetEvent(false); // false means blocking
            Mre.Reset();
        }
        ReqId = 0;
        Canceled = false;
        Host = host;
        Udp = udp;
    }

    public int Id;
    public ManualResetEvent Mre;
    public ushort ReqId; //used to verify packet order
    public readonly List<SerializationBuffer> reqBuffers = new();
    public readonly List<SerializationBuffer> respBuffers = new();
    public bool Canceled;
    public HostData Host { get; internal set; }
    public UdpCommon Udp { get; internal set; }
    public static ChannelData Rent(int id, HostData host, UdpCommon udp)
    {
        ChannelData cd = null;
        lock (channelDataPool)
        {
            if (channelDataPool.Count > 0)
            {
                cd = channelDataPool.Dequeue();
                cd.Id = id;
                cd.Udp = udp;
                cd.Host = host;
                if (udp.IsUdpClient)
                {
                    cd.Mre = new ManualResetEvent(false);
                    cd.Mre.Reset();
                }
            }
            else
            {
                cd = new ChannelData(id, host, udp);
            }
        }
        return cd;
    }
    //This test for out of order in theory is not 100% accurate.
    //In real life a seriously doubt this will be an issue. To get more
    //accuracy I could use a int or a long, but I  think a ushort should
    //give us a large enough window 
    public bool IsReqIdOutOfOrder(ushort reqId)
    {
        if (reqId < ReqId)
        {
            //This allows for reqId wrapping back to 0
            if (ReqId - reqId > short.MaxValue) return false;
            return true;
        }
        return false;
    }
    public void NextRequest()
    {
        ReqId++;
    }

    public void Dispose()
    {
        reqBuffers.Clear();
        respBuffers.Clear();
        Id = -1;
        ReqId = 0;
        Mre = null;
        lock (channelDataPool)
        {
            channelDataPool.Enqueue(this);
        }
        //GC.SuppressFinalize(this);
    }
}
