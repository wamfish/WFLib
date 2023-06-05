//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class DoubleExtensions
{
    public static string RightJust(this double val, int beforeDec, int afterDec)
    {
        string mask = Util.NumericMask(beforeDec, afterDec);
        string strVal = val.ToString(mask);
        if (strVal.Length > mask.Length) return strVal; //This is an overflow unless it is neg, but I don't want to throw away the results
        if (strVal.Length < mask.Length + 1)
        {
            strVal = strVal.PadLeft(mask.Length + 1);
        }
        return strVal;
    }
    public static string AsString(this double val)
    {
        return Convert.ToString(val);
    }
    public static int AsInt(this double val)
    {
        return (int)val;
    }
    public static double AsDouble(this ReadOnlySpan<char> value)
    {
        if (!double.TryParse(value, out double result)) result = 0;
        return result;
    }

    public static double AsDouble(this String value) => value.AsSpan().AsDouble();

    public static void MakeKey(this double val, byte[] keydata, int pos)
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
