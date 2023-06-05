public static class FormFieldListFactory 
{
    static readonly Queue<List<FormField>> pool = new();
    static int PoolCount()
    {
        lock (pool)
        {
            return pool.Count;
        }
    }
    static readonly PoolStats stats = new( nameof(FormFieldListFactory), PoolCount);
    public static List<FormField> Rent()
    {
        List<FormField> list;
        lock (pool)
        {
            stats.RentCount++;
            if (pool.Count > 0)
            {
                stats.RentFromPoolCount++;
                list = pool.Dequeue();
            }
            else
            {
                stats.RentFromNewCount++;
                list = new List<FormField>();
            }
        }
        return list;
    }
    public static void Return(List<FormField> list)
    {
        lock (pool)
        {
            stats.ReturnCount++;
            foreach (var field in list)
            {
                field.Dispose();
            }
            list.Clear();
            pool.Enqueue(list);
        }
    }
}