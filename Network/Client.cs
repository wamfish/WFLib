using System.Data;
using System.Net;
using System.Net.Sockets;
using WFLib;
using static WFLib.Global;
namespace WFLib.Network;
public class Client
{
    public event Action<Client, byte[], int, int> OnReceive;
    public event Action<Client> OnConnect;
    public event Action<Client, SocketError> OnConnectError;
    public event Action<Client> OnAfterClose;
    public event Action<Client, int> OnSent;
    private void ClearEvents()
    {
        OnReceive = null;
        OnConnect = null;

        OnAfterClose = null;
        OnSent = null;
    }
    long _TotalBytesRead = 0;
    public long TotalBytesRead => _TotalBytesRead;
    internal IPEndPoint ServerEP;
    internal Socket socket;
    internal NetworkServer server = null;
    SocketAsyncEventArgs connectEventArgs;
    public bool IsConnected { get; internal set; } = false;
    public int SessionID { get; internal set; }
    public bool ClosedByTunnel { get; internal set; } = false;
    internal SerializationBuffer connectBuffer; // Used by tunnelClient in the connect process
    public string Description { get; set; }
    public bool DoOnAfterClose = true;
    public int BufferSize { get; private set; }
    
    
    public void Connect()
    {
        if (socket != null)
        {
            LogError($"{Description} already connected");
            return;
        }
        socket = new Socket(ServerEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, BufferSize);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, BufferSize);
        bool willRaiseEvent = socket.ConnectAsync(connectEventArgs);
        if (!willRaiseEvent)
        {
            ProcessConnect(connectEventArgs);
        }
    }
    void ProcessConnect(SocketAsyncEventArgs e)
    {
        socket = e.ConnectSocket;
        if (e.SocketError == SocketError.Success)
        {
            IsConnected = true;
            if (OnConnect != null)
            {
                OnConnect(this);
                if (Description == "Client")
                {
                    Description = $"Client:[{SessionID}]";
                }
            }
            else
            {
                Log($"Socket connected: local: {socket.LocalEndPoint} remote: {socket.RemoteEndPoint} ");
            }
            StartRecieve();
            return;
        }
        socket = null;
        if (OnConnectError != null)
        {
            OnConnectError(this, e.SocketError);
        }
        else
        {
            LogError($"{Description} connect error: {e.SocketError}");
        }
        
    }
    private void ConnectCompleted(object sender, SocketAsyncEventArgs e)
    {
        ProcessConnect(e);
    }
    public void Close()
    {
        string msg ="";
        lock(this)
        {
            if (socket == null) return;
            msg = $"Socket closed: local: {socket.LocalEndPoint} remote: {socket.RemoteEndPoint}";
            IsConnected = false;
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception) { }
            socket.Close();
            socket = null;
        }
        if (server != null)
        {
            lock (server.sessionIDs)
            {
                server.sessionIDs.Remove((IPEndPoint)socket.LocalEndPoint);
                server.clients.Remove(SessionID);
                SessionID = -1;
            }
            Interlocked.Decrement(ref server._NumConnectedSockets);
            server = null;
        }
        if (DoOnAfterClose && OnAfterClose != null)
        {
            OnAfterClose(this);
            return;
        }
        Log(msg);
    }
    public void StartRecieve()
    {
        SocketAEArgs readArgs;
        Init();
        bool willRaiseEvent = false;
        while(!willRaiseEvent)
        {
            if (!IsConnected)
            {
                LogError($"Client:{SessionID} not connected");
                return;
            }
            willRaiseEvent = socket.ReceiveAsync(readArgs.eventArgs);
            if (!willRaiseEvent)
            {
                if (readArgs.eventArgs.SocketError != SocketError.Success)
                {
                    LogError($"Socket error: {readArgs.eventArgs.SocketError}");
                    Close();
                    return;
                }
                if (readArgs.eventArgs.BytesTransferred == 0)
                {               
                    LogError($"Socket closed: {readArgs.eventArgs.SocketError}");
                    Close();
                    return;
                }
                ProcessReceive(readArgs.eventArgs);
                Init();
            }
        }
        void Init()
        {
            readArgs = SocketAEArgs.Rent(this,ReceiveCompleted,BufferSize);
            //readArgs.eventArgs.SetBuffer(0,8192);
        }
    }
    void ProcessReceive(SocketAsyncEventArgs e)
    {
        var args = (SocketAEArgs)e.UserToken;
        if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
        {
            Interlocked.Add(ref _TotalBytesRead, e.BytesTransferred);
            if (OnReceive != null)
            {
                OnReceive(this, e.Buffer, e.Offset, e.BytesTransferred);
            }
            else
            {
                Log($"{socket.LocalEndPoint} received {e.BytesTransferred} bytes from {socket.RemoteEndPoint} total bytes: {TotalBytesRead}");
            }
            args.Return();
            return;
        }
        args.Return();
    }
    void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {

        if (e.SocketError != SocketError.Success)
        {
            LogError($"Socket error: {e.SocketError} bytes transferred: {e.BytesTransferred}");
            Close();
            return;
        }
        if (e.BytesTransferred == 0)
        {
            LogError($"Socket closed: {e.SocketError}");
            Close();
            return;
        }
        ProcessReceive(e);
        StartRecieve();
    }
    internal int SentSize;
    internal int SentId;
    public void Send(byte[] buffer, int offset, int size)
    {
        if (socket == null)
        {
            LogError($"Socket is null: {SessionID}  ");
            return;
        }
        socket.Send(buffer, offset, size, SocketFlags.None);
        if (OnSent != null)
        {
            using var sb = SerializationBuffer.RentSpecialReadonly(buffer, offset, size);
            if (size >= 8)
            {
                SentSize = sb.ReadInt();
                SentId = sb.ReadInt();
            }
            else
            {
                SentSize = -1;
                SentId = -1;
            }
            OnSent(this, size);
        }
        else
        {
            Log($"{socket.LocalEndPoint} sent {size} bytes to {socket.RemoteEndPoint}");
        }
        return;
    }
    public void SendAsync(byte[] buffer, int offset, int size)
    {
        if (socket == null)
        {
            LogError("Socket is null");
            return;
        }
        if (size > 8192)
        {
            socket.Send(buffer, offset, size, SocketFlags.None);
            return;
        }
        var args = SocketAEArgs.Rent(this,SendCompleted,BufferSize);
        Buffer.BlockCopy(buffer, offset, args.eventArgs.Buffer, 0, size);
        args.eventArgs.SetBuffer(0, size);
        bool willRaiseEvent = socket.SendAsync(args.eventArgs);
        if (!willRaiseEvent)
        {
            ProcessSend(args.eventArgs);
        }
    }
    public void ProcessSend(SocketAsyncEventArgs e)
    {
        var args = (SocketAEArgs)e.UserToken;
        if (e.SocketError == SocketError.Success)
        {
            if (OnSent != null)
            {
                OnSent(this, e.BytesTransferred);
            }
            else
            {
                Log($"{socket.LocalEndPoint} sent: {e.BytesTransferred} bytes to {socket.RemoteEndPoint}");
            }
            args.Return();
            return;
        }
        args.Return();
        if (e.SocketError != SocketError.Success)
        {
            LogError($"Socket error: {e.SocketError} bytes transferred: {e.BytesTransferred}");
            Close();
        }
        Close();
    }
    public void SendCompleted(object sender,SocketAsyncEventArgs e)
    {
        ProcessSend(e);
    }
    #region pool
    private Client() 
    {
        connectEventArgs = new SocketAsyncEventArgs();
        connectEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectCompleted);
    }
    private static Client Create()
    {
        return new Client();
    }
    private static Pool<Client> pool = new(Create);
    public static Client Rent(IPEndPoint serverEP, int bufferSize)
    {
        var client = pool.Rent();
        client.ServerEP = serverEP;
        client.connectEventArgs.RemoteEndPoint = serverEP;
        client.Description = "Client";  
        client.SessionID = -1;
        client.DoOnAfterClose = true;
        client.ClosedByTunnel = false;
        client.BufferSize = bufferSize;
        return client;
    }
    public static string PoolStats => pool.Stats;
    public static void PoolClear() => pool.Clear();
    public void Return() => Dispose();
    public void Dispose()
    {
        if (socket != null) Close();
        server = null;
        ClearEvents();
        Description = "";
        pool.Return(this);
    }
    #endregion
}
