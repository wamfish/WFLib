//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class ByteArrayExtensions
{
    unsafe static int memcmp(byte[] b1, byte[] b2, long count)
    {
        unsafe
        {
            if (b1.Length < count) return -1;
            if (b2.Length < count) return 1;
            fixed (byte* ap = b1, bp = b2)
            {
                long len = count;
                long* alp = (long*)ap, blp = (long*)bp;
                for (; count >= 8; count -= 8)
                {
                    if (*alp != *blp)
                    {
                        byte* ap3 = (byte*)alp, bp3 = (byte*)blp;
                        int rcount = 8;
                        for (; rcount > 0; rcount--)
                        {
                            if (*ap3 != *bp3)
                            {
                                if (*ap3 < *bp3) return -1;
                                return 1;
                            }
                            ap3++;
                            bp3++;
                        }
                        if (*alp < *blp) return -1;
                        return 1;
                    }
                    alp++;
                    blp++;
                }
                byte* ap2 = (byte*)alp, bp2 = (byte*)blp;
                for (; count > 0; count--)
                {
                    if (*ap2 != *bp2)
                    {
                        if (*alp < *blp) return -1;
                        return 1;
                    }
                    ap2++;
                    bp2++;
                }
                return 0;
            }
        }
    }
    public static int Compare(this byte[] b1, byte[] b2)
    {
        if (b1.Length != b2.Length)
        {
            int len = b1.Length;
            if (b1.Length > b2.Length) len = b2.Length;
            int c = memcmp(b1, b2, len);
            if (c == 0)
            {
                if (b1.Length > b2.Length) return 1;
                return -1;
            }
            return (c);
        }
        return memcmp(b1, b2, b1.Length);
    }
    public static string ToString(byte[] input, int index, int count)
    {
        UTF8Encoding utf8enc = new UTF8Encoding();
        return utf8enc.GetString(input, index, count);
    }
    public static string ToString(byte[] input)
    {
        UTF8Encoding utf8enc = new UTF8Encoding();
        return utf8enc.GetString(input);
    }
    public static string AsBase64(this byte[] ba)
    {
        return Convert.ToBase64String(ba, 0, ba.Length, Base64FormattingOptions.InsertLineBreaks);
    }
    public static string AsBase64(this byte[] ba, int bytesUsed)
    {
        return Convert.ToBase64String(ba, 0, bytesUsed, Base64FormattingOptions.InsertLineBreaks);
    }
}
