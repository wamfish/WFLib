using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using WFLib;
namespace WFLib.Network;
public class SocketAEArgs
{
    public readonly SocketAsyncEventArgs eventArgs;
    private EventHandler<SocketAsyncEventArgs> eh;
    public Client Client { get; private set; }
    #region pool
    private SocketAEArgs() 
    { 
        eventArgs = new SocketAsyncEventArgs();
    }
    private static SocketAEArgs Create()
    {
        return new SocketAEArgs();
    }
    private static Pool<SocketAEArgs> pool = new(Create);
    public static SocketAEArgs Rent(Client client, Action<object, SocketAsyncEventArgs> onCompleted, int bufferSize)
    {
        var args = pool.Rent();
        args.Client = client;
        args.eh = new EventHandler<SocketAsyncEventArgs>(onCompleted);
        args.eventArgs.Completed += args.eh;
        args.eventArgs.UserToken = args;
        args.eventArgs.AcceptSocket = client.socket;
        args.eventArgs.SetBuffer(ByteArrayPool.Rent(bufferSize), 0, bufferSize);
        return args;
    }
    public static string PoolStats => pool.Stats;
    public static void PoolClear() => pool.Clear();
    public void Return() => Dispose();
    public void Dispose()
    {
        eventArgs.Completed -= eh;
        eh = null;
        Client = null;
        ByteArrayPool.Return(eventArgs.Buffer);
        eventArgs.SetBuffer(null, 0, 0);
        pool.Return(this);
    }
    #endregion



}
