//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public class ByteArray
{
    static Queue<ByteArray> baPool = new Queue<ByteArray>();
    private ByteArray()
    {
        Clear();
    }
    bool isSpecial = false;
    /// <summary>
    /// This is a special case method for the tunnel server.  It allows the server to use a buffer that is already allocated.
    /// Do not use this method unless you know what you are doing, or at least treat it as a readonly ByteArray.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static ByteArray RentSpecialReadonly(byte[] buffer, int offset, int size)
    {
        ByteArray ba = Rent();
        ba.isSpecial = true;
        ByteArrayPool.Return(ba.data);
        ba.data = buffer;
        ba.readIndex = offset;
        ba.writeIndex = offset + size;
        return ba;
    }
    public static ByteArray Rent()
    {
        lock (baPool)
        {
            ByteArray ba;
            if (baPool.Count > 0)
                ba = baPool.Dequeue();
            else
                ba = new ByteArray();
            return ba;
        }
    }
    public void Return()
    {
        if (isSpecial)
        {
            // We don't own the buffer, so don't return it to the pool.
            data = ByteArrayPool.Empty;
            isSpecial = false;
        }
        Clear();
        lock (baPool)
        {
            baPool.Enqueue(this);
        }
    }
    private byte[] data = ByteArrayPool.Empty;
    public byte[] Data => data;
    public Memory<byte> AsMemory => data.AsMemory<byte>();
    public int BytesToRead => writeIndex - readIndex;
    public int BytesAvailable => Length - writeIndex;
    public int BytesUsed => writeIndex;
    private int readIndex;
    public int ReadIndex => readIndex;
    private int writeIndex = 0;
    public int WriteIndex => writeIndex;
    public int Length => Data.Length;
    public bool AtEnd => readIndex >= writeIndex;
    /// <summary>
    /// Prepares this ByteArrray for reuse. This method does not clear the data in the buffer.
    /// </summary>
    public void Clear()
    {
        //data = ByteArrayPool.Return(data);
        readIndex = 0;
        writeIndex = 0;
    }
    /// <summary>
    ///
    /// Resize this ByteArray to the new size. All data that will fit in the new buffer
    /// will be copied. Existing data may be truncated if reqSize is smaller than
    /// the current size.
    /// 
    /// </summary>
    /// <param name="reqSize"></param>
    public void Resize(int reqSize)
    {
        var saveData = data;
        data = ByteArrayPool.RentBlock(reqSize);
        if (writeIndex > Length)
            writeIndex = Length;
        Buffer.BlockCopy(saveData, 0, data, 0, WriteIndex);
        ByteArrayPool.Return(saveData);
    }
    /// <summary>
    /// Append the data in the buffer to the end of this ByteArray.
    /// </summary>
    /// <param name="buffer">the data to append</param>
    public void Append(Span<byte> buffer)
    {
        int reqSize = writeIndex + buffer.Length;
        if (reqSize > Length)
        {
            Resize(reqSize);
        }
        buffer.CopyTo(data.AsSpan(writeIndex, buffer.Length));
        writeIndex += buffer.Length;
    }
    /// <summary>
    /// 
    /// This method is used to bypass the "Write" methods that always append data
    /// to the buffer. You can manualy load data in an out of order fashion, but you may
    /// need to manual adjust the writeIndex when using this method.
    ///
    /// This method will move writeIndex to the position after the BlockCopy only if doing so
    /// would increase the size of the buffer. It will not shrink the buffer./// 
    /// 
    /// </summary>
    /// <param name="src">the byte[] to copy into this ByteArray</param>
    /// <param name="srcPos">the position in the src byte[] to start from </param>
    /// <param name="destPos">the position in the ByteArray's buffer to copy to</param>
    /// <param name="size">the number of bytes to copy into this ByteArray</param>
    public void BlockCopy(byte[] src, int srcPos, int destPos, int size)
    {
        int reqSize = destPos + size;
        if (reqSize > Length)
        {
            Resize(reqSize);
        }
        Buffer.BlockCopy(src, srcPos, data, destPos, size);
        int newEnd = destPos + size;
        //this is line does not work for all possible
        if (newEnd > writeIndex) writeIndex = newEnd;
    }
    /// <summary>
    /// Appends buffer to this ByteArray.
    /// </summary>
    /// <param name="buffer">the buffer to copy into this ByteArray.</param>
    /// <param name="offset">the offset in buffer to copy from.</param>
    /// <param name="count">the number of bytes to copy</param>
    public void Write(byte[] buffer, int offset, int count)
    {
        if (offset >= buffer.Length) return; //no error, but we do nothing
        if (offset + count > buffer.Length) //if count goes past the end of the buffer
            count = buffer.Length - offset; //adjust count to fit
        int reqSize = writeIndex + count;
        if (reqSize > Length)
        {
            Resize(reqSize);
        }
        Buffer.BlockCopy(buffer, offset, data, WriteIndex, count);
        writeIndex += count;
    }
    /// <summary>
    /// Appends Span<byte> to this ByteArray.
    /// </summary>
    /// <param name="src">the span to append</param>
    public void Write(Span<byte> src)
    {
        int reqSize = writeIndex + src.Length;
        if (reqSize > Length)
        {
            Resize(reqSize);
        }
        src.CopyTo(data.AsSpan(writeIndex, src.Length));
        writeIndex += src.Length;
    }
    /// <summary>
    /// Append a byte to this ByteArray.
    /// </summary>
    /// <param name="b"></param>
    public void Write(byte b)
    {
        // Length+1 is misleading. Resize grows the buf in chunks
        // This will automatically grow to the next chunk size
        if (BytesAvailable < 1) Resize(Length + 1);
        data[writeIndex++] = b;
    }
    /// <summary>
    /// Append a ByteArray to this ByteArray.
    /// </summary>
    /// <param name="src">the ByteArray to append</param>
    public void Write(ByteArray src)
    {
        if (BytesAvailable < src.BytesUsed) Resize(Length + (src.BytesUsed - BytesAvailable));
        Write(src.data, 0, src.BytesUsed);
    }
    /// <summary>
    /// Read a byte from this ByteArray.
    /// </summary>
    /// <param name="outByte">the byte read</param>
    /// <returns>true: if there was a byte to read. false: if we have no more bytes to read</returns>
    public bool TryReadByte(out byte outByte)
    {
        outByte = 0;
        if (BytesToRead < 1) return false;
        outByte = data[readIndex++];
        return true;
    }
    /// <summary>
    /// Read a byte from this ByteArray.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">thown if we are at the end of our buffer</exception>
    public byte ReadByte()
    {
        if (BytesToRead < 1) throw new Exception("EOB");
        return data[readIndex++];
    }
    /// <summary>
    /// Check to see what the next byte is without advancing the readIndex.
    /// </summary>
    /// <param name="outByte">the next byte to be returned from ReadByte()</param>
    /// <returns>true: if we have bytes left to read</returns>
    public bool PeekByte(out byte outByte)
    {
        outByte = 0;
        if (BytesToRead < 1) return false;
        outByte = data[readIndex];
        return true;
    }
    /// <summary>
    /// Read more than one byte from this ByteArray with out advancing the readIndex.
    /// </summary>
    /// <param name="peekBytes">will contain the requested bytes</param>
    /// <param name="count">how many bytes to copy to peekBytes</param>
    /// <returns>true: if there are at least count bytes left to read</returns>
    public bool PeekBytes(ByteArray peekBytes, int count)
    {
        peekBytes.Clear();
        if (count > BytesToRead) return false;
        peekBytes.Write(data, readIndex, count);
        return true;
    }
    /// <summary>
    /// Reads buffer.Length bytes from this ByteArray.
    /// </summary>
    /// <param name="outBytes"></param>
    /// <returns>true: if we have at least buffer.Length bytes to read</returns>
    public bool TryRead(Span<byte> outBytes)
    {
        if (outBytes.Length > BytesToRead) return false;
        var sd = data.AsSpan().Slice(readIndex, outBytes.Length);
        sd.CopyTo(outBytes);
        readIndex += outBytes.Length;
        return true;
    }
    /// <summary>
    /// Read count bytes into buffer from this ByteArray.
    /// </summary>
    /// <param name="buffer">the buffer to read into</param>
    /// <param name="offset">the offset to read into at</param>
    /// <param name="count">the number of bytes to read</param>
    /// <exception cref="Exception"></exception>
    public void Read(byte[] buffer, int offset, int count)
    {
        if (buffer.Length < offset + count) throw new ArgumentException("invalid arguments");
        if (count > BytesToRead) throw new Exception("No data");
        Buffer.BlockCopy(data, readIndex, buffer, offset, count);
        readIndex += count;
    }
    /// <summary>
    /// Read count bytes into ba from this ByteArray.
    /// </summary>
    /// <param name="ba"></param>
    /// <param name="count"></param>
    /// <exception cref="Exception"></exception>
    public void Read(ByteArray ba, int count)
    {
        if (count > BytesToRead) throw new Exception("No data");
        ba.Write(data, readIndex, count);
        readIndex += count;
    }
    /// <summary>
    /// Read span.Length bytes into span from this ByteArray.
    /// </summary>
    /// <param name="span"></param>
    /// <exception cref="Exception">if requesting more bytes than we have left</exception>
    public void Read(Span<byte> span)
    {
        if (span.Length > BytesToRead) throw new Exception("No data");
        var dspan = data.AsSpan(readIndex, span.Length);
        dspan.CopyTo(span);
        readIndex += span.Length;
    }
    /// <summary>
    /// Advance the readindex by count bytes.
    /// </summary>
    /// <param name="count"></param>
    /// <exception cref="Exception"></exception>
    public void Skip(int count)
    {
        if (count > BytesToRead) throw new Exception("No data");
        readIndex += count;
    }
    /// <summary>
    /// Manually set the readIndex. This will not allow the readIndex to be set past the writeIndex.
    /// </summary>
    /// <param name="index"></param>
    public void SetReadIndex(int index)
    {
        if (index < 0) index = 0;
        readIndex = index;
        if (writeIndex < readIndex)
            writeIndex = readIndex;
    }
    /// <summary>
    /// Manually set the writeIndex. This will not allow the writeIndex to be set in front of readIndex.
    /// </summary>
    /// <param name="index"></param>
    public void SetWriteIndex(int index)
    {
        if (index < 0) index = 0;
        if (index > Length) index = Length;
        writeIndex = index;
        if (writeIndex < readIndex)
            readIndex = writeIndex;
    }
}
