//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class StringBuilderExt
{
    public static void Return(this StringBuilder sb)
    {
        StringBuilderPool.Return(sb);
    }
    public static void Quote(this StringBuilder sb, string val = "")
    {
        sb.Append('"');
        sb.Append(val);
        sb.Append('"');
    }
    public static void NumericItem(this StringBuilder sb, string property, string number)
    {
        sb.Append(',');
        sb.Quote(property);
        sb.Append(':');
        sb.Append(number);
    }
    public static void NumericItemFirst(this StringBuilder sb, string property, string number)
    {
        sb.Quote(property);
        sb.Append(':');
        sb.Append(number);
    }
    public static void StringItem(this StringBuilder sb, string property, string str)
    {
        sb.Append(',');
        sb.Quote(property);
        sb.Append(':');
        sb.Quote(str);
    }
    public static void StringItemFirst(this StringBuilder sb, string property, string str)
    {
        sb.Quote(property);
        sb.Append(':');
        sb.Quote(str);
    }
}
