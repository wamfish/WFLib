//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.


using System.Collections;

namespace WFLib;

public class FilterList : IDisposable, IEnumerable
{
    private readonly List<FilterDef> List = new();
    public int Count => List.Count;
    public int RecordCountInTable { get; set; } = 0;
    public FilterDef this[int i]
    {
        get
        {
            if (i >= 0 && i < List.Count)
            {
                return List[i];
            }
            return null;
        }
    }
    public void Add(int field, string filter)
    {
        var fd = FilterDef.Rent(field, filter);
        List.Add(fd);
    }
    public void Clear()
    {
        for (int i = 0; i < List.Count; i++)
        {
            List[i].Dispose();
            List[i] = null;
        }
        List.Clear();
    }
    public IEnumerator GetEnumerator()
    {
        return List.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return List.GetEnumerator();
    }
    #region pool
    private FilterList() { }
    private static FilterList Create()
    {
        return new FilterList();
    }
    private static Pool<FilterList> pool = new(Create);
    public static FilterList Rent()
    {
        var item = pool.Rent();
        return item;
    }
    public static string PoolStats => pool.Stats;
    public static void PoolClear() => pool.Clear();
    public void Return() => Dispose();
    public void Dispose()
    {
        Clear();
        pool.Return(this);
    }
    #endregion
}

