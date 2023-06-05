//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class ShortExtensions
{
    public static string RightJust(this short val, int width)
    {
        long lval = val;
        return lval.RightJust(width);
    }
    public static string AsString(this short val)
    {
        return Convert.ToString(val);
    }
    public static short AsShort(this ReadOnlySpan<char> value)
    {
        if (!short.TryParse(value, out short val)) val = 0;
        return val;
    }
    public static short AsShort(this String value)
    {
        return Convert.ToInt16(value);
    }
    public static ushort AsUShort(this ReadOnlySpan<char> value)
    {
        if (!ushort.TryParse(value, out ushort val)) val = 0;
        return val;
    }
    public static ushort AsUShort(this String value)
    {
        return Convert.ToUInt16(value);
    }
    public static string RightJust(this ushort val, int width)
    {
        long lval = val;
        return lval.RightJust(width);
    }
    public static string AsString(this ushort val)
    {
        return Convert.ToString(val);
    }
    public static void MakeKey(this short val, byte[] keydata, int pos)
    {
#if NETSTANDARD || NET472 
        var data = BitConverter.GetBytes(val);
        Buffer.BlockCopy(data,0,keydata,pos,2);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(keydata, pos, 2);
#else
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos, 2), val))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 2);
        }
#endif
    }
    public static void MakeKey(this ushort val, byte[] keydata, int pos)
    {
#if NETSTANDARD || NET472
            var data = BitConverter.GetBytes(val);
            Buffer.BlockCopy(data,0,keydata,pos,2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 2);

#else
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos, 2), val))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 2);
        }
#endif

    }
}
