//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using System.Security.Cryptography;
namespace WFLib;
public class AESDecryptor
{
    readonly Aes aes;
    readonly MemoryStream memStream = new MemoryStream();
    readonly CryptoStream decryptStream = null;
    readonly ICryptoTransform decryptor = null;
    public AESDecryptor(AESKey key)
    {
        aes = Aes.Create();
        aes.Key = key.Key;
        aes.IV = key.IV;
        aes.Padding = PaddingMode.PKCS7;
        memStream = new MemoryStream();
        decryptor = aes.CreateDecryptor();
        decryptStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Write);
    }
    ~AESDecryptor()
    {
        if (aes != null) aes.Clear();
        if (decryptor != null) decryptor.Dispose();
        if (decryptStream != null) decryptStream.Close();
        if (memStream != null) memStream.Close();
    }
    public byte[] Decrypt(byte[] data)
    {
        if (data.Length < 1) return null;
        memStream.Flush();
        decryptStream.Write(data, 0, data.Length);
        decryptStream.FlushFinalBlock();
        return memStream.ToArray();
    }

}
