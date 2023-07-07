namespace WFLib.Network;

public static class PacketType
{
    public const byte Ping = 0;
    public const byte Pong = 1;
    public const byte Close = 2;
    public const byte Packet = 3;
    public const byte PacketAck = 4;
}
