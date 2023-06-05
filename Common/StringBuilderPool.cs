//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class StringBuilderPool
{
    private static Queue<StringBuilder> sbPool = new Queue<StringBuilder>();
    public static StringBuilder Rent()
    {
        StringBuilder sb = null;
        lock (sbPool)
        {
            if (sbPool.Count > 0)
            {
                sb = sbPool.Dequeue();
            }
        }
        if (sb == null)
            sb = new StringBuilder();
        return sb;
    }
    public static void Return(StringBuilder sb)
    {
        sb.Clear();
        lock (sbPool)
        {
            sbPool.Enqueue(sb);
        }
    }
}
