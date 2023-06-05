//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;

/// <summary>
/// 
/// A Wrapper for many of the RecordContext functions. If you are doing
/// high volume work, you can use the RecordContext directly. This class
/// gives you ease of use at the expense of a small bit of performance.
///
/// </summary>

// Note: I considered using the source generator to add these methods to the Record. I decided
// against it because I wanted to keep the Record class as simple as possible. I also like the
// idea of having a separate class for these related functions. 

public abstract class RecordUtil<R> where R : Record, new()
{
    public static Status Read(R rec, int id,int EditByID=-2)
    {
        using var context = RentContext(EditByID);
        return context.Read(rec, id);
    }
    public static Status Add(R rec, bool useNextId = false, int EditByID = -2)
    {
        using var context = RentContext(EditByID);
        return context.Add(rec, useNextId);
    }
    public static Status Update(R rec, int EditByID = -2)
    {
        using var context = RentContext(EditByID);
        return context.Update(rec);
    }
    public static Status Delete(R rec, int EditByID=-2)
    {
        using var context = RentContext(EditByID);
        return context.Delete(rec);
    }
    public static RecordContext<R> RentContext(int EditByID=-2)
    {
        return RecordContextFactory<R>.Rent(EditByID);
    }
    public static R RentRecord() => RecordFactory<R>.Rent();
    public static void ReturnRecord(R rec) => RecordFactory<R>.Return(rec);
    public static bool RecIsDirty(R rec, R origRec)
    {
        for (int i = 0; i < rec.FieldCount; i++)
        {
            if (!rec.FieldIsEqual(origRec, i)) return true;
        }
        return false;
    }
}
