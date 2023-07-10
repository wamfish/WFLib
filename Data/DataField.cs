namespace WFLib;

/// <summary>
/// DataField is used to include a Data class as a field in the Record class. DataField maintans a list of one or
/// more Data items. The idea is that you can use this field as a single Data item, or as a list of data items.

/// This class works a little different from a normal list in that we always have at least one data item. This
/// allows us to ignore the list aspect of the field if we want to. 
/// 
/// To use DataField as a list, use the Count and CurrentIndex properties to iterate through the list:
/// 
/// for(int i = 0; i < dataField.Count; i++)
/// {
///     var data = dataField[i];
///     // so something with data
/// }
/// 
/// Use Add, Create, and Remove to manage the list. The property Data is a reference to the Data item
/// at CurrentIndex. You can set CurrentIndex to point to any valid item in the list, you can not set 
/// CurrentIndex to a value that is out of range. 
/// 
/// </summary>
/// <typeparam name="DF"></typeparam>
/// <typeparam name="C"></typeparam>
/// <typeparam name="D"></typeparam>

//public abstract class DataField<DF, C, D> where DF : DataField<DF, C, D>, new() where C : DataContext<D>, new() where D : Data, new()
//public abstract class DataField<DF, D> where DF : DataField<DF, D>, new() where D : Data, new()
public abstract class DataField<D> : IDataField where D : Data, new()
{
    public int Field { get; protected set; }
    public event Action OnInit;
    DataContext<D> context;
    public DataContext<D> Context => context;
    List<D> dataList = new List<D>();
    public D Data
    {
        get
        {
            lock (dataList)
            {
                //We should always have at least one data item, but just in case ...
                if (CurrentIndex < 0 || CurrentIndex >= dataList.Count) return null;
                //Console.WriteLine($"DataField.Data: CurrentIndex={CurrentIndex}");
                return dataList[CurrentIndex];
            }
        }
    }
    public D this[int i]
    {
        get
        {
            lock (dataList)
            {
                if (i >= 0 && i < dataList.Count)
                {
                    //Console.WriteLine($"DataField.[]: I = {i} CurrentIndex={CurrentIndex}");
                    return dataList[i];
                }
            }
            return null;
        }
        set
        {
            lock (dataList)
            {
                if (i < 0 || i >= dataList.Count) throw new IndexOutOfRangeException();
                DataFactory<D>.Return(dataList[i]);
                dataList[i] = value;
            }
        }
    }
    public int Count => dataList.Count;
    private int _currentIndex = -1;
    public int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            if (value >= 0 && value < dataList.Count)
            {
                _currentIndex = value;
                return;
            }
            if (value < 0)
            {
                _currentIndex = 0;
                return;
            }
            _currentIndex = dataList.Count - 1;
        }
    }

    /// <summary>
    /// Create a new data and add it to the list
    /// </summary>
    /// <returns>Returns the newly created data</returns>
    public D Create()
    {
        D data = DataFactory<D>.Rent();
        int index = Add(data);
        return this[index];
    }
    /// <summary>
    /// Add data to the list, and sets the current index to the new data
    /// The caller can still use data, but this DataField now owns it, and 
    /// will return it to the factory when it is removed
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>

    public int Add(D data)
    {
        lock (dataList)
        {
            dataList.Add(data);
            CurrentIndex = dataList.Count - 1;
            return CurrentIndex;
        }
    }
    /// <summary>
    /// Removes data from the list and returns it to the factory
    /// the caller should no longer use data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool Remove(D data)
    {
        lock (dataList)
        {
            int index = dataList.IndexOf(data);
            if (index < 0) return false;
            if (Count == 1)
            {
                DataFactory<D>.Return(dataList[index]);
                dataList.RemoveAt(index);
                Create();
                //This is different from RemoveAt
                //in this case we notify success 
                return true;
            }
            DataFactory<D>.Return(dataList[index]);
            dataList.RemoveAt(index);
            CurrentIndex = index;
            return true;
        }
    }
    /// <summary>
    /// Removes data at index from the list and returns it to the factory
    /// The caller should no longer use any held refrences to data
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool RemoveAt(int index)
    {
        lock (dataList)
        {
            if (index < 0 || index >= dataList.Count) return false;
            if (Count == 1)
            {
                Data.Clear();
                //We return false to let the caller know that we are empty
                return false;
            }
            DataFactory<D>.Return(dataList[index]);
            dataList.RemoveAt(index);
            CurrentIndex = index;
            return true;
        }
    }
    /// <summary>
    /// Adds Data after at index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool InsertAfter(int index)
    {
        lock (dataList)
        {
            if (index < 0 || index >= dataList.Count) return false;
            D data = DataFactory<D>.Rent();
            if (index == dataList.Count - 1)
            {
                dataList.Add(data);
                CurrentIndex = dataList.Count - 1;
                return true;
            }
            dataList.Insert(index + 1, data);
            CurrentIndex = index + 1;
            return true;
        }
    }
    public DataField()
    {
        context = DataContextFactory<D>.Rent();
    }
    public void XInit()
    {
        //data.Init already does the xint stuff
    }
    public void Init()
    {
        for (int i = 1; i < dataList.Count; i++)
        {
            DataFactory<D>.Return(dataList[i]);
        }
        if (dataList.Count == 0)
        {
            Create();
            OnInit?.Invoke(); //allow for overrides
            return;
        }
        if (dataList.Count > 1)
        {
            dataList.RemoveRange(1, dataList.Count - 1);
        }
        dataList[0].Init();
        OnInit?.Invoke(); //allow for overrides
    }
    public void Clear()
    {
        for (int i = 1; i < dataList.Count; i++)
        {
            DataFactory<D>.Return(dataList[i]);
        }
        if (dataList.Count == 0)
        {
            Create();
            return;
        }
        if (dataList.Count > 1)
        {
            dataList.RemoveRange(1, dataList.Count - 1);
        }
        dataList[0].Clear();
    }
    public void WriteToBuf(SerializationBuffer sb)
    {
        sb.WriteSize(dataList.Count);
        using SerializationBuffer sb2 = SerializationBuffer.Rent();
        for (int i = 0; i < dataList.Count; i++)
        {
            dataList[i].WriteToBuf(sb2);
            sb.Write(sb2.Buf);
            sb2.Clear();
        }
    }
    public void ReadFromBuf(SerializationBuffer sb)
    {
        using SerializationBuffer sb2 = SerializationBuffer.Rent();
        int count = sb.ReadSize();
        for (int i = 0; i < count; i++)
        {
            sb.Read(sb2.Buf);
            if (i == 0)
            {
                dataList[0].ReadFromBuf(sb2);
            }
            else
            {
                var d = DataFactory<D>.Rent();
                d.ReadFromBuf(sb2);
                if (count > 2)
                {
                    Console.WriteLine("ReadFromBuf: " + d.ToString());
                }
                dataList.Add(d);
            }
            sb2.Clear();
        }
    }
    private void DataAsString(D data, StringBuilder sb)
    {
        int fcount = data.FieldCount;
        sb.Append(DATASTART);
        for (int i = 0; i < fcount; i++)
        {
            //if (!context.FieldIsSelected(i)) continue;
            if (data.FieldIsDefault(i)) continue;
            sb.Append(i.AsString());
            sb.Append(VALUESEP);
            sb.Append(data.FieldAsString(i));
            sb.Append(FIELDSEP);
        }
        sb.Append(DATAEND);
    }
    public string AsString()
    {
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < dataList.Count; i++)
        {
            DataAsString(dataList[i], sb);
        }
        string r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    int GetField(ReadOnlySpan<char> s, ref int i)
    {
        int startPos = i;
        var c = s[i++];
        if (c == DATAEND) return -1;
        if (c == VALUESEP)
        {
            return 0;
        }
        for (; i < s.Length; i++)
        {
            c = s[i];
            if (c == VALUESEP)
            {
                int endPos = i++;
                int field = s.Slice(startPos, endPos - startPos).AsInt();
                return field;
            }
        }
        return -1;
    }
    public enum ParseState
    {
        Start,
        Field,
        Value,
        End
    }
    public void FromString(string value)
    {
        Clear();
        var s = value.AsSpan();
        if (s.Length < 1) return;
        if (s[0] != DATASTART) return;
        D d = null;
        var sb = StringBuilderPool.Rent();
        int field = -1;
        ParseState state = ParseState.Start;
        for (int i = 0; i < s.Length; i++)
        {
            if (state == ParseState.Field)
            {
                field = GetField(s, ref i);
                if (field < 0) //end of data
                {
                    if (d != null) dataList.Add(d);
                    d = null;
                    state = ParseState.Start;
                    continue;
                }
                state = ParseState.Value;
                continue;
            }
            var c = s[i];
            if (c == DATASTART)
            {
                if (state != ParseState.Start) throw new Exception("Invalid data string");
                d = DataFactory<D>.Rent();
                state = ParseState.Field;
                continue;
            }
            //this should not happen, treat it as an error
            if (c == DATAEND)
            {
                if (state != ParseState.Value) throw new Exception("Invalid data string");
                continue;
            }
            if (c == FIELDSEP)
            {
                d.FieldFromString(sb.ToString(), field);
                sb.Clear();
                state = ParseState.Field;
                continue;
            }
            sb.Append(c);
        }
        return;
    }
    private bool DataIsEqualTo(D src, D to)
    {
        int fcount = src.FieldCount;
        for (int i = 0; i < fcount; i++)
        {
            if (!src.FieldIsEqual(to, i)) return false;
        }
        return true;
    }
    public bool IsEqualTo(DataField<D> df)
    {
        if (dataList.Count != df.dataList.Count) return false;
        for (int i = 0; i < dataList.Count; i++)
        {
            var isEqual = DataIsEqualTo(dataList[i], df.dataList[i]);
            if (!isEqual) return false;
        }
        return true;
    }
    public void CopyTo(DataField<D> to)
    {
        to.Clear();
        for (int i = 0; i < dataList.Count; i++)
        {
            if (i == 0)
            {
                dataList[i].CopyTo(to.dataList[0]);
                continue;
            }
            var d = DataFactory<D>.Rent();
            dataList[i].CopyTo(d);
            to.dataList.Add(d);
        }
    }
    public bool DataIsDefault(D data)
    {
        int fcount = data.FieldCount;
        for (int i = 0; i < fcount; i++)
        {
            if (!data.FieldIsDefault(i)) return false;
        }
        return true;
    }
    public bool IsDefault()
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            if (!DataIsDefault(dataList[i])) return false;
        }
        return true;
    }
    public void Skip(SerializationBuffer sb)
    {
        using SerializationBuffer sb2 = SerializationBuffer.Rent();
        int count = sb.ReadSize();
        for (int i = 0; i < count; i++)
        {
            sb.Read(sb2.Buf);
            sb2.Clear();
        }
        sb2.Clear();
    }

    Data IDataField.Data => Data;

    Data IDataField.this[int i]
    {
        get => this[i];
        set => this[i] = (D)value;
    }

    int IDataField.Add(Data data)
    {
        return Add((D)data);
    }

    void IDataField.CopyTo(object to)
    {
        CopyTo((DataField<D>)to);
    }

    Data IDataField.Create()
    {
        return Create();
    }

    bool IDataField.DataIsDefault(Data data)
    {
        return DataIsDefault((D)data);
    }

    bool IDataField.IsEqualTo(object df)
    {
        return IsEqualTo((DataField<D>)df);
    }

    bool IDataField.Remove(Data data)
    {
        return Remove((D)data);
    }
}
