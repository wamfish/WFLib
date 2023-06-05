//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class ListExtensions
{
    public static string AsString(this List<bool> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static List<bool> AsListOfBool(this string str)
    {
        List<bool> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsBool());
        }
        return l;
    }
    public static List<Godot.Color> AsListOfColor(this string str)
    {
        List<Godot.Color> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.ToString().AsColor());
        }
        //GD.Print($"Color Count: {l.Count}");
        return l;
    }
    public static List<string> AsListOfString(this string str)
    {
        List<string> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            l.Add(ca.Slice(startPos, endPos - startPos).ToString());
        }
        return l;
    }
    public static List<DateTime> AsListOfDateTime(this string str)
    {
        List<DateTime> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsDateTime());
        }
        return l;
    }
    public static List<short> AsListOfShort(this string str)
    {
        List<short> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsShort());
        }
        return l;
    }
    public static List<ushort> AsListOfUShort(this string str)
    {
        List<ushort> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsUShort());
        }
        return l;
    }
    public static List<int> AsListOfInt(this string str)
    {
        List<int> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsInt());
        }
        return l;
    }
    public static List<uint> AsListOfUInt(this string str)
    {
        List<uint> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsUInt());
        }
        return l;
    }
    public static List<long> AsListOfLong(this string str)
    {
        List<long> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsLong());
        }
        return l;
    }
    public static List<ulong> AsListOfULong(this string str)
    {
        List<ulong> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsULong());
        }
        return l;
    }
    public static List<float> AsListOfFloat(this string str)
    {
        List<float> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsFloat());
        }
        return l;
    }
    public static List<double> AsListOfDouble(this string str)
    {
        List<double> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsDouble());
        }
        return l;
    }
    public static List<decimal> AsListOfDecimal(this string str)
    {
        List<decimal> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsDecimal());
        }
        return l;
    }
    public static List<Vector2> AsListOfVector2(this string str)
    {
        List<Vector2> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsVector2());
        }
        return l;
    }
    public static List<Vector2I> AsListOfVector2I(this string str)
    {
        List<Vector2I> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsVector2I());
        }
        return l;
    }
    public static List<Vector3> AsListOfVector3(this string str)
    {
        List<Vector3> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsVector3());
        }
        return l;
    }
    public static List<Vector3I> AsListOfVector3I(this string str)
    {
        List<Vector3I> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsVector3I());
        }
        return l;
    }
    public static List<Quaternion> AsListOfQuaternion(this string str)
    {
        List<Quaternion> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsQuaternion());
        }
        return l;
    }
    public static List<Vector4I> AsListOfVector4I(this string str)
    {
        List<Vector4I> l = new();
        var ca = str.AsSpan();
        int startPos;
        int endPos;
        for (int i = 0; i < ca.Length; i++)
        {
            startPos = i;
            for (i++; i < ca.Length && ca[i] != LISTSEP; i++) { }
            endPos = i;
            var s = ca.Slice(startPos, endPos - startPos);
            l.Add(s.AsVector4I());
        }
        return l;
    }

    public static string AsString(this List<Godot.Color> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var val = sb.ToString();
        StringBuilderPool.Return(sb);
        return val;
    }
    public static string AsString(this List<string> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i]);
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static string AsString(this List<DateTime> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i]);
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static string AsString(this List<decimal> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }

    public static string AsString(this List<short> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static string AsString(this List<ushort> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }

    public static string AsString(this List<int> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static string AsString(this List<uint> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }

    public static string AsString(this List<long> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static string AsString(this List<ulong> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }

    public static string AsString(this List<float> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static string AsString(this List<double> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }

    public static string AsString(this List<Vector2> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static string AsString(this List<Vector3> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static string AsString(this List<Quaternion> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }

    public static string AsString(this List<Vector2I> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static string AsString(this List<Vector3I> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }
    public static string AsString(this List<Vector4I> l)
    {
        if (l.Count < 1) return string.Empty;
        var sb = StringBuilderPool.Rent();
        for (int i = 0; i < l.Count; i++)
        {
            if (i > 0) sb.Append(LISTSEP);
            sb.Append(l[i].AsString());
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;
    }


    public static void ToUIntList(this string s, List<uint> l)
    {
        int pos = 0;
        l.Clear();
        var ca = s.AsSpan();
        while (pos < ca.Length)
        {
            var seg = ParseString(ca, ref pos);
            if (uint.TryParse(seg, out uint val))
            {
                l.Add(val);
            }
            else
            {
                throw new WamfishException();
            }
        }

    }
    private static ReadOnlySpan<char> ParseString(ReadOnlySpan<char> ca, ref int pos)
    {
        int start = pos;
        while (pos < ca.Length && ca[pos] != LISTSEP)
        {
            pos++;
        }
        if (pos < ca.Length) pos++;
        return ca.Slice(start, pos - start);
    }
}
