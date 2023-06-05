//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class CharSpanExtensions
{
    public static int Count(this ReadOnlySpan<char> span, char val)
    {
        int count = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == val) count++;
        }
        return count;
    }
}
