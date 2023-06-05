//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;

public partial class GetAllIds<R> : TableFilter<R> where R : Record, new()
{
    List<int> ids;
    public bool Run(Table<R> table, List<int> ids)
    {
        this.ids = ids;
        ids.Clear();
        R rec = RecordFactory<R>.Rent();
        table.Filter(this, rec);
        RecordFactory<R>.Return(rec);
        return true;
    }
    public override void FilterInit(out int maxField)
    {
        ActiveOnly = true;
        maxField = 0;
    }
    public override FilterResult FilterRecord(R data)
    {
        ids.Add(data.ID);
        return FilterResult.Continue;
    }
}


public partial class IdFilter<R> : TableFilter<R> where R : Record, new()
{
    List<int> ids;
    Func<R, bool> filter;
    public bool Run(Table<R> table, List<int> ids, Func<R, bool> filter)
    {
        this.filter = filter;
        this.ids = ids;
        ids.Clear();
        R rec = RecordFactory<R>.Rent();
        table.Filter(this, rec);
        RecordFactory<R>.Return(rec);
        return true;
    }
    public override void FilterInit(out int maxField)
    {
        ActiveOnly = true;
        maxField = int.MaxValue;
    }
    public override FilterResult FilterRecord(R data)
    {
        if (filter(data)) ids.Add(data.ID);
        return FilterResult.Continue;
    }
}

public partial class RecordFilter<R> : TableFilter<R> where R : Record, new()
{
    int MaxFieldArg;
    Action<R> filter;
    public bool Run(Table<R> table, Action<R> filter, int maxFieldArg = int.MaxValue)
    {
        MaxFieldArg = maxFieldArg;
        R rec = RecordFactory<R>.Rent();
        this.filter = filter;
        table.Filter(this, rec);
        RecordFactory<R>.Return(rec);
        return true;
    }
    public override void FilterInit(out int maxField)
    {
        ActiveOnly = true;
        maxField = MaxFieldArg;
        if (maxField < 0) maxField = int.MaxValue;
    }
    public override FilterResult FilterRecord(R rec)
    {
        filter(rec);
        return FilterResult.Continue;
    }
}
