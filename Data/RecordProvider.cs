namespace WFLib;

public class RecordProvider<R> : IDataProvider<R> where R : Record, new()
{
    public RecordContext<R> Ctx { get; private set; } = RecordContextFactory<R>.Rent();
    private RecordProvider() 
    { 
    }
    public R RentData()
    {
        return RecordFactory<R>.Rent();
    }
    public bool Read(R rec, int id)
    {
        if (Ctx.Read(rec, id) == Status.Ok)
        {
            return true;
        }
        return false;
    }
    public DSList<R> Read(int skip, int take, FilterList filters = null, SortList  sortFields = null)
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
        DSList<R> recs = DSList<R>.Rent();
        Ctx.FilterData(recs, skip, take, filterField, filterValue, sortFieldId, sortAscending);
        return recs;
    }
    public bool Add(R rec)
    {
        if (Ctx.Add(rec, true) == Status.Ok)
        {
            return true;
        }
        return false;
    }
    public bool Delete(R data)
    {
        if (Ctx.Delete(data) == Status.Ok)
        {
            return true;
        }
        return false;
    }
    public bool Update(R data)
    {
        if (Ctx.Update(data) == Status.Ok)
        {
            return true;
        }
        return false;
    }
    private static RecordProvider<R> Create()
    {
        return new RecordProvider<R>();
    }
    private static Pool<RecordProvider<R>> pool = new(Create);
    public static RecordProvider<R> Rent(User user)
    {
        var rp = pool.Rent();
        rp.Ctx = RecordContextFactory<R>.Rent(user.ID);
        return rp;
    }
    public static string PoolStats => pool.Stats;
    public static void PoolClear() => pool.Clear();
    ~RecordProvider()
    {
        Dispose();
    }
    public void Dispose()
    {
        Ctx.Dispose();
        Ctx = null;
        pool.Return(this);
    }
}
