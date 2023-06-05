//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
using System.Net;
using System.Runtime.InteropServices;
namespace WFLib;
public class SerializationBuffer : IDisposable
{
    private ByteArray buf;
    public ByteArray Buf => buf;
    public byte[] Data => buf.Data;
    public int ReadIndex => buf.ReadIndex;
    public int BytesToRead => buf.BytesToRead;
    public int BytesAvailable => buf.BytesAvailable;
    public int WriteIndex => buf.WriteIndex;
    public int BytesUsed => buf.BytesUsed;
    public void SetWriteIndex(int index) => buf.SetWriteIndex(index);
    public void SetReadIndex(int index) => buf.SetReadIndex(index);
    public EndPoint ClientEndPoint;  // used in UdpServer to pass info
    public int RequestSize;
    public int RequestBytesRead;
    public ByteArray GetBuf()
    {
        var myba = buf;
        buf = ByteArray.Rent();
        buf.Clear();
        return myba;
    }
    byte[] reverse = new byte[64];
    private SerializationBuffer()
    {
        buf = ByteArray.Rent();
    }
    private static Pool<SerializationBuffer> pool = new(() => new SerializationBuffer());
    /// <summary>
    /// 
    /// Use this to get a SerializationBuffer object. 
    /// 
    /// Example: using var sb = SerializationBuffer.Rent();
    ///     
    /// </summary>
    /// <returns> SerializationBuffer </returns>
    public static SerializationBuffer Rent()
    {
        return pool.Rent();
    }
    /// <summary>
    /// 
    /// Use this to get a SerializationBuffer object with
    /// the size set to size. 
    /// 
    /// Example: using var sb = SerializationBuffer.Rent(1024);
    ///     
    /// </summary>
    /// <returns> SerializationBuffer </returns>
    public static SerializationBuffer Rent(int size)
    {
        SerializationBuffer sb = pool.Rent();
        sb.Buf.Resize(size);
        return sb;
    }
    /// <summary>
    /// 
    /// Use this to get a SerializationBuffer object initialized
    /// with a dcopy of the ByteArray ba.
    /// 
    /// Example: using var sb = SerializationBuffer.Rent(myByteArray);
    ///     
    /// </summary>
    /// <returns> SerializationBuffer </returns>
    public static SerializationBuffer Rent(ByteArray ba)
    {
        SerializationBuffer sb = pool.Rent();
        sb.buf.Clear();
        sb.buf.Resize(ba.Length);
        sb.buf.Write(ba.Data, 0, ba.BytesUsed);
        return sb;
    }
    /// <summary>
    /// Returns a string with stats about the pool
    /// </summary>
    public static string PoolStats => pool.Stats;
    /// <summary>
    /// Clears the pool
    /// </summary>
    public static void PoolClear() => pool.Clear();
    /// <summary>
    /// If it is not practical to use the using clause
    /// You can return an object to the pool with this method.
    /// The using clause is preferred.
    /// </summary>
    public void Return() => Dispose();
    public void Dispose()
    {
        buf.Clear();
        pool.Return(this);
    }
    public void Clear()
    {
        buf.Clear();
    }
    public static int CalcSize(string output)
    {
        if (output == null) return 1;
        if (output.Length < 1) return 1;
        int strSize = System.Text.Encoding.UTF8.GetByteCount(output);
        return strSize += SizeSize(strSize);
    }
    //public static int CalcSize(BitArray ba)
    //{
    //    int bits = ba.Length;
    //    int nBytes = bits / 8;
    //    if ((bits % 8) > 0) nBytes++;
    //    int size = 1;
    //    size += nBytes;
    //    return size;
    //}
    public static int SizeSize(int size)
    {
        if (size >= -3 && size < 251) return 1;
        if (size >= 0 && size <= UInt16.MaxValue) return 3;
        return 5;
    }
    /// <summary>
    /// WriteSize can be used in situations where you now you are not going to be writeing
    /// large int values often. It will compress the output to use only 1 byte if the input size 
    /// is between -3 and 250. It will only use 3 bytes for numbers up to UInt16.MaxValue. A size value 
    /// greater than that will use 5 bytes, or in other words 1 byte more than if we used WriteInt instead. 
    /// </summary>
    /// <param name="size">the value you are writing. This method was first used for outputing size values, but it
    /// can be used for any situation where the int value will be small most of the time.</param>
    public void WriteSize(int size)
    {
        if (size >= 0 && size < 251)
        {
            Write((byte)size);
            return;
        }
        if (size >= 0 && size <= UInt16.MaxValue)
        {
            UInt16 osize = (UInt16)size;
            Write((byte)254);
            Write(osize);
            return;
        }
        if (size == -3) // it would break files if I added it later so I included an extra
        {
            Write((byte)251);
            return;
        }
        if (size == -2) // it would break files if I added it later so I included an extra
        {
            Write((byte)252);
            return;
        }
        if (size == -1) // we only use one byte for -1. -1 has special meaning and is used enough to get its on flag
        {
            Write((byte)253);
            return;
        }
        Write((byte)255);
        Write(size);
        return;
    }
    public bool TryReadSize(out int size)
    {
        size = 0;
        if (!buf.TryReadByte(out byte b)) return false;
        size = (int)b;
        if (size >= 0 && size < 251) return true;
        if (size == 254)
        {
            if (!TryReadUShort(out ushort val)) return false;
            size = (int)val;
            return true;
        }
        if (size == 251)
        {
            size = -3;
            return true;
        }
        if (size == 252)
        {
            size = -2;
            return true;
        }
        if (size == 253)
        {
            size = -1;
            return true;
        }
        if (!TryReadInt(out int ival)) return false;
        size = (int)ival;
        return true;
    }
    public int ReadSize()
    {
        int size = (int)buf.ReadByte();
        if (size >= 0 && size < 251) return size;
        if (size == 254) return ReadUShort();
        if (size == 251) return -3;
        if (size == 252) return -2;
        if (size == 253) return -1;
        return ReadInt();
    }
    public bool AtEnd => buf.AtEnd;
    #region Helper Methods
    void MoveBytes(int count)
    {
        buf.Read(reverse, 0, count);
        Util.Reverse(reverse, count);
    }
    void SkipBytes(int count) => buf.Skip(count);
    byte[] data = ByteArrayPool.Rent(1024);
    private void ReadData(int size)
    {
        if (data.Length < size)
        {
            ByteArrayPool.Return(data);
            data = ByteArrayPool.Rent(size);
        }
        buf.Read(data, 0, size);
        return;
    }
    private bool TryReadData(int size)
    {
        if (buf.BytesToRead < size) return false;
        if (data.Length < size)
        {
            ByteArrayPool.Return(data);
            data = ByteArrayPool.Rent(size);
        }
        buf.Read(data, 0, size);
        return true;
    }
    // This method is used to by pass the "Write" methods that always append data
    // to the buffer. You can manualy load data in an out of order fashion, but you may
    // need to manual adjust the writeIndex if you use this method.
    //
    // This method will move writeIndex to the position after the BlockCopy only if doing so
    // would increase the size of the buffer. It will not shrink the buffer.
    public void BlockCopy(byte[] src, int srcPos, int destPos, int size)
    {
        buf.BlockCopy(src, srcPos, destPos, size);
    }
    #endregion
    #region Read/Write methods
    public bool PeekByte(out byte outByte) => buf.PeekByte(out outByte);
    public bool PeekBytes(ByteArray ba, int count) => buf.PeekBytes(ba, count);
    public void Read(out byte val) => val = buf.ReadByte();
    public bool TryReadByte(out byte val) => buf.TryReadByte(out val);
    public byte ReadByte() => buf.ReadByte();
    public void Write(byte val) => buf.Write(val);
    public void Read(ByteArray ba)
    {
        ba.Clear();
        int size = ReadSize();
        if (size == 0) return;
        buf.Read(ba, size);
    }
    // Use this when an actual byte[] with a given size is needed. 
    // Prefer to use Read(ByteArray ba) when ever possible.
    public byte[] ReadByteArray()
    {
        int size = ReadSize();
        byte[] data = new byte[size];
        if (size > 0)
            buf.Read(data, 0, size);
        return data;
    }
    public void Write(ByteArray ba)
    {
        WriteSize(ba.BytesUsed);
        if (ba.BytesUsed > 0)
            buf.Write(ba);
    }
    // Use this when it fits to write out a byte[]. Sometimes that is what you have.
    // Prefer to use Write(ByteArray ba) when ever possible.
    public void Write(byte[] data, int offset, int count)
    {
        WriteSize(count);
        if (count > 0) buf.Write(data, offset, count);
    }
    public void Write(byte[] data)
    {
        WriteSize(data.Length);
        if (data.Length > 0) buf.Write(data, 0, data.Length);
    }
    public void WriteNoSize(Span<byte> data)
    {
        if (data.Length > 0) buf.Write(data);
    }
    public void WriteNoSize(byte[] data)
    {
        if (data.Length > 0) buf.Write(data, 0, data.Length);
    }
    public void WriteNoSize(byte[] data, int offset, int count)
    {
        if (count > 0) buf.Write(data, offset, count);
    }
    public void Skip(byte _)
    {
        SkipBytes(1);
    }
    public void Skip(ByteArray _)
    {
        int size = ReadSize();
        SkipBytes(size);
    }
    public void Skip(byte[] _)
    {
        int size = ReadSize();
        SkipBytes(size);
    }

    public bool ReadBool()
    {
        if (buf.ReadByte() == 0) return false;
        return true;
    }
    public void Read(out bool oval) => oval = ReadBool();
    public void Read(List<bool> outvar)
    {
        outvar.Clear();
        int count = ReadSize();
        for (int i = 0; i < count; i++)
        {
            outvar.Add(ReadBool());
        }
    }
    public void Write(bool val)
    {
        if (val)
            buf.Write((byte)1);
        else
            buf.Write((byte)0);
    }
    public void Write(List<bool> list)
    {
        WriteSize(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            Write(list[i]);
        }
    }
    public void Skip(bool _)
    {
        SkipBytes(1);
    }
    public void Skip(List<bool> _)
    {
        int size = ReadSize();
        SkipBytes(size);
    }

    public DateTime ReadDateTime()
    {
        MoveBytes(8);
        long dateval;
        dateval = BitConverter.ToInt64(reverse, 0);
        return DateTime.FromBinary(dateval);
    }
    public void Read(out DateTime val) => val = ReadDateTime();
    public void Read(List<DateTime> list)
    {
        int count = ReadSize();
        list.Clear();
        for (int i = 0; i < count; i++)
            list.Add(ReadDateTime());
    }
    public void Write(DateTime val)
    {
        Span<byte> data = stackalloc byte[8];
        long lval = val.ToBinary();
        MemoryMarshal.Write<long>(data, ref lval);
        Util.Reverse(data);
        buf.Append(data);
    }
    public void Write(List<DateTime> data)
    {
        WriteSize(data.Count);
        for (int i = 0; i < data.Count; i++)
        {
            Write(data[i]);
        }
    }
    public void Skip(DateTime d)
    {
        SkipBytes(8);
    }
    public void Skip(List<DateTime> _)
    {
        int size = ReadSize() * 8;
        SkipBytes(size);
    }

    public Godot.Color ReadColor()
    {
        MoveBytes(4);
        uint c = BitConverter.ToUInt32(reverse, 0);
        return new Godot.Color(c);
    }
    public void Read(out Godot.Color color) => color = ReadColor();
    public void Read(List<Godot.Color> list)
    {
        int count = ReadSize();
        list.Clear();
        for (int i = 0; i < count; i++)
            list.Add(ReadColor());

    }
    public void Write(Godot.Color color)
    {
        uint cval = color.ToRgba32();
        Write(cval);
    }
    public void Write(List<Godot.Color> list)
    {
        WriteSize(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            Write(list[i]);
        }
    }
    public void Skip(Godot.Color _)
    {
        SkipBytes(4);
    }
    public void Skip(List<Godot.Color> _)
    {
        int size = ReadSize() * 4;
        SkipBytes(size);
    }

    public bool TryReadString(out string val)
    {
        val = string.Empty;
        if (!TryReadSize(out int size)) return false;
        if (size == 0) return true;
        if (!TryReadData(size)) return false;
        val = Encoding.UTF8.GetString(data, 0, size);
        return true;
    }
    public string ReadString()
    {
        int size = ReadSize();
        if (size == 0) return string.Empty;
        ReadData(size);
        return Encoding.UTF8.GetString(data, 0, size);
    }
    public void Read(out string val) => val = ReadString();
    public void Read(List<string> data)
    {
        int count = ReadSize();
        data.Clear();
        for (int i = 0; i < count; i++)
            data.Add(ReadString());
    }
    public void Write(string output)
    {
        if (output.Length < 1)
        {
            Write((byte)0);
            return;
        }
        int byteCount = Encoding.UTF8.GetByteCount(output);
        if (data.Length < byteCount)
        {
            ByteArrayPool.Return(data);
            data = ByteArrayPool.Rent(byteCount);
        }
        Encoding.UTF8.GetBytes(output.AsSpan(), data.AsSpan());
        WriteSize(byteCount);
        buf.Write(data, 0, byteCount);
    }
    public void Write(List<string> data)
    {
        WriteSize(data.Count);
        for (int i = 0; i < data.Count; i++)
        {
            Write(data[i]);
        }
    }
    public void Skip(string _)
    {
        int size = ReadSize();
        SkipBytes(size);
    }
    public void Skip(List<string> _)
    {
        int size = ReadSize();
        for (int i = 0; i < size; i++)
            Skip("");
    }

    public short ReadShort()
    {
        MoveBytes(2);
        return BitConverter.ToInt16(reverse, 0);
    }
    public void Read(out short val) => val = ReadShort();
    public void Read(List<short> list)
    {
        int count = ReadSize();
        list.Clear();
        for (int i = 0; i < count; i++)
            list.Add(ReadShort());
    }
    public void Write(short val)
    {
        Span<byte> data = stackalloc byte[2];
        MemoryMarshal.Write<short>(data, ref val);
        Util.Reverse(data);
        buf.Append(data);
    }
    public void Write(List<short> data)
    {
        WriteSize(data.Count);
        for (int i = 0; i < data.Count; i++)
        {
            Write(data[i]);
        }
    }
    public void Skip(short _)
    {
        SkipBytes(2);
    }
    public void Skip(List<short> _)
    {
        int size = ReadSize() * 2;
        SkipBytes(size);
    }

    public bool TryReadUShort(out ushort result)
    {
        result = 0;
        Span<byte> data = stackalloc byte[2];
        if (!buf.TryRead(data)) return false;
        Util.Reverse(data);
        result = MemoryMarshal.Read<ushort>(data);
        return true;
    }
    public ushort ReadUShort()
    {
        MoveBytes(2);
        return BitConverter.ToUInt16(reverse, 0);
    }
    public void Read(out ushort val) => val = ReadUShort();
    public void Read(List<ushort> list)
    {
        int count = ReadSize();
        list.Clear();
        for (int i = 0; i < count; i++)
            list.Add(ReadUShort());
    }
    public void Write(ushort val)
    {
        Span<byte> data = stackalloc byte[2];
        MemoryMarshal.Write<ushort>(data, ref val);
        Util.Reverse(data);
        buf.Append(data);
    }
    public void Write(List<ushort> data)
    {
        WriteSize(data.Count);
        for (int i = 0; i < data.Count; i++)
        {
            Write(data[i]);
        }
    }
    public void Skip(ushort _)
    {
        SkipBytes(2);
    }
    public void Skip(List<ushort> _)
    {
        int size = ReadSize() * 2;
        SkipBytes(size);
    }

    public bool TryReadInt(out int result)
    {
        result = 0;
        Span<byte> reverse = stackalloc byte[4];
        if (!buf.TryRead(reverse)) return false;
        Util.Reverse(reverse);
        result = MemoryMarshal.Read<int>(reverse);
        //result = BitConverter.ToInt32(reverse, 0);
        return true;
    }
    public int ReadInt()
    {
        MoveBytes(4);
        return BitConverter.ToInt32(reverse, 0);
    }
    public void Read(out int val) => val = ReadInt();
    public void Read(List<int> data)
    {
        int count = ReadSize();
        data.Clear();
        for (int i = 0; i < count; i++)
            data.Add(ReadInt());
    }
    public void Write(int val)
    {
        Span<byte> data = stackalloc byte[4];
        MemoryMarshal.Write<int>(data, ref val);
        Util.Reverse(data);
        buf.Append(data);
    }
    public void Write(List<int> data)
    {
        WriteSize(data.Count);
        for (int i = 0; i < data.Count; i++)
        {
            Write(data[i]);
        }
    }
    public void Skip(int _)
    {
        SkipBytes(4);
    }
    public void Skip(List<int> _)
    {
        int size = ReadSize() * 4;
        SkipBytes(size);
    }

    public uint ReadUInt()
    {
        MoveBytes(4);
        return BitConverter.ToUInt32(reverse, 0);
    }
    public void Read(out uint val) => val = ReadUInt();
    public void Read(List<uint> data)
    {
        int count = ReadSize();
        data.Clear();
        for (int i = 0; i < count; i++)
            data.Add(ReadUInt());
    }
    public void Write(uint val)
    {
        Span<byte> data = stackalloc byte[4];
        MemoryMarshal.Write<uint>(data, ref val);
        Util.Reverse(data);
        buf.Append(data);
    }
    public void Write(List<uint> data)
    {
        WriteSize(data.Count);
        for (int i = 0; i < data.Count; i++)
        {
            Write(data[i]);
        }
    }
    public void Skip(uint _)
    {
        SkipBytes(4);
    }
    public void Skip(List<uint> _)
    {
        int size = ReadSize() * 4;
        SkipBytes(size);
    }

    public long ReadLong()
    {
        MoveBytes(8);
        return BitConverter.ToInt64(reverse, 0);
    }
    public void Read(out long val) => val = ReadLong();
    public void Read(List<long> list)
    {
        int count = ReadSize();
        list.Clear();
        for (int i = 0; i < count; i++)
            list.Add(ReadLong());
    }
    public void Write(long val)
    {
        Span<byte> data = stackalloc byte[8];
        MemoryMarshal.Write<long>(data, ref val);
        Util.Reverse(data);
        buf.Append(data);
    }
    public void Write(List<long> data)
    {
        WriteSize(data.Count);
        for (int i = 0; i < data.Count; i++)
        {
            Write(data[i]);
        }
    }
    public void Skip(long _)
    {
        SkipBytes(8);
    }
    public void Skip(List<long> _)
    {
        int size = ReadSize() * 8;
        SkipBytes(size);
    }

    public ulong ReadULong()
    {
        MoveBytes(8);
        return BitConverter.ToUInt64(reverse, 0);
    }
    public void Read(out ulong val) => val = ReadULong();
    public void Read(List<ulong> list)
    {
        int count = ReadSize();
        list.Clear();
        for (int i = 0; i < count; i++)
            list.Add(ReadULong());
    }
    public void Write(ulong val)
    {
        Span<byte> data = stackalloc byte[8];
        MemoryMarshal.Write<ulong>(data, ref val);
        Util.Reverse(data);
        buf.Append(data);
    }
    public void Write(List<ulong> data)
    {
        WriteSize(data.Count);
        for (int i = 0; i < data.Count; i++)
        {
            Write(data[i]);
        }
    }
    public void Skip(ulong _)
    {
        SkipBytes(8);
    }
    public void Skip(List<ulong> _)
    {
        int size = ReadSize() * 8;
        SkipBytes(size);
    }

    public float ReadFloat()
    {
        MoveBytes(4);
        return BitConverter.ToSingle(reverse, 0);
    }
    public void Read(out float val) => val = ReadFloat();
    public void Read(List<float> outvar)
    {
        outvar.Clear();
        int count = ReadSize();
        for (int i = 0; i < count; i++)
        {
            outvar.Add(ReadFloat());
        }
    }
    public void Write(float val)
    {
        Span<byte> data = stackalloc byte[4];
        MemoryMarshal.Write<float>(data, ref val);
        Util.Reverse(data);
        buf.Append(data);
    }
    public void Write(List<float> l)
    {
        WriteSize(l.Count);
        for (int i = 0; i < l.Count; i++)
        {
            Write(l[i]);
        }
    }
    public void Skip(float _)
    {
        SkipBytes(4);
    }
    public void Skip(List<float> _)
    {
        int size = ReadSize() * 4;
        SkipBytes(size);
    }

    public double ReadDouble()
    {
        MoveBytes(8);
        return BitConverter.ToDouble(reverse, 0);
    }
    public void Read(out double val) => val = ReadDouble();
    public void Read(List<double> list)
    {
        int count = ReadSize();
        list.Clear();
        for (int i = 0; i < count; i++)
            list.Add(ReadDouble());
    }
    public void Write(double val)
    {
        Span<byte> data = stackalloc byte[8];
        MemoryMarshal.Write<double>(data, ref val);
        Util.Reverse(data);
        buf.Append(data);
    }
    public void Write(List<double> data)
    {
        WriteSize(data.Count);
        for (int i = 0; i < data.Count; i++)
        {
            Write(data[i]);
        }
    }
    public void Skip(double _)
    {
        SkipBytes(8);
    }
    public void Skip(List<double> _)
    {
        int size = ReadSize() * 8;
        SkipBytes(size);
    }

    public Vector2 ReadVector2()
    {
        var x = ReadFloat();
        var y = ReadFloat();
        return new Vector2(x, y);
    }
    public void Read(out Vector2 val) => val = ReadVector2();
    public void Read(List<Vector2> outvar)
    {
        outvar.Clear();
        int count = ReadSize();
        for (int i = 0; i < count; i++)
        {
            outvar.Add(ReadVector2());
        }
    }
    public void Write(Vector2 output)
    {
        Write(output.X);
        Write(output.Y);
    }
    public void Write(List<Vector2> l)
    {
        WriteSize(l.Count);
        for (int i = 0; i < l.Count; i++)
        {
            Write(l[i]);
        }
    }
    public void Skip(Vector2 _)
    {
        SkipBytes(8);
    }
    public void Skip(List<Vector2> _)
    {
        int size = ReadSize() * 8;
        SkipBytes(size);
    }

    public Vector3 ReadVector3()
    {
        var x = ReadFloat();
        var y = ReadFloat();
        var z = ReadFloat();
        return new Vector3(x, y, z);
    }
    public void Read(out Vector3 val) => val = ReadVector3();
    public void Read(List<Vector3> outvar)
    {
        outvar.Clear();
        int count = ReadSize();
        for (int i = 0; i < count; i++)
        {
            outvar.Add(ReadVector3());
        }
    }
    public void Write(Vector3 output)
    {
        Write(output.X);
        Write(output.Y);
        Write(output.Z);
    }
    public void Write(List<Vector3> l)
    {
        WriteSize(l.Count);
        for (int i = 0; i < l.Count; i++)
        {
            Write(l[i]);
        }
    }
    public void Skip(Vector3 _)
    {
        SkipBytes(12);
    }
    public void Skip(List<Vector3> _)
    {
        int size = ReadSize() * 12;
        SkipBytes(size);
    }

    public Vector2I ReadVector2I()
    {
        var x = ReadInt();
        var y = ReadInt();
        return new Vector2I(x, y);
    }
    public void Read(out Vector2I val) => val = ReadVector2I();
    public void Read(List<Vector2I> outvar)
    {
        outvar.Clear();
        int count = ReadSize();
        for (int i = 0; i < count; i++)
        {
            outvar.Add(ReadVector2I());
        }
    }
    public void Write(Vector2I output)
    {
        Write(output.X);
        Write(output.Y);
    }
    public void Write(List<Vector2I> l)
    {
        WriteSize(l.Count);
        for (int i = 0; i < l.Count; i++)
        {
            Write(l[i]);
        }
    }
    public void Skip(Vector2I _)
    {
        SkipBytes(8);
    }
    public void Skip(List<Vector2I> _)
    {
        int size = ReadSize() * 8;
        SkipBytes(size);
    }

    public Vector3I ReadVector3I()
    {
        var x = ReadInt();
        var y = ReadInt();
        var z = ReadInt();
        return new Vector3I(x, y, z);
    }
    public void Read(out Vector3I val) => val = ReadVector3I();
    public void Read(List<Vector3I> outvar)
    {
        outvar.Clear();
        int count = ReadSize();
        for (int i = 0; i < count; i++)
        {
            outvar.Add(ReadVector3I());
        }
    }
    public void Write(Vector3I output)
    {
        Write(output.X);
        Write(output.Y);
        Write(output.Z);
    }
    public void Write(List<Vector3I> l)
    {
        WriteSize(l.Count);
        for (int i = 0; i < l.Count; i++)
        {
            Write(l[i]);
        }
    }
    public void Skip(Vector3I _)
    {
        SkipBytes(12);
    }
    public void Skip(List<Vector3I> _)
    {
        int size = ReadSize() * 12;
        SkipBytes(size);
    }

    public Vector4I ReadVector4I()
    {
        var x = ReadInt();
        var y = ReadInt();
        var z = ReadInt();
        var w = ReadInt();
        return new Vector4I(x, y, z, w);
    }
    public void Read(out Vector4I val) => val = ReadVector4I();
    public void Read(List<Vector4I> outvar)
    {
        outvar.Clear();
        int count = ReadSize();
        for (int i = 0; i < count; i++)
        {
            outvar.Add(ReadVector4I());
        }
    }
    public void Write(Vector4I output)
    {
        Write(output.X);
        Write(output.Y);
        Write(output.Z);
        Write(output.W);
    }
    public void Write(List<Vector4I> l)
    {
        WriteSize(l.Count);
        for (int i = 0; i < l.Count; i++)
        {
            Write(l[i]);
        }
    }
    public void Skip(Vector4I _)
    {
        SkipBytes(16);
    }
    public void Skip(List<Vector4I> _)
    {
        int size = ReadSize() * 16;
        SkipBytes(size);
    }

    public Quaternion ReadQuaternion()
    {
        float x;
        Read(out x);
        float y;
        Read(out y);
        float z;
        Read(out z);
        float w;
        Read(out w);
        return new Quaternion(x, y, z, w);
    }
    public void Read(out Quaternion val) => val = ReadQuaternion();
    public void Read(List<Quaternion> list)
    {
        int count = ReadSize();
        list.Clear();
        for (int i = 0; i < count; i++)
            list.Add(ReadQuaternion());
    }
    public void Write(Quaternion output)
    {
        Write(output.X);
        Write(output.Y);
        Write(output.Z);
        Write(output.W);
    }
    public void Write(List<Quaternion> data)
    {
        WriteSize(data.Count);
        for (int i = 0; i < data.Count; i++)
        {
            Write(data[i]);
        }
    }
    public void Skip(Quaternion _)
    {
        SkipBytes(16);
    }
    public void Skip(List<Quaternion> _)
    {
        int size = ReadSize() * 16;
        SkipBytes(size);
    }

    public decimal ReadDecimal()
    {
        int[] vals = new int[4];
        Read(out vals[0]);
        Read(out vals[1]);
        Read(out vals[2]);
        Read(out vals[3]);
        return new decimal(vals);
    }
    public void Read(out decimal val) => val = ReadDecimal();
    public void Read(List<decimal> list)
    {
        int count = ReadSize();
        list.Clear();
        for (int i = 0; i < count; i++)
            list.Add(ReadDecimal());
    }
    public void Write(decimal output)
    {
        var vals = decimal.GetBits(output);
        Write(vals[0]);
        Write(vals[1]);
        Write(vals[2]);
        Write(vals[3]);
    }
    public void Write(List<decimal> data)
    {
        WriteSize(data.Count);
        for (int i = 0; i < data.Count; i++)
        {
            Write(data[i]);
        }
    }
    public void Skip(decimal _)
    {
        SkipBytes(16);
    }
    public void Skip(List<decimal> _)
    {
        int size = ReadSize() * 16;
        SkipBytes(size);
    }

    #endregion
    #region Misc
    public void CopyTo(SerializationBuffer toBuf)
    {
        toBuf.buf.Resize(buf.Length);
        toBuf.buf.Clear();
        Buffer.BlockCopy(buf.Data, 0, toBuf.buf.Data, 0, buf.Length);
        toBuf.buf.SetReadIndex(buf.ReadIndex);
        toBuf.buf.SetWriteIndex(buf.WriteIndex);
    }


    #endregion
}
