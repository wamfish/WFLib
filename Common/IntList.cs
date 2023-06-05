//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;
/// <summary>
/// I pooled object that can be used to store a list of ints. Example:
/// 
///     using var idlist = IntList.Rent();
///     
/// </summary>
public class IntList : IDisposable
{
    private IntList() { }
    public readonly List<int> Ints = new List<int>();
    public int this[int i]
    {
        get
        {
            if (i >= 0 && i < Ints.Count) return Ints[i];
            return -1;
        }
        set
        {
            if (i < 0 || i >= Ints.Count) throw new IndexOutOfRangeException();
            Ints[i] = value;
        }
    }
    public int Count => Ints.Count;
    private static IntList Create()
    {
        return new IntList();
    }
    private static Pool<IntList> pool = new(Create);
    /// <summary>
    /// 
    /// Use this to get an IdList. 
    /// 
    /// Example: using var idlist = IdList.Rent();
    ///     
    /// </summary>
    /// <returns> SerializationBuffer </returns>
    public static IntList Rent()
    {
        return pool.Rent();
    }
    /// <summary>
    /// Returns a string with stats about the pool
    /// </summary>
    public static string PoolStats => pool.Stats;
    /// <summary>
    /// Clears the pool
    /// </summary>
    public static void PoolClear() => pool.Clear();
    /// <summary>
    /// If it is not practical to use the using clause
    /// You can return an object to the pool with this method.
    /// The using clause is preferred.
    /// </summary>
    public void Return() => Dispose();
    public void Dispose()
    {
        Ints.Clear();
        pool.Return(this);
    }
}
