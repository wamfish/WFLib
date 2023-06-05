using System.Net;
using System.Net.Sockets;

namespace WFLib;

public static class NetworkExtensionMethods
{
    public static IPEndPoint IPEndPoint(this EndPoint ep)
    {
        return (IPEndPoint)ep;
    }
    public static EndPoint EndPoint(this IPEndPoint ep)
    {
        return ((EndPoint)ep);
    }
    public static EndPoint Duplicate(this EndPoint ep)
    {
        var ip = ep.Address();
        var port = ep.Port();
        var newep = new IPEndPoint(ip, port);
        return newep;
    }
    public static AddressFamily AddressFamily(this EndPoint ep)
    {
        return ep.IPEndPoint().AddressFamily;
    }
    public static int Port(this EndPoint ep)
    {
        return ep.IPEndPoint().Port;
    }
    public static string IP(this EndPoint ep)
    {
        return ep.Address().ToString();
    }
    public static IPAddress Address(this EndPoint ep)
    {
        return ep.IPEndPoint().Address;

    }
    public static byte[] Key(this EndPoint ep)
    {
        byte[] ip = ep.IPEndPoint().Address.GetAddressBytes();
        byte[] port = BitConverter.GetBytes(ep.IPEndPoint().Port);
        byte[] key = new byte[ip.Length + port.Length];
        Buffer.BlockCopy(port, 0, key, 0, port.Length);
        Buffer.BlockCopy(ip, 0, key, port.Length, ip.Length);
        return key;
    }
    //public static void Write(this byte[] dst, byte[] src, int srcPos, int len, ref int pos)
    //{
    //    int bytesLeft = dst.Length - pos;
    //    if (len > bytesLeft)
    //        throw new Exception(Wamfish.Base.Messages.OutOfMemory);
    //    Buffer.BlockCopy(src, srcPos, dst, pos, len);
    //    pos += len;
    //}
    //public static int ReadInt(this byte[] data, ref int pos)
    //{
    //    byte[] tmp = new byte[4];
    //    Buffer.BlockCopy(data, pos, tmp, 0, 4);
    //    Network.Reverse(tmp);
    //    pos += 4;
    //    return BitConverter.ToInt32(tmp, 0);
    //}
    //public static uint ReadUInt(this byte[] data, ref int pos)
    //{
    //    byte[] tmp = new byte[4];
    //    Buffer.BlockCopy(data, pos, tmp, 0, 4);
    //    Network.Reverse(tmp);
    //    pos += 4;
    //    return BitConverter.ToUInt32(tmp, pos);
    //}
    //public static short ReadShort(this byte[] data, ref int pos)
    //{
    //    byte[] tmp = new byte[2];
    //    Buffer.BlockCopy(data, pos, tmp, 0, 2);
    //    Network.Reverse(tmp);
    //    pos += 2;
    //    return BitConverter.ToInt16(tmp, 0);
    //}
    //public static ushort ReadUShort(this byte[] data, ref int pos)
    //{
    //    byte[] tmp = new byte[2];
    //    Buffer.BlockCopy(data, pos, tmp, 0, 2);
    //    Network.Reverse(tmp);
    //    pos += 2;
    //    return BitConverter.ToUInt16(tmp, 0);
    //}
    //public static void Write(this byte[] buf, short output, ref int pos)
    //{
    //    byte[] data = BitConverter.GetBytes(output);
    //    Network.Reverse(data);
    //    Buffer.BlockCopy(data, 0, buf, pos, data.Length);
    //    pos += data.Length;
    //}
    //public static void Write(this byte[] buf, ushort output, ref int pos)
    //{
    //    byte[] data = BitConverter.GetBytes(output);
    //    Network.Reverse(data);
    //    Buffer.BlockCopy(data, 0, buf, pos, data.Length);
    //    pos += data.Length;
    //}
    //public static void Write(this byte[] buf, int output, ref int pos)
    //{
    //    byte[] data = BitConverter.GetBytes(output);
    //    Network.Reverse(data);
    //    Buffer.BlockCopy(data, 0, buf, pos, data.Length);
    //    pos += data.Length;
    //}
    //public static void Write(this byte[] buf, uint output, ref int pos)
    //{
    //    byte[] data = BitConverter.GetBytes(output);
    //    Network.Reverse(data);
    //    Buffer.BlockCopy(data, 0, buf, pos, data.Length);
    //    pos += data.Length;
    //}
}
