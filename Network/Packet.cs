using System.Diagnostics;

namespace WFLib;

public class Packet : IDisposable
{
    public SerializationBuffer Buf;
    public HostData Host;
    public int RequestSize;
    public int RequestBytesRead;
    public long ArriveTimeStamp;

    //buf.WriteSize(cd.Host.Id);
    //        buf.WriteSize(cd.Id); //ChannelData.Id
    //        buf.Write(reqId);
    //        buf.WriteSize(i);
    //        buf.WriteSize(count);
    //        buf.WriteSize(cursize); //size of curbuf
    //        buf.WriteSize(offset);  //in 


    private Packet()
    {
        Init();
    }
    private void Init()
    {
        Buf = null;
        RequestSize = 0;
        RequestBytesRead = 0;
        ArriveTimeStamp = 0;
        Host = null;
    }

    //public ChannelData AssembleResponse(UdpCommon udp, SerializationBuffer inbuf)
    //{
    //    List<SerializationBuffer> buffers;
    //    if (!inbuf.TryReadSize(out int hostId)) return null;
    //    if (!inbuf.TryReadSize(out int channelId)) return null;
    //    if (!inbuf.TryReadUShort(out ushort reqId)) return null;

    //    var cd = Channel.GetChannelData(udp, hostId, channelId, inbuf.ClientEndPoint);
    //    if (cd == null) return null;
    //    buffers = cd.respBuffers;
    //    //We don't care about out of order request. This is
    //    //not the same thing as out of order packets.
    //    if (cd.IsReqIdOutOfOrder(reqId)) return null;

    //    if (reqId != cd.ReqId)
    //    {
    //        // I don't think this should ever happend
    //        // The client should set everything before sending the request
    //        LogError("Does this ever happen?");
    //        return null;
    //    }

    //    if (!inbuf.TryReadSize(out int bufIndex)) return null;
    //    if (!inbuf.TryReadSize(out int bufCount)) return null;
    //    if (!inbuf.TryReadSize(out int curSize)) return null;
    //    if (!inbuf.TryReadSize(out int outOffset)) return null;
    //    //The problem is RequestSize and RequesBytesRead is not set correctly 
    //    //when we come back through on the 2nd call, I need to get this hole scope into my head a little better
    //    while (buffers.Count < bufCount)
    //    {
    //        var rb = SerializationBuffer.Rent(udp.PacketSize);
    //        rb.RequestSize = 0;
    //        rb.RequestBytesRead = 0;
    //        buffers.Add(rb);
    //    }
    //    var outbuf = buffers[bufIndex];
    //    if (outbuf.RequestSize == 0)
    //    {
    //        outbuf.RequestSize = curSize;
    //    }
    //    //copy buf to the request buf
    //    outbuf.BlockCopy(inbuf.Data, inbuf.ReadIndex, outOffset, inbuf.BytesToRead);
    //    outbuf.RequestBytesRead += inbuf.BytesToRead;
    //    bool reqComplete = true;
    //    for (int i = 0; i < bufCount; i++)
    //    {
    //        outbuf = buffers[i];
    //        if (outbuf.RequestSize != outbuf.RequestBytesRead)
    //        {
    //            reqComplete = false;
    //            udp.Log($"Channel Id: {cd.Id} Size: {outbuf.RequestSize} Bytes Read: {outbuf.RequestBytesRead} ");
    //            break;
    //        }
    //    }
    //    if (reqComplete) return cd;
    //    return null;
    //}



    private static Pool<Packet> pool = new(() => new Packet());
    /// <summary>
    /// 
    /// Use this to get a MeshData object. 
    /// 
    /// Example: using var md = MeshData.Rent();
    ///     
    /// </summary>
    /// <returns> TestObjectPooling </returns>
    public static Packet Rent(SerializationBuffer buf, HostData host)
    {
        var packet = pool.Rent();
        packet.Init();
        packet.Buf = SerializationBuffer.Rent(buf.BytesUsed);
        packet.Buf.BlockCopy(buf.Data, 0, 0, buf.BytesUsed);
        packet.ArriveTimeStamp = Stopwatch.GetTimestamp();
        packet.Host = host;
        return packet;
    }
    /// <summary>
    /// Returns a string with stats about the pool
    /// </summary>
    public static string PoolStats => pool.Stats;
    /// <summary>
    /// Clears the pool
    /// </summary>
    public static void PoolClear() => pool.Clear();
    /// <summary>
    /// If it is not practical to use the using clause
    /// You can return an object to the pool with this method.
    /// The using clause is preferred.
    /// </summary>
    public void Return() => Dispose();
    public void Dispose()
    {
        Buf.Return();
        Init();
        pool.Return(this);
    }
}
