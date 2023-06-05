//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class IntExtensions
{
    public static string RightJust(this int val, int width)
    {
        long lval = val;
        return lval.RightJust(width);
    }
    public static string AsString(this int val)
    {
        return Convert.ToString(val);
    }
    public static char AsChar(this string val)
    {
        if (!char.TryParse(val, out char r)) r = (char)0;
        return r;
    }
    public static char AsChar(this int val)
    {
        return (char)val;
    }

    public static int AsInt(this ReadOnlySpan<char> val)
    {
        if (!int.TryParse(val, out int r)) r = 0;
        return r;
    }

    public static int AsInt(this string val) => val.AsSpan().AsInt();

    public static int AsIntOrNeg(this string val)
    {
        if (Int32.TryParse(val, out int result))
        {
            return result;
        }
        return -1;
    }

    public static uint AsUInt(this ReadOnlySpan<char> val)
    {
        if (!uint.TryParse(val, out uint r)) r = 0;
        return r;
    }
    public static uint AsUInt(this string val) => val.AsSpan().AsUInt();

    public static string RightJust(this uint val, int width)
    {
        long lval = val;
        return lval.RightJust(width);
    }
    public static string AsString(this uint val)
    {
        return Convert.ToString(val);
    }
    public static void AsKey(this int val, SerializationBuffer sb)
    {
        sb.Write(val);
    }
    public static string AsDisplay(this int val)
    {
        return val.ToString("#,0.##");
    }
    public static void MakeKey(this int val, byte[] keydata, int pos)
    {
#if NETSTANDARD || NET472
        var data = BitConverter.GetBytes(val);
        Buffer.BlockCopy(data,0,keydata,pos,4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(keydata, pos, 4);
#else
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos, 4), val))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 4);
        }
#endif
    }
    public static void MakeKey(this uint val, byte[] keydata, int pos)
    {
#if NETSTANDARD || NET472
        var data = BitConverter.GetBytes(val);
        Buffer.BlockCopy(data,0,keydata,pos,4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(keydata, pos, 4);
#else
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos, 4), val))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 4);
        }
#endif
    }
}
