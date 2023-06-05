//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;
public abstract class IndexFile<R> where R : Record, new()
{
    Table<R> Table;
    public Func<R, byte[]> MakeKey { get; private set; }
    public IndexFile(Table<R> table, string indexName, Func<R, byte[]> MakeKey)
    {
        var rec = RecordFactory<R>.Rent();
        Table = table;
        IndexName = indexName;
        this.MakeKey = MakeKey;
        var keydata = MakeKey(rec);
        RecordFactory<R>.Return(rec);
        KEYLENGTH = keydata.Length;
        KEYDATACOPYSIZE = HALFNODESIZE * KEYLENGTH;
    }
    public string IndexPath => Path.Combine(Table.TableDir, IndexName + ".idx");
    public string IndexName { get; private set; }
    public bool NeedToRebuild { get { return needToRebuild; } }
    public bool IsOpen { get { return isOpen; } }
    public bool Create(R rec)
    {
        if (!isOpen) throw new WamfishException();
        if (rec.ID < 0) throw new WamfishException();
        var key = MakeKey(rec);
        CheckKeyLength(key);
        if (++flushCount >= 1000) Flush();
        lock (headNode)
        {
            Node node = BinarySearch(key, out int result);
            if (result != 0)
            {
                if (result < 0)
                    DataInsert(node, key, rec.ID);
                if (result > 0)
                    DataAdd(node, key, rec.ID);
                return true;
            }
            return false;
        }
    }
    public bool Delete(R rec)
    {
        var keyData = MakeKey(rec);
        if (!isOpen) throw new WamfishException();
        CheckKeyLength(keyData);
        if (++flushCount >= 1000) Flush();
        lock (headNode)
        {
            Node node = BinarySearch(keyData, out int result);
            if (result != 0)
                return false;
            DeleteKey(node);
            return true;
        }
    }
    public bool Read(R rec, out int id)
    {
        var keyData = MakeKey(rec);
        if (!isOpen) throw new WamfishException();
        CheckKeyLength(keyData);
        if (++flushCount >= 1000) Flush();
        lock (headNode)
        {
            Node node = BinarySearch(keyData, out int result);
            if (result == 0)
            {
                id = (int)node.offset[node.curIndex];
                return true;
            }
            id = -1;
            return false;
        }
    }
    public bool ReadBatchDesc(List<int> into, R startRec, int count, out int restartId)
    {
        restartId = 0;
        if (startRec == null)
            return false;
        var key = MakeKey(startRec);

        lock (headNode)
        {
            var node = BinarySearch(key, out int result);
            int i;
            for (i = 0; i < count && node != null; i++)
            {
                int id = (int)node.offset[node.curIndex];
                into.Add(id);
                node = PrevKey(node);
            }
            if (i == count)
            {
                if (node == null)
                    return false;
                restartId = (int)node.offset[node.curIndex];
                return true;
            }
            return false;
        }
    }
    public bool ReadBatch(List<int> into, R startRec, int count, out int restartId)
    {
        restartId = 0;
        if (startRec == null)
            return false;
        var key = MakeKey(startRec);
        lock (headNode)
        {
            var node = BinarySearch(key, out int result);
            int i;
            for (i = 0; i < count && node != null; i++)
            {
                int id = (int)node.offset[node.curIndex];
                into.Add(id);
                node = NextKey(node);
            }
            if (i == count)
            {
                if (node == null)
                    return false;
                restartId = (int)node.offset[node.curIndex];
                //Table.Read(startRec, into[into.Count - 1]);
                return true;
            }
            return false;
        }
    }
    public void Close()
    {
        if (isOpen)
        {
            Flush();
            fs.Close();
        }
        isOpen = false;
    }
    public void DeleteIndexFile()
    {
        string path = IndexPath;
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    public void Flush()
    {
        flushCount = 0;
        if (headNode != null)
        {
            if (headNode.isDirty) SaveNodeToDisk(headNode);
            ClearNodes(headNode);
        }
        if (isDirty)
        {
            fs.SeekBegin(0);
            fs.Write((byte)0); //set dirty flag off
            isDirty = false;
        }
        return; // we should be flushing data as we go
    }
    public void Open()
    {
        if (isOpen) throw new WamfishException();
        isDirty = false;
        string fileName = IndexPath;
        fs = new WfFile(fileName);
        fs.Open();
        LoadIndex();
        isOpen = true;
    }
    public bool ReadFirst(out int id)
    {
        var node = FirstNode();
        id = -1;
        if (node.count < 1)
            return false;
        id = (int)node.offset[node.curIndex];
        return true;
    }
    public bool ReadLast(out int id)
    {
        var node = LastNode();
        id = -1;
        if (node.count < 1)
            return false;
        id = (int)node.offset[node.curIndex];
        return true;
    }
    private int flushCount = 0;
    private readonly Queue<Node> nodeCache = new Queue<Node>();
    private Node headNode = null;
    private const int NODESIZE = 128;
    private const int HALFNODESIZE = NODESIZE >> 1;
    private readonly int KEYDATACOPYSIZE;
    private readonly int KEYLENGTH = 0;
    private WfFile fs;
    private bool needToRebuild = false;
    private bool isOpen = false;
    private void CheckKeyLength(byte[] keydata)
    {
        if (keydata.Length != KEYLENGTH)
        {
            throw new Exception("keydata size mismatch");
        }
    }
    private Node BinarySearch(byte[] data, out int result)
    {
        Node node = headNode;
        int min = 0;
        int max = node.count - 1;
        int check;
        int i;
        result = -1;
    Loop:
        while (min <= max)
        {
            node.curIndex = (byte)(min + (max - min >> 1));
            int curIndexOffset = node.curIndex * KEYLENGTH;
            check = 0;
            for (i = 0; i < KEYLENGTH; i++)
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
            if (node.nodes[node.curIndex] == null)
            {
                node.nodes[node.curIndex] = LoadNode(node, node.offset[node.curIndex]);
            }
            node = node.nodes[node.curIndex];
            min = 0;
            max = node.count - 1;
            goto Loop;
        }
        return node;
    }
    private void InitIndex()
    {
        if (headNode != null)
        {
            ClearNodes(headNode);
        }
        try
        {
            fs.SeekBegin(0);
            fs.Write((byte)0); //dirty flag
            fs.Write(KEYLENGTH);
            fs.Write(IndexName);
            long fileOffset = fs.Position;
            headNode = NewNode(null, fileOffset);
            SaveNodeToDisk(headNode);
        }
        catch (Exception)
        {
            isOpen = false;
            throw;
        }
    }
    private void LoadIndex()
    {
        needToRebuild = false;
        if (fs.Length == 0)
        {
            InitIndex();
            needToRebuild = true;
        }
        fs.SeekBegin(0);
        byte isDirty = fs.ReadByte();
        int keyLen = fs.ReadInt();
        string idString = fs.ReadString();
        if (isDirty != 0) needToRebuild = true;
        if (keyLen != KEYLENGTH) needToRebuild = true;
        if (idString != IndexName) needToRebuild = true;
        long fileOffset = fs.Position;
        headNode = LoadNode(null, fileOffset);
        return;
    }
    private Node FindLevel0(Node node)
    {
        while (node.level != 0)
        {
            if (node.nodes[node.curIndex] == null)
            {
                node.nodes[node.curIndex] = LoadNode(node, node.offset[node.curIndex]);
            }
            node = node.nodes[node.curIndex];
            node.curIndex = node.count;
            node.curIndex--;
        }
        return node;
    }
    private void ShiftNodeRight(Node node)
    {
        int destLen = NODESIZE - node.curIndex - 1;
        //Note: Buffer.BlockCopy copies bytes that is way I am using Array.Copy for this
        Array.Copy(node.offset, node.curIndex, node.offset, node.curIndex + 1, destLen);
        if (node.level != 0)
        {
            //Note: Buffer.BlockCopy copies bytes that is way I am using Array.Copy for this
            Array.Copy(node.nodes, node.curIndex, node.nodes, node.curIndex + 1, destLen);
        }
        int srcOffset = node.curIndex * KEYLENGTH;
        int destOffset = (node.curIndex + 1) * KEYLENGTH;
        destLen = (NODESIZE - node.curIndex - 1) * KEYLENGTH;
        Buffer.BlockCopy(node.keyData, srcOffset, node.keyData, destOffset, destLen);
        node.count++;
    }
    private void ShiftNodeLeft(Node node)
    {
        int destLen = NODESIZE - node.curIndex - 1;
        Array.Copy(node.offset, node.curIndex + 1, node.offset, node.curIndex, destLen);
        if (node.level != 0)
        {
            ClearNode(node);
            Array.Copy(node.nodes, node.curIndex + 1, node.nodes, node.curIndex, destLen);
        }
        int srcOffset = (node.curIndex + 1) * KEYLENGTH;
        int destOffset = node.curIndex * KEYLENGTH;
        destLen = (NODESIZE - node.curIndex - 1) * KEYLENGTH;
        Buffer.BlockCopy(node.keyData, srcOffset, node.keyData, destOffset, destLen);
        node.count--;
    }
    private void CopyData(Node srcNode, int srcIndex, Node destNode)
    {
        int destLen = KEYLENGTH;
        int srcOffset = srcIndex * KEYLENGTH;
        int destOffset = destNode.curIndex * KEYLENGTH;
        Buffer.BlockCopy(srcNode.keyData, srcOffset, destNode.keyData, destOffset, destLen);
    }
    private void DeleteKey(Node node)
    {
        if (node.count == 1)
        {
            if (node.level > 0)
            {
                ClearNode(node.nodes[0]);
                node.nodes[0] = null;
            }
            node.count = 0;
            node.curIndex = 0;
            SaveNode(node);
            if (node.parent != null)
            {
                DeleteKey(node.parent);
            }
            return;
        }
        if (node.curIndex == node.count - 1)
        {
            Array.Clear(node.keyData, node.curIndex * KEYLENGTH, KEYLENGTH);
            node.offset[node.curIndex] = 0;
            if (node.level > 0)
            {
                ClearNode(node.nodes[node.curIndex]);
                node.nodes[node.curIndex] = null;
            }
            node.curIndex--;
            node.count--;
            SaveNode(node);
            if (node.parent != null)
            {
                UpdateParent(node);
            }
            return;
        }
        if (node.level > 0)
        {
            ClearNode(node.nodes[node.curIndex]);
            node.nodes[node.curIndex] = null;
        }
        ShiftNodeLeft(node);
        SaveNode(node);
    }
    private void ClearNode(Node node)
    {
        Node cnode = node.nodes[node.curIndex];
        if (cnode != null)
        {
            if (cnode.isDirty) SaveNodeToDisk(cnode);
            lock (nodeCache) nodeCache.Enqueue(cnode);
            node.nodes[node.curIndex] = null;
        }
    }
    private void ClearNodes(Node node)
    {
        for (int i = 0; i < NODESIZE; i++)
        {
            if (node.nodes[i] != null)
            {
                ClearNodes(node.nodes[i]);
                if (node.nodes[i].isDirty) SaveNodeToDisk(node.nodes[i]);
                lock (nodeCache) nodeCache.Enqueue(node.nodes[i]);
                node.nodes[i] = null;
            }
        }
    }
    private void NodeSplit(Node node)
    {
        if (node.parent == null)
        {
            NewHead(node);
            return;
        }
        if (node.parent.count == NODESIZE)
        {
            NodeSplit(node.parent);
            return;
        }
        Node newNode = NewNode(node.parent, fs.Length);
        newNode.count = HALFNODESIZE;
        node.count = HALFNODESIZE;
        newNode.curIndex = newNode.count;
        newNode.curIndex--;
        node.curIndex = node.count;
        node.curIndex--;
        Array.Copy(node.keyData, newNode.keyData, KEYDATACOPYSIZE);
        Array.Copy(node.keyData, KEYDATACOPYSIZE, node.keyData, 0, KEYDATACOPYSIZE);
        Array.Clear(node.keyData, KEYDATACOPYSIZE, KEYDATACOPYSIZE);
        Array.Copy(node.offset, newNode.offset, HALFNODESIZE);
        Array.Copy(node.offset, HALFNODESIZE, node.offset, 0, HALFNODESIZE);
        Array.Clear(node.offset, HALFNODESIZE, HALFNODESIZE);
        if (node.level != 0)
        {
            Array.Copy(node.nodes, newNode.nodes, HALFNODESIZE);
            Array.Copy(node.nodes, HALFNODESIZE, node.nodes, 0, HALFNODESIZE);
            Array.Clear(node.nodes, HALFNODESIZE, HALFNODESIZE);
        }
        if (newNode.level > 0)
        {
            for (int i = 0; i < HALFNODESIZE; i++)
            {
                if (newNode.nodes[i] != null)
                {
                    newNode.nodes[i].parent = newNode;
                }
            }
        }
        SaveNodeToDisk(newNode);
        SaveNodeToDisk(node);
        ShiftNodeRight(node.parent);
        CopyData(newNode, newNode.count - 1, node.parent);
        node.parent.offset[node.parent.curIndex] = newNode.fileOffset;
        node.parent.nodes[node.parent.curIndex] = newNode;
        SaveNodeToDisk(node.parent);
    }
    private void NewHead(Node node)
    {
        fs.SeekEnd();
        long fileOffset = fs.Position;
        Node newhead = NewNode(null, fileOffset);
        long saveoffset = newhead.fileOffset;
        newhead.fileOffset = node.fileOffset;
        node.fileOffset = saveoffset;
        newhead.level = (byte)(node.level + 1);
        node.parent = newhead;
        newhead.count = 1;
        newhead.curIndex = 0;
        CopyData(node, node.count - 1, newhead);
        newhead.offset[0] = node.fileOffset;
        newhead.nodes[0] = node;
        SaveNodeToDisk(node);
        SaveNodeToDisk(newhead);
        headNode = newhead;
    }
    private Node PrevKey(Node node)
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
                    if (node.nodes[node.curIndex] == null)
                    {
                        node.nodes[node.curIndex] = LoadNode(node, node.offset[node.curIndex]);
                    }
                    node = node.nodes[node.curIndex];
                    node.curIndex = node.count;
                    node.curIndex--;
                }
                return node;
            }
            return null;
        }
        node.curIndex--;
        return node;
    }
    private Node NextKey(Node node)
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
                    if (node.nodes[node.curIndex] == null)
                    {
                        node.nodes[node.curIndex] = LoadNode(node, node.offset[node.curIndex]);
                    }
                    node = node.nodes[node.curIndex];
                    node.curIndex = 0;
                }
                return node;
            }
            return null;
        }
        node.curIndex++;
        return node;
    }
    private void DataInsert(Node node, byte[] data, int id)
    {
        if (node.count == NODESIZE)
        {
            NodeSplit(node);
            node = BinarySearch(data, out int result);
            if (result < 0)
                DataInsert(node, data, id);
            else
                DataAdd(node, data, id);
            return;
        }
        if (node.count > 0)
            ShiftNodeRight(node);
        else
            node.count++;
        int offset = node.curIndex * KEYLENGTH;
        Buffer.BlockCopy(data, 0, node.keyData, offset, KEYLENGTH);
        node.offset[node.curIndex] = id;
        SaveNode(node);
        //Log.Debug("[DataInsert]=" + data.FieldToString(2));
        //DisplayNode(headNode, 0);
        //Log.Debug("[DataInsertEnd]");
    }
    private void UpdateParent(Node node)
    {
        CopyData(node, node.count - 1, node.parent);
        SaveNode(node.parent);
        if (node.parent.curIndex == node.parent.count - 1 && node.parent.parent != null)
        {
            UpdateParent(node.parent);
        }
    }
    private void DataAdd(Node node, byte[] data, int id)
    {
        bool updateParent = false;
        if (node.count == NODESIZE)
        {
            NodeSplit(node);
            node = BinarySearch(data, out int result);
            if (result < 0)
                DataInsert(node, data, id);
            else
                DataAdd(node, data, id);
            return;
        }
        if (node.curIndex < node.count - 1)
        {
            node.curIndex++;
            ShiftNodeRight(node);
        }
        else
        {
            node.count++;
            node.curIndex = node.count;
            node.curIndex--;
            if (node.parent != null)
            {
                updateParent = true;
            }
        }
        int offset = node.curIndex * KEYLENGTH;
        Buffer.BlockCopy(data, 0, node.keyData, offset, KEYLENGTH);
        node.offset[node.curIndex] = id;
        SaveNode(node);
        if (updateParent) UpdateParent(node);
        // Log.Debug("[DataAdd]=" + data.FieldToString(2));
        // DisplayNode(headNode, 0);
        // Log.Debug("[DataAddEnd]");
    }
    public class Node
    {
        public bool isDirty;
        public Node parent;
        public long fileOffset;
        public byte level;
        public byte count;
        public byte[] keyData;
        public long[] offset; //at level 0 offset is actually the record id
        public byte curIndex;
        //public sbyte compareResult;
        public Node[] nodes;
    }
    private Node NewNode(Node parent, long fileOffset)
    {
        Node node;
        if (nodeCache.Count > 0)
        {
            lock (nodeCache) node = nodeCache.Dequeue();
        }
        else
        {
            node = new Node();
        }
        node.isDirty = false;
        node.fileOffset = fileOffset;
        node.level = 0;
        node.count = 0;
        node.parent = parent;
        node.curIndex = 0;
        if (node.keyData == null) node.keyData = new byte[NODESIZE * KEYLENGTH];
        if (node.offset == null) node.offset = new long[NODESIZE];
        if (node.nodes == null) node.nodes = new Node[NODESIZE];
        if (parent != null)
        {
            node.level = parent.level;
            node.level--;
        }
        return node;
    }
    private Node FirstNode()
    {
        Node node = headNode;
        node.curIndex = 0;
        while (node.level != 0)
        {
            if (node.nodes[0] == null)
            {
                node.nodes[0] = LoadNode(node, node.offset[0]);
            }
            node = node.nodes[0];
            node.curIndex = 0;
        }
        return node;
    }
    private Node LastNode()
    {
        Node node = headNode;
        node.curIndex = node.count;
        if (node.count == 0) return node;
        node.curIndex--;
        while (node.level != 0)
        {
            if (node.nodes[node.curIndex] == null)
            {
                node.nodes[node.curIndex] = LoadNode(node, node.offset[node.curIndex]);
            }
            node = node.nodes[node.curIndex];
            node.curIndex = node.count;
            node.curIndex--;
        }
        return node;
    }
    private Node LoadNode(Node parent, long fileOffset)
    {
        Node node = NewNode(parent, fileOffset);
        fs.SeekBegin(fileOffset);
        node.fileOffset = fs.ReadLong();
        if (node.fileOffset != fileOffset) throw new WamfishException();
        node.level = fs.ReadByte();
        node.count = fs.ReadByte();
        if (fs.ReadRaw(node.keyData) != node.keyData.Length) throw new WamfishException();
        for (int i = 0; i < NODESIZE; i++)
        {
            node.offset[i] = fs.ReadLong();
        }
        if (node.level > 0)
        {
            node.nodes = new Node[NODESIZE];
        }
        return node;
    }
    private bool isDirty = false;
    private void SaveNode(Node node)
    {
        if (!isDirty)
        {
            fs.SeekBegin(0);
            fs.Write((byte)1); //set dirty flag
            isDirty = true;
        }
        node.isDirty = true;
    }
    private void SaveNodeToDisk(Node node)
    {
        node.isDirty = false;
        fs.SeekBegin(node.fileOffset);
        fs.Write(node.fileOffset);
        fs.Write(node.level);
        fs.Write(node.count);
        fs.WriteRaw(node.keyData, 0, node.keyData.Length);
        for (int i = 0; i < NODESIZE; i++)
        {
            fs.Write(node.offset[i]);
        }
    }
}

