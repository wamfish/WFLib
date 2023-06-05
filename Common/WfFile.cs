//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public class WfFile : IDisposable
{
    StringBuilder sb = new StringBuilder();
    byte[] buf = new byte[256];
    public string Path { get; private set; }
    public FileStream fs = null;
    public bool IsOpen { get; private set; } = false;
    public WfFile(string path)
    {
        this.Path = path;
    }
    public bool ReadPacketBuffer(SerializationBuffer buf, long recordOffset)
    {
        if (recordOffset >= 0)
        {
            if (fs.Seek(recordOffset, SeekOrigin.Begin) != recordOffset)
                throw new Exception("invalid record offset");
        }
        buf.Clear();
        int size = ReadInt();
        if (size < 1) return false;
        buf.Buf.Resize(size);
        int bytesRead = fs.Read(buf.Buf.Data, 0, size);
        if (bytesRead != size) throw new WamfishException();
        buf.Buf.SetWriteIndex(bytesRead);
        return true;
    }
    //public bool ReadPacketBuffer(PacketBuffer buf, long recordOffset, bool readArchive = false)
    //{
    //    if (recordOffset >= 0)
    //    {
    //        if (fs.Seek(recordOffset, SeekOrigin.Begin) != recordOffset)
    //            throw new Exception("invalid record offset");
    //    }
    //    buf.Clear();
    //    int size;
    //    int rsize = size = ReadInt();
    //    if (size < 0)
    //    {
    //        if (!readArchive)
    //            return false;
    //        rsize = size = size * -1;
    //    }
    //    int psize = buf.PacketSize;
    //    int bytesToRead;
    //    byte[] data = ByteArrayPool.Rent(psize);
    //    if (size > psize)
    //    {
    //        bytesToRead = psize;
    //    }
    //    else
    //    {
    //        bytesToRead = size;
    //    }
    //    while (size > 0)
    //    {
    //        int bytesRead = fs.Read(data, 0, bytesToRead);
    //        if (bytesRead != bytesToRead)
    //            throw new WamfishException();
    //        buf.BlockCopy(data, 0, bytesRead);
    //        size -= bytesRead;
    //        if (size > psize)
    //        {
    //            bytesToRead = psize;
    //        }
    //        else
    //        {
    //            bytesToRead = size;
    //        }
    //    }
    //    ByteArrayPool.Return(data);
    //    return true;
    //}
    public void ReadFileRowPtr(List<long> ptrs)
    {
        ptrs.Clear();
        ptrs.Capacity = (int)fs.Length / 8;
        bool closeFile = false;
        if (!IsOpen)
        {
            Open();
            closeFile = true;
        }
        SeekBegin(0);
        const int BUFSIZE = 1024 * 128;
        byte[] buf = ByteArrayPool.Rent(BUFSIZE);
        int bytesRead;
        int pos;

        long ReadLong()
        {
            int val =
                buf[pos] << 56 | buf[pos + 1] << 48 | buf[pos + 2] << 40 | buf[pos + 3] << 32 |
                buf[pos + 4] << 24 | buf[pos + 5] << 16 | buf[pos + 6] << 8 | buf[pos + 7];
            pos += 8;
            return val;
        }
        bytesRead = fs.Read(buf, 0, BUFSIZE);
        while (bytesRead > 0)
        {
            pos = 0;
            int count = bytesRead / 8;
            for (int i = 0; i < count; i++)
            {
                ptrs.Add(ReadLong());
            }
            bytesRead = fs.Read(buf, 0, BUFSIZE);
        }
        ByteArrayPool.Return(buf);
        if (closeFile)
            Close();
    }
    public void WriteFileRowPtr(List<long> ptrs)
    {
        bool closeFile = false;
        if (!IsOpen)
        {
            Open();
            closeFile = true;
        }
        SeekBegin(0);
        const int BUFSIZE = 1024 * 128;
        byte[] buf = ByteArrayPool.Rent(BUFSIZE);
        //byte[] lbuf = new byte[8];
        int pos = 0;
        int bufCount = BUFSIZE / 8;
        for (int i = 0; i < ptrs.Count;)
        {
            pos = 0;
            for (int b = 0; i < ptrs.Count && b < bufCount; b++)
            {
                Util.WriteLong(ptrs[i], buf, pos);
                pos += 8;
                i++;
            }
            fs.Write(buf, 0, pos);
        }
        fs.Flush();
        ByteArrayPool.Return(buf);
        if (closeFile)
            Close();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close();
        }
    }
    public bool Exists
    {
        get
        {
            if (System.IO.File.Exists(Path)) return true;
            return false;
        }
    }
    ~WfFile()
    {
        //Should already be closed, but just in case
        if (IsOpen)
        {
            Close();
        }
    }
    public void Delete()
    {
        if (IsOpen) throw new WamfishException();
        if (System.IO.File.Exists(Path))
        {
            System.IO.File.Delete(Path);
        }
    }
    public void Open()
    {
        if (IsOpen)
        {
            Close();
            throw (new Exception("Error: file already open File: " + Path));
        }
        try
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            fs = new System.IO.FileStream(Path, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite);
            //fs = new BufferedStream(fsf, 1024);
            IsOpen = true;
        }
        finally
        {
            if (!IsOpen) Close(); //an error occurred make sure our file is closed
        }
    }
    public void OpenAppend()
    {
        if (IsOpen)
        {
            Close();
            throw (new Exception("Error: file already open File: " + Path));
        }
        try
        {
            //fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough);
            fs = new System.IO.FileStream(Path, FileMode.Append, System.IO.FileAccess.Write, FileShare.None);
            //fs = new BufferedStream(fsf, 1024);
            IsOpen = true;
        }
        finally
        {
            if (!IsOpen) Close(); //an error occurred make sure our file is closed
        }
    }
    public void Close()
    {
        if (!IsOpen) return;
        try
        {
            if (fs.CanWrite)
                fs.Flush();
            fs.Close();
            fs = null;
            //fsf.Close();
            //fsf = null;
        }
        finally
        {
            IsOpen = false;
            fs = null;
        }
    }
    public long SeekEnd()
    {
        if (fs.Position == fs.Length)
            return fs.Position;
        fs.Position = fs.Length;
        return fs.Position;
        //return SeekEnd(0);
    }
    public long SeekBegin(long offset)
    {
        return fs.Seek(offset, SeekOrigin.Begin);
    }
    public long SeekCurrent(long offset)
    {
        return fs.Seek(offset, SeekOrigin.Current);
    }
    public long Position
    {
        get
        {
            return fs.Position;
        }
    }
    public long Length
    {
        get
        {
            return fs.Length;
        }
    }
    public void Flush()
    {
        fs.Flush();
    }
    public void Grow(long size)
    {
        long endPos = SeekEnd();
        if (endPos < size)
        {
            for (; endPos < size; endPos++) //write out 0
            {
                Write((byte)0);
            }
            //fs.Flush();
        }
    }
    //int bufPos = 0;
    //int bufSize = 0;
    public bool AtEnd
    {
        get
        {
            if (fs.Position == fs.Length) return true;
            return false;
        }
    }
    public void WriteSize(int size)
    {
        if (size < 254)
        {
            byte hsize = (byte)size;
            Write(hsize);
            return;
        }
        if (size <= UInt16.MaxValue)
        {
            UInt16 osize = (UInt16)size;
            Write((byte)254);
            Write(osize);
            return;
        }
        Write((byte)255);
        Write(size);
        return;
    }
    public int ReadSize()
    {
        byte rb;
        rb = (byte)fs.ReadByte();
        int size;
        size = rb;
        if (size < 254) return size;
        if (size == 254)
        {
            return ReadUShort();
        }
        return ReadInt();
    }
    public string ReadString()
    {
        int size = ReadSize();
        if (size == 0) return string.Empty;
        byte[] data = new byte[size];
        fs.Read(data, 0, size);
        string into = Encoding.UTF8.GetString(data, 0, size);
        return into;
    }
    public string ReadString(out int size)
    {
        size = ReadSize();
        if (size == 0) return string.Empty;
        byte[] data = new byte[size];
        fs.Read(data, 0, size);
        string into = Encoding.UTF8.GetString(data, 0, size);
        return into;
    }
    public byte ReadByte()
    {
        return (byte)fs.ReadByte();
    }
    public char ReadChar()
    {
        buf[0] = ReadByte();
        buf[1] = ReadByte();
        Util.Reverse(buf, 2);
        Util.Reverse(buf, 2);
        return BitConverter.ToChar(buf, 0);
    }
    public int ReadInt()
    {
        fs.Read(buf, 0, 4);
        Util.Reverse(buf, 4);
        return BitConverter.ToInt32(buf, 0);
    }
    public uint ReadUInt()
    {
        uint into = 0;
        if (fs.Read(buf, 0, 4) != 4)
        {
            throw new WamfishException();
        }
        Util.Reverse(buf, 4);
        into = BitConverter.ToUInt32(buf, 0);
        return into;
    }
    public long ReadLong()
    {
        long into = 0;
        if (fs.Read(buf, 0, 8) != 8) throw new WamfishException();
        Util.Reverse(buf, 8);
        into = BitConverter.ToInt64(buf, 0);
        return into;
    }
    public ulong ReadULong()
    {
        ulong into = 0;
        if (fs.Read(buf, 0, 8) != 8) throw new WamfishException();
        Util.Reverse(buf, 8);
        into = BitConverter.ToUInt64(buf, 0);
        return into;
    }
    public short ReadShort()
    {
        short into = 0;
        if (fs.Read(buf, 0, 2) != 2) throw new WamfishException();
        Util.Reverse(buf, 2);
        into = BitConverter.ToInt16(buf, 0);
        return into;
    }
    public ushort ReadUShort()
    {
        ushort into = 0;
        if (fs.Read(buf, 0, 2) != 2)
            throw new WamfishException();
        Util.Reverse(buf, 2);
        into = BitConverter.ToUInt16(buf, 0);
        return into;
    }
    public double ReadDouble()
    {
        double into = 0;
        if (fs.Read(buf, 0, 8) != 8) throw new WamfishException();
        Util.Reverse(buf, 8);
        into = BitConverter.ToDouble(buf, 0);
        return into;
    }
    public float ReadFloat()
    {
        float into = 0f;
        if (fs.Read(buf, 0, 4) != 4) throw new WamfishException();
        Util.Reverse(buf, 4);
        into = BitConverter.ToSingle(buf, 0);
        return into;
    }
    public DateTime ReadDateTime()
    {
        DateTime into = DateTime.MinValue;
        if (fs.Read(buf, 0, 8) != 8) throw new WamfishException();
        Util.Reverse(buf, 8);
        long val = (long)BitConverter.ToUInt64(buf, 0);
        into = DateTime.FromBinary(val);
        return into;
    }
    public Vector2 ReadVec2()
    {
        float x;
        float y;
        x = ReadFloat();
        y = ReadFloat();
        return new Vector2(x, y);
    }
    public Vector3 ReadVec3()
    {
        float x = ReadFloat();
        float y = ReadFloat();
        float z = ReadFloat();
        return new Vector3(x, y, z);
    }
    public Quaternion ReadQuat()
    {
        float x = ReadFloat();
        float y = ReadFloat();
        float z = ReadFloat();
        float w = ReadFloat();
        return new Quaternion(x, y, z, w);
    }
    public decimal ReadDecimal()
    {
        int[] vals = new int[4];
        vals[0] = ReadInt();
        vals[1] = ReadInt();
        vals[2] = ReadInt();
        vals[3] = ReadInt();
        return new decimal(vals);
    }
    public int ReadRaw(byte[] into)
    {
        return fs.Read(into, 0, into.Length);
    }
    public List<uint> ReadUIntList()
    {
        int count = ReadSize();
        List<uint> into = new List<uint>(count);
        for (int i = 0; i < count; i++)
        {
            into.Add(ReadUInt());
        }
        return into;
    }
    public void ReadLongList(List<long> into)
    {
        into.Clear();
        int count = ReadSize();
        for (int i = 0; i < count; i++)
        {
            into.Add(ReadLong());
        }
        return;
    }
    public string[] ReadLines()
    {
        if (IsOpen)
            Close();
        return System.IO.File.ReadAllLines(Path);
    }
    //Write methods
    public void Write(SerializationBuffer buf)
    {
        int size = buf.BytesUsed;
        Write(size);
        fs.Write(buf.Buf.Data, 0, size);
    }
    //public void Write(PacketBuffer buf)
    //{
    //    buf.ReadPosition = 0;
    //    int bytesLeft = buf.BytesUsed;
    //    Write(bytesLeft);
    //    buf.Process((data, bytes) =>
    //    {
    //        fs.Write(data, 0, bytes);
    //    });
    //}
    public void Write(byte output)
    {
        fs.WriteByte(output);
    }
    public void Write(sbyte output)
    {
        fs.WriteByte((byte)output);
    }
    public void Write(char outvar)
    {
        byte[] data = BitConverter.GetBytes(outvar);
        Util.Reverse(data);
        fs.Write(data, 0, data.Length);
    }
    public void Write(string output)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(output);
        if (data == null) throw new WamfishException();
        WriteSize(data.Length);
        fs.Write(data, 0, data.Length);
    }
    public void Write(int output)
    {
        byte[] data = BitConverter.GetBytes(output);
        Util.Reverse(data);
        fs.Write(data, 0, data.Length);
    }
    public void Write(uint output)
    {
        byte[] data = BitConverter.GetBytes(output);
        Util.Reverse(data);
        fs.Write(data, 0, data.Length);
    }
    public void Write(long output)
    {
        byte[] data = BitConverter.GetBytes(output);
        Util.Reverse(data);
        fs.Write(data, 0, data.Length);
    }
    public void Write(ulong output)
    {
        byte[] data = BitConverter.GetBytes(output);
        Util.Reverse(data);
        fs.Write(data, 0, data.Length);
    }
    public void Write(short output)
    {
        byte[] data = BitConverter.GetBytes(output);
        Util.Reverse(data);
        fs.Write(data, 0, data.Length);
    }
    public void Write(ushort output)
    {
        byte[] data = BitConverter.GetBytes(output);
        Util.Reverse(data);
        fs.Write(data, 0, data.Length);
    }
    public void Write(double output)
    {
        byte[] data = BitConverter.GetBytes(output);
        Util.Reverse(data);
        fs.Write(data, 0, data.Length);
    }
    public void Write(float output)
    {
        byte[] data = BitConverter.GetBytes(output);
        Util.Reverse(data);
        fs.Write(data, 0, data.Length);
    }
    public void Write(DateTime output)
    {
        long val = output.ToBinary();
        byte[] data = BitConverter.GetBytes(val);
        Util.Reverse(data);
        fs.Write(data, 0, data.Length);
    }
    public void Write(Vector2 output)
    {
        Write(output.X);
        Write(output.Y);
    }
    public void Write(Vector3 output)
    {
        Write(output.X);
        Write(output.Y);
        Write(output.Z);
    }
    public void Write(Quaternion output)
    {
        Write(output.X);
        Write(output.Y);
        Write(output.Z);
        Write(output.W);
    }
    public void Write(decimal output)
    {
        var vals = decimal.GetBits(output);
        Write(vals[0]);
        Write(vals[1]);
        Write(vals[2]);
        Write(vals[3]);
    }
    //Write Array Methods
    public void Write(byte[] data)
    {
        WriteSize(data.Length);
        fs.Write(data, 0, data.Length);
    }
    public void WriteByteArray(byte[] data, int srcOffset, int srcLen)
    {
        fs.Write(data, srcOffset, srcLen);
    }
    public void WriteRaw(byte[] data, int srcOffset, int srcLen)
    {
        fs.Write(data, srcOffset, srcLen);
    }
    public void Write(byte[] data, int srcOffset, int srcLen)
    {
        if (data.Length < 1 || srcLen < 1)
        {
            WriteSize(0);
            return;
        }
        WriteSize(srcLen);
        fs.Write(data, srcOffset, srcLen);
    }
    public void Write(string[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(int[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(uint[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(List<uint> output)
    {
        WriteSize(output.Count);
        for (int i = 0; i < output.Count; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(List<long> output)
    {
        WriteSize(output.Count);
        for (int i = 0; i < output.Count; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(ulong[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(short[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(ushort[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(double[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(float[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(Vector2[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(Vector3[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(DateTime[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(Quaternion[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void Write(decimal[] output)
    {
        WriteSize(output.Length);
        for (int i = 0; i < output.Length; i++)
        {
            Write(output[i]);
        }
    }
    public void WriteNoLine(string output)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(output);
        fs.Write(data, 0, data.Length);
    }
    public void WriteLine(string output)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(output);
        fs.Write(data, 0, data.Length);
        fs.WriteByte(13);
        fs.WriteByte(10);
    }
    public void WriteKeyString(string from, int len)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(from);
        if (data.Length >= len)
        {
            fs.Write(data, 0, len);
            return;
        }
        byte[] spaces = System.Text.Encoding.UTF8.GetBytes(new string(' ', len));
        fs.Write(data, 0, data.Length);
        fs.Write(spaces, 0, len - data.Length);
    }
    public void WriteNullTermString(string output)
    {
        byte nullTerm = 0;
        if (output.Length < 1)
        {
            Write(nullTerm);
            return;
        }
        byte[] data = System.Text.Encoding.UTF8.GetBytes(output);
        fs.Write(data, 0, data.Length);
        Write(nullTerm);
    }
    public static void WriteFile(string dir, string fileName, string text)
    {
        Directory.CreateDirectory(dir);
        var filePath = System.IO.Path.Combine(dir, fileName);
        System.IO.File.WriteAllText(filePath, text);
    }
    // public RowInfo ReadFileRowId(Action<RowInfo> act)
    // {
    //     bool closeFile = false;
    //     if (!IsOpen)
    //     {
    //         Open();
    //         closeFile = true;
    //     }
    //     SeekBegin(4);
    //     const int BUFSIZE = 1024 * 128;
    //     RowInfo ri = new RowInfo();
    //     ri.FileOffset = 4;
    //     ri.FileSize = fs.Length;
    //     PacketBuffer pb = PacketBuffer.Create(BUFSIZE);
    //     byte[] buf = ByteArrayManager.GetArray(BUFSIZE);
    //     int bytesRead = 0;
    //     int pos = 0;
    //     int BytesLeft() => bytesRead - pos;
    //     int ReadInt()
    //     {
    //         int val = buf[pos] << 24 | buf[pos + 1] << 16 | buf[pos + 2] << 8 | buf[pos + 3];
    //         pos += 4;
    //         return val;
    //     }
    //     ushort ReadUShort()
    //     {
    //         ushort val = (ushort) (buf[pos] << 8 | buf[pos + 1]);
    //         pos += 2;
    //         return val;
    //     }
    //     bytesRead = fs.Read(buf, 0, buf.Length);
    //     pos = 0;
    //     while (bytesRead > 0)
    //     {
    //         while (BytesLeft() >= 4)
    //         {
    //             int size = ReadInt();
    //             int nextPos = pos + size;
    //             if (size <= 0)
    //             {
    //                 throw new InvalidDataException();
    //             }
    //             if (size <= BytesLeft())
    //             {
    //                 if (buf[pos + size - 1] != Byte.MaxValue)
    //                     throw new InvalidDataException();
    //                 ri.BytesRead += size + 4;
    //                 ri.FieldCount = ReadUShort();
    //                 ri.StatusCode = (STATUSCODE) buf[pos++];
    //                 ri.Id = ReadInt();
    //                 pos = nextPos;
    //                 act(ri);
    //                 ri.FileOffset += size + 4;
    //                 continue;
    //             }
    //             else
    //             {
    //                 pb.Clear();
    //                 pb.BlockCopy(buf, pos, BytesLeft());
    //                 while (pb.BytesUsed < size)
    //                 {
    //                     bytesRead = fs.Read(buf, 0, BUFSIZE);
    //                     int bytesLeft = size - pb.BytesUsed;
    //                     if (bytesLeft >= BUFSIZE)
    //                     {
    //                         pb.BlockCopy(buf, 0, BUFSIZE);
    //                         pos = BUFSIZE;
    //                     }
    //                     else
    //                     {
    //                         pb.BlockCopy(buf, 0, bytesLeft);
    //                         pos = bytesLeft;
    //                     }
    //                 }
    //                 ri.BytesRead += pb.BytesUsed + 4;
    //                 ri.FieldCount = pb.ReadUShort();
    //                 ri.StatusCode = (STATUSCODE) pb.ReadByte();
    //                 ri.Id = pb.ReadInt();
    //                 pb.ReadPosition = size - 1;
    //                 if (pb.ReadByte() != Byte.MaxValue)
    //                     throw new InvalidDataException();
    //                 act(ri);
    //                 ri.FileOffset += size + 4;
    //                 continue;
    //             }
    //         }
    //         int bl = BytesLeft();
    //         if (BytesLeft() > 0)
    //         {
    //             Buffer.BlockCopy(buf, pos, buf, 0, bl);
    //         }
    //         bytesRead = fs.Read(buf, bl, buf.Length - bl);
    //         bytesRead += bl;
    //         pos = 0;
    //     }
    //     ri.t.Stop();
    //     if (closeFile)
    //         Close();
    //     return ri;
    // }
    //public void ReadFileRow(Action<PacketBuffer> act, int paketBufferSize=1024)
    //{
    //    bool closeFile = false;
    //    if (!IsOpen)
    //    {
    //        Open();
    //        closeFile = true;
    //    } 
    //    SeekBegin(4);
    //    const int BUFSIZE = 1024 * 128;
    //    byte[] buf = ByteArrayPool.Rent(BUFSIZE);
    //    int bytesRead = 0;
    //    int pos = 0;
    //    int BytesLeft() => bytesRead - pos;
    //    int ReadInt()
    //    {
    //        int val = buf[pos] << 24 | buf[pos + 1] << 16 | buf[pos + 2] << 8 | buf[pos + 3];
    //        pos += 4;
    //        return val;
    //    }
    //    bytesRead = fs.Read(buf, 0, buf.Length);
    //    pos = 0;
    //    PacketBuffer pb = PacketBuffer.Create(paketBufferSize);
    //    while (bytesRead >  0)
    //    {
    //        while (BytesLeft() >= 4)
    //        {
    //            int size = ReadInt();
    //            int nextPos = pos + size;
    //            if (size <= 0)
    //            {
    //                throw new InvalidDataException();
    //            }
    //            if (size <= BytesLeft())
    //            {
    //                pb.Clear();
    //                pb.BlockCopy(buf, pos, size);
    //                pos = nextPos;
    //                if (buf[pos - 1] != Byte.MaxValue)
    //                    throw new InvalidDataException();
    //                act(pb);
    //                continue;
    //            }
    //            else
    //            {
    //                pb.Clear();
    //                pb.BlockCopy(buf, pos, BytesLeft());
    //                while (pb.BytesUsed < size)
    //                {
    //                    bytesRead = fs.Read(buf, 0, BUFSIZE);
    //                    int bytesLeft = size - pb.BytesUsed;
    //                    if (bytesLeft >= BUFSIZE)
    //                    {
    //                        pb.BlockCopy(buf, 0, BUFSIZE);
    //                        pos = BUFSIZE;
    //                        continue;
    //                    }
    //                    pb.BlockCopy(buf, 0, bytesLeft);
    //                    pos = bytesLeft;
    //                }
    //                if (buf[pos-1] != Byte.MaxValue)
    //                    throw new InvalidDataException();
    //                act(pb);
    //                continue;
    //            }
    //        }
    //        int bl = BytesLeft();
    //        if (BytesLeft() > 0)
    //        {
    //            Buffer.BlockCopy(buf, pos, buf, 0, bl);
    //        }
    //        bytesRead = fs.Read(buf, bl, buf.Length - bl);
    //        bytesRead += bl;
    //        pos = 0;
    //    }
    //    if (closeFile)
    //        Close();
    //    pb.Release();
    //}
    //public void ReadFileRowPtr(List<long> ptrs)
    //{
    //    ptrs.Clear();
    //    ptrs.Capacity = (int) fs.Length / 8;
    //    bool closeFile = false;
    //    if (!IsOpen)
    //    {
    //        Open();
    //        closeFile = true;
    //    }
    //    SeekBegin(0);
    //    const int BUFSIZE = 1024 * 128;
    //    byte[] buf = ByteArrayManager.GetArray(BUFSIZE);
    //    int bytesRead;
    //    int pos;

    //    long ReadLong()
    //    {
    //        int val =
    //            buf[pos] << 56 | buf[pos + 1] << 48 | buf[pos + 2] << 40 | buf[pos + 3] << 32 |
    //            buf[pos + 4] << 24 | buf[pos + 5] << 16 | buf[pos + 6] << 8 | buf[pos + 7];
    //        pos += 8;
    //        return val;
    //    }
    //    bytesRead = fs.Read(buf, 0, BUFSIZE);
    //    while (bytesRead > 0)
    //    {
    //        pos = 0;
    //        int count = bytesRead / 8;
    //        for (int i = 0; i < count; i++)
    //        {
    //            ptrs.Add(ReadLong());
    //        }
    //        bytesRead = fs.Read(buf, 0, BUFSIZE);
    //    }
    //    ByteArrayManager.Release(buf);
    //    if (closeFile)
    //        Close();
    //}
    //public void WriteFileRowPtr(List<long> ptrs)
    //{
    //    NumberValues nv = new NumberValues();
    //    bool closeFile = false;
    //    if (!IsOpen)
    //    {
    //        Open();
    //        closeFile = true;
    //    }
    //    SeekBegin(0);
    //    const int BUFSIZE = 1024 * 128;
    //    byte[] buf = ByteArrayManager.GetArray(BUFSIZE);
    //    byte[] lbuf = new byte[8];
    //    int pos = 0;
    //    int bufCount = BUFSIZE / 8;
    //    for(int i=0;i<ptrs.Count;)
    //    {
    //        pos = 0;
    //        for (int b=0;i<ptrs.Count && b<bufCount;b++)
    //        {
    //            nv.WriteLong(ptrs[i], buf, pos);
    //            pos += 8;
    //            i++;
    //        }
    //        fs.Write(buf, 0, pos);
    //    }
    //    fs.Flush();
    //    ByteArrayManager.Release(buf);
    //    if (closeFile)
    //        Close();
    //}
}
