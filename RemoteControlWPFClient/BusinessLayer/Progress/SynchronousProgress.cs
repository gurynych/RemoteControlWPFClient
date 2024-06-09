using System;

namespace RemoteControlWPFClient.BusinessLayer.Progress;

public sealed class SynchronousProgress<T> : IProgress<T>
{
    private readonly Action<T> callback;

    public SynchronousProgress(Action<T> callback)
    {
        this.callback = callback;
    }
    
    void IProgress<T>.Report(T data) => callback(data);
}