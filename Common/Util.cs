//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using System.Security.Cryptography;
namespace WFLib;
public static partial class Util
{
    public static string GetParentDir(string dir)
    {
        string path = dir;
        if (path.EndsWith("\\") || path.EndsWith("/"))
            path = System.IO.Path.GetDirectoryName(path);
        path = System.IO.Path.GetDirectoryName(path);
        return path;
    }
    public static void Reverse(this Span<byte> span)
    {
        if (BitConverter.IsLittleEndian)
            span.Reverse<byte>();
    }
    public static void Reverse(byte[] data)
    {
        if (BitConverter.IsLittleEndian)
            Array.Reverse(data);
    }
    public static void Reverse(byte[] data, int size)
    {
        if (BitConverter.IsLittleEndian)
            Array.Reverse(data, 0, size);
    }
    public static Func<float, float> WidthToInches;
    public static Func<float, float> WidthFromInches;
    public static Func<float, float> HeightToInches;
    public static Func<float, float> HeightFromInches;
    public const char SplitChar = ':';
    public const char SpaceChar = ' ';
    public static readonly char[] SplitChars = { SplitChar };
    public static readonly char[] SplitOnSpace = { SpaceChar };
    static float DefWidthToInches(float w)
    {
        return w / 90f;

    }
    static float DefWidthFromInches(float i)
    {
        return 90 * i;
    }
    static Util()
    {

        WidthToInches = DefWidthToInches;
        WidthFromInches = DefWidthFromInches;
        HeightToInches = DefWidthToInches;
        HeightFromInches = DefWidthFromInches;
    }
    public static string PasswordEncrypt(string password)
    {
        var provider = MD5.Create();
        string salt = "!23jk09$%6_231ffxcdagl";
        byte[] bytes = provider.ComputeHash(Encoding.UTF32.GetBytes(salt + password));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
    //public static void WriteRecordListAsJSON<R>(StringBuilder sb, List<R> list) where R : Record, new()
    //{
    //    R r = new R();
    //    var rs = r.CreateFieldset();
    //    sb.Append('[');
    //    for (int i = 0; i < list.Count; i++)
    //    {
    //        if (i > 0)
    //        {
    //            sb.Append(',');
    //        }
    //        list[i].ToJSON(rs, sb);
    //    }
    //    sb.Append(']');
    //}
    public static void WriteEnumListAsJSON<E>(StringBuilder sb, List<E> list) where E : Enum
    {
        sb.Append("[");
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(",");
            }
            Type t = list[i].GetType();
            byte val = (byte)Enum.Parse(t, list[i].ToString());
            sb.Append(val.ToString());
        }
        sb.Append("]");
    }
    public static void WriteListAsJSON<T>(StringBuilder sb, List<T> list)
    {
        sb.Append("[");
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(",");
            }
            sb.Quote(list[i].ToString());
        }
        sb.Append("]");
    }
    public static void WriteNumericListAsJSON<T>(StringBuilder sb, List<T> list)
    {
        sb.Append("[");
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(",");
            }
            sb.Append(list[i]);
        }
        sb.Append("]");
    }
    public static void WriteVector2AsJSON(StringBuilder sb, Vector2 val)
    {
        sb.Append("{");
        sb.NumericItemFirst("X", val.X.ToString());
        sb.NumericItem("Y", val.Y.ToString());
        sb.Append("}");
    }
    public static void WriteVector2ListAsJSON(StringBuilder sb, List<Vector2> list)
    {
        sb.Append("[");
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
                sb.Append(",");
            WriteVector2AsJSON(sb, list[i]);
        }
        sb.Append("]");
    }
    public static void WriteVector3AsJSON(StringBuilder sb, Vector3 val)
    {
        sb.Append("{");
        sb.NumericItemFirst("X", val.X.ToString());
        sb.NumericItem("Y", val.Y.ToString());
        sb.NumericItem("Z", val.Z.ToString());
        sb.Append("}");
    }
    public static void WriteVector3ListAsJSON(StringBuilder sb, List<Vector3> list)
    {
        sb.Append("[");
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
                sb.Append(",");
            WriteVector3AsJSON(sb, list[i]);
        }
        sb.Append("]");
    }
    public static void WriteQuaternionAsJSON(StringBuilder sb, Quaternion val)
    {
        sb.Append("{");
        sb.NumericItemFirst("X", val.X.ToString());
        sb.NumericItem("Y", val.Y.ToString());
        sb.NumericItem("Z", val.Z.ToString());
        sb.NumericItem("W", val.W.ToString());
        sb.Append("}");
    }
    public static void WriteQuaternionListAsJSON(StringBuilder sb, List<Quaternion> list)
    {
        sb.Append("[");
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
                sb.Append(",");
            WriteQuaternionAsJSON(sb, list[i]);
        }
        sb.Append("]");
    }
    const char charNumber = '#';
    const char char0 = '0';
    static string[] numericMask = new string[256];
    public static string NumericMask(int beforeDec, int afterDec)
    {
        if (beforeDec > 15) beforeDec = 15;
        if (afterDec > 15) afterDec = 15;
        int index = beforeDec & (afterDec >> 4);
        if (numericMask[index] == null)
        {
            if (afterDec == 0)
            {
                if (beforeDec == 0)
                {
                    numericMask[index] = "#########0";
                }
                else
                {
                    if (beforeDec == 1)
                    {
                        numericMask[index] = "0";
                    }
                    else
                    {
                        numericMask[index] = new string(charNumber, beforeDec - 1) + "0";
                    }
                }
            }
            else if (beforeDec == 0)
            {
                numericMask[index] = "." + new string(char0, afterDec);

            }
            else
            {
                if (beforeDec == 1)
                {
                    numericMask[index] = "0." + new string(char0, afterDec);
                }
                else
                {
                    numericMask[index] = new string(charNumber, beforeDec - 1) + "0." + new string(char0, afterDec); ;
                }
            }
        }
        return numericMask[index];
    }
    public static int CalcMSec(DateTime start, DateTime stop)
    {
        TimeSpan diff = stop - start;
        int msec = (int)diff.TotalMilliseconds;
        return msec;
    }
    public static class Random
    {
        static System.Random ran = new System.Random();
        public static int Integer(int max)
        {
            return ran.Next(max);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteLong(long val, Span<byte> to, int pos = 0)
    {
        const int bytes = sizeof(long);
        if (pos < 0 || to.Length < bytes + pos) throw new Exception();
        Span<byte> temp = stackalloc byte[bytes];
        BitConverter.TryWriteBytes(temp, val);
        if (BitConverter.IsLittleEndian)
        {
            to[pos + 0] = temp[7];
            to[pos + 1] = temp[6];
            to[pos + 2] = temp[5];
            to[pos + 3] = temp[4];
            to[pos + 4] = temp[3];
            to[pos + 5] = temp[2];
            to[pos + 6] = temp[1];
            to[pos + 7] = temp[0];
        }
        else
        {
            to[pos + 0] = temp[0];
            to[pos + 1] = temp[1];
            to[pos + 2] = temp[2];
            to[pos + 3] = temp[3];
            to[pos + 4] = temp[4];
            to[pos + 5] = temp[5];
            to[pos + 6] = temp[6];
            to[pos + 7] = temp[7];
        }
        return;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadLong(ReadOnlySpan<byte> from, int pos = 0)
    {
        const int bytes = sizeof(long);
        if (pos < 0 || from.Length < bytes + pos) throw new Exception();
        Span<byte> temp = stackalloc byte[bytes];
        if (BitConverter.IsLittleEndian)
        {
            int fi = pos + bytes - 1;
            temp[0] = from[fi - 0];
            temp[1] = from[fi - 1];
            temp[2] = from[fi - 2];
            temp[3] = from[fi - 3];
            temp[4] = from[fi - 4];
            temp[5] = from[fi - 5];
            temp[6] = from[fi - 6];
            temp[7] = from[fi - 7];
        }
        else
        {
            temp[0] = from[pos + 0];
            temp[1] = from[pos + 1];
            temp[2] = from[pos + 2];
            temp[3] = from[pos + 3];
            temp[4] = from[pos + 4];
            temp[5] = from[pos + 5];
            temp[6] = from[pos + 6];
            temp[7] = from[pos + 7];
        }
        return BitConverter.ToInt64(temp);
    }
}
