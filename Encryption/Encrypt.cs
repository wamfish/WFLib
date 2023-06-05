//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace WFLib;

public static class Encrypt
{
    public readonly static PasswordHasher<string> PasswordHasher = new PasswordHasher<string>();
    public static byte[] SHA512Hash(string src)
    {
        var data = src.AsByteArray();
        byte[] result;
        SHA512 sha = SHA512.Create();
        result = sha.ComputeHash(data);
        return result;
    }
}
