//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class Vector2Extensions
{
    public static bool NotZero(this Vector2 value)
    {
        if (!(value.X == 0 && value.Y == 0)) return true;
        return false;
    }
    public static string AsString(this Vector2 value)
    {
        var sb = StringBuilderPool.Rent();
        sb.Append(value.X.ToString());
        sb.Append(VALUESEP);
        sb.Append(value.Y.ToString());
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static Vector2 AsVector2(this ReadOnlySpan<char> value)
    {
        if (value.Length == 0) return Vector2.Zero;
        Span<float> floats = stackalloc float[2];
        ParseFloats(value, floats, VALUESEP);
        //GD.Print($"Float0: {floats[0]} Float1: {floats[1]}");
        return new Vector2(floats[0], floats[1]);
    }
    public static Vector2 AsVector2(this String value) => value.AsSpan().AsVector2();
    public static void MakeKey(this Vector2 val, byte[] keydata, int pos)
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
