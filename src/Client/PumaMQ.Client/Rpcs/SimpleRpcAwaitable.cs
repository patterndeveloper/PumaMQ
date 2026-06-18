using PumaMQ.Client.Framings;

namespace PumaMQ.Client.Rpcs;

internal class SimpleRpcAwaitable : RpcAwaitable<bool>
{
    public SimpleRpcAwaitable(ClassMethod classMethod, TimeSpan timeout, CancellationToken userCancellationToken) : base(classMethod, timeout, userCancellationToken)
    {
    }


    public override void Complete(L2Frame l2Frame)
    {
        if (l2Frame.ClassMethod == _classMethod)
        {
            _taskComSrc.SetResult(true);
        }
        else
        {
            string message = $"RPC expecting frame with ClassMethod: {_classMethod}, but actual ClassMethod is {l2Frame.ClassMethod}";
            InvalidOperationException exception = new InvalidOperationException(message);
            _taskComSrc.SetException(exception);
        }
    }
}