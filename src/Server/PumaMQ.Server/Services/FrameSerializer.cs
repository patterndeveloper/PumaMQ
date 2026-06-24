using System.Buffers.Binary;
using System.Buffers;
using PumaMQ.Server.Framings;

namespace PumaMQ.Server.Services;

internal static class FrameSerializer
{
    private const byte _marker = 0xCE;
    private const ushort _headerClassId = 0x00_3C;
    private const ushort _headerWeight = 0x00_00;
    private const ushort _headerFlags = 0x00_00;

    internal static RentedMemory Serialize<TMethod>(ref TMethod frame, ReadOnlyMemory<byte> body, ushort channelNo) where TMethod : IClientResponseMethodFrame
    {
        int methodOverheadLen = FrameLen.TotalFixed;
        int methodPayloadLen = frame.GetPayloadLen();
        int methodTotalLen = methodOverheadLen + methodPayloadLen;

        int headerOverheadLen = FrameLen.TotalFixed;
        int headerPayloadLen = HeaderFrameLen.Total;
        int headerTotalLen = headerOverheadLen + headerPayloadLen;

        int bodyOverheadLen = FrameLen.TotalFixed;
        int bodyPayloadLen = body.Length;
        int bodyTotalLen = bodyOverheadLen + bodyPayloadLen;

        int frameTotalLen = methodTotalLen + headerTotalLen + bodyTotalLen;

        byte[] buffer = ArrayPool<byte>.Shared.Rent(frameTotalLen);
        Span<byte> methodSpan = buffer.AsSpan(0, methodTotalLen);

        //Serialize method
        int currentIndex = 0;
        methodSpan[currentIndex] = (byte)FrameType.Method;
        currentIndex += FrameLen.Type;

        BinaryPrimitives.WriteUInt16BigEndian(methodSpan.Slice(currentIndex), channelNo);
        currentIndex += FrameLen.Channel;

        BinaryPrimitives.WriteUInt32BigEndian(methodSpan.Slice(currentIndex), (uint)methodPayloadLen);
        currentIndex += FrameLen.Length;

        int writtenMethodPayload = frame.SerializeAndWrite(methodSpan.Slice(currentIndex));
        currentIndex += writtenMethodPayload;

        methodSpan[currentIndex] = _marker;


        //Serialize header
        currentIndex = 0;
        Span<byte> headerSpan = buffer.AsSpan(methodTotalLen, headerTotalLen);
        headerSpan[currentIndex] = (byte)FrameType.Header;
        currentIndex += FrameLen.Type;

        BinaryPrimitives.WriteUInt16BigEndian(headerSpan.Slice(currentIndex), channelNo);
        currentIndex += FrameLen.Channel;

        BinaryPrimitives.WriteUInt32BigEndian(headerSpan.Slice(currentIndex), (uint)headerPayloadLen);

        currentIndex += FrameLen.Length;

        BinaryPrimitives.WriteUInt16BigEndian(headerSpan.Slice(currentIndex), _headerClassId);
        currentIndex += HeaderFrameLen.Class;

        BinaryPrimitives.WriteUInt16BigEndian(headerSpan.Slice(currentIndex), _headerWeight);
        currentIndex += HeaderFrameLen.Weight;

        BinaryPrimitives.WriteUInt64BigEndian(headerSpan.Slice(currentIndex), (ulong)bodyPayloadLen);
        currentIndex += HeaderFrameLen.BodySize;

        BinaryPrimitives.WriteUInt16BigEndian(headerSpan.Slice(currentIndex), _headerFlags);
        currentIndex += HeaderFrameLen.PropertyFlags;

        headerSpan[currentIndex] = _marker;

        //Todo: Add specialized class for handling header properties. this class only add header with no properties.

        //Serialize body
        currentIndex = 0;
        Span<byte> bodySpan = buffer.AsSpan(methodTotalLen + headerTotalLen, bodyTotalLen);
        bodySpan[currentIndex] = (byte)FrameType.Body;
        currentIndex += FrameLen.Type;

        BinaryPrimitives.WriteUInt16BigEndian(bodySpan.Slice(currentIndex), channelNo);
        currentIndex += FrameLen.Channel;

        BinaryPrimitives.WriteUInt32BigEndian(bodySpan.Slice(currentIndex), (uint)bodyPayloadLen);
        currentIndex += FrameLen.Length;

        body.Span.CopyTo(bodySpan.Slice(currentIndex));
        currentIndex += bodyPayloadLen;

        bodySpan[currentIndex] = _marker;

        ReadOnlyMemory<byte> memory = new(buffer, 0, frameTotalLen);
        RentedMemory serializedFrame = new(buffer, memory);
        return serializedFrame;
    }


    internal static RentedMemory Serialize<TMethod>(ref TMethod frame, ushort channelNo) where TMethod : IClientResponseMethodFrame
    {
        int methodOverheadLen = FrameLen.TotalFixed;
        int methodPayloadLen = frame.GetPayloadLen();
        int methodTotalLen = methodOverheadLen + methodPayloadLen;

        int frameTotalLen = methodTotalLen;

        byte[] buffer = ArrayPool<byte>.Shared.Rent(frameTotalLen);
        Span<byte> methodSpan = buffer.AsSpan(0, methodTotalLen);

        //Serialize method
        int currentIndex = 0;
        methodSpan[currentIndex] = (byte)FrameType.Method;
        currentIndex += FrameLen.Type;

        BinaryPrimitives.WriteUInt16BigEndian(methodSpan.Slice(currentIndex), channelNo);
        currentIndex += FrameLen.Channel;

        BinaryPrimitives.WriteUInt32BigEndian(methodSpan.Slice(currentIndex), (uint)methodPayloadLen);
        currentIndex += FrameLen.Length;

        int writtenMethodPayload = frame.SerializeAndWrite(methodSpan.Slice(currentIndex));
        currentIndex += writtenMethodPayload;

        methodSpan[currentIndex] = _marker;

        ReadOnlyMemory<byte> memory = new(buffer, 0, frameTotalLen);
        RentedMemory serializedFrame = new(buffer, memory);
        return serializedFrame;
    }
}