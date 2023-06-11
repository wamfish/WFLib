using System.Data.Common;

namespace WFLib;
// Note: If you change this class keep in mind that we are using one static instance.
// Keep the methods thread safe and do not add state to the class.

public class DataProvider<D> : IDataProvider<D> where D : Data, new()
{
    int nextid = 0;
    //Func<D> _factory;
    private readonly Dictionary<int, D> dataSource = new();
    static D staticData = new();
    public D RentData()
    {
        return DataFactory<D>.Rent();
    }
    private DataProvider() 
    { 
    }
    public DSList<D> Read(int skip, int take, FilterList filters = null, SortList sortFields = null)
    {
        int filterField = -1;
        string filterValue = string.Empty;
        if (filters != null && filters.Count > 0)
        {
            filterField = filters[0].Field;
            filterValue = filters[0].Filter;
        }
        int sortFieldId = -1;
        bool sortAscending = true;
        if (sortFields != null && sortFields.Count > 0)
        {
            sortFieldId = sortFields[0].Field;
            sortAscending = sortFields[0].Ascending;
        }
        DSList<D> result = DSList<D>.Rent();
        FilterData(result, skip, take, filterField, filterValue, sortFieldId, sortAscending);
        return result;
    }
    public void Clear()
    {
        lock (dataSource)
        {
            dataSource.Clear();
        }
    }
    public bool Add(D data)
    {
        lock (dataSource)
        {
            if (staticData.IsRecord)
            {
                // Do not allow duplicate records
                if (dataSource.ContainsKey(data._id)) return false;
            }
            else
            {
                // Data is not a record so we need to assign an id
                data._id = nextid++;
            }
            dataSource.Add(data._id,data);
            return true;
        }
    }
    public bool Update(D data)
    {
        lock (dataSource)
        {
            if (dataSource.TryGetValue(data._id, out D dsData))
            {
                data.CopyTo(dsData);
                return true;
            }
            return false;
        }
    }
    public bool Delete(D data)
    {
        lock (dataSource)
        {
            return dataSource.Remove(data._id);
        }
    }
    private void FilterData(DSList<D> result, int skip, int take, int filterFieldId, string filter, int sortFieldId, bool sortAscending = true)
    {
        var rlist = result._GetList();    
        using var sb = SerializationBuffer.Rent();
        if (sortFieldId < 0 || sortFieldId >= staticData.FieldCount)
        {
            sortFieldId = 1; // default sort on id
            sortAscending = true;
        }
        int keySize = GetKeySize(ref sortFieldId);
        using var mi = MemoryIndex.Rent("FilterData", keySize);
        if (filterFieldId >= 0)
            result.RecordCountInTable = DataFilter(FilterWithFilter, mi);
        else
            result.RecordCountInTable = DataFilter(FilterNoFilter, mi);
        using var ids = IntList.Rent();
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
            if (dataSource.TryGetValue(ids.Ints[i], out D d))
            {
                var newd = RentData();
                d.CopyTo(newd);
                newd._id = d._id;
                rlist.Add(newd);
            }
        }
        return;
        void FilterWithFilter(D data, int index, MemoryIndex mi)
        {
            if (data.FieldAsString(filterFieldId).Contains(filter))
            {
                sb.Clear();
                data.FieldAsKey(sortFieldId, sb, 20);
                sb.Write(index);
                mi.Create(sb.Data);
            }
        }
        void FilterNoFilter(D data, int index, MemoryIndex mi)
        {
            sb.Clear();
            data.FieldAsKey(sortFieldId, sb, 20);
            sb.Write(index);
            mi.Create(sb.Data);
        }
    }
    private int GetKeySize(ref int sortFieldId)
    {
        using var sb = SerializationBuffer.Rent();
        int keySize = 0;

        staticData.FieldAsKey(sortFieldId, sb, 20);
        keySize = sb.BytesUsed;
        if (keySize < 1)
        {
            sortFieldId = 1;
            keySize = 4;
        }
        keySize += 4;
        return keySize;
    }
    private int DataFilter(Action<D, int, MemoryIndex> filter, MemoryIndex mi)
    {
        int count = 0;
        for (int i = 0; i < dataSource.Count; i++)
        {
            //if (dataSource[i].isDeleted) continue;
            count++;
            filter.Invoke(dataSource[i], i, mi);
        }
        return count;
    }

    private static DataProvider<D> Create()
    {
        return new DataProvider<D>();
    }
    private static Pool<DataProvider<D>> pool = new(Create);
    public static DataProvider<D> Rent()
    {
        var ds = pool.Rent();
        return ds;
    }
    public static string PoolStats => pool.Stats;
    public static void PoolClear() => pool.Clear();
    ~DataProvider()
    {
        Dispose();
    }
    public void Dispose()
    {
        dataSource.Clear();
        pool.Return(this);
    }

}
