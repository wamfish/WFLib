//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using System.Globalization;
namespace WFLib;
public enum DATETYPE : byte { Display, DisplayWithSec, DisplayWithMSec, Key, FileDate, HtmlInput }
public static class DateTimeExtensions
{
    public const string DTDisplay = "M/d/yyyy h:mm tt";
    public const string DTHtmlInput = "yyyy-MM-ddTHH:mm";
    public const string DTDisplayWithSec = "M/d/yyyy h:mm:ss tt";
    public const string DTDisplayWithMSec = "M/d/yyyy h:mm:ss.fff tt";
    public const string DTKey = "yyyyMMddHHmmssfff";
    public const string DTFileDate = "yyyyMMddHHmmss";

    public static string AsString(this DateTime value, DATETYPE dt)
    {
        switch (dt)
        {
            case DATETYPE.Display:
                return value.ToString(DTDisplay);
            case DATETYPE.DisplayWithSec:
                return value.ToString(DTDisplayWithSec);
            case DATETYPE.DisplayWithMSec:
                return value.ToString(DTDisplayWithMSec);
            case DATETYPE.Key:
                return value.ToString(DTKey);
            case DATETYPE.FileDate:
                return value.ToString(DTFileDate);
            case DATETYPE.HtmlInput:
                return value.ToString(DTHtmlInput);
        }
        return value.ToString(DTDisplay);
    }
    public static string AsString(this DateTime value)
    {
        if (value == DateTime.MinValue) return string.Empty;
        return value.ToString(DTKey);
    }
    public static DateTime AsDateTime(this ReadOnlySpan<char> val)
    {
        //"01234567890123456"
        //"YYYYMMDDHHMMSSmmm"
        if (val.Length != 17)
        {
            return DateTime.MinValue;
        }
        if (!int.TryParse(val.Slice(0, 4), out int year)) return Date.Current;
        if (!int.TryParse(val.Slice(4, 2), out int month)) return Date.Current;
        if (!int.TryParse(val.Slice(6, 2), out int day)) return Date.Current;
        if (!int.TryParse(val.Slice(8, 2), out int hour)) return Date.Current;
        if (!int.TryParse(val.Slice(10, 2), out int min)) return Date.Current;
        if (!int.TryParse(val.Slice(12, 2), out int sec)) return Date.Current;
        if (!int.TryParse(val.Slice(14, 3), out int msec)) return Date.Current;
        try
        {
            return new DateTime(year, month, day, hour, min, sec, msec, DateTimeKind.Utc);
        }
        catch (Exception ex)
        {
            LogException(ex);
            return Date.Current;
        }
    }
    public static DateTime AsDateTime(this string val)
    {
        if (val.Length != 17) return DateTime.MinValue;
        return val.AsSpan().AsDateTime();
    }
    static CultureInfo enUS = new CultureInfo("en-US");
    public static bool IsEqual(this DateTime d1, DateTime d2)
    {
        var diff = d1.Subtract(d2);
        if (diff.TotalMilliseconds >= 1 || diff.TotalMilliseconds <= -1)
            return false;
        return true;
    }
    public static void MakeKey(this DateTime val, byte[] keydata, int pos)
    {
#if NETSTANDARD || NET472
        var data = BitConverter.GetBytes(val.ToBinary());
        Buffer.BlockCopy(data,0,keydata,pos,8);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(keydata, pos, 8);
#else
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos, 8), val.ToBinary()))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 8);
        }
#endif
    }
}
