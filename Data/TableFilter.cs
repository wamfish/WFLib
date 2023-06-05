//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;


public enum FilterResult { Stop, Continue }
//TableFilter is the base class for all Table filter classes. The use of generics allows for creating filters
//that can be used with any table.
//public abstract class TableFilter<T, R, D> where T : Table<T, R, D>, new() where R : IRecord<D>, new() where D : IDataFields, new()
public abstract class TableFilter<R> : IDisposable where R : Record, new()
{
    public bool ActiveOnly { get; set; } = true;
    public int NotActiveCount;
    protected Table<R> table;
    //protected R filterRec;
    protected long offset;
    protected int maxField;
    public TableFilter() { }
    //int maxField;
    public void Init(Table<R> table)
    {
        this.table = table;
        NotActiveCount = 0;
        R data = RecordFactory<R>.Rent();
        RecordFactory<R>.Return(data);
        FilterInit(out maxField);
        //StatusCode == 0 ID == 1  
        if (maxField < 1) maxField = 1; 
    }
    public abstract void FilterInit(out int maxField);
    public FilterResult ProcessPB(R data, SerializationBuffer pb, long offset)
    {
        this.offset = offset;
        byte statuscode;
        if (!pb.PeekByte(out statuscode))
        {
            return FilterResult.Stop;
        }
        if (ActiveOnly)
        {
            if (statuscode != 'A' && statuscode != 'U')
            {
                NotActiveCount++;
                return FilterResult.Continue;
            }
        }
        data.ReadFromBuf(pb, maxField);
        //filterRec.Deserialize(data, pb, maxField);
        var r = FilterRecord(data);
        return r;
    }
    public abstract FilterResult FilterRecord(R data);

    public void Dispose()
    {
        table = null;
    }
}
