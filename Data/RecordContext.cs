//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public class RecordContext<R> : DataContext<R> where R : Record, new()
{
    protected int EditByID = -1;
    public R Rec => Data;
    public RecordContext()
    {
        Data = RecordFactory<R>.Rent();
    }
    protected Table<R> table;
    public virtual Table<R> Table => table;
    public string TableName => Table.TableName;
    public bool GetAllIds(List<int> ids)
    {
        var result = Table.GetAllIds(ids);
        return result;
    }
    public bool FilterIds(List<int> ids, Func<R, bool> filter)
    {
        var result = Table.FilterIds(ids, filter);
        return result;
    }
    public void Open() => Table.Open();
    public void Close() => Table.Close();
    public void Flush() => Table.Flush();
    public void DeleteTable() => Table.DeleteTable();
    public Status Add(R data, bool assignNextId = false)
    {
        return Table.Add(data, EditByID, assignNextId);
    }
    public Status Update(R rec)
    {
        return Table.Update(rec,EditByID);
    }
    public Status Delete(R rec)
    {
        return Table.Delete(rec, EditByID);
    }
    public Status Read(R rec, int id)
    {
        return Table.Read(rec, id, EditByID);
    }
    public void Filter(TableFilter<R> filter, R rec)
    {
        table.Filter(filter, rec);
    }
    public void FilterData(DSList<R> result, int skip, int take, int filterFieldId, string filter, int sortFieldId, bool sortAscending = true)
    {
        table.FilterData(EditByID, result, skip, take, filterFieldId, filter, sortFieldId, sortAscending);
    }

    public R RentRecord()
    {
        return RecordFactory<R>.Rent();
    }
    public void ReturnRecord(R rec)
    {
        RecordFactory<R>.Return(rec);
    }

    public override void Dispose()
    {
        throw new NotImplementedException();
    }
    //We need to kick off the static init of the RecordContextFactory from a Record
    public static int KickOffStaticInit = 0;
    protected static void _setEditById(RecordContext<R> recordContext,int editById)
    {
        recordContext.EditByID = editById;
    }
}


