using PumaMQ.Server.Framings;
using PumaMQ.Server.Models;
using System.Text;

namespace PumaMQ.Server.Services;

internal partial class Channel
{
    internal async Task HandleL2FrameAsync(L1Frame l1Frame)
    {
        L2Frame? l2Frame = _l2Parser.Parse(l1Frame);

        if (l2Frame == null)
        {
            return;
        }

        if (l2Frame.ClassMethod == ClassMethod.ChannelOpen)
        {
            await HandleChannelOpenAsync(l2Frame).ConfigureAwait(false);
        }

        if (l2Frame.ClassMethod == ClassMethod.BasicConsume)
        {
            await HandleBasicConsumeAsync(l2Frame).ConfigureAwait(false);
        }

        if(l2Frame.ClassMethod == ClassMethod.ExchangeDeclare)
        {
            await HandleExchangeDeclareAsync(l2Frame).ConfigureAwait(false);
        }
        string receivedMessage = Encoding.UTF8.GetString(l2Frame.Body.Memory.Span);

        Console.WriteLine($"Message: {receivedMessage} received from client");
    }


    internal async Task HandleChannelOpenAsync(L2Frame l2Frame)
    {
        ChannelOpenOk channelOpenOk = new();
        RentedMemory serializedChannelOpenOk = FrameSerializer.Serialize(ref channelOpenOk, ChannelNo);
        await _socketFrameHandler.WriteAsync(serializedChannelOpenOk).ConfigureAwait(false);
    }


    internal async Task HandleBasicConsumeAsync(L2Frame l2Frame)
    {
        //1- Parse payload of method frame
        BasicConsume basicConsume = new BasicConsume(l2Frame);

        //2- Insert consumer (consumerTag, queueId, channelId) into db
        Queue? queue = await _queueRepository.GetAsync(basicConsume.Queue).ConfigureAwait(false);

        if (queue == null)
        {
            //Todo: Add Queue not found exception
            throw new Exception("Queue not exist");
        }

        Consumer consumer = new()
        {
            QueueId = queue.Id,
            ChannelId = Id,
            Tag = basicConsume.Tag,
        };
        await _consumerRepository.CreateAsync(consumer).ConfigureAwait(false);

        //3- Return Basic.Consume-Ok
        BasicConsumeOk basicConsumeOk = new BasicConsumeOk(basicConsume.Tag);
        RentedMemory serializedBasicConsumeOk = FrameSerializer.Serialize(ref basicConsumeOk, ChannelNo);
        await _socketFrameHandler.WriteAsync(serializedBasicConsumeOk).ConfigureAwait(false);
    }


    internal async Task HandleExchangeDeclareAsync(L2Frame l2Frame)
    {
        ExchangeDeclare exchangeDeclare = new(l2Frame);

        Exchange exchange = new()
        {
            Name = exchangeDeclare.Exchange
        };

        await _exchangeRepository.CreateAsync(exchange).ConfigureAwait(false);

        ExchangeDeclareOk exchangeDeclareOk = new();
        RentedMemory serializedExchangeDeclareOk = FrameSerializer.Serialize(ref exchangeDeclareOk, ChannelNo);
        await _socketFrameHandler.WriteAsync(serializedExchangeDeclareOk).ConfigureAwait(false);
    }

    //Todo: Add heartbeat handling for channel 0
}