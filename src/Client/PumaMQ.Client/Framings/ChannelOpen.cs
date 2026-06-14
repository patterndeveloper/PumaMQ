using System.Buffers.Binary;

namespace PumaMQ.Client.Framings;

internal readonly struct ChannelOpen : IMethodFrame
{
    public int GetPayloadLen()
    {
        int payloadLen = MethodFrameLen.Class
                         + MethodFrameLen.Method;

        return payloadLen;
    }

    public int SerializeAndWrite(Span<byte> buffer)
    {
        int currentIndex = 0;

        BinaryPrimitives.WriteUInt32BigEndian(buffer, (uint)ClassMethod.ChannelOpen);
        currentIndex += MethodFrameLen.Class + MethodFrameLen.Method;

        return currentIndex;
    }
}