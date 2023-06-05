//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;
//public partial class Table<T, R, D> where T : Table<T, R, D>, new() where R : IRecord<D>, new() where D : IDataFields, new()
public abstract partial class Table<R> where R : Record, new()
{
    public const byte ADD = (byte)'A';
    public const byte ARCHIVEADD = (byte)'a';
    public const byte UPDATE = (byte)'U';
    public const byte ARCHIVEUPDATE = (byte)'u';
    public const byte DELETE = (byte)'D';
    public const byte ARCHIVEDELETE = (byte)'d';

    public event Action OnInit;
    internal virtual int Version => 1;
    internal string TableName { get; private set; }
    internal string TableFilePath => Path.Combine(TableDir, TableName + ".dat");
    internal bool IsOpen { get; private set; }

    internal WfFile DataFile = null;
    internal string TableDir = Directories.Tables;
    internal List<KeyBase> keys = new();

    private object lockObj = new object();
    private int nextId = 0; //NextId increments this before returning so we start at 1
    private int maxId = 0;
    private SerializationBuffer pb;

    protected Table()
    {
        var t = typeof(R);
        TableName = t.Name;
    }

    public void DeleteTable()
    {
        if (IsOpen) throw new WamfishException();
        lock (lockObj)
        {
            if (File.Exists(TableFilePath))
            {
                File.Delete(TableFilePath);
            }
            //if (File.Exists(DataptrPath))
            //{
            //    File.Delete(DataptrPath);
            //}
        }
        return;
    }
    internal bool Open()
    {
        if (IsOpen) return true;
        lock (lockObj)
        {
            return OpenNoLock();
        }
    }
    internal void Close()
    {
        if (!IsOpen) return;
        lock (lockObj)
        {
            IsOpen = false;
            DataFile.Flush();
            DataFile.Close();
            //WriteDataptr();
            //if (IndexTracker != null)
            //{
            //    IndexTracker.Close();
            //}
            DataFile.Dispose();
            DataFile = null;
            pb.Return();
            pb = null;
        }
    }
    Status AddNoLock(R data, int EditByID, bool AssignNextId = false)
    {
        bool alreadyOpen = IsOpen;
        try
        {
            if (!IsOpen) OpenNoLock();
            data.EditByID = EditByID;
            if (AssignNextId) data.ID = NextId;
            if (data.ID < 0) return Status.NegativeId;
            long recordOffset = GetRecordOffset(data.ID);
            if (recordOffset != 0) return Status.DuplicateId;
            for (int i = 0; i < keys.Count; i++)
            {
                if (!keys[i].CanAddKey(data)) return Status.DuplicateKey;
            }
            for (int i = 0; i < keys.Count; i++)
            {
                keys[i].AddKey(data);
            }
            data.StatusCode = ADD;
            data.Timestamp = DateTime.UtcNow;
            WriteRecordOffset(data.ID, WriteData(data));
            return Status.Ok;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if (!alreadyOpen) Close();
        }
    }

    internal Status Add(R data, int EditByID, bool AssignNextId = false)
    {
        lock (lockObj)
        {
            return AddNoLock(data, EditByID, AssignNextId);
        }
    }
    internal Status Update(R data, int editByID)
    {
        if (data.ID < 0) return Status.NegativeId;
        lock (lockObj)
        {
            bool alreadyOpen = IsOpen;
            try
            {
                long oldRecordOffset = 0;
                if (!IsOpen) OpenNoLock();
                var oldData = RentRecord();
                if (ReadNoLock(oldData, data.ID, out oldRecordOffset) != Status.Ok)
                {
                    ReturnRecord(oldData);
                    return Status.IdNotFound;
                }
                if (((Data)data).IsEqual(oldData))
                {
                    ReturnRecord(oldData);
                    return Status.NoChange;
                }
                if (oldData.Timestamp != data.Timestamp)
                {
                    ReturnRecord(oldData);
                    return Status.DataChangedBeforeUpdate;
                }
                for (int i = 0; i < keys.Count; i++)
                {
                    if (!keys[i].CanUpdateKey(data, oldData))
                    {
                        ReturnRecord(oldData);
                        return Status.DuplicateKey;
                    }
                }
                data.EditByID = editByID;
                data.StatusCode = UPDATE;
                data.Timestamp = DateTime.UtcNow;
                for (int i = 0; i < keys.Count; i++)
                {
                    keys[i].UpdateKey(data, oldData);
                }
                WriteRecordOffset(data.ID, WriteData(data));
                byte nstatus = (byte)'X';
                switch (oldData.StatusCode)
                {
                    case ADD:
                        nstatus = ARCHIVEADD;
                        break;
                    case UPDATE:
                        nstatus = ARCHIVEUPDATE;
                        break;
                    case DELETE:
                        nstatus = ARCHIVEDELETE;
                        break;
                }
                UpdateStatusCode(oldRecordOffset, nstatus);
                ReturnRecord(oldData);
                return Status.Ok;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!alreadyOpen) Close();
            }
        }
    }
    internal Status Delete(R data, int editByID)
    {
        if (!data.IsActive) return Status.DeleteInactive;
        if (data.ID < 0) return Status.NegativeId;
        if (data.ID == 0) return Status.CanNotDeleteID0;
        lock (lockObj)
        {
            bool alreadyOpen = IsOpen;
            try
            {
                if (!alreadyOpen) OpenNoLock();
                long oldRecordOffset = 0;
                var oldData = RentRecord();
                if (ReadNoLock(oldData, data.ID, out oldRecordOffset) != Status.Ok)
                {
                    ReturnRecord(oldData);
                    return Status.IdNotFound;
                }
                if (data.Timestamp != oldData.Timestamp)
                {
                    ReturnRecord(oldData);
                    return Status.DataChangedBeforeDelete;
                }
                for (int i = 0; i < keys.Count; i++)
                {
                    keys[i].DeleteKey(oldData);
                }
                data.StatusCode = DELETE;
                data.EditByID = editByID;
                WriteRecordOffset(data.ID, WriteData(data));
                byte nstatus = (byte)'X';
                switch (oldData.StatusCode)
                {
                    case ADD:
                        nstatus = ARCHIVEADD;
                        break;
                    case UPDATE:
                        nstatus = ARCHIVEUPDATE;
                        break;
                    case DELETE:
                        nstatus = ARCHIVEDELETE;
                        break;
                }
                UpdateStatusCode(oldRecordOffset, nstatus);
                ReturnRecord(oldData);
                return Status.Ok;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!alreadyOpen) Close();
            }
        }
    }
    internal Status Read(R data, int id, int editByID)
    {
        _ = editByID; // suppress warning
        lock (lockObj)
        {
            bool alreadyOpen = IsOpen;
            try
            {
                if (!alreadyOpen) Open();
                return ReadNoLock(data, id);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (!alreadyOpen)
                {
                    Close();
                }
            }
        }
    }
    private void OpenAtLeastOnce()
    {
        if (IsOpen) return;
        Open();
        Close();
    }
    internal void Filter(TableFilter<R> filter, R rec, long offset = 4)
    {
        OpenAtLeastOnce();
        //if (IsOpen) throw new WamfishException();
        filter.Init(this);
        try
        {
            FileStream fs;
            if (!File.Exists(TableFilePath)) return;
            using (fs = new FileStream(TableFilePath, FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Seek(offset, SeekOrigin.Begin);
                const int BUFSIZE = 1024 * 128;
                byte[] buf = ByteArrayPool.Rent(BUFSIZE);
                int bytesRead;
                int pos;
                int BytesLeft() => bytesRead - pos;
                int ReadInt()
                {
                    int val = buf[pos] << 24 | buf[pos + 1] << 16 | buf[pos + 2] << 8 | buf[pos + 3];
                    pos += 4;
                    return val;
                }
                long RecordPos = fs.Position;
                long NextRecordPos = 0;
                bytesRead = fs.Read(buf, 0, buf.Length);
                pos = 0;
                SerializationBuffer rapb = SerializationBuffer.Rent();
                while (bytesRead > 0)
                {
                    while (BytesLeft() > 4)
                    {
                        int size = ReadInt();
                        NextRecordPos = RecordPos + 4 + size;
                        int nextPos = pos + size;
                        if (size <= 0)
                        {
                            throw new InvalidDataException();
                        }
                        if (size <= BytesLeft())
                        {
                            rapb.Clear();
                            rapb.BlockCopy(buf, pos, 0, size);
                            pos = nextPos;
                            //if (buf[pos - 1] != byte.MaxValue)
                            //    throw new InvalidDataException();
                            var r = filter.ProcessPB(rec, rapb, RecordPos);
                            if (r == FilterResult.Stop)
                            {
                                Close();
                                return;
                            }
                            RecordPos = NextRecordPos;
                            continue;
                        }
                        else
                        {
                            rapb.Clear();
                            rapb.BlockCopy(buf, pos, 0, BytesLeft());
                            while (rapb.BytesUsed < size)
                            {
                                bytesRead = fs.Read(buf, 0, BUFSIZE);
                                int bytesLeft = size - rapb.BytesUsed;
                                if (bytesLeft >= BUFSIZE)
                                {
                                    rapb.BlockCopy(buf, 0, rapb.WriteIndex, BUFSIZE);
                                    pos = BUFSIZE;
                                    continue;
                                }
                                rapb.BlockCopy(buf, 0, rapb.WriteIndex, bytesLeft);
                                pos = bytesLeft;
                            }
                            var r = filter.ProcessPB(rec, rapb, RecordPos);
                            if (r == FilterResult.Stop)
                            {
                                Close();
                                return;
                            }
                            RecordPos = NextRecordPos;
                            continue;
                        }
                    }
                    int bl = BytesLeft();
                    if (BytesLeft() > 0)
                    {
                        Buffer.BlockCopy(buf, pos, buf, 0, bl);
                    }
                    bytesRead = fs.Read(buf, bl, buf.Length - bl);
                    bytesRead += bl;
                    pos = 0;
                }
                Close();
                return;
                void Close()
                {
                    buf = ByteArrayPool.Return(buf);
                    //rapb.Return();
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
    internal void FilterData(int editByID, DSList<R> result, int skip, int take, int filterFieldId, string filter, int sortFieldId, bool sortAscending = true)
    {
        OpenAtLeastOnce();
        var rlist = result._GetList();
        int recCount = 0;
        result.RecordCountInTable = 0;
        R rec = RecordFactory<R>.Rent();
        int keySize = 0;
        using var sb = SerializationBuffer.Rent();
        if (sortFieldId < 0)
        {
            sortFieldId = 2; // default sort on id
            sortAscending = true;
        }
        if (sortFieldId >= 0)
        {
            rec.FieldAsKey(sortFieldId, sb, 20);
            keySize = sb.BytesUsed;
            if (keySize < 1)
            {
                sortFieldId = 2;
                keySize = 4;
            }
            keySize += 4;

        }
        using var mi = MemoryIndex.Rent("LoadData", keySize);
        using var ids = IntList.Rent();
        int maxField = 0;
        if (sortFieldId > maxField)
            maxField = sortFieldId;
        if (filterFieldId > maxField)
            maxField = filterFieldId;
        RecordFactory<R>.Return(rec);
        rec = null;
        try
        {
            if (filterFieldId >= 0)
            {
                if (!RecordFilter(FilterWithFilter, maxField))
                {
                    return;
                }
            }
            else
            {
                if (!RecordFilter(FilterNoFilter, maxField))
                {
                    return;
                }
            }
            if (sortAscending)
            {
                mi.ReadAllAscending(ids.Ints);
            }
            else
            {
                mi.ReadAllDescending(ids.Ints);
            }
            int start = 0;
            int end = ids.Count;
            if (take > 0)
            {
                start = skip;
                end = start + take;
                if (end > ids.Count)
                    end = ids.Count;
            }
            for (int i = start; i < end; i++)
            {
                var d = RecordFactory<R>.Rent();
                if (Read(d, ids[i], editByID) == Status.Ok)
                {
                    rlist.Add(d);
                }
            }
            result.RecordCountInTable = recCount;
            return;
            void FilterWithFilter(R rec)
            {
                if (rec.FieldAsString(filterFieldId).Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    sb.Clear();
                    rec.FieldAsKey(sortFieldId, sb, 20);
                    sb.Write(rec.ID);
                    mi.Create(sb.Data);
                    recCount++; //Total Count of Records Filtered
                }
            }
            void FilterNoFilter(R rec)
            {
                sb.Clear();
                rec.FieldAsKey(sortFieldId, sb, 20);
                sb.Write(rec.ID);
                mi.Create(sb.Data);
                recCount++; //Total Count of Records in Table
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
            throw;
        }
    }

    internal void Flush(bool applyLock = true)
    {
        if (!IsOpen) throw new WamfishException();
        if (applyLock)
        {
            lock (lockObj)
            {
                DataFile.Flush();
            }
        }
        else
        {
            DataFile.Flush();
        }
    }
    internal bool RecordExist(int id)
    {
        if (!IsOpen) throw new WamfishException();
        lock (lockObj)
        {
            try
            {
                if (GetRecordOffset(id) == 0)
                    return false;
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
    internal int MaxId(bool callOpen = true)
    {
        lock (lockObj)
        {
            try
            {
                if (!IsOpen && callOpen) OpenNoLock();
                return maxId;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }

    internal R RentRecord()
    {
        return RecordFactory<R>.Rent();
    }
    internal void ReturnRecord(R data)
    {
        RecordFactory<R>.Return(data);
    }
    internal void WriteRecordOffset(int id, long recordOffset)
    {
        if (id > maxId)
            maxId = id;
        while (id > dataPtr.Count - 1)
            dataPtr.Add(0);
        dataPtr[id] = recordOffset;
        //dataptrIsDirty = true;
    }

    //private void InitKeys()
    //{
    //    var fs = GetType().GetFields();
    //    foreach (var f in fs)
    //    {

    //        if (f.FieldType == typeof(KeyBase) || f.FieldType.BaseType == typeof(KeyBase))
    //        {
    //            var k = (KeyBase)f.GetValue(this);
    //            k.table = this;
    //            keys.Add(k);
    //        }
    //    }
    //}
    private void ClearKeys()
    {
        for (int i = 0; i < keys.Count; i++)
        {
            keys[i].ClearKeys();
        }
    }
    private bool OpenNoLock()
    {
        if (IsOpen) return true;
        pb = SerializationBuffer.Rent();
        DataFile = new WfFile(TableFilePath);
        DataFile.Open();
        IsOpen = true;
        if (DataFile.Length < 1)
        {
            ClearKeys();
            InitDataFile();
            OnInit?.Invoke();
        }
        else
        {
            if (dataPtr.Count == 0)
            {
                //ClearKeys();
                //LoadDataFile();
                using var f = new LoadKeysFilter();
                f.Run(this);
            }
        }
        //if (IndexTracker != null)
        //    IndexTracker.Open();
        return true;
    }
    private Status ReadNoLock(R data, int id)
    {
        if (!IsOpen) Open();
        data.ID = id;
        if (data.ID < 0) return Status.NegativeId;
        try
        {
            var RecordOffset = GetRecordOffset(data.ID);
            if (RecordOffset == 0) return Status.IdNotFound;
            DataFile.ReadPacketBuffer(pb, RecordOffset);
            data.ReadFromBuf(pb);
            //rec.Deserialize(data, pb);
            if (data.IsActive) return Status.Ok;
            return Status.ReadInactive;
        }
        catch (Exception)
        {
            throw;
        }
    }
    private Status ReadNoLock(R data, int id, out long recordOffset)
    {
        if (!IsOpen) throw new WamfishException();
        recordOffset = 0;
        data.ID = id;
        if (data.ID < 0) return Status.NegativeId;
        try
        {
            recordOffset = GetRecordOffset(id);
            if (recordOffset == 0) return Status.IdNotFound;
            DataFile.ReadPacketBuffer(pb, recordOffset);
            data.ReadFromBuf(pb);
            //rec.Deserialize(data, pb);
            if (data.IsActive) return Status.Ok;
            return Status.ReadInactive;
        }
        catch (Exception)
        {
            throw;
        }
    }
    private void InitDataFile()
    {
        nextId = 0;
        DataFile.Write(Version);
        dataPtr.Clear();
        var r = RentRecord();
        r.Clear();
        r.ID = 0;
        r.EditByID = -1;
        Add(r,-1,false);
    }
    private readonly List<long> dataPtr = new(1000);
    private protected long GetRecordOffset(int id)
    {
        if (id > dataPtr.Count - 1)
            return 0;
        return dataPtr[id];
    }
    private long WriteData(R data)
    {
        long offset = DataFile.SeekEnd();
        pb.Clear();
        data.WriteToBuf(pb);
        //rec.Serialize(data, pb);
        DataFile.Write(pb);
        return offset;
    }
    private void UpdateStatusCode(long recordOffset, byte statusCode)
    {
        DataFile.SeekBegin(recordOffset + 4);
        DataFile.Write(statusCode);
    }
    private int NextId
    {
        get
        {
            return Interlocked.Increment(ref nextId);
        }
    }
    //We have only one instance of a table shared by all threads
    #region Internal Classes
    public abstract class KeyBase
    {
        internal int fieldId;
        internal Table<R> table; //InitKeys will set this
        public abstract Status Read(R rec, int editByID);
        internal abstract void ClearKeys();
        internal abstract void AddKey(R rec);
        internal abstract bool CanAddKey(R rec);
        internal abstract void DeleteKey(R rec);
        internal abstract void UpdateKey(R newRec, R oldRec);
        internal abstract bool CanUpdateKey(R newRec, R oldRec);
    }
    public class Key : KeyBase
    {
        internal readonly Dictionary<string, int> keys = new(StringComparer.OrdinalIgnoreCase);
        public Key(Table<R> table, int fieldId)
        {
            this.table = table;
            table.keys.Add(this);
            this.fieldId = fieldId;
        }
        public override Status Read(R rec, int editByID)
        {

            bool isAlreadyOpen = table.IsOpen;
            try
            {
                if (!isAlreadyOpen) table.Open();
                if (keys.TryGetValue(rec.FieldAsString(fieldId), out int id))
                {
                    return table.Read(rec, id, editByID);
                }
                return Status.KeyNotFound;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!isAlreadyOpen) table.Close();
            }
        }
        internal override void AddKey(R rec)
        {
            if (keys.TryAdd(rec.FieldAsString(fieldId), rec.ID))
                return;
            //throw new WamfishException();
        }
        internal override bool CanAddKey(R rec)
        {
            if (keys.ContainsKey(rec.FieldAsString(fieldId)))
            {
                return false;
            }
            return true;
        }
        internal override void DeleteKey(R rec)
        {
            keys.Remove(rec.FieldAsString(fieldId));
        }
        internal override void UpdateKey(R newRec, R oldRec)
        {
            var ns = newRec.FieldAsString(fieldId);
            var os = oldRec.FieldAsString(fieldId);
            if (string.Compare(ns, os, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return;
            }
            if (keys.ContainsKey(ns)) throw new WamfishException();
            keys.Remove(os);
            keys.Add(ns, newRec.ID);
        }
        internal override bool CanUpdateKey(R newRec, R oldRec)
        {
            var ns = newRec.FieldAsString(fieldId);
            var os = oldRec.FieldAsString(fieldId);
            if (string.Compare(ns, os, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            if (keys.ContainsKey(ns))
                return false;
            return true;
        }
        internal override void ClearKeys()
        {
            keys.Clear();
        }
    }
    internal bool GetAllIds(List<int> ids)
    {
        using var f = new GetAllIds<R>();
        var result = f.Run(this, ids);
        return result;

    }
    internal bool RecordFilter(Action<R> filter, int maxField = int.MaxValue)
    {
        using var f = new RecordFilter<R>();
        var result = f.Run(this, filter, maxField);
        return result;
    }
    internal bool FilterIds(List<int> ids, Func<R, bool> filter)
    {
        using var f = new IdFilter<R>();
        var result = f.Run(this, ids, filter);
        return result;
    }
    private class LoadKeysFilter : TableFilter<R>
    {
        public bool Run(Table<R> table)
        {
            this.table = table;
            for (int i = 0; i < table.keys.Count; i++)
            {
                table.keys[i].ClearKeys();
            }
            table.dataPtr.Clear();
            R rec = RecordFactory<R>.Rent();
            table.Filter(this, rec);
            RecordFactory<R>.Return(rec);
            table.nextId = table.maxId;
            return true;
        }
        public override void FilterInit(out int maxField)
        {
            ActiveOnly = true;
            maxField = 0;
            for (int i = 0; i < table.keys.Count; i++)
            {
                int fid = table.keys[i].fieldId;
                if (fid > maxField)
                    maxField = fid;
            }
        }
        public override FilterResult FilterRecord(R data)
        {
            var sc = data.StatusCode;
            if (sc == 'A' || sc == 'U')
            {
                table.WriteRecordOffset(data.ID, offset);
                for (int i = 0; i < table.keys.Count; i++)
                {
                    table.keys[i].AddKey(data);
                }
            }
            return FilterResult.Continue;
        }
    }
    #endregion
}