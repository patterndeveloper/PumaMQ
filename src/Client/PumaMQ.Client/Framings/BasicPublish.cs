using System.Buffers.Binary;
using System.Text;

namespace PumaMQ.Client.Framings;

internal readonly struct BasicPublish : IMethodFrame
{
    internal readonly string Exchange { get; }
    internal readonly string RoutingKey { get; }
    internal readonly bool Mandatory { get; }
    internal readonly bool Immediate { get; }


    public BasicPublish(string exchange, string routingKey, bool mandatory, bool immediate) : this()
    {
        Exchange = exchange;
        RoutingKey = routingKey;
        Mandatory = mandatory;
        Immediate = immediate;
    }

    public int GetPayloadLen()
    {
        int payloadLen = MethodFrameLen.Class
                          + MethodFrameLen.Method
                          + MethodFrameLen.Ticket
                          + MethodFrameLen.ExchangeLen
                          + Encoding.UTF8.GetByteCount(Exchange)
                          + MethodFrameLen.RoutingKeyLen
                          + Encoding.UTF8.GetByteCount(RoutingKey)
                          + MethodFrameLen.Flags;

        return payloadLen;
    }

    public int SerializeAndWrite(Span<byte> buffer)
    {
        int currentIndex = 0;

        BinaryPrimitives.WriteUInt32BigEndian(buffer, (uint)ClassMethod.BasicPublish);
        currentIndex += MethodFrameLen.Class + MethodFrameLen.Method;

        BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(currentIndex), 0x00_00);
        currentIndex += MethodFrameLen.Ticket;

        byte exchangeLen = (byte)Encoding.UTF8.GetByteCount(Exchange);
        buffer[currentIndex] = exchangeLen;
        currentIndex += MethodFrameLen.ExchangeLen;

        Encoding.UTF8.GetBytes(Exchange, buffer.Slice(currentIndex));
        currentIndex += exchangeLen;

        byte routingKeyLen = (byte)Encoding.UTF8.GetByteCount(RoutingKey);
        buffer[currentIndex] = routingKeyLen;
        currentIndex += 1;

        Encoding.UTF8.GetBytes(RoutingKey, buffer.Slice(currentIndex));
        currentIndex += routingKeyLen;

        byte flags = 0;
        flags |= (byte)(Mandatory ? 0x01 : 0x00);
        flags |= (byte)(Immediate ? 0x02 : 0x00);
        buffer[currentIndex] = flags;
        currentIndex += MethodFrameLen.Flags;

        return currentIndex;
    }
}