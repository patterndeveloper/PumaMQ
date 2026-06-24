using System.Buffers.Binary;
using System.Text;

namespace PumaMQ.Server.Framings;

internal readonly struct ExchangeDeclare : IClientRequestMethodFrame
{
    internal readonly string Exchange { get; }


    internal ExchangeDeclare(L2Frame l2Frame)
    {
        int currentIndex = 0;

        ReadOnlySpan<byte> methodSpan = l2Frame.Method.Memory.Span;
        ushort ticket = BinaryPrimitives.ReadUInt16BigEndian(methodSpan);
        currentIndex += SpecMethodFrameLen.Ticket;

        byte exchangeLen = methodSpan[currentIndex];
        currentIndex += SpecMethodFrameLen.StringLen;
        ReadOnlySpan<byte> exchangeSpan = methodSpan.Slice(currentIndex);

        if (exchangeLen == 0)
        {
            string message = $"Exchange name can not be empty";
            throw new ArgumentOutOfRangeException(message);
        }
        if (exchangeLen > exchangeSpan.Length)
        {
            string message = $"Length of Exchange is {exchangeLen}, but length of actual bytes is {exchangeSpan.Length}";
            throw new ArgumentOutOfRangeException(message);
        }
        exchangeSpan = methodSpan.Slice(currentIndex, exchangeLen);
        Exchange = Encoding.UTF8.GetString(exchangeSpan);
        currentIndex += exchangeLen;

        //Todo: pars exchange-type

        //Todo: pars flags

        //Todo: pars Args
    }
}