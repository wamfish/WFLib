//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

using System.Runtime.InteropServices;

namespace WFLib;
public class MemoryIndexNode : IDisposable
{
    public MemoryIndexNode parent;
    public byte level;
    public short count;
    public short curIndex;
    public byte[] keyData;
    public MemoryIndexNode[] nodes;
    public int GetId()
    {
        int keySize = keyData.Length / nodes.Length;
        int pos = ((curIndex + 1) * keySize) - 4;
        Span<byte> idBuf = stackalloc byte[4];
        var src = keyData.AsSpan(pos, 4);
        src.CopyTo(idBuf);
        Util.Reverse(idBuf);
        return MemoryMarshal.Read<int>(idBuf);
    }
    public void Init(MemoryIndex mem, MemoryIndexNode parent)
    {
        level = 0;
        count = 0;
        curIndex = 0;
        keyData = ByteArrayPool.Rent(mem.KEYDATASIZE);
        if (nodes == null)
            nodes = new MemoryIndexNode[MemoryIndex.NODESIZE];
        this.parent = parent;
        if (this.parent != null)
        {
            level = parent.level;
            if (level > 0)
                level--;
        }
    }
    public void ClearNodes()
    {
        if (nodes == null)
        {
            Free();
            return;
        }
        for (int i = 0; i < count; i++)
        {
            if (nodes[i] != null)
            {
                if (nodes[i].level > 0)
                {
                    nodes[i].ClearNodes();
                    nodes[i] = null;
                }
                else
                {
                    nodes[i].Free();
                    nodes[i] = null;
                }
            }
        }
        Free();
    }
    void Free()
    {
        parent = null;
        level = 0;
        count = 0;
        curIndex = 0;
        if (keyData != null)
            keyData = ByteArrayPool.Return(keyData);
        Return();
    }


    private static MemoryIndexNode CreateIndexNode()
    {
        return new MemoryIndexNode();
    }
    private static Pool<MemoryIndexNode> pool = new(CreateIndexNode);
    /// <summary>
    /// 
    /// Use this to get a SerializationBuffer object. 
    /// 
    /// Example: using var sb = SerializationBuffer.Rent();
    ///     
    /// </summary>
    /// <returns> SerializationBuffer </returns>
    public static MemoryIndexNode Rent()
    {
        return pool.Rent();
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
        ByteArrayPool.Return(keyData);
        keyData = null;
        pool.Return(this);
    }
}