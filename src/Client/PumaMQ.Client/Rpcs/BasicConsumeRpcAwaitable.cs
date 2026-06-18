using PumaMQ.Client.Consumers;
using PumaMQ.Client.Framings;

namespace PumaMQ.Client.Rpcs;

internal class BasicConsumeRpcAwaitable : RpcAwaitable<string>
{
    private readonly Consumer _consumer;

    public BasicConsumeRpcAwaitable(ClassMethod classMethod,
                                    Consumer consumer,
                                    TimeSpan timeout,
                                    CancellationToken userCancellationToken) : base(classMethod, timeout, userCancellationToken)
    {
        _consumer = consumer;
    }


    public override async void Complete(L2Frame l2Frame)
    {
        if(l2Frame.ClassMethod == ClassMethod.BasicConsumeOk)
        {
            BasicConsumeOk basicConsumeOk = new BasicConsumeOk(l2Frame);
            _taskComSrc.SetResult(basicConsumeOk.ConsumerTag);

            //Todo: verirfy moving to channel
            await _consumer.HandleBasicConsumeOk(basicConsumeOk.ConsumerTag).ConfigureAwait(false);
        }
        else
        {
            string message = $"RPC expecting frame with ClassMethod: {_classMethod}, but actual ClassMethod is {l2Frame.ClassMethod}";
            InvalidOperationException exception = new InvalidOperationException(message);
            _taskComSrc.SetException(exception);
        }
    }
}