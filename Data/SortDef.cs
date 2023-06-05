namespace WFLib;

public class SortDef
{
    public int Field;
    public bool Ascending = true;
    #region pool
    private SortDef() { }
    private static SortDef Create()
    {
        return new SortDef();
    }
    private static Pool<SortDef> pool = new(Create);
    public static SortDef Rent(int field, bool ascending=true)
    {
        var item = pool.Rent();
        item.Field = field;
        item.Ascending = ascending;
        return item;
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
