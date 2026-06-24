using System.Buffers.Binary;
using System.Text;

namespace PumaMQ.Server.Framings;

internal readonly struct BasicConsumeOk : IClientResponseMethodFrame
{
    public string ConsumerTag { get; }


    public BasicConsumeOk(string consumerTag)
    {
        ConsumerTag = consumerTag;
    }

    public int GetPayloadLen()
    {
        int payloadLen = MethodFrameLen.Class
                         + MethodFrameLen.Method
                         + SpecMethodFrameLen.StringLen
                         + Encoding.UTF8.GetByteCount(ConsumerTag);

        return payloadLen;
    }


    public int SerializeAndWrite(Span<byte> buffer)
    {
        int currentIndex = 0;

        BinaryPrimitives.WriteUInt32BigEndian(buffer, (uint)ClassMethod.BasicConsumeOk);
        currentIndex += MethodFrameLen.Class + MethodFrameLen.Method;

        byte consumerTagLen = (byte)Encoding.UTF8.GetByteCount(ConsumerTag);
        buffer[currentIndex] = consumerTagLen;
        currentIndex += SpecMethodFrameLen.StringLen;

        Encoding.UTF8.GetBytes(ConsumerTag, buffer.Slice(currentIndex));
        currentIndex += consumerTagLen;

        return currentIndex;
    }
}