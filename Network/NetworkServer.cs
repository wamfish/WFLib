using System.Net;
using System.Net.Sockets;
using WFLib;
using static WFLib.Global;
using Semaphore = System.Threading.Semaphore;

namespace WFLib.Network;
public class NetworkServer
{
    IPEndPoint _ServerEP;
    public IPEndPoint ServerEP => _ServerEP;
    SocketAsyncEventArgs  acceptEventArg;
    Socket listenSocket;
    Semaphore _MaxClients;
    Semaphore MaxClients => _MaxClients;
    internal int _NumConnectedSockets;
    public int NumConnectedSockets => _NumConnectedSockets;
    long _TotlalBytesRead;
    public long TotalBytesRead => _TotlalBytesRead;
    internal Dictionary<IPEndPoint, int> sessionIDs = new Dictionary<IPEndPoint, int>();
    internal Dictionary<int, Client> clients = new Dictionary<int, Client>();
    internal string Name { get; private set; }
    public readonly int BufferSize;
    public NetworkServer(string name, IPEndPoint serverEP, int bufferSize)
    {
        BufferSize = bufferSize;
        Name = name;
        _ServerEP = serverEP;
        _MaxClients = new Semaphore(1000, 1000);
    }
    public void Start()
    {
        listenSocket = new Socket(ServerEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listenSocket.Bind(ServerEP);
        listenSocket.Listen(100);
        listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, BufferSize);
        listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, BufferSize);
        acceptEventArg = new SocketAsyncEventArgs();
        acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventCompleted);
        Log($"{Name} started on {ServerEP.Address}:{ServerEP.Port}");
        StartAccept();
    }
    void AcceptEventCompleted(object sender, SocketAsyncEventArgs e)
    {
        ProcessAccept();
        StartAccept();
    }
    public void StartAccept()
    {
        bool willRaiseEvent = false;
        while (!willRaiseEvent)
        {
            MaxClients.WaitOne();
            acceptEventArg.AcceptSocket = null;
            willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept();
            }
        }
    }
    public event Action<Client> HandleConnection;
    int NextSessionID = 0;
    public Client GetClientById(int sessionID)
    {
        

        Client client;
        lock (sessionIDs)
        {
            if (clients.TryGetValue(sessionID, out client))
            {
                return client;
            }
        }
        return null;
    }
    private void ProcessAccept()
    {
        Interlocked.Increment(ref _NumConnectedSockets);
        var client = Client.Rent((IPEndPoint)acceptEventArg.AcceptSocket.RemoteEndPoint,BufferSize);
        client.socket = acceptEventArg.AcceptSocket;
        client.IsConnected = true;
        lock (sessionIDs)
        {
            client.SessionID = NextSessionID++;
            sessionIDs.Add((IPEndPoint)client.socket.RemoteEndPoint, client.SessionID);
            clients.Add(client.SessionID, client);
        }
        
        if (HandleConnection != null)
        {
            HandleConnection(client);
        }
        else
        {
            Log($"Client connection accepted. There are {NumConnectedSockets} clients connected to the server");
        }
        client.StartRecieve();
    }
}
