using PumaMQ.Client.Framings;
using System.Runtime.CompilerServices;

namespace PumaMQ.Client.Rpcs;

internal abstract class RpcAwaitable<TResult> : IDisposable
{
    protected readonly TaskCompletionSource<TResult> _taskComSrc;
    private readonly ConfiguredTaskAwaitable<TResult> _configuredTaskAwaitable;

    private readonly TimeSpan _timeout;
    private readonly CancellationToken _timeoutCt;
    private readonly CancellationTokenSource _timeoutCts;
    private readonly CancellationTokenRegistration _timeoutCtr;

    private readonly CancellationTokenSource _linkedCts;
    private readonly CancellationToken _linkedCt;
    public CancellationToken LinkedCancellationToken => _linkedCt;

    protected readonly ClassMethod _classMethod;

    private int _disposed;


    internal RpcAwaitable(ClassMethod classMethod, TimeSpan timeout, CancellationToken userCancellationToken)
    {
        _taskComSrc = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        _configuredTaskAwaitable = _taskComSrc.Task.ConfigureAwait(false);

        _timeout = timeout;
        _timeoutCts = new(_timeout);
        _timeoutCt = _timeoutCts.Token;
        _timeoutCtr = _timeoutCt.UnsafeRegister(TimeoutCtCallback, _taskComSrc);

        _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCancellationToken, _timeoutCt);
        _linkedCt = _linkedCts.Token;

        _classMethod = classMethod;
    }


    internal abstract void Complete(L2Frame l2Frame);


    internal ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter GetAwaiter()
    {
        ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter awaiter = _configuredTaskAwaitable.GetAwaiter();
        return awaiter;
    }


    private void TimeoutCtCallback(object? state)
    {
        TaskCompletionSource<TResult> taskComSrc = (TaskCompletionSource<TResult>)state!;
        taskComSrc.TrySetCanceled();
    }


    public void Dispose()
    {
        int disposed = Interlocked.CompareExchange(ref _disposed, 1, 0);

        if (disposed == 0)
        {
            _timeoutCtr.Dispose();
            _timeoutCts.Dispose();
            _linkedCts.Dispose();
        }
    }
}