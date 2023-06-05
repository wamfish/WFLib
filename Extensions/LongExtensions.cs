//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class LongExtensions
{
    public static string AsDisplay(this long val)
    {
        return val.ToString("#,0.##");
    }
    public static string RightJust(this long val, int width)
    {
        string mask = Util.NumericMask(width, 0);
        string strVal = val.ToString(mask);
        if (strVal.Length > mask.Length) return strVal; //This is an overflow unless it is neg, but I don't want to throw away the results
        if (strVal.Length < mask.Length + 1)
        {
            strVal = strVal.PadLeft(mask.Length + 1);
        }
        return strVal;
    }
    public static string AsString(this long val)
    {
        return Convert.ToString(val);
    }

    public static long AsLong(this ReadOnlySpan<char> value)
    {
        if (!long.TryParse(value, out long r)) r = 0;
        return r;
    }
    public static long AsLong(this String value)
    {
        return Convert.ToInt64(value);
    }

    public static ulong AsULong(this ReadOnlySpan<char> value)
    {
        if (!ulong.TryParse(value, out ulong r)) r = 0;
        return r;
    }

    public static ulong AsULong(this String value)
    {
        return Convert.ToUInt64(value);
    }


    public static string RightJust(this ulong val, int width)
    {
        string mask = Util.NumericMask(width, 0);
        string strVal = val.ToString(mask);
        if (strVal.Length > mask.Length) return strVal; //This is an overflow unless it is neg, but I don't want to throw away the results
        if (strVal.Length < mask.Length + 1)
        {
            strVal = strVal.PadLeft(mask.Length + 1);
        }
        return strVal;
    }
    public static string AsString(this ulong val)
    {
        return Convert.ToString(val);
    }
    public static void MakeKey(this long val, byte[] keydata, int pos)
    {
#if NETSTANDARD || NET472
        var data = BitConverter.GetBytes(val);
        Buffer.BlockCopy(data,0,keydata,pos,8);
        if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 8);

#else
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos, 8), val))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 8);
        }
#endif
    }
    public static void MakeKey(this ulong val, byte[] keydata, int pos)
    {
#if NETSTANDARD || NET472
        var data = BitConverter.GetBytes(val);
        Buffer.BlockCopy(data,0,keydata,pos,8);
        if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 8);
#else
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos, 8), val))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 8);
        }
#endif
    }
}
