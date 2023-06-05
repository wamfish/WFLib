//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;
/// <summary>
/// The ByteArray class uses this class to manage its memory. The goal is
/// for ByteArray to be friendly for game apps that want to avoid the GC from
/// being called. As this can cause unwanted lag spikes. The MaxPoolSize is 
/// the largest chunk of memory that this class will try and pool. Anything
/// larger and it is left up to the GC to handle.
/// </summary>
public static class ByteArrayPool
{
    const int MaxPoolSize = 1024 * 128; //this is the size table filter uses
    public readonly static byte[] Empty = new byte[0];
    static readonly Dictionary<int, Pool> pools = new Dictionary<int, Pool>();
    static Pool lastPool = null;
    public static string PoolStats
    {
        get
        {
            StringBuilder sb = StringBuilderPool.Rent();
            lock (pools)
            {
                foreach (var pool in pools.Values)
                {
                    sb.AppendLine(pool.Stats);
                }
            }
            var r = sb.ToString();
            StringBuilderPool.Return(sb);
            return r;
        }
    }
    private class Pool
    {
        public int PoolCount => pool.Count;
        private long rentCount = 0;
        public long RentCount => rentCount;
        private long fromNewcount = 0;
        public long FromNewCount => fromNewcount;
        private long fromPoolCount = 0;
        public long FromPoolCount => fromPoolCount;
        private long returnCount = 0;
        public long ReturnCount => returnCount;
        public int Size { get; private set; }
        readonly Queue<byte[]> pool = new Queue<byte[]>();
        public Pool(int size)
        {
            Size = size;
        }
        public byte[] Rent()
        {
            //if (Size == 1536)
            //{
            //    Console.WriteLine("");
            //}

            lock (pool)
            {
                rentCount++;
                if (pool.Count > 0)
                {
                    fromPoolCount++;
                    return pool.Dequeue();
                }
                fromNewcount++;
                return (new byte[Size]);
            }
        }
        public void Return(byte[] array)
        {
            lock (pool)
            {
                returnCount++;
                if (array.Length == Size)
                    pool.Enqueue(array);
            }
        }
        public string Stats
        {
            get
            {
                //return $"Pool {Size}: has {pool.Count} items";
                var sb = StringBuilderPool.Rent();
                lock (pool)
                {
                    sb.AppendLine($"Stats for ByteArrayPool({Size}):<br/>");
                    sb.AppendLine($"Pool Count {pool.Count}<br/>");
                    sb.AppendLine($"Rent Count {rentCount}<br/>");
                    sb.AppendLine($"From Pool  {fromPoolCount}<br/>");
                    sb.AppendLine($"From New   {fromNewcount}<br/>");
                    sb.AppendLine($"Returned   {returnCount}<br/>");
                }
                var r = sb.ToString();
                sb.Return();
                return r;
            }
        }
    }
    public static int GetArraySize(int reqSize)
    {
        //if reqSize >= 8k doll out chunks with 8k boundrys. This allows
        //ByteArray to grow in chunks, and hopefully not bog down on
        //constant Resize request.
        if (reqSize >= 8192)
        {
            int n8k = reqSize / 8192;
            if (reqSize % 8192 != 0) n8k++;
            return n8k * 8192;
        }
        //if reqSize >= 1k doll out chunks with 1k boundrys.
        if (reqSize >= 1024)
        {
            int nk = reqSize / 1024;
            if (reqSize % 1024 != 0) nk++;
            return nk * 1024;
        }
        //doll out chunks with with 64 byte boundrys.
        int n64 = reqSize / 64;
        if (reqSize % 64 != 0) n64++;
        return n64 * 64;
    }
    /// <summary>
    /// This method is used by ByteArray to get a block of memory. It rounds up
    /// reqSize to predefined boundries. This is to help ByteArray grow in chunks.
    /// Look at the ByteArray class to see how use this function.
    /// </summary>
    /// <param name="reqSize"></param>
    /// <returns> returns a byte[] of the size returned by GetArraySize(reqSize). </returns>
    public static byte[] RentBlock(int reqSize)
    {
        int size = GetArraySize(reqSize);
        if (size > MaxPoolSize) return new byte[size];
        return Rent(size);
    }
    /// <summary>
    /// Returns a pooled byte[] of the size requested.
    ///     
    ///     var buf = ByteArrayPool.Rent(1024); // get a byte[] of length 1024;
    ///     DoStuffwith(buf);
    ///     buf = ByteArrayPool.Return(buf); // return buf to pool and remove the ref to it.
    ///
    /// </summary>
    /// <param name="size"></param>
    /// <returns>the size in bytes of the byte[] to return </returns>
    public static byte[] Rent(uint size) => Rent((int)size);
    /// <summary>
    /// Returns a pooled byte[] of the size requested. Example:
    ///
    ///     var buf = ByteArrayPool.Rent(1024); // get a byte[] of length 1024;
    ///     DoStuffwith(buf);
    ///     buf = ByteArrayPool.Return(buf); // return buf to pool and remove the ref to it.
    ///
    /// </summary>
    /// <param name="size"></param>
    /// <returns>the size in bytes of the byte[] to return </returns>
    public static byte[] Rent(int reqSize)
    {
        lock (pools)
        {
            Pool pool = lastPool;
            if (pool != null && pool.Size == reqSize)
            {
                return pool.Rent();
            }
            if (pools.TryGetValue(reqSize, out pool))
            {
                lastPool = pool;
                return pool.Rent();
            }
            lastPool = pool = new Pool(reqSize);
            pools.Add(reqSize, pool);
            return pool.Rent();
        }
    }
    /// <summary>
    /// Return a byte[] to our pool to be reused. Example:
    /// 
    ///     var buf = ByteArrayPool.Rent(1024); // get a byte[] of length 1024;
    ///     DoStuffwith(buf);
    ///     buf = ByteArrayPool.Return(buf); // return buf to pool and remove the ref to it.
    ///     
    /// </summary>
    /// <param name="array"></param>
    /// <returns> 
    /// we return a zero length byte[] useful for removing the ref of and array:
    /// </returns>
    public static byte[] Return(byte[] array)
    {
        // let the GC handle the byte[] it's length > MaxPoolSize
        if (array.Length > MaxPoolSize) return Empty;
        int size = array.Length;
        lock (pools)
        {
            Pool pool = lastPool;
            if (pool != null && pool.Size == size)
            {
                pool.Return(array);
                return Empty;
            }
            if (pools.TryGetValue(size, out pool))
            {
                lastPool = pool;
                pool.Return(array);
                return Empty;
            }
            // if we did not get the byte[] from Rent above, then let GC handle it
            // maybe change this to create a pool, but these functions should be used
            // by code that is aware of how/why we are pooling in the first palce.
            return Empty;
        }
    }
}
