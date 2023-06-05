//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using System.Security.Cryptography;
namespace WFLib;
public class AESEncryptor : IDisposable
{
    readonly Aes aes;
    readonly MemoryStream memStream = new MemoryStream();
    readonly CryptoStream encryptStream;
    readonly ICryptoTransform encryptor;
    public AESEncryptor(AESKey key)
    {
        aes = Aes.Create();
        aes.Key = key.Key;
        aes.IV = key.IV;
        aes.Padding = PaddingMode.PKCS7;
        memStream = new MemoryStream();
        encryptor = aes.CreateEncryptor();
        encryptStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write);
    }
    public void Encrypt(ByteArray input, ByteArray output)
    {
        output.Clear();
        if (input.BytesUsed < 1) return;
        encryptStream.Write(input.Data, 0, input.BytesUsed);
        // When FlushFinalBlock is called and padding is used it will output more bytes than the input.
        encryptStream.FlushFinalBlock();
        output.Resize((int)memStream.Length);
        int bytesRead = memStream.Read(output.Data, 0, (int)memStream.Length);
        output.SetWriteIndex(bytesRead);
    }
    public void Encrypt(byte[] input, ByteArray output)
    {
        output.Clear();
        if (input == null || input.Length < 1) return;
        encryptStream.Write(input, 0, input.Length);
        // When FlushFinalBlock is called and padding is used it will output more bytes than the input.
        encryptStream.FlushFinalBlock();
        output.Resize((int)memStream.Length);
        int bytesRead = memStream.Read(output.Data, 0, (int)memStream.Length);
        output.SetWriteIndex(bytesRead);
    }
    ~AESEncryptor()
    {
        Dispose();
    }
    bool IsDisposd = false;
    public void Dispose()
    {
        if (IsDisposd) return;
        IsDisposd = true;
        if (aes != null)
        {
            aes.Clear();
        }
        if (encryptor != null) encryptor.Dispose();
        if (encryptStream != null) encryptStream.Close();
        if (memStream != null) memStream.Close();
    }


}
