using SkiaSharp;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using WFLib;
using static WFLib.Global;
namespace WFLib.Network;
public class TunnelClient
{
    SerializationBuffer swapBuffer;
    SerializationBuffer readBuffer;
    Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public Client tunnelClient;
    IPEndPoint serverEP;
    public TunnelClient(IPEndPoint tunnelEP, IPEndPoint serverEP)
    {
        this.serverEP = serverEP;
        tunnelClient = Client.Rent(tunnelEP,8192+8);
        tunnelClient.OnConnect += HandleOnTunnelConnected;
        tunnelClient.OnAfterClose += HandleOnTunnelClose;
        tunnelClient.OnConnectError += HandleOnTunnelConnectError;
        tunnelClient.OnReceive += HandleOnTunnelReceive;
        tunnelClient.OnSent += HandleOnTunnelSent;
    }
    public void Start()
    {
        tunnelClient.Connect();
    }
    public void Stop()
    {
        tunnelClient.Close();
    }
    void HandleOnTunnelConnected(Client client)
    {
        readBuffer = SerializationBuffer.Rent();
        swapBuffer = SerializationBuffer.Rent();
        client.Description = $"tc:";
        Log($"{client.Description} connect to {client.socket.RemoteEndPoint}");
    }
    void HandleOnTunnelSent(Client client, int bytesSent)
    {
        Debug.Assert(Log($"{client.Description} sent {bytesSent} bytes id: {client.SentId}"));
    }
    void CloseSession(Client client, int sessionID)
    {
        if (clients.TryGetValue(sessionID, out var webClient))
        {
            Debug.Assert(LogWarning($"{client.Description} recieved a close request for: {webClient.Description}"));
            webClient.ClosedByTunnel = true;
            return;
        }
        Debug.Assert(LogError($"{client.Description} recieved a close request for undefined session: {sessionID}"));

    }

    // Note: To any fellow programmers reading this. I did not relize that TCP unlike UDP does not
    // keep your send/recv as seperate pacekts. Tha bad part is sometimes it does, and if you did not
    // know better, you might think it did like me.
    //
    // Depending on the situation this might not actually be anything to track. The tunnel part of the
    // TunnelClient/TunnelServer needs to parse the data looking for a sessionid, we have to treat the
    // reads as a stream of data. I am a little pissed off that I did not know this, but I am glad I
    // figured it out. I spent three damn days trying to figure out what was going on. One bad
    // assumption kept me blind to the real issue.

    void SwapBuffers()
    {
        if (readBuffer.BytesToRead == 0)
        {
            readBuffer.Clear();
            return;
        }
        swapBuffer.Clear();
        swapBuffer.Append(readBuffer.Data,readBuffer.ReadIndex,readBuffer.BytesToRead);
        readBuffer.Clear();
        var temp = readBuffer;
        readBuffer = swapBuffer;
        swapBuffer = temp;
    }
    //ToDo: the stream logic below is duplicated in tunnel server,
    //it should be moved to a common class
    void HandleOnTunnelReceive(Client client, byte[] buffer, int offset, int size)
    {
        readBuffer.SetReadIndex(0);
        readBuffer.Append(buffer, offset, size);
        while (readBuffer.BytesToRead >= 12)
        {
            int packetSize = readBuffer.ReadInt();
            if (packetSize - 4 > readBuffer.BytesToRead)
            {
                Debug.Assert(LogWarning($"{client.Description} received a partial packet"));
                return;
            }
            int sessionID = readBuffer.ReadInt();
            if (sessionID == -1)
            {
                sessionID = readBuffer.ReadInt();
                CloseSession(client, sessionID);
                SwapBuffers();
                return;
            }
            Debug.Assert(Log($"{client.Description} received {size} bytes for {sessionID}"));
            if (clients.TryGetValue(sessionID, out var webClient))
            {
                if (webClient.SessionID != sessionID)
                {
                    LogError($"{client.Description} recieved a packet for {sessionID} but the session id does not match");
                    return;
                }
                webClient.Send(readBuffer.Data, readBuffer.ReadIndex, packetSize - 8);
                readBuffer.SetReadIndex(readBuffer.ReadIndex + packetSize - 8);
                SwapBuffers();
            }
            else
            {
                var webServerClient = Client.Rent(serverEP, 8192);
                webServerClient.SessionID = sessionID;
                webServerClient.Description = $"wsc:[{sessionID}]";
                clients.Add(sessionID, webServerClient);
                webServerClient.OnReceive += HandleOnWebServerClientReceive;
                webServerClient.OnAfterClose += HandleOnWebServerClientClose;
                webServerClient.OnSent += HandleOnWebServerClientSent;
                webServerClient.OnConnect += HandleOnWebServerClientConnect;
                webServerClient.connectBuffer = SerializationBuffer.Rent();
                webServerClient.connectBuffer.Append(readBuffer.Data, readBuffer.ReadIndex, packetSize - 8);
                readBuffer.SetReadIndex(readBuffer.ReadIndex + packetSize - 8);
                SwapBuffers();
                webServerClient.Connect();
            }
        }
    }
    void HandleOnTunnelConnectError(Client client, SocketError error)
    {
        LogError($"{client.Description} connect error: {error}");
        Task.Run(()=>TunnelRestart());
    }
    void HandleOnTunnelClose(Client client)
    {
        if (readBuffer.BytesToRead > 0)
        {
              LogError($"{client.Description} Closed with {readBuffer.BytesToRead} bytes left in the buffer");
        }
        readBuffer.Return();
        readBuffer = null;
        swapBuffer.Return();
        swapBuffer = null;
        Log($"{client.Description} Closed");
        Task.Run(() => TunnelRestart());
    }
    void TunnelRestart()
    {
        Log($"Retry Connect in 10 secounds");
        Task.Delay(1000 * 10).Wait();
        if (tunnelClient.IsConnected)
            return;
        Log("Attempting to reconnect");
        tunnelClient.Connect();
    }
    void HandleOnWebServerClientConnect(Client c)
    {
        if (c.connectBuffer != null)
        {
            var sb = c.connectBuffer;
            c.Send(sb.Data, sb.ReadIndex, sb.BytesToRead);
            c.connectBuffer.Return();
            c.connectBuffer = null;
        }
    }
    void HandleOnWebServerClientClose(Client client)
    {
        Debug.Assert(LogWarning($"{client.Description} Closed"));
        using var sb = SerializationBuffer.Rent();
        sb.Write((int)12);
        sb.Write((int)-1);
        sb.Write(client.SessionID);
        tunnelClient.Send(sb.Data, 0, sb.BytesToRead);
    }
    void HandleOnWebServerClientSent(Client client, int bytesSent)
    {
        Debug.Assert(Log($"{client.Description} sent {bytesSent} bytes"));
    }
    void HandleOnWebServerClientReceive(Client client, byte[] buffer, int offset, int size)
    {
        if (tunnelClient != null && tunnelClient.IsConnected)
        {
            Debug.Assert(Log($"{client.Description} recvieved {size} bytes"));
            using var sb = SerializationBuffer.Rent();
            sb.Write(size + 8);
            sb.Write(client.SessionID);
            sb.Append(buffer, offset, size);
            sb.SetReadIndex(0);
            tunnelClient.Send(sb.Data, 0, size+8);
            return;
        }
        LogError($"{client.Description} is not connected, but recieved {size} bytes");
    }
}
