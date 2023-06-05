//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class FloatExtensions
{
    public static string RightJust(this float val, int beforeDec, int afterDec)
    {
        double dval = val;
        return dval.RightJust(beforeDec, afterDec);
    }
    public static string AsString(this float val)
    {
        return Convert.ToString(val);
    }

    public static float AsFloat(this ReadOnlySpan<char> value)
    {
        if (!float.TryParse(value, out var val)) val = 0f;
        return val;
    }

    public static float AsFloat(this String value) => value.AsSpan().AsFloat();

    public static void MakeKey(this float val, byte[] keydata, int pos)
    {
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos, 4), val))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 4);
        }
    }
}
