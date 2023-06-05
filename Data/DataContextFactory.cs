//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;

public static class DataContextFactory<D> where D : Data, new()
{
    static readonly Queue<DataContext<D>> pool = new();
    public static DataContext<D> Rent()
    {
        lock (pool)
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
        }
        return new DataContext<D>();
    }
    public static void Return(DataContext<D> context)
    {
        lock (pool)
        {
            pool.Enqueue(context);
        }
    }
}
