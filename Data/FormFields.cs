namespace WFLib;
public class FormFields : IDisposable
{
    private FormFields() {}
    public readonly List<List<FormField>> Rows = new();
    private static FormFields Create()
    {
        return new FormFields();
    }
    private static readonly Pool<FormFields> pool = new(Create);
    public static FormFields  Rent(int fieldCount)
    {
        var formFields = pool.Rent();
        for(int i=0;i<fieldCount;i++)
        {
            formFields.Rows.Add(FormFieldListFactory.Rent());
        }
        return formFields;
    }
    public static string PoolStats => pool.Stats;
    public static void PoolClear() => pool.Clear();
    ~FormFields()
    {
        Dispose();
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var list in Rows)
        {
            FormFieldListFactory.Return(list);
        }
        Rows.Clear();
        pool.Return(this);
    }
}
