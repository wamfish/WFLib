//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class ByteExtensions
{
    public static string AsString(this byte val)
    {
        return Convert.ToString(val);
    }
    public static string AsString(this byte[] val, int len)
    {
        return Encoding.UTF8.GetString(val, 0, len);
    }
    public static string AsString(this byte[] val, int start, int len)
    {
        if (val.Length < 1 || len < 1)
            return "";
        return Encoding.UTF8.GetString(val, start, len);
    }
    public static string AsString(this byte[] val)
    {
        return Encoding.UTF8.GetString(val);
    }
    public static char AsChar(this byte val)
    {
        return Convert.ToChar(val);
    }
    public static byte AsByte(this String value)
    {
        return (byte)value.AsInt();
    }
    public static void MakeKey(this byte val, byte[] keydata, int pos)
    {
        keydata[pos] = val;
    }
}
