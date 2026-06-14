using System.Buffers.Binary;

namespace PumaMQ.Server.Framings;

internal readonly struct ChannelOpenOk : IMethodFrame
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

        BinaryPrimitives.WriteUInt32BigEndian(buffer, (uint)ClassMethod.ChannelOpenOk);
        currentIndex += MethodFrameLen.Class + MethodFrameLen.Method;

        return currentIndex;
    }
}
