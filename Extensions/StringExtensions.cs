//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using System.Text.RegularExpressions;
namespace WFLib;
public static class StringExtensions
{
    public static string MakeName(this string str, string ext)
    {
        str = str.Replace(" ", "");
        return str + ext;
    }
    public static byte[] AsByteArray(this string value)
    {
        UTF8Encoding utf8enc = new UTF8Encoding();
        return utf8enc.GetBytes(value);
    }
    public static byte[] AsByteArrayFromBase64(this string value)
    {
        return Convert.FromBase64String(value);
    }
    public static int ToByteArray(this string input, byte[] dest)
    {
        UTF8Encoding utf8enc = new UTF8Encoding();
        var src = utf8enc.GetBytes(input);
        int len;
        if (src.Length < dest.Length)
            len = src.Length;
        else
            len = dest.Length;
        Buffer.BlockCopy(src, 0, dest, 0, len);
        if (src.Length < dest.Length)
        {
            for (int i = src.Length; i < dest.Length; i++)
            {
                dest[i] = (byte)' ';
            }
        }
        return len;
    }
    public static int ByteCount(this string input)
    {
        UTF8Encoding utf8enc = new UTF8Encoding();
        return utf8enc.GetByteCount(input);
    }
    /// <summary>
    /// AsLabel will take a name and convert it to a Lable with spaces based
    /// on the capitalization of the name. The rules for the conversion are a bit
    /// complex to describe, so instead listed below are some examples:
    /// 
    ///     SFXVolume => SFX Volume
    ///     MusicVolume => Music Volume
    ///     ShowFPS => Show FPS
    ///     
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string AsLabel(this string name)
    {
        var sb = StringBuilderPool.Rent();
        var s = name.AsSpan();
        if (s.Length < 1) return string.Empty;

        int upperCount = 0;
        int i = 0;
        char pc = 'a';
        while (i < s.Length)
        {
            var c = s[i];
            var nc = i < s.Length - 1 ? s[i + 1] : 'A';
            if (c.IsUpper())
            {
                if (upperCount == 0 && i > 0 && (pc.NotDigit() || pc.IsDigit() && nc.NotUpper())) sb.Append(' ');
                if (upperCount > 0 && nc.NotUpper()) sb.Append(' ');
                upperCount++;
                sb.Append(c);
            }
            else
            {
                upperCount = 0;
                sb.Append(c);
            }
            pc = c;
            i++;
        }
        var r = sb.ToString();
        StringBuilderPool.Return(sb);
        return r;

    }
    //public static string AsDisplayName(this string input)
    //{
    //    var temp = input;
    //    if (temp.Length > 2) temp = temp.Replace("Id", "");
    //    return System.Text.RegularExpressions.Regex.Replace(temp, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    //}
    public static void AsKey(this string instr, SerializationBuffer sb, int keySize)
    {
        int bc = Encoding.UTF8.GetByteCount(instr);
        if (keySize > bc) bc = keySize;
        Span<byte> keyData = stackalloc byte[bc];
        int count = Encoding.UTF8.GetBytes(instr.ToUpper().AsSpan(), keyData);
        if (count < keySize)
        {
            for (int i = count; i < keySize; i++)
            {
                keyData[i] = 32;
            }
        }
        sb.WriteNoSize(keyData.Slice(0,keySize));
    }
    public static void MakeKey(this string instr, byte[] keyData, int pos, int size)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(instr);
        if (data.Length >= size)
        {
            Buffer.BlockCopy(data, 0, keyData, pos, size);
            return;
        }
        Buffer.BlockCopy(data, 0, keyData, pos, data.Length);
        for (int i = pos + data.Length; i < pos + size; i++)
        {
            keyData[i] = 32;
        }
    }
    public static string[] ToArray(this string code)
    {
        return Regex.Split(code, "\r\n|\r|\n");
    }
    public static string CamelCase(this string val)
    {
        return val.Substring(0, 1).ToLower() + val.Substring(1, val.Length - 1);
    }
    public static string PascalCase(this string val)
    {
        return val.Substring(0, 1).ToUpper() + val.Substring(1, val.Length - 1);
    }
    //Converts span to a DateTime, on error it returns DateTime.UtcNow
    public static bool AsBool(this ReadOnlySpan<char> val)
    {
        if (!bool.TryParse(val, out bool r)) r = false;
        return r;
    }
    public static bool AsBool(this String value) => value.AsSpan().AsBool();
}
