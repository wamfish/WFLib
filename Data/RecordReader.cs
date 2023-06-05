namespace WFLib;
public partial class RecordReader<R> where R : Record, new()
{
    readonly RecordContext<R> ctx;
    public string TableName => ctx.TableName;
    public RecordReader()
    {
        ctx = RecordContextFactory<R>.Rent();
    }
    public Memory<byte> ReadRecord(int id)
    {
        var rec = ctx.RentRecord();
        if (ctx.Read(rec, id) == Status.Ok)
        {
            Console.WriteLine($"Read {TableName}:{id}");
            SerializationBuffer b = SerializationBuffer.Rent();
            //ctx.Serialize(data, b);
            rec.WriteToBuf(b);
            ctx.ReturnRecord(rec);
            var result = b.Buf.Data.AsMemory().Slice(0, b.Buf.BytesUsed);
            return result;
        }
        return Memory<byte>.Empty;
    }
}
