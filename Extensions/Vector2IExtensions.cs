//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class Vector2IExtensions
{
    public static bool NotZero(this Vector2I value)
    {
        if (!(value.X == 0 && value.Y == 0)) return true;
        return false;
    }
    public static string AsString(this Vector2I value)
    {
        var sb = StringBuilderPool.Rent();
        sb.Append(value.X.ToString());
        sb.Append(VALUESEP);
        sb.Append(value.Y.ToString());
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static Vector2I AsVector2I(this ReadOnlySpan<char> value)
    {
        if (value.Length == 0) return Vector2I.Zero;
        Span<int> ints = stackalloc int[2];
        ParseInts(value, ints, VALUESEP);
        return new Vector2I(ints[0], ints[1]);
    }
    public static Vector2I AsVector2I(this String value) => value.AsSpan().AsVector2I();
    public static void MakeKey(this Vector2I val, byte[] keydata, int pos)
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
    }
}
