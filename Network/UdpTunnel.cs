//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

using WFLib.Network;
using static WFLib.Global;
using WFLib;

public class UdpTunnel : UdpTunnelBase
{
    public override void OnError(string message)
    {
        LogError($"{Name} error: {message}");
    }

    public override void OnException(Exception ex)
    {
        LogError($"{Name} error: {ex.Message}");
    }

    public override void OnReceive(SerializationBuffer sb)
    {
        Log($"{Name} received: {sb.BytesUsed} bytes on channel {sb.ChannelID} Seq: {sb.SeqNum}");
    }

    public override void OnSend(SerializationBuffer sb)
    {
        Log($"{Name} sent: {sb.BytesUsed} bytes on channel {sb.ChannelID} Seq: {sb.SeqNum}");
    }
    public override void OnReceiveAck(SerializationBuffer sb)
    {
        Log($"{Name} received ack for: seq {sb.SeqNum} channel {sb.ChannelID}");
    }

    public override void OnStart()
    {
        Log($"{Name} Started on {socket.LocalEndPoint}");
    }

    public override void OnStop()
    {
        Log($"{Name} stoped on {LocalEndPoint}");
    }
}
