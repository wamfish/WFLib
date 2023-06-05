//using System.Diagnostics;
//using System.Net;

//namespace WFLib;

//public partial class WFUdpServer
//{
//    public class IgnoreHost
//    {
//        private readonly static long OneHour = 60 * 60 * Stopwatch.Frequency;
//        Dictionary<string, IgnoreInfo> InfoDict = new();

//        private class IgnoreInfo
//        {
//            public IgnoreInfo(IPEndPoint endPoint)
//            {
//                this.endPoint = endPoint;
//                IPCount = 1;
//                PortCount = new Dictionary<int, int>();
//                PortCount.Add(endPoint.Port, 1);
//                Expires = new Dictionary<int, long>();
//                long curTick = Stopwatch.GetTimestamp();
//                Expires.Add(endPoint.Port, curTick + OneHour);
//            }
//            public bool IPBanned = false;
//            public IPEndPoint endPoint;
//            public Dictionary<int, long> Expires;
//            public Dictionary<int,int> PortCount;
//            public int IPCount;
//            public bool PortBanned
//            {
//                get
//                {
//                    return false;
//                }
//            }

//        }
//        public bool IgnoreEndpoint(EndPoint endPoint)
//        {
//            var ep = endPoint.IPEndPoint();
//            IgnoreInfo info;
//            lock (InfoDict)
//            {

//                if (!InfoDict.TryGetValue(ep.Address.ToString(), out info)) return false;
//            }
//            if (info.IPBanned) return true;
//            if (info.PortBanned) return true;
//            return false;
//        }

//        public void UpdateIgnoreEndpoint(EndPoint endPoint)
//        {
//            var ep = endPoint.IPEndPoint();
//            IgnoreInfo info;
//            lock (InfoDict)
//            {
//                if (!InfoDict.TryGetValue(ep.Address.ToString(), out info))
//                {
//                    info = new IgnoreInfo(ep);
//                    InfoDict.Add(ep.Address.ToString(), info);
//                    return;
//                }
//            }
//            info.IPCount++;




//            long curTicks = Stopwatch.GetTimestamp();
//            if (IgnoreDict.TryGetValue(endPoint, out long endIgnore))
//            {
//                IgnoreDict.TryAdd(endPoint, curTicks + 60 * 60 * Stopwatch.Frequency);
//                return;
//            }
//            IgnoreDict.TryAdd(endPoint, curTicks + 5 * Stopwatch.Frequency);
//        }

//    }
//}
