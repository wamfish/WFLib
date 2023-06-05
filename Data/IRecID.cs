namespace WFLib
{
    public interface IRecID 
    {
        int ID { get; set; }
        string AsString();
        void Clear();
        bool RefreshRec();
        string RecAsString();
        RecordList GetList(User user, string filter, int skip, int take);
    }
}