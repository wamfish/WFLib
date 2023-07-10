//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;

public static class DataFactory<D> where D : Data, new()
{
    static readonly Queue<D> pool = new();
    static int PoolCount()
    {
        lock (pool)
        {
            return pool.Count;
        }
    }
    static readonly PoolStats stats = new($"Data: {typeof(D).Name}", PoolCount);
    public static D Rent()
    {
        D data;
      
        lock (pool)
        {
            stats.RentCount++;
            if (pool.Count > 0)
            {
                stats.RentFromPoolCount++;
                data = pool.Dequeue();
            }
            else
            {
                stats.RentFromNewCount++;
                data = new D();
            }
        }
        return data;
    }
    public static void Return(D data)
    {
        lock (pool)
        {
            stats.ReturnCount++;
            data.Init();
            pool.Enqueue(data);
        }
    }
}
