using PumaMQ.Client.Framings;

namespace PumaMQ.Client.Services;

public partial class Channel
{
    internal async Task HandleReceivedFrameAsync(L1Frame l1Frame, CancellationToken cancellationToken = default)
    {
        //1- create l2Frame from l1Frame
        L2Frame? l2Frame = _l2Parser.Parse(l1Frame);

        if(l2Frame == null) 
        {
            return;
        }

        //2- if frame is broker request, satisfy it (Basic.Deliver   Connection.Start   Connection.Tune   Connection.Open)
        if(l2Frame.ClassMethod == ClassMethod.BasicDeliver)
        {
            await HandleBasicDeliver(l2Frame);
        }

        //3- if frame is broker response to client request

            //3.1 if client request is sync, call _rpcAwaitable.Complete()   (Channel.Open   Basic.Consume)

        if(l2Frame.ClassMethod == ClassMethod.ChannelOpenOk)
        {
            await HandleChannelOpenOk(l2Frame);
        }

        if(l2Frame.ClassMethod == ClassMethod.BasicConsumeOk)
        {
            _rpcAwaitable!.Complete(l2Frame);
            //BasicConsumeOk basicConsumeOk = new BasicConsumeOk(l2Frame);
        }

            //3.2 if client request is async, handle that (no use of _rpc awaitable)   (Basic.Ack   Basic.Nack)

        if(l2Frame.ClassMethod == ClassMethod.BasicAck)
        {

        }

        return;
    }


    private async Task HandleBasicDeliver(L2Frame l2Frame)
    {
        //1- Pass frame to related consumer

        //2- Send Basic.Ack or by consumer?
        return;
    }


    private async Task HandleChannelOpenOk(L2Frame l2Frame)
    {
        _rpcAwaitable!.Complete(l2Frame);
    }
}
