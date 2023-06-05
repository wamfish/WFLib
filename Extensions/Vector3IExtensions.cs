//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class Vector3IExtensions
{
    public static bool NotZero(this Vector3I value)
    {
        if (!(value.X == 0 && value.Y == 0 && value.Z == 0)) return true;
        return false;
    }
    public static string AsString(this Vector3I value)
    {
        var sb = StringBuilderPool.Rent();
        sb.Append(value.X.ToString());
        sb.Append(VALUESEP);
        sb.Append(value.Y.ToString());
        sb.Append(VALUESEP);
        sb.Append(value.Z.ToString());
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static Vector3I AsVector3I(this ReadOnlySpan<char> value)
    {
        if (value.Length == 0) return Vector3I.Zero;
        Span<int> ints = stackalloc int[3];
        ParseInts(value, ints, VALUESEP);
        return new Vector3I(ints[0], ints[1], ints[2]);
    }
    public static Vector3I AsVector3I(this String value) => value.AsSpan().AsVector3I();
    public static void MakeKey(this Vector3I val, byte[] keydata, int pos)
    {
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos, 4), val.X))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 4);
        }
        pos += 4;
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos, 4), val.Y))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 4);
        }
        pos += 4;
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos, 4), val.Z))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 4);
        }
    }
}
