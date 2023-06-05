using System.Net.NetworkInformation;
using WFLib;

namespace WFLib;

public class FieldEditCtx<D> : IDisposable where D : Data, new()
{
    //private constructor to force use of Rent()
    private FieldEditCtx() { }

    //keep a copy of the record definition 
    //D data;

    // Public Interface
    public int Order { get; private set; }
    public int Field { get; private set; }
    public string FieldName { get; private set; }
    public string FieldLabel { get; private set; } 
    public string HeaderText { get; private set; }
    public Type Type { get; private set; }
    public string TypeName { get; private set; } 
    public bool AllowFiltering { get; private set; }
    public bool AllowSorting { get; private set; }
    public bool AllowEditing { get; private set; }
    public SORTSTATE SortState { get; private set; }
    public bool IsFilterSet { get; private set; }
    public string FilterValue { get; private set; }
    public bool IsVisible { get; set; }
    public int ColWidth { get; set; }
    public Object Editor { get; set; }
    //public IEditFieldHelper<D> EditFieldHelper { get; set; }
    public event Action<bool> OnValidate;
    private string _ValidationMessage;
    public string ValidationMessage => _ValidationMessage;
    /// <summary>
    /// This function calls the FieldValidate for this field, and will invoke the OnValidate event
    /// </summary>
    /// <param name="data">Pass the Data of the field to validate </param>
    /// <returns></returns>
    public bool Validate(D data)  
    { 
        var isValid = data.FieldValidate(Field,out _ValidationMessage);
        OnValidate?.Invoke(isValid);
        return isValid; 
    }
    public bool HasValidation(D data) => data.FieldHasValidation(Field);
    /// <summary>
    /// Same as the Validate function expept it does not invoke the OnValidate event
    /// </summary>
    /// <param name="data">Pass the Data of the field to validate </param>
    /// <returns></returns>
    public bool IsValid(D data)
    {
        return data.FieldValidate(Field, out _ValidationMessage);
    }
    public string AsString(D data)
    {
        return data.FieldAsString(Field);
    }
    public object AsObject(D data)
    {
        return data.FieldAsObject(Field);
    }
    public void FromString(D data, string value)
    {
        data.FieldFromString(value, Field);
    }
    public void FromObject(D data, object value)
    {
        data.FieldFromObject(value, Field);
    }
    public int FormRow { get; private set; }
    public int FormCol { get; private set; }
    public int FormColSpan { get; private set; }

    static D _data = new();
    private void InitDefaultValues(int field)
    {
        
        Field = field;
        FieldName = _data.FieldName(field);
        HeaderText = _data.FieldColumnLabel(field);
        FieldLabel = _data.FieldLabel(field);
        Type = _data.FieldType(field);
        TypeName = _data.FieldTypeName(field);
        AllowFiltering = true;
        AllowSorting = true;
        AllowEditing = true;
        SortState = SORTSTATE.NONE;
        IsFilterSet = false;
        FilterValue = "";
        IsVisible = true;
        ColWidth = 100;
        Order = 0;
        FormRow = -1;
        FormCol = -1;
        FormColSpan = 12;
    }

    public FieldEditCtx<D> DoFormRowSet(int row)
    {
        if (row < 0) row = Field;
        if (row >= _data.FieldCount) row = _data.FieldCount-1;
        FormRow = row;
        return this;
    }
    public FieldEditCtx<D> DoFormColSet(int col)
    {
        if (col < 0) col = 0;
        if (col > 11) col = 11; 
        FormCol = col;
        return this;
    }
    public FieldEditCtx<D> DoFormColSpanSet(int colSpan)
    {
        if (colSpan < 1) colSpan = 1;
        if (colSpan > 12) colSpan = 12; //bootstrap colspan range 1-12
        FormColSpan = colSpan;
        return this;
    }
    public FieldEditCtx<D> DoOrderSet(int order)
    {
        Order = order;
        return this;
    }
    public FieldEditCtx<D> DoColWidthSet(int colWidth)
    {
        ColWidth = colWidth;
        return this;
    }
    public FieldEditCtx<D> DoHeaderTextSet(string headerText)
    {
        HeaderText = headerText;
        return this;
    }
    public FieldEditCtx<D> DoFieldLabelSet(string labelText)
    {
        FieldLabel = labelText;
        return this;
    }
    public FieldEditCtx<D> DoShow()
    {
        IsVisible = true;
        return this;
    }
    public FieldEditCtx<D> DoHide()
    {
        IsVisible = false;
        return this;
    }
    public FieldEditCtx<D> DoFilterSet(string filterValue)
    {
        if (AllowFiltering == false)
        {
            IsFilterSet = false;
            return this;
        }
        if (string.IsNullOrEmpty(filterValue))
        {
            IsFilterSet = false;
            return this;
        }
        IsFilterSet = true;
        FilterValue = filterValue;
        return this;
    }
    public FieldEditCtx<D> DoFilterEnable()
    {
        AllowFiltering = true;
        return this;
    }
    public FieldEditCtx<D> DoFilterDisable()
    {
        AllowFiltering = false;
        return this;
    }
    public FieldEditCtx<D> DoSortSet(SORTSTATE sortState)
    {
        SortState = sortState;
        return this;
    }
    public FieldEditCtx<D> DoSortEnable()
    {
        AllowSorting = true;
        return this;
    }
    public FieldEditCtx<D> DoSortDisable()
    {
        AllowSorting = false;
        return this;
    }
    public FieldEditCtx<D> DoSortToggle()
    {
        if (AllowSorting == false)
        {
            SortState = SORTSTATE.NONE;
            return this;
        }
        int sortState = (int)SortState;
        sortState++;
        if (sortState > 2) sortState = 0;
        SortState = (SORTSTATE)sortState;
        return this;
    }
    public FieldEditCtx<D> DoEditEnable()
    {
        AllowEditing = true;
        return this;
    }
    public FieldEditCtx<D> DoEditDisable()
    {
        AllowEditing = false;
        return this;
    }



    ~FieldEditCtx()
    {
        //We forgot to call Dispose, Pool it anyway
        Dispose();
    }
    public void Dispose()
    {
        InitDefaultValues(0);
        pool.Return(this);
    }
    private static FieldEditCtx<D> Create()
    {
        return new FieldEditCtx<D>();
    }
    private static Pool<FieldEditCtx<D>> pool = new(Create);

    public static FieldEditCtx<D> Rent(int field)
    {
        
        var fieldDef = pool.Rent();
        fieldDef.InitDefaultValues(field);
        return fieldDef;
    }
    public static string PoolStats => pool.Stats;
    public static void PoolClear() => pool.Clear();
}