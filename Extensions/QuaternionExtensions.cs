//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class QuaternionExtensions
{
    public static bool NotZero(this Quaternion value)
    {
        if (!(value.X == 0 && value.Y == 0 && value.Z == 0 && value.W == 0)) return true;
        return false;
    }
    public static string AsString(this Quaternion value)
    {
        var sb = StringBuilderPool.Rent();
        sb.Append(value.X.ToString());
        sb.Append(VALUESEP);
        sb.Append(value.Y.ToString());
        sb.Append(VALUESEP);
        sb.Append(value.Z.ToString());
        sb.Append(VALUESEP);
        sb.Append(value.W.ToString());
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static Quaternion AsQuaternion(this ReadOnlySpan<char> value)
    {
        if (value.Length == 0) return Quaternion.Identity;
        Span<float> floats = stackalloc float[4];
        ParseFloats(value, floats, VALUESEP);
        return new Quaternion(floats[0], floats[1], floats[2], floats[3]);
    }
    public static Quaternion AsQuaternion(this String value) => value.AsSpan().AsQuaternion();
    public static void MakeKey(this Quaternion val, byte[] keydata, int pos)
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
        pos += 4;
        if (BitConverter.TryWriteBytes(keydata.AsSpan(pos, 4), val.W))
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(keydata, pos, 4);
        }
    }
}
