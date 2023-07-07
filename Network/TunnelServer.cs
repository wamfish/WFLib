using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using WFLib;
using static WFLib.Global;
namespace WFLib.Network;
public class TunnelServer
{
    SerializationBuffer swapBuffer;
    SerializationBuffer readBuffer;
    NetworkServer tunnelServer;
    NetworkServer webBrowserServer;
    Client tunnelClient = null;
    public TunnelServer(IPEndPoint tunnelEP, IPEndPoint webBrowserEP, IPAddress remoteAddress)
    {
        List<IPAddress> valid = new List<IPAddress>();
        valid.Add(remoteAddress);
        valid.Add(IPAddress.Loopback);
        tunnelServer = new NetworkServer("ts:",tunnelEP,8192+8,valid);
        tunnelServer.HandleConnection += TunnelConnected;
        webBrowserServer = new NetworkServer("wbs:",webBrowserEP,8192);
        webBrowserServer.HandleConnection += BrowserConnected;
    }
    public void TunnelConnected(Client client)
    {
        if (tunnelClient != null && tunnelClient.IsConnected)
        {
            LogError($"ts: already has a connection");
            client.Close();
            return;
        }
        if (tunnelClient != null) tunnelClient.Return();
        tunnelClient = client;
        readBuffer = SerializationBuffer.Rent();
        swapBuffer = SerializationBuffer.Rent();
        tunnelClient.Description = $"ts:";
        Log($"{tunnelClient.Description} connected to {client.socket.RemoteEndPoint}");
        tunnelClient.OnReceive += HandleOnTunnelClientReceive;
        tunnelClient.OnAfterClose += (c) =>
        {
            if (readBuffer.BytesToRead > 0)
            {
                LogError($"{c.Description} Closed with {readBuffer.BytesToRead} bytes left in the buffer");
            }
            readBuffer.Return();
            readBuffer = null;
            swapBuffer.Return();
            swapBuffer = null;
            Log($"ts:{c.ServerEP} closed");
            tunnelClient = null;
        };
        tunnelClient.OnSent += (c,bs) =>
        {
            Debug.Assert(Log($"{c.Description} sent {bs} bytes"));
        };
    }
    void CloseSession(Client client, int sessionID)
    {
        var wbc = webBrowserServer.GetClientById(sessionID);
        if (wbc != null)
        {
            Debug.Assert(LogWarning($"{client.Description} recieved a close request for: {wbc.Description}"));
            wbc.ClosedByTunnel = true;
            return;
        }
        LogError($"{client.Description} recieved a close request for undefined session: {sessionID}");

    }
    void SwapBuffers()
    {
        if (readBuffer.BytesToRead == 0)
        {
            readBuffer.Clear();
            return;
        }
        swapBuffer.Clear();
        swapBuffer.Append(readBuffer.Data, readBuffer.ReadIndex, readBuffer.BytesToRead);
        readBuffer.Clear();
        (swapBuffer, readBuffer) = (readBuffer, swapBuffer);
    }
    public void HandleOnTunnelClientReceive(Client client, byte[] buffer, int offset, int size)
    {
        readBuffer.SetReadIndex(0);
        readBuffer.Append(buffer, offset, size);
        while(readBuffer.BytesToRead >= 12)
        {
            int packetSize = readBuffer.ReadInt();
            if (packetSize-4 > readBuffer.BytesToRead)
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
            var wb = webBrowserServer.GetClientById(sessionID);
            if (wb.SessionID != sessionID)
                LogError($"wb sessionID mismatch: {wb.SessionID} != {sessionID}");
            if (wb != null)
            {
                wb.Send(readBuffer.Data, readBuffer.ReadIndex, packetSize - 8);
                readBuffer.SetReadIndex(readBuffer.ReadIndex + packetSize - 8);
                SwapBuffers();
            }
            else
            {
                readBuffer.SetReadIndex(readBuffer.ReadIndex + packetSize - 8);
                SwapBuffers();
                LogError($"wbs:[{sessionID}] not found");
            }
        }
    }
    public void BrowserConnected(Client client)
    {
        client.Description = $"wbs:[{client.SessionID}]";
        Debug.Assert(Log($"{client.Description} connected to wb"));
        client.OnReceive += FromBrowser;
        client.OnAfterClose += (c) =>
        {
            using var sb = SerializationBuffer.Rent();
            sb.Write((int)12);
            sb.Write((int)-1);
            sb.Write(c.SessionID);
            tunnelClient?.Send(sb.Data,0,sb.BytesUsed);
            Debug.Assert(LogWarning($"{c.Description} closed"));
        };
        client.OnSent += (c,bs) =>
        {
            Debug.Assert(Log($"{c.Description} sent {bs} bytes"));
        };
    }
    public void FromBrowser(Client client, byte[] buffer, int offset, int size)
    {
        if (tunnelClient != null && tunnelClient.IsConnected)
        {
            Debug.Assert(Log($"{client.Description} recieved {size} bytes"));
            using var sb = SerializationBuffer.Rent();
            sb.Write(size+8);
            sb.Write(client.SessionID);
            sb.Append(buffer, offset, size);
            tunnelClient.Send(sb.Data, 0, size+8);
            return;
        }
        LogError($"{client.Description} is not connected, but recieved {size} bytes");
    }

    public void Start()
    {
        tunnelServer.Start();
        webBrowserServer.Start();
    }
    public void Stop()
    {
        tunnelServer.Stop();
        webBrowserServer.Stop();
    }
}
