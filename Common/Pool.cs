//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;

/// <summary>
/// 
///  This is a helper class for implementing object pooling in your class. Below is an example 
///  of what to add to your class in order to implement pooling. You should also make your constructor 
///  private. Making the constructor private will insure the user needs to call Rent in order to
///  get an instance of your class.
///  
///  private static Pool<YourClass> pool = new(()=>new YourClass());
///  public static YourClass Rent()
///  {
///     var obj = pool.Rent();
///     return obj;
///  }
///  public static string PoolStats => pool.Stats;
///  public static void PoolClear() => pool.Clear();
///  public void Return() => pool.Return(this);
///  public void Dispose()
///  {
///     pool.Return(this);
///  }
/// 
/// </summary>
/// <typeparam name="T"> The type of the object to be pooled </typeparam>
public class Pool<T>
{
    PoolStats stats;
    Func<T> newObjFunc;
    public Pool(Func<T> newObjFuncArg)
    {
        newObjFunc = newObjFuncArg;
        stats = new(typeof(T).Name, () => { return pool.Count; });
    }
    Queue<T> pool = new();
    /// <summary>
    /// When renting this object use this syntax:
    /// 
    ///     using var obj = TestObjectPooling.Rent();
    ///     
    /// The object will be pooled for reuse when disposed
    /// </summary>
    /// <returns> TestObjectPooling </returns>
    public T Rent()
    {
        T obj;
        lock (pool)
        {
            inClear = false;
            stats.RentCount++;
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
                stats.RentFromPoolCount++;
            }
            else
            {
                obj = newObjFunc();
                stats.RentFromNewCount++;
            }
        }
        return obj;
    }
    public void Return(T obj)
    {
        if (inClear) return; //let the gc do its thing
        lock (pool)
        {
            stats.ReturnCount++;
            if (pool.Contains(obj))
            { 
                Console.WriteLine(Stats);
            }
            pool.Enqueue(obj);
        }
    }
    /// <summary>
    /// Returns a string with stats about the pool
    /// </summary>
    public string Stats => stats.Stats;
    public string HtmlStats => stats.HtmlStats;
    private bool inClear = false;
    public void Clear()
    {
        lock (pool)
        {
            inClear = true;
            pool.Clear();
        }
    }
}

public class PoolStats
{
    static List<PoolStats> stats = new();
    private static List<string> StatsList(string lineSep)
    {
        List<string> statsList = new();
        lock (stats)
        {
            foreach (var s in stats)
            {
                statsList.Add(s.BuildStats(lineSep));
            }
        }
        return statsList;
    }
    public static List<string> AllStats => StatsList("");
    public static List<string> AllHtmlStats => StatsList("<br/>");
    public string Stats => BuildStats("");
    public string HtmlStats => BuildStats("<br/>");
    private string BuildStats(string lineSep)
    {
        var sb = StringBuilderPool.Rent();
        sb.AppendLine($"{Name} Stats:{lineSep}");
        if (PoolCount != null)
        {
            sb.AppendLine($"Pool Count: {PoolCount()}{lineSep}");
        }
        sb.AppendLine($"Rent Count: {RentCount}{lineSep}");
        sb.AppendLine($"Rent From Pool: {RentFromPoolCount}{lineSep}");
        sb.AppendLine($"Rent From New: {RentFromNewCount}{lineSep}");
        sb.AppendLine($"Return count: {ReturnCount}{lineSep}");
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public readonly string Name;
    public readonly Func<int> PoolCount;
    public PoolStats(string name, Func<int> poolCount)
    {
        Name = name;
        PoolCount = poolCount;
        lock (stats)
        {
            stats.Add(this);
        }
    }
    public long RentCount = 0;
    public long RentFromPoolCount = 0;
    public long RentFromNewCount = 0;
    public long ReturnCount = 0;
}
