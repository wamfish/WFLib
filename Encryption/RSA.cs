//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using System.Security.Cryptography;

namespace WFLib;

public class RSA
{
    readonly RSACryptoServiceProvider csp;
    RSAParameters privateKey;
    RSAParameters publicKey;
    public RSA()
    {
        csp = new RSACryptoServiceProvider(2048);
        privateKey = csp.ExportParameters(true);
        publicKey = csp.ExportParameters(false);
    }
    // Use this constructor if you already have a key pair and need to create
    // a new RSA instance.
    public RSA(ByteArray privateKeyBA)
    {
        privateKeyBA.SetReadIndex(0);
        privateKey = new RSAParameters();
        SetPrivateKey(privateKeyBA); //privateKey contains both public and private keys
        csp = new RSACryptoServiceProvider(2048);
        csp.ImportParameters(privateKey); //import both keys
        publicKey = csp.ExportParameters(false);
    }
    public ByteArray GetPublicKey()
    {
        SerializationBuffer pb = SerializationBuffer.Rent();
        pb.Write(publicKey.Modulus);
        pb.Write(publicKey.Exponent);
        var ba = pb.GetBuf();
        //pb.Return();
        return ba;
    }
    public void SetPublicKey(ByteArray ba)
    {
        ba.SetReadIndex(0);
        SerializationBuffer pb = SerializationBuffer.Rent(ba);
        publicKey.Modulus = pb.ReadByteArray();
        publicKey.Exponent = pb.ReadByteArray();
        //pb.Return();
    }
    public ByteArray GetPrivateKey()
    {
        SerializationBuffer pb = SerializationBuffer.Rent();
        pb.Clear();
        pb.Write(privateKey.Modulus);
        pb.Write(privateKey.Exponent);
        pb.Write(privateKey.P);
        pb.Write(privateKey.Q);
        pb.Write(privateKey.DP);
        pb.Write(privateKey.DQ);
        pb.Write(privateKey.InverseQ);
        pb.Write(privateKey.D);
        var ba = pb.GetBuf();
        //pb.Return();
        return ba;
    }
    public void SetPrivateKey(ByteArray ba)
    {
        ba.SetReadIndex(0);
        SerializationBuffer pb = SerializationBuffer.Rent(ba);
        privateKey.Modulus = pb.ReadByteArray();
        privateKey.Exponent = pb.ReadByteArray();
        privateKey.P = pb.ReadByteArray();
        privateKey.Q = pb.ReadByteArray();
        privateKey.DP = pb.ReadByteArray();
        privateKey.DQ = pb.ReadByteArray();
        privateKey.InverseQ = pb.ReadByteArray();
        privateKey.D = pb.ReadByteArray();
        //pb.Return();
    }
    public string Encrypt(string data)
    {
        var buf = Encoding.Unicode.GetBytes(data);
        var eBuf = csp.Encrypt(buf, false);
        var eText = Convert.ToBase64String(eBuf);
        return eText;
    }
    public string Decrypt(string data)
    {
        var buf = Convert.FromBase64String(data);
        var dBuf = csp.Decrypt(buf, false);
        string dText = Encoding.Unicode.GetString(dBuf);
        return dText;
    }
}
