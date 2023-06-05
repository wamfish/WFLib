//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.

namespace WFLib;
public static class BoolExtensions
{
    public static string AsString(this bool val)
    {
        return Convert.ToString(val);
    }
    public static void MakeKey(this bool val, byte[] keydata, int pos)
    {
#if NETSTANDARD || NET472
        var data = BitConverter.GetBytes(val);
        Buffer.BlockCopy(data,0,keydata,pos,1);
#else
        BitConverter.TryWriteBytes(keydata.AsSpan(pos, 1), val);
#endif
    }
}
