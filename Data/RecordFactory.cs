//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;
public static class RecordFactory<R> where R : Record, new()
{
    static readonly Queue<R> pool = new();
    static int PoolCount()
    {
        lock (pool)
        {
            return pool.Count;
        }
    }
    static readonly PoolStats stats = new(typeof(R).Name, PoolCount);
    public static R Rent()
    {
        lock (pool)
        {
            stats.RentCount++;
            if (pool.Count > 0)
            {
                stats.RentFromPoolCount++;
                return pool.Dequeue();
            }
            stats.RentFromNewCount++;
            return new R();
        }

    }
    public static void Return(R record)
    {
        lock (pool)
        {
            stats.ReturnCount++;
            record.Init(); //set to the default state
            pool.Enqueue(record);
        }
    }
}