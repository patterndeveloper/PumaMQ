using System.Buffers.Binary;
using System.Text;

namespace PumaMQ.Client.Framings;

internal readonly struct ExchangeDeclare : IMethodFrame
{
    internal string Exchange { get; }

    private const ushort _reservedTicket = 0x00_00;
    private const byte _emptyExchangeType = 0x00;
    private const byte _emptyFlags = 0x00;
    private const uint _emptyArgs = 0x00_00_00_00;


    public ExchangeDeclare(string exchange)
    {
        Exchange = exchange;
    }


    public int GetPayloadLen()
    {
        int payloadLen = MethodFrameLen.Class
                         + MethodFrameLen.Method
                         + SpecMethodFrameLen.Ticket
                         + SpecMethodFrameLen.StringLen
                         + Encoding.UTF8.GetByteCount(Exchange)
                         + SpecMethodFrameLen.StringLen     //Empty exchange-type
                         + SpecMethodFrameLen.Flags
                         + SpecMethodFrameLen.Args;

        return payloadLen;
    }


    public int SerializeAndWrite(Span<byte> buffer)
    {
        int currentIndex = 0;

        BinaryPrimitives.WriteUInt32BigEndian(buffer, (uint)ClassMethod.ExchangeDeclare);
        currentIndex += MethodFrameLen.Class + MethodFrameLen.Method;

        BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(currentIndex), _reservedTicket);
        currentIndex += SpecMethodFrameLen.Ticket;

        byte exchangeLen = (byte)Encoding.UTF8.GetByteCount(Exchange);
        buffer[currentIndex] = exchangeLen;
        currentIndex += SpecMethodFrameLen.StringLen;

        Encoding.UTF8.GetBytes(Exchange, buffer.Slice(currentIndex));
        currentIndex += exchangeLen;

        //Empty consumer-tag
        buffer[currentIndex] = _emptyExchangeType;
        currentIndex += SpecMethodFrameLen.StringLen;

        buffer[currentIndex] = _emptyFlags;
        currentIndex += SpecMethodFrameLen.Flags;

        BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(currentIndex), _emptyArgs);
        currentIndex += SpecMethodFrameLen.Args;

        return currentIndex;
    }
}