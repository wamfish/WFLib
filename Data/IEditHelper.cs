namespace WFLib;
public interface IEditHelper : IDisposable
{
    public Task SetValidState(bool isValid);
}