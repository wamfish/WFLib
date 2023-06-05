//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;
public class DataContext<D> : IDisposable where D : Data, new()
{
    protected D Data;
    protected virtual void OnBaseConstruct() { }
    public DataContext()
    {
        OnBaseConstruct();
    }
    public bool FieldIsDefault(int field) => Data.FieldIsDefault(field);
    public bool FieldIsEqual(Data toFld, int field) => Data.FieldIsEqual(toFld, field);
    public string FieldAsString(int field) => Data.FieldAsString(field);
    public void FieldFromString(string str, int field) => Data.FieldFromString(str, field);
    public string FieldName(int field) => Data.FieldName(field);
    public string FieldLabel(int field) => Data.FieldLabel(field);
    public void FieldLabelSet(int field, string label) => Data.FieldLabelSet(field, label);
    public string FieldColumnLabel(int field) => Data.FieldColumnLabel(field);
    public void FieldColumnLabelSet(int field, string label) => Data.FieldColumnLabelSet(field, label);
    public Type FieldType(int field) => Data.FieldType(field);
    public Object FieldAsObject(int field) => Data.FieldAsObject(field);
    public void FieldMinSet(int field, object min) => Data.FieldMinSet(field, min);
    public object FieldMin(int field) => Data.FieldMin(field);
    public void FieldMaxSet(int field, object max) => Data.FieldMaxSet(field, max);
    public object FieldMax(int field) => Data.FieldMax(field);
    public int FieldIdFromName(string name) => Data.FieldIdFromName(name);
    public void FieldAsKey(int field, SerializationBuffer sb, int maxSize) => Data.FieldAsKey(field, sb, maxSize);
    //protected D RentData()  
    //{ 
    //    var d = DataFactory<D>.Rent();
    //    return d;
    //}
    //protected void ReturnData(D data) => DataFactory<D>.Return(data);

    public bool IsDirty(D data, D origData)
    {
        for (int i = 0; i < data.FieldCount; i++)
        {
            if (!data.FieldIsEqual(origData, i)) return true;
        }
        return false;
    }
    public virtual void Dispose()
    {

    }
}
