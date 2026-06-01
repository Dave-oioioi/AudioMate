namespace AudioMate.Core.Runtime;

public sealed class SingleInstanceGuard : IDisposable
{
    private readonly Mutex _mutex;
    private bool _disposed;

    private SingleInstanceGuard(Mutex mutex, bool hasOwnership)
    {
        _mutex = mutex;
        HasOwnership = hasOwnership;
    }

    public bool HasOwnership { get; }

    public static SingleInstanceGuard TryAcquire(string instanceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceName);

        var mutex = new Mutex(initiallyOwned: true, instanceName, out var createdNew);

        return new SingleInstanceGuard(mutex, createdNew);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (HasOwnership)
        {
            _mutex.ReleaseMutex();
        }

        _mutex.Dispose();
        _disposed = true;
    }
}
