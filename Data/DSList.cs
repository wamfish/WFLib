//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.


using System.Collections;

namespace WFLib;

/// <summary>
/// This class is used to return a list of Data objects from a DataProvider. It is
/// a readonly list of Data objects that are copies of the Data objects in the DataProvider.
/// To update the Data objects in the DataProvider, you must call the Update methods 
/// of the DataProvider. If you update the data provider with Add or Delete calls
/// then you should get a new list from the DataProvider's Read method.
/// </summary>
/// <typeparam name="D"></typeparam>

public class DSList<D> : IDisposable, IEnumerable<D> where D : Data, new()
{
    private readonly List<D> List = new();
    public int Count => List.Count;
    public int RecordCountInTable { get; set; } = 0;
    public D this[int i]
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
    private void Clear()
    {
        for (int i = 0; i < List.Count; i++)
        {
            List[i].Dispose();
            List[i] = null;
        }
        List.Clear();
    }
    /// <summary>
    /// This method should only be called by DataProvider
    /// </summary>
    /// <returns></returns>
    public List<D> _GetList()
    {
        return List;
    }
    public IEnumerator<D> GetEnumerator()
    {
        return List.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return List.GetEnumerator();
    }


    #region pool
    private DSList() { }
    private static DSList<D> Create()
    {
        return new DSList<D>();
    }
    private static Pool<DSList<D>> pool = new(Create);
    public static DSList<D> Rent()
    {
        var dl = pool.Rent();
        return dl;
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


