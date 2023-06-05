//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class DecimalExtensions
{
    public static string RightJust(this decimal val, int beforeDec, int afterDec)
    {
        double dval = (double)val;
        return dval.RightJust(beforeDec, afterDec);
    }
    public static string AsString(this decimal val)
    {
        return Convert.ToString(val);
    }

    public static decimal AsDecimal(this ReadOnlySpan<char> value)
    {
        if (!decimal.TryParse(value, out decimal val)) val = 0;
        return val;
    }
    public static decimal AsDecimal(this String value)
    {
        return Convert.ToDecimal(value);
    }

    public static void MakeKey(this decimal val, byte[] keydata, int pos)
    {
        var vals = decimal.GetBits(val);
#if NETSTANDARD || NET472
        var data = BitConverter.GetBytes(vals[0]);
        Buffer.BlockCopy(data,0,keydata,pos + 0, 4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(keydata, pos + 0, 4);

        data = BitConverter.GetBytes(vals[1]);
        Buffer.BlockCopy(data,0,keydata,pos + 4, 4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(keydata, pos + 4, 4);
        
        data = BitConverter.GetBytes(vals[2]);
        Buffer.BlockCopy(data,0,keydata,pos + 8, 4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(keydata, pos + 8, 4);

        data = BitConverter.GetBytes(vals[3]);
        Buffer.BlockCopy(data,0,keydata,pos + 12, 4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(keydata, pos + 12, 4);
#else
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos + 0, 4), vals[0]))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos + 0, 4);
        }
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos + 4, 4), vals[1]))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos + 4, 4);
        }
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos + 8, 4), vals[2]))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos + 8, 4);
        }
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos + 12, 4), vals[3]))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos + 12, 4);
        }
#endif
    }
}
