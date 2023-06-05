namespace WFLib;
public static class Channel
{
    //ToDo: Make this a configuration option.
    public const int MaxChannelId = 5;
    //The server looks up the channel by the channelId recieved from the client.
    public static ChannelData ServerChannel(HostData host, UdpCommon udp, int channelId)
    {
        if (channelId > MaxChannelId) return null;
        lock (host.ChannelData)
        {
            while (channelId >= host.ChannelData.Count)
            {
                host.ChannelData.Add(ChannelData.Rent(host.ChannelData.Count, host, udp));
            }
            return host.ChannelData[channelId];
        }
    }
    //The client looks up the channel by the ThreadId.
    public static ChannelData ClientChannelByThread(HostData host, UdpCommon udp, ref int NextChannel)
    {
        int tid = Thread.CurrentThread.ManagedThreadId;
        if (!(udp is WFUdpClient udpc)) return null;
        lock (host.ChannelByThread)
        {
            while (tid >= host.ChannelByThread.Count)
            {
                host.ChannelByThread.Add(-1);
            }
        }
        int channel = host.ChannelByThread[tid];
        if (channel < 0)
        {
            if (NextChannel > MaxChannelId) return null;
            channel = NextChannel++;
            host.ChannelByThread[tid] = channel;
            lock (host.ChannelData)
            {
                while (channel >= host.ChannelData.Count)
                {
                    host.ChannelData.Add(ChannelData.Rent(host.ChannelData.Count, host, udp));
                }
            }
        }
        lock (host.ChannelData)
        {
            return host.ChannelData[channel];
        }
    }
    public static ChannelData ClientChannelByChannelId(HostData host, UdpCommon udp, int channelId)
    {
        lock (host.ChannelData)
        {
            while (channelId >= host.ChannelData.Count)
            {
                host.ChannelData.Add(ChannelData.Rent(host.ChannelData.Count, host, udp));
            }
            var cd = host.ChannelData[channelId];
            return cd;
        }
    }
    public static ChannelData GetChannelData(UdpCommon udp, HostData host, int channelId)
    {
        if (udp.IsUdpClient)
        {
            if (udp is WFUdpClient udpc)
            {
                //host = udpc.localHost;
                return ClientChannelByChannelId(host, udp, channelId);
            }
        }
        if (udp.IsUdpServer)
        {
            if (udp is WFUdpServer udps)
            {
                //host = udps.hosts.GetHost(clientEndPoint);
                //if (host == null) return null;
                return ServerChannel(host, udp, channelId);
            }
        }
        throw new NotImplementedException();
    }

}
