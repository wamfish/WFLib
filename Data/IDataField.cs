namespace WFLib
{
    public interface IDataField
    {
        Data this[int i] { get; set; }

        
        int Count { get; }
        int CurrentIndex { get; }
        Data Data { get; }

        event Action OnInit;

        int Add(Data data);
        string AsString();
        void Clear();
        void CopyTo(object to);
        Data Create();
        bool DataIsDefault(Data data);
        void FromString(string value);
        void Init();
        bool IsDefault();
        bool IsEqualTo(object df);
        void ReadFromBuf(SerializationBuffer sb);
        bool Remove(Data data);
        bool RemoveAt(int index);
        void Skip(SerializationBuffer sb);
        void WriteToBuf(SerializationBuffer sb);
        void XInit();
    }
}