namespace WFLib;

public class FormField : IDisposable 
{
    private FormField() { }
    public int Field= -1;
    public int Row = -1;
    public int Col = -1;
    public int ColSpan = -1;
    private static FormField Create()
    {
        return new FormField();
    }
    private static readonly Pool<FormField> pool = new(Create);
    public static FormField Rent(int field, int row, int col, int colSpan)
    {
        var formField = pool.Rent();
        formField.Field = field;
        formField.Row = row;
        formField.Col = col;
        formField.ColSpan = colSpan;
        return formField;
    }
    public static string PoolStats => pool.Stats;
    public static void PoolClear() => pool.Clear();
    ~FormField()
    {
        Dispose();
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Field = -1;
        Row = -1;
        Col = -1;
        ColSpan = -1;
        pool.Return(this);
    }
}
