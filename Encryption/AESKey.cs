//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using System.Security.Cryptography;

namespace WFLib;

public class AESKey
{
    public byte[] Key { get; set; }
    public byte[] IV { get; set; }
    public AESKey()
    {
        var aes = Aes.Create();
        aes.Padding = PaddingMode.ISO10126;
        aes.GenerateKey();
        aes.GenerateIV();
        Key = aes.Key;
        IV = aes.IV;
    }
    public AESKey(byte[] key, byte[] iv)
    {
        Key = key;
        IV = iv;
    }
}
