namespace Memory_Monitor
{
    public interface IMonitor : IDisposable
    {
        bool IsAvailable { get; }
    }
}
