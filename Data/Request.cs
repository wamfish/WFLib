//using System.Diagnostics;
//using System.Net;
//using System.Security.Principal;

//namespace WFLib;


//public class Request
//{
//    public Request(int reqId, RequestType reqType,  HostData host, UdpCommon udpCommon)
//    {
//        Init(reqId, reqType, host, udpCommon);
//    }
//    public void Init(int reqId, RequestType reqType, HostData host, UdpCommon udpCommon)
//    {
//        Canceled = false;
//        RequestId = reqId;
//        RequestType = reqType;
//        Host = host;
//        Udp = udpCommon;
//        RequestType = RequestType.Ignore;
//        OnReqResponse = null;
//        SentTimeStamp = -1;
//    }
//    public HostData Host { get; private set; }
//    public int RequestId { get; private set; }
//    public UdpCommon Udp { get; internal set; }
//    public RequestType RequestType { get; private set; }
//    private Action OnReqResponse;
//    public readonly List<SerializationBuffer> ReqBuffers = new List<SerializationBuffer>();
//    public long SentTimeStamp = 0;
//    public bool Cancel()
//    {
//        return true;
//    }
//    public void Clear()
//    {
//        Init(-1, RequestType.Ignore, null, null);
//        ReqBuffers.Clear();
//    }
//    public readonly SerializationBuffer RecvBuffer = SerializationBuffer.Rent(UdpCommon.PacketSize);
//    //public void ServerPing(string msg)
//    //{
//    //    OnReqResponse = LogServerPing;
//    //    Log($"Send Server Ping");
//    //    RequestType = RequestType.ConnectRequest;
//    //    SerializationBuffer buf = SerializationBuffer.Rent();
//    //    buf.Write((byte)RequestType);
//    //    buf.WriteSize(Host.Id);
//    //    buf.Write(msg);
//    //    Udp.SendRequest(this);
//    //}
//    public void Ping(string msg)
//    {
//        OnReqResponse = LogPing;
//        RequestType = RequestType.Ping;
//        SerializationBuffer buf = SerializationBuffer.Rent();
//        buf.Write((byte)RequestType);
//        Udp.SendRequest(this);
//    }

//    private void LogPing()
//    {
//        string msg = "";
//        if (ReqBuffers.Count > 0) msg = $": {ReqBuffers[0].ReadString()}";
//        if (SentTimeStamp < 0) 
//        {
//            Udp.Log($"Ping Response{msg}");
//            return;
//        }
//        long curTimeStamp = Stopwatch.GetTimestamp();
//        long diff = (curTimeStamp - SentTimeStamp) / 1000;
//        Udp.Log($"Ping Response({diff}){msg}");
//        return;
//    }
//    private void LogServerPing()
//    {
//        string msg = "";
//        var buf = ReqBuffers[0];
//        int id = buf.ReadSize();
//        if (buf.BytesToRead > 0)
//        {
//            msg = $"Message: {buf.ReadString()}";
//        }
//        Host.id = id;
//        if (SentTimeStamp < 0)
//        {
//            Udp.Log($"Server Ping Id {id}{msg}");
//            return;
//        }
//        long curTimeStamp = Stopwatch.GetTimestamp();
//        long diff = (curTimeStamp - SentTimeStamp) / 1000;
//        Udp.Log($"Server Ping ({diff}) Id {id}{msg}");
//        return;
//    }

//    private void ServerPingResponse(SerializationBuffer packet, EndPoint remote)
//    {
//        if (!remote.Equals(Host.ServerEndPoint))
//        {
//            LogError($"ServerPing from {remote}: not my server.");
//            return;
//        }
//        int clientId = packet.ReadInt();
//        Host.id = clientId;
//        OnReqResponse?.Invoke();

//    }
//    void PingResponse(SerializationBuffer packet)
//    {
//        string msg = packet.ReadString();
//        Log($"Ping Message:{msg}");
//    }

//    void HandleReadRecord(SerializationBuffer buf)
//    {
//        try
//        {
//            byte packetType = buf.ReadByte();
//            int reqId = buf.ReadSize();
//            int bufIndex = buf.ReadSize();
//            int recId = buf.ReadSize();
//            var req = GetRequest(reqId, packetType, localHost);
//            if (req == null)
//            {
//                LogError($"HandleReadRecord: Invalid request({reqId})");
//                return;
//            }
//            if (req.DoOnReadRecord(buf))
//            {
//                buf = null; //Caller wil return buf
//                return;
//            }
//        }
//        catch (Exception ex)
//        {
//            LogException(ex);
//            return;
//        }
//        finally
//        {
//            buf?.Return();
//        }
//    }
//    void HandleReadRecords(SerializationBuffer packet)
//    {

//    }



//    private static Queue<Request> reqPool = new Queue<Request>();
//    public void Return()
//    {
//        Clear();
//        lock (reqPool)
//        {
//            reqPool.Enqueue(this);
//        }
//    }
//    public static Request Rent(int reqId, RequestType packetType, HostData host, UdpCommon udp)
//    {
//        Request req;
//        lock (reqPool)
//        {
//            if (reqPool.Count > 0)
//            {
//                req = reqPool.Dequeue();
//                req.Init(reqId, packetType, host, udp);
//            }
//            else
//            {
//                req = new Request(reqId, packetType, host, udp);
//            }
//        }
//        return req;
//    }
//}
