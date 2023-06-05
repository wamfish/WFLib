namespace WFLib;

public class FilterDef
{
    public int Field;
    public string Filter;
    #region pool
    private FilterDef() { }
    private static FilterDef Create()
    {
        return new FilterDef();
    }
    private static Pool<FilterDef> pool = new(Create);
    public static FilterDef Rent(int field, string filter)
    {
        var dl = pool.Rent();
        dl.Field = field;
        dl.Filter = filter;
        return dl;
    }
    public static string PoolStats => pool.Stats;
    public static void PoolClear() => pool.Clear();
    public void Return() => Dispose();
    public void Dispose()
    {
        pool.Return(this);
    }
    #endregion


}
