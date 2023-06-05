//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class Vector3Extensions
{
    public static bool NotZero(this Vector3 value)
    {
        if (!(value.X == 0 && value.Y == 0 && value.Z == 0)) return true;
        return false;
    }
    public static bool IsVeryClose(this Vector3 v0, Vector3 v1)
    {
        float dif = MathF.Abs(v0.X - v1.X);
        if (dif > .0001) return false;
        dif = MathF.Abs(v0.Y - v1.Y);
        if (dif > .0001) return false;
        dif = MathF.Abs(v0.Z - v1.Z);
        if (dif > .0001) return false;
        return true;
    }
    public static string AsString(this Vector3 value)
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
    public static Vector3 AsVector3(this ReadOnlySpan<char> value)
    {
        if (value.Length == 0) return Vector3.Zero;
        Span<float> floats = stackalloc float[3];
        ParseFloats(value, floats, VALUESEP);
        return new Vector3(floats[0], floats[1], floats[2]);
    }
    public static Vector3 AsVector3(this String value) => value.AsSpan().AsVector3();
    public static void MakeKey(this Vector3 val, byte[] keydata, int pos)
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
