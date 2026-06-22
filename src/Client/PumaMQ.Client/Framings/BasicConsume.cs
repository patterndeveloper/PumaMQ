using System.Buffers.Binary;
using System.Text;

namespace PumaMQ.Client.Framings;

internal readonly struct BasicConsume : IMethodFrame
{
    internal readonly string Queue { get; }
    internal readonly string Tag { get; }

    private const ushort _reservedTicket = 0x00_00;
    private const byte _emptyConsumerTag = 0x00;
    private const byte _emptyFlags = 0x00;
    private const uint _emptyArgs = 0x00_00_00_00;


    public BasicConsume(string queue, string tag)
    {
        Queue = queue;
        Tag = tag;
    }


    public int GetPayloadLen()
    {
        int payloadLen = MethodFrameLen.Class
                         + MethodFrameLen.Method
                         + SpecMethodFrameLen.Ticket
                         + SpecMethodFrameLen.StringLen
                         + Encoding.UTF8.GetByteCount(Queue)
                         + SpecMethodFrameLen.StringLen     //Empty consumer-tag need this
                         + Encoding.UTF8.GetByteCount(Tag)
                         + SpecMethodFrameLen.Flags
                         + SpecMethodFrameLen.Args;

        return payloadLen;
    }

    public int SerializeAndWrite(Span<byte> buffer)
    {
        int currentIndex = 0;

        BinaryPrimitives.WriteUInt32BigEndian(buffer, (uint)ClassMethod.BasicConsume);
        currentIndex += MethodFrameLen.Class + MethodFrameLen.Method;

        BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(currentIndex), _reservedTicket);
        currentIndex += SpecMethodFrameLen.Ticket;

        byte queueLen = (byte)Encoding.UTF8.GetByteCount(Queue);
        buffer[currentIndex] = queueLen;
        currentIndex += SpecMethodFrameLen.StringLen;

        Encoding.UTF8.GetBytes(Queue, buffer.Slice(currentIndex));
        currentIndex += queueLen;

        //Empty consumer-tag
        byte tagLen = (byte) Encoding.UTF8.GetByteCount(Tag);
        buffer[currentIndex] = tagLen;
        currentIndex += SpecMethodFrameLen.StringLen;

        Encoding.UTF8.GetBytes(Tag, buffer.Slice(currentIndex));
        currentIndex += tagLen;

        buffer[currentIndex] = _emptyFlags;
        currentIndex += SpecMethodFrameLen.Flags;

        BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(currentIndex), _emptyArgs);
        currentIndex += SpecMethodFrameLen.Args;

        return currentIndex;
    }
}