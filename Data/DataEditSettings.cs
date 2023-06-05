using WFLib;

namespace WFLib;

public enum SORTSTATE { NONE, ASCENDING, DESCENDING }

public partial class DataEditSettings<D> : IDisposable where D : Data, new()
{
    private DataEditSettings() { }
    private static readonly D data = new();
    private readonly List<FieldEditCtx<D>> fields = new();
    public List<FieldEditCtx<D>> Fields => fields;
    public async Task<bool> ValidateFields()
    {
        bool isValid = true;
        foreach (var field in Fields)
        {
            if (!field.IsVisible) continue;
            bool valid = field.Validate(data);
            if (!valid) isValid = false;
            var eh = data.FieldEditHelper(field.Field);
            if (eh != null) await eh.SetValidState(valid);
        }
        return isValid;
    }
    public List<FieldEditCtx<D>> SortedVisableFields
    {
        get
        {
            List<FieldEditCtx<D>> list = VisibleFields;
            list.Sort((x, y) => x.Order.CompareTo(y.Order));
            return list;
        }
    }
    private readonly SortList sortDefs = SortList.Rent();
    public SortList SortDefs
    {
        get
        {
            SortDefsBuild(); //ToDo: Figure out how to do this only when needed
            return sortDefs;
        }
    }
    private void SortDefsBuild()
    {
        sortDefs.Clear();
        foreach (var field in Fields)
        {
            if (field.SortState != SORTSTATE.NONE)
            {
                sortDefs.Add(field.Field, field.SortState == SORTSTATE.ASCENDING);
            }
        }
    }
    private readonly FilterList  filterDefs = FilterList.Rent();
    public FilterList FilterDefs
    {
        get
        {
            FilterDefsBuild(); //ToDo: Figure out how to do this only when needed
            return filterDefs;
        }
    }
    private void FilterDefsBuild()
    {
        filterDefs.Clear();
        foreach (var field in Fields)
        {
            if (field.IsFilterSet)
            {
                filterDefs.Add(field.Field, field.FilterValue);
            }
        }
    }
    public FieldEditCtx<D> Add(FieldEditCtx<D> fieldDef)
    {
        Fields.Add(fieldDef);
        return fieldDef;
    }
    public List<FieldEditCtx<D>> VisibleFields
    {
        get
        {
            var list = new List<FieldEditCtx<D>>();
            foreach (var field in Fields)
            {
                if (field.IsVisible) list.Add(field);
            }
            return list;
        }
    }
    public FormFields FormFields 
    { 
        get
        {
            FormFields formFields = FormFields.Rent(data.FieldCount);
            foreach (var field in Fields)
            {
                if (field.IsVisible)
                {
                    if (field.FormRow < 0 || field.FormRow >= data.FieldCount) field.DoFormRowSet(field.Field);
                    if (field.FormCol < 0) field.DoFormColSet(0);
                    if (field.FormColSpan < 0 || field.FormColSpan > 12) field.DoFormColSpanSet(12);
                    var formField = FormField.Rent(field.Field,field.FormRow,field.FormCol,field.FormColSpan);
                    formFields.Rows[field.FormRow].Add(formField);
                }
            }
            foreach (var row in formFields.Rows)
            {
                row.Sort((x, y) => x.Col.CompareTo(y.Col));
            }
            return formFields;
        }
      
    }

    //public FieldDef<D> Add(string fieldName)
    //{
    //    int field = rec.FieldIdFromName(fieldName);
    //    if (field < 0) throw new Exception($"Field {fieldName} not found");
    //    var info = FieldDef<D>.Rent(field,fieldName);
    //    Fields.Add(info);
    //    return info;
    //}
    public int TableWidth
    {
        get
        {
            int width = 0;
            foreach (var field in Fields)
            {
                if (field.IsVisible) width += field.ColWidth;
            }
            return width;
        }
    }


    private static DataEditSettings<D> Create()
    {
        return new DataEditSettings<D>();
    }
    private static readonly Pool<DataEditSettings<D>> pool = new(Create);
    public static DataEditSettings<D> Rent()
    {
        return pool.Rent();
    }
    public static string PoolStats => pool.Stats;
    public static void PoolClear() => pool.Clear();
    ~DataEditSettings()
    {
        Dispose();
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var field in Fields)
        {
            field.Dispose();
        }
        Fields.Clear();
        SortDefs.Clear();
        FilterDefs.Clear();
        pool.Return(this);
    }
}