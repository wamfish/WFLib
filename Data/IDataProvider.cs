namespace WFLib;

public interface IDataProvider<D> : IDisposable where D : Data, new()
{
    DSList<D> Read(int skip, int take, FilterList filters = null, SortList sortFields = null);
    bool Add(D data);
    bool Update(D data);
    bool Delete(D data);
    D RentData();
}