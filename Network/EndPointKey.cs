using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace WFLib.Network;

public struct EndPointKey
{
    byte[] ipData;
    int port;
    public EndPointKey(EndPoint ep)
    {
        ipData = ((IPEndPoint)ep).Address.GetAddressBytes();
        port = ((IPEndPoint)ep).Port;
    }
    public EndPointKey(IPEndPoint ep)
    {
        ipData = ep.Address.GetAddressBytes();
        port = ep.Port;
    }
    public void Init(EndPoint ep)
    {
        ipData = ((IPEndPoint)ep).Address.GetAddressBytes();
        port = ((IPEndPoint)ep).Port;
    }
    public void Init(IPEndPoint ep)
    {
        ipData = ep.Address.GetAddressBytes();
        port = ep.Port;
    }
    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is not EndPointKey epk) return false;
        if (epk.port != port) return false;
        if (epk.ipData is null && ipData is null) return true;
        if (epk.ipData is null) return false;
        if (ipData is null) return false;
        if (ipData.Length != epk.ipData.Length) return false;
        for(int i = 0; i < ipData.Length; i++)
        {
            if (ipData[i] != epk.ipData[i])
            {
                return false;
            }
        }
        return true;
    }
    public override string ToString()
    {
        return $"{Convert.ToBase64String(ipData)}:{port}";
    }
}
