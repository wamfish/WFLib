//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class RecordContextFactory<R> where R : Record, new()
{
    #region Some Nonsense 
    // Note: I wanted RecordContextFactorty to be outside of the RecordContext class.
    // I also did not want to use reflection, as I think AOT compiliation (I am not 100%) may not work with reflection?
    // So I am using a bit of code that does not feel correct, but is working. This is part of that code below. Parts also
    // in RecordContext, and in the Generated Record class. If someone reads this and has a better way, please let me know.

    // Here is the flow of how this works:

    // RecordContextFactory<R>.Rent() will create a Record rec and then call rec.InitContextFactory()
    // InitContextFactory refrences a static member of RecordContext<R> so that the static Initializer for RecordContext<R> will be called.
    // The static Initializer for RecordContext<R> will call RecordContextFactory<R>.SetCreateMethod() with helper function that allow
    // RecordContextFactory<R>.Rent() to create a new RecordContext<R> and set the EditByID value.

    // This is just ugly, ugly, ugly! I will admit that working with generics is not my strong suit. I am sure there must be a better way.
    
    private static Action<RecordContext<R>,int> setEditByIDMethod;
    private static Func<RecordContext<R>> createMethod;
    public static void SetCreateMethod(Func<RecordContext<R>> creatMethodArg, Action<RecordContext<R>, int> setEditByID)
    {
        createMethod = creatMethodArg;
        setEditByIDMethod = setEditByID;
    }
    #endregion
    
    static Queue<RecordContext<R>> pool = new();
    static int PoolCount()
    {
        lock (pool)
        {
            return pool.Count;
        }
    }
    public static readonly PoolStats stats = new(typeof(R).Name, PoolCount);
    public static RecordContext<R> Rent(int EditByID=-2)
    {
        lock (pool)
        {
            if (stats.RentCount == 0)
            {
                var r = new R();
                r.InitContextFactory();
                RecordFactory<R>.Return(r);
            }
            stats.RentCount++;
            RecordContext<R> ctx;
            if (pool.Count > 0)
            {
                stats.RentFromPoolCount++;
                ctx = pool.Dequeue();
            }
            else
            {
                stats.RentFromNewCount++;
                ctx = createMethod();
            }
            setEditByIDMethod(ctx, EditByID);
            return ctx;
        }
    }
    public static void Return(RecordContext<R> ctx)
    {
        lock (pool)
        {
            stats.ReturnCount++;
            ctx.Close();
            //Console.WriteLine("StackTrace: '{0}'", GetStackTrace(4));
            pool.Enqueue(ctx);
        }
    }
}
