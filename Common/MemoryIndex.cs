//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public partial class MemoryIndex : IDisposable
{
    private MemoryIndex() { }
    private void Clear()
    {
        if (headNode != null)
        {
            headNode.ClearNodes();
            headNode = null;
        }
    }
    public void InitIndex()
    {
        Clear();
        NewHead(null);
    }
    public string HeadInfo()
    {
        return $"Head Level:{headNode.level}";
    }
    private MemoryIndexNode headNode = null;
    internal const int NODESIZE = 128;
    private const int HALFNODESIZE = 64;
    internal int KEYDATASIZE;
    private int KEYDATAHALFSIZE;
    private int KEYLENGTH = 0;
    private string _idString = "";
    public string IdString { get { return _idString; } }
    public byte[] FirstKey
    {
        get
        {
            lock (headNode)
            {
                MemoryIndexNode node = headNode;
                node.curIndex = 0;
                while (node.level != 0)
                {
                    if (node.count == 0)
                        return null;
                    node = node.nodes[0];
                    node.curIndex = 0;
                }
                return CurrentKey(node);
            }
        }
    }
    public byte[] LastKey
    {
        get
        {
            lock (headNode)
            {
                MemoryIndexNode node = headNode;
                if (node.count == 0)
                    return null;
                node.curIndex = node.count;
                node.curIndex--;
                while (node.level != 0)
                {
                    node = node.nodes[node.curIndex];
                    node.curIndex = node.count;
                    node.curIndex--;
                }
                return CurrentKey(node);
            }
        }
    }
    public void Create(byte[] key)
    {
        lock (headNode)
        {
            CheckKeyLength(key);
            MemoryIndexNode node = BinarySearch(key, out int result);
            if (result != 0)
            {
                if (result < 0)
                    DataInsert(node, key);
                if (result > 0)
                    DataAdd(node, key);
                return;
            }
            throw new WamfishException();
        }
    }
    public bool Delete(byte[] key)
    {
        lock (headNode)
        {
            CheckKeyLength(key);
            MemoryIndexNode node = BinarySearch(key, out int result);
            if (result != 0)
                return false;
            DeleteKey(node);
            return true;
        }
    }
    public bool Exist(byte[] key)
    {
        lock (headNode)
        {
            CheckKeyLength(key);
            MemoryIndexNode node = BinarySearch(key, out int result);
            if (result == 0)
            {
                return true;
            }
            return false;
        }
    }
    public bool ExistNoDup(byte[] key, out int id)
    {
        lock (headNode)
        {
            id = -1;
            CheckKeyLength(key);
            MemoryIndexNode node = BinarySearchNoDup(key, out int result);
            if (result == 0)
            {
                id = node.GetId();
                return true;
            }
            return false;
        }
    }
    public bool ReadPrev(byte[] key, int count, List<int> ids, out byte[] nextKey)
    {
        lock (headNode)
        {
            bool foundKey = false;
            nextKey = null;
            CheckKeyLength(key);
            MemoryIndexNode node = BinarySearch(key, out int result);
            if (node == null || node.count < 1)
                return false;
            for (int i = 0; i < count && node != null; i++)
            {
                ids.Add(node.GetId());
                foundKey = true;
                node = PrevKey(node);
            }
            nextKey = CurrentKey(node);
            return foundKey;
        }
    }
    /// <summary>
    /// 
    /// Returns Descending list of Ids. Example: 
    /// 
    /// using var ids = index.ReadAllDescending();
    /// if (ids.Count > 0) DoSomething(ids.Ints);
    ///     
    /// </summary>
    public bool ReadAllDescending(List<int> ids)
    {
        ids.Clear();
        var key = LastKey;
        if (key == null)
        {
            return false;
        }
        lock (headNode)
        {
            CheckKeyLength(key);
            MemoryIndexNode node = BinarySearch(key, out int result);
            if (node == null || node.count < 1)
            {
                ByteArrayPool.Return(key);
                return false;
            }

            while (node != null)
            {
                ids.Add(node.GetId());
                node = PrevKey(node);
            }
            ByteArrayPool.Return(key);
            return true;
        }
    }
    /// <summary>
    /// 
    /// Returns Ascending list of Ids. Example: 
    /// 
    /// using var ids = index.ReadAllAscending();
    /// if (ids.Count > 0) DoSomething(ids.Ints);
    ///     
    /// </summary>
    public bool ReadAllAscending(List<int> ids)
    {
        ids.Clear();
        var key = FirstKey;
        if (key == null)
        {
            return false;
        }
        lock (headNode)
        {
            CheckKeyLength(key);
            MemoryIndexNode node = BinarySearch(key, out int result);
            if (node == null || node.count < 1)
            {
                ByteArrayPool.Return(key);
                return false;
            }
            while (node != null)
            {
                ids.Add(node.GetId());
                node = NextKey(node);
            }
            ByteArrayPool.Return(key);
            return true;
        }
    }


    public bool ReadNext(byte[] key, int count, List<int> ids, out byte[] nextKey)
    {
        lock (headNode)
        {
            bool foundKey = false;
            nextKey = null;
            CheckKeyLength(key);
            MemoryIndexNode node = BinarySearch(key, out int result);
            if (node == null || node.count < 1)
            {
                return false;
            }
            for (int i = 0; i < count && node != null; i++)
            {
                ids.Add(node.GetId());
                foundKey = true;
                node = NextKey(node);
            }
            nextKey = CurrentKey(node);
            return foundKey;
        }
    }
    private byte[] CurrentKey(MemoryIndexNode node)
    {
        if (node == null || node.count < 1)
            return null;
        int offset = KEYLENGTH * node.curIndex;
        byte[] key = ByteArrayPool.Rent(KEYLENGTH);
        Buffer.BlockCopy(node.keyData, offset, key, 0, KEYLENGTH);
        return key;
    }
    private void CheckKeyLength(byte[] keydata)
    {
        if (keydata.Length < KEYLENGTH)
        {
            throw new Exception("keydata size mismatch");
        }
    }
    private MemoryIndexNode BinarySearchNoDup(byte[] data, out int result)
    {
        return BinarySearch(data, out result, KEYLENGTH - 4);
    }
    private MemoryIndexNode BinarySearch(byte[] data, out int result, int searchLength = 0)
    {
        if (searchLength < 1)
            searchLength = KEYLENGTH;
        MemoryIndexNode node = headNode;
        int min = 0;
        int max = node.count - 1;
        int check;
        int i;
        result = -1;
        if (max < 0)
        {
            result = -1;
            return null;
        }
    Loop:
        while (min <= max)
        {
            node.curIndex = (byte)(min + (max - min >> 1));
            int curIndexOffset = (node.curIndex * KEYLENGTH);
            check = 0;
            for (i = 0; i < searchLength; i++)
            {
                int cindex = i + curIndexOffset;
                if (data[i] < node.keyData[cindex])
                {
                    check = -1;
                    result = -1;
                    break;
                }
                if (data[i] > node.keyData[cindex])
                {
                    check = 1;
                    result = 1;
                    break;
                }
            }
            if (check == 0)
            {
                if (node.level == 0)
                {
                    result = 0;
                    return node;
                }
                node = FindLevel0(node);
                result = 0;
                return node;
            }
            if (check < 0)
                max = node.curIndex - 1;
            else
                min = node.curIndex + 1;
        }
        if (node.level != 0)
        {
            if (result > 0 && node.curIndex < node.count - 1)
            {
                node.curIndex++;
            }
            //IndexNode save;
            //if (node.nodes[node.curIndex] == null)
            //{
            //    save = node;
            //}
            node = node.nodes[node.curIndex];
            min = 0;
            max = node.count - 1;
            goto Loop;
        }
        return node;
    }
    private MemoryIndexNode FindLevel0(MemoryIndexNode node)
    {
        while (node.level != 0)
        {
            //if (node.nodes[node.curIndex] == null)
            //{
            //    node.nodes[node.curIndex] = LoadNode(node, node.nodes[node.curIndex]);
            //}
            node = node.nodes[node.curIndex];
            node.curIndex = node.count;
            node.curIndex--;
        }
        return node;
    }
    private void ShiftNodeRight(MemoryIndexNode node)
    {
        if (node.count == 0)
        {
            node.count++;
            return;
        }
        int destLen = (node.count - node.curIndex);
        if (node.level != 0)
        {
            Array.Copy(node.nodes, node.curIndex, node.nodes, node.curIndex + 1, destLen);
        }
        int srcOffset = node.curIndex * KEYLENGTH;
        int destOffset = (node.curIndex + 1) * KEYLENGTH;
        destLen = ((NODESIZE - node.curIndex) - 1) * KEYLENGTH;
        Buffer.BlockCopy(node.keyData, srcOffset, node.keyData, destOffset, destLen);
        node.count++;
    }
    private void CopyKeyData(MemoryIndexNode srcNode, int srcIndex, MemoryIndexNode destNode)
    {
        int destLen = KEYLENGTH;
        int srcOffset = srcIndex * KEYLENGTH;
        int destOffset = destNode.curIndex * KEYLENGTH;
        Buffer.BlockCopy(srcNode.keyData, srcOffset, destNode.keyData, destOffset, destLen);
    }
    private void DeleteTheNode(MemoryIndexNode node)
    {
        //head node
        if (node.parent == null)
        {
            Array.Clear(node.keyData, 0, KEYDATASIZE);
            node.count = 0;
            node.nodes[0].ClearNodes();
            node.nodes[0] = null;
            return;
        }
        DeleteKey(node.parent); //this will free the node
    }
    private void DeleteTheRightKey(MemoryIndexNode node)
    {
        if (node.level > 0)
        {
            node.nodes[node.curIndex].ClearNodes();
            node.nodes[node.curIndex] = null;
        }
        int srcOffset = (node.curIndex) * KEYLENGTH;
        Array.Clear(node.keyData, srcOffset, KEYLENGTH);
        node.count--;
        node.curIndex--;
        if (node.parent != null)
        {
            UpdateParent(node);
        }
    }
    private void DeleteKey(MemoryIndexNode node)
    {
        //Should not ever happen
        if (node == null)
            return;
        //If there is only 1 key in the node delete the node
        if (node.count == 1)
        {
            DeleteTheNode(node);
            return;
        }
        //If the node to delete is the last key
        if (node.curIndex == node.count - 1)
        {
            DeleteTheRightKey(node);
            return;
        }
        //Normal delete of the current key
        int destLen = (node.count - node.curIndex) - 1;
        if (node.level != 0)
        {
            if (node.nodes[node.curIndex] != null)
            {
                node.nodes[node.curIndex].ClearNodes();
                node.nodes[node.curIndex] = null;
            }
            Array.Copy(node.nodes, node.curIndex + 1, node.nodes, node.curIndex, destLen);
        }
        int srcOffset = (node.curIndex + 1) * KEYLENGTH;
        int destOffset = (node.curIndex) * KEYLENGTH;
        destLen = ((node.count - node.curIndex) - 1) * KEYLENGTH;
        Buffer.BlockCopy(node.keyData, srcOffset, node.keyData, destOffset, destLen);
        node.count--;
    }
    private bool NodeSplit(MemoryIndexNode node)
    {
        if (node.parent == null)
        {
            NewHead(node);
            return false;
        }
        if (node.parent.count == NODESIZE)
        {
            NodeSplit(node.parent);
            return false;
        }
        MemoryIndexNode newNode = NewNode(node.parent);
        newNode.count = HALFNODESIZE;
        node.count = HALFNODESIZE;
        newNode.curIndex = newNode.count;
        newNode.curIndex--;
        node.curIndex = node.count;
        node.curIndex--;
        Array.Copy(node.keyData, newNode.keyData, KEYDATAHALFSIZE);
        Array.Copy(node.keyData, KEYDATAHALFSIZE, node.keyData, 0, KEYDATAHALFSIZE);
        Array.Clear(node.keyData, KEYDATAHALFSIZE, KEYDATAHALFSIZE);
        if (node.level > 0)
        {
            Array.Copy(node.nodes, newNode.nodes, HALFNODESIZE);
            Array.Copy(node.nodes, HALFNODESIZE, node.nodes, 0, HALFNODESIZE);
            Array.Clear(node.nodes, HALFNODESIZE, HALFNODESIZE);
            for (int i = 0; i < HALFNODESIZE; i++)
            {
                if (newNode.nodes[i] != null)
                {
                    newNode.nodes[i].parent = newNode;
                }
            }
        }
        ShiftNodeRight(node.parent);
        CopyKeyData(newNode, newNode.count - 1, node.parent);
        node.parent.nodes[node.parent.curIndex] = newNode;
        return true;
    }
    private void NewHead(MemoryIndexNode node)
    {
        MemoryIndexNode newhead = NewNode(null);
        if (node == null)
        {
            newhead.level = 1;
            headNode = newhead;
            return;
        }
        newhead.level = (byte)(node.level + 1);
        node.parent = newhead;
        newhead.count = 1;
        newhead.curIndex = 0;
        CopyKeyData(node, node.count - 1, newhead);
        newhead.nodes[0] = node;
        headNode = newhead;
    }
    private MemoryIndexNode PrevKey(MemoryIndexNode node)
    {
        if (node.curIndex == 0)
        {
            node = node.parent;
            while (node != null)
            {
                if (node.curIndex == 0)
                {
                    node = node.parent;
                    continue;
                }
                node.curIndex--;
                while (node.level != 0)
                {
                    node = node.nodes[node.curIndex];
                    node.curIndex = node.count;
                    node.curIndex--;
                }
                return node;
            }
            return (null);
        }
        node.curIndex--;
        return node;
    }
    private MemoryIndexNode NextKey(MemoryIndexNode node)
    {
        if (node.curIndex == node.count - 1)
        {
            node = node.parent;
            while (node != null)
            {
                if (node.curIndex == node.count - 1)
                {
                    node = node.parent;
                    continue;
                }
                node.curIndex++;
                while (node.level != 0)
                {
                    node = node.nodes[node.curIndex];
                    node.curIndex = 0;
                }
                return node;
            }
            return (null);
        }
        node.curIndex++;
        return node;
    }
    private void DataInsert(MemoryIndexNode node, byte[] data)
    {
        if (node == null)
        {
            //empty head node
            node = NewNode(headNode);
            headNode.nodes[headNode.curIndex] = node;
            Buffer.BlockCopy(data, 0, headNode.keyData, 0, KEYLENGTH);
            headNode.count++;
            //int offset = 0;
            Buffer.BlockCopy(data, 0, node.keyData, 0, KEYLENGTH);
            node.count = 1;
            return;
        }
        if (node.count == NODESIZE)
        {
            while (!NodeSplit(node))
            {
                node = BinarySearch(data, out _);
            }
            node = BinarySearch(data, out int result);
            if (result < 0)
                DataInsert(node, data);
            else
                DataAdd(node, data);
            return;
        }
        ShiftNodeRight(node);
        int offset = node.curIndex * KEYLENGTH;
        Buffer.BlockCopy(data, 0, node.keyData, offset, KEYLENGTH);
    }
    private void UpdateParent(MemoryIndexNode node)
    {
        CopyKeyData(node, node.count - 1, node.parent);
        if (node.parent.curIndex == node.parent.count - 1 && node.parent.parent != null)
        {
            UpdateParent(node.parent);
        }
    }
    private void DataAdd(MemoryIndexNode node, byte[] data)
    {
        bool updateParent = false;
        if (node.count == NODESIZE)
        {
            while (!NodeSplit(node))
            {
                node = BinarySearch(data, out _);
            }
            node = BinarySearch(data, out int result);
            if (result < 0)
            {
                DataInsert(node, data);
                return;
            }
        }
        if (node.curIndex < node.count - 1)
        {
            node.curIndex++;
            ShiftNodeRight(node);
        }
        else
        {
            node.count++;
            node.curIndex = (byte)(node.count - 1);
            if (node.parent != null)
            {
                updateParent = true;
            }
        }
        int offset = node.curIndex * KEYLENGTH;
        Buffer.BlockCopy(data, 0, node.keyData, offset, KEYLENGTH);
        if (updateParent)
            UpdateParent(node);
    }
    private MemoryIndexNode NewNode(MemoryIndexNode parent)
    {
        MemoryIndexNode node = MemoryIndexNode.Rent();
        node.Init(this, parent);
        return node;
    }

    //ascending descending
    public IntList GetIdsAscending()
    {
        var list = IntList.Rent();
        return list;
    }
    public IntList GetIdsDescending()
    {
        var list = IntList.Rent();
        var key = LastKey;

        bool foundKey = ReadPrev(key, 100, list.Ints, out key);
        ByteArrayPool.Return(key);
        key = null;
        return list;
    }

    public BatchReader Reader(int batchSize)
    {
        var reader = new BatchReader(this, batchSize);
        return reader;
    }
    /// <summary>
    /// I will leave this for now, but I am not using memindex in this fashion, and may not need it.
    /// </summary>
    public class BatchReader
    {
        public BatchReader(MemoryIndex index, int count)
        {
            this.index = index;
            this.count = count;

        }

        readonly int count = 0;
        readonly MemoryIndex index;
        byte[] NextKey = null;
        bool isDesc = false;
        public void StartFirst()
        {
            isDesc = false;
            NextKey = index.FirstKey;
        }
        public void StartLast()
        {
            isDesc = true;
            NextKey = index.LastKey;
        }
        public bool ReadNext(List<int> ids)
        {
            if (NextKey == null)
                return false;
            bool foundKey;
            if (isDesc)
            {
                //ids.Clear();
                foundKey = index.ReadPrev(NextKey, count, ids, out NextKey);
            }
            else
            {
                //ids.Clear();
                foundKey = index.ReadNext(NextKey, count, ids, out NextKey);
            }
            return foundKey;
        }
    }

    private static MemoryIndex CreateMemoryIndex()
    {
        return new MemoryIndex();
    }
    private static Pool<MemoryIndex> pool = new(CreateMemoryIndex);
    /// <summary>
    /// 
    /// Use this to get a SerializationBuffer object. 
    /// 
    /// Example: using var sb = SerializationBuffer.Rent();
    ///     
    /// </summary>
    /// <returns> SerializationBuffer </returns>
    public static MemoryIndex Rent(string idStr, int keyLength)
    {
        var mi = pool.Rent();
        mi._idString = idStr;
        mi.KEYLENGTH = keyLength;
        mi.KEYDATASIZE = NODESIZE * mi.KEYLENGTH;
        mi.KEYDATAHALFSIZE = HALFNODESIZE * mi.KEYLENGTH;
        mi.InitIndex();
        return mi;
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
        Clear();
        pool.Return(this);
    }
}
