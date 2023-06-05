namespace WFLib;

public struct RecID<R> : IRecID where R : Record, new()
{
    public RecID()
    {
        _Rec = new();
        ID = 0;
    }
    public R RentRecord() => RecordFactory<R>.Rent();
    private R _Rec;
    public R Rec => _Rec;
    public int ID
    {
        get => Rec.ID;
        set 
        {
            if (Rec.ID != value) 
            {
                Rec.ID = value;
            }
        }
    }
    public static implicit operator RecID<R>(int id) => new RecID<R> { ID = id };
    public static implicit operator int(RecID<R> rf) => rf.ID;
    public void Clear()
    {
        if (Rec == null) _Rec = new();
        Rec.Clear();
        ID = 0;
    }
    public string AsString() => ID.ToString();
    public string RecAsString()
    {
        if (Rec == null) return string.Empty;
        for (int i = 0; i < Rec.FieldCount; i++)
        {
            if (Rec.FieldType(i) == typeof(string))
            {
                return Rec.FieldAsString(i);
            }
        }
        return Rec.FieldAsString(4);
    }
    public bool RefreshRec()
    {
        int id = Rec.ID;
        Rec.Clear();
        Rec.ID = id;
        if (ID < 0) return true;
        if (Rec == null) _Rec = new();
        using var ctx = RecordContextFactory<R>.Rent();
        if (ctx.Read(Rec, ID) != Status.Ok) return false;
        return true;
    }
    
    public RecordList GetList(User user,  string filter,int skip, int take)
    {
        using var prov = RecordProvider<R>.Rent(user);
        int nameField = 4;
        for (int i = 4; i < Rec.FieldCount; i++)
        {
            if (Rec.FieldType(i) == typeof(string))
            {
                nameField = i;
                break;
            }
        }
        //ToDo: Filters need to be more advanced
        //Data should define default filters that can be overridden
        //Filters should be able to be combined with and/or
        using var fl = FilterList.Rent();
        fl.Add(nameField, filter);
        using var sl = SortList.Rent();
        sl.Add(nameField, true);
        using var dsl = prov.Read(skip, take, fl, sl);
        RecordList records = RecordList.Rent();
        for(int i=0;i<dsl.Count; i++)
        {
            var d = RecordFactory<R>.Rent();
            dsl[i].CopyTo(d);
            records.Add(d);
        }
        return records;
    }
}
