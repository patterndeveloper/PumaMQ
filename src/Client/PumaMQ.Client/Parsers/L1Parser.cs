using System.Buffers.Binary;
using System.Buffers;
using System.IO.Pipelines;
using PumaMQ.Client.Framings;
using PumaMQ.Client.Exceptions;

namespace PumaMQ.Client.Parsers;

internal static class L1Parser
{
    private const int _commonSegmentLen = FrameLen.Type + FrameLen.Channel + FrameLen.Length;
    private const int _maxFramePayloadLen = 1024;
    private const byte _marker = 0xCE;

    internal static async Task<L1Frame> ReadFrameFromPipeAsync(PipeReader _pipeReader, CancellationToken cancellationToken = default)
    {
        ReadResult readResult = await _pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);
        ReadOnlySequence<byte> buffer = readResult.Buffer;
        CheckAndThrowIfStreamFinished(readResult, buffer);
        L1Frame l1Frame = new();

        while (!TryParseL1Frame(ref buffer, l1Frame))
        {
            _pipeReader.AdvanceTo(buffer.Start, buffer.End);
            readResult = await _pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            buffer = readResult.Buffer;
            CheckAndThrowIfStreamFinished(readResult, buffer);
        }

        _pipeReader.AdvanceTo(buffer.Start);

        return l1Frame;
    }


    private static bool TryParseL1Frame(ref ReadOnlySequence<byte> buffer, L1Frame frame)
    {
        if (buffer.Length < _commonSegmentLen)
        {
            return false;
        }

        int currentIndex = 0;

        byte rawFrameType = buffer.First.Span[currentIndex];
        FrameType frameType = (FrameType)rawFrameType;
        frame.Type = frameType;

        if (IsFrameTypeValid(frameType))
        {
            throw new ProtocolException("Invalid Frame Type");
        }
        currentIndex += FrameLen.Type;

        ReadOnlySpan<byte> channelSpan = buffer.Slice(currentIndex).First.Span;
        ushort channelNo = BinaryPrimitives.ReadUInt16BigEndian(channelSpan);
        frame.Channel = channelNo;
        currentIndex += FrameLen.Channel;

        ReadOnlySpan<byte> payloadLengthSpan = buffer.Slice(currentIndex).First.Span;
        int payloadLength = BinaryPrimitives.ReadInt32BigEndian(payloadLengthSpan);
        frame.Length = payloadLength;

        if (payloadLength != 0 && payloadLength > _maxFramePayloadLen)
        {
            throw new ProtocolException("Invalid Frame Payload size");
        }

        int unCommonSegmentLen = payloadLength + FrameLen.Marker;

        if (unCommonSegmentLen > buffer.Length - _commonSegmentLen)
        {
            //There is more byte that must be received to complete frame
            return false;
        }

        int completeFrameLen = _commonSegmentLen + unCommonSegmentLen;

        byte expectedMarker = buffer.First.Span[completeFrameLen - 1];

        if (expectedMarker != _marker)
        {
            throw new ProtocolException("Invalid Marker");
        }

        byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(unCommonSegmentLen);
        buffer.Slice(_commonSegmentLen, unCommonSegmentLen).CopyTo(rentedBuffer);
        ReadOnlyMemory<byte> memory = new ReadOnlyMemory<byte>(rentedBuffer, 0, unCommonSegmentLen);
        frame.Payload = new RentedMemory(rentedBuffer, memory);

        buffer = buffer.Slice(completeFrameLen);

        return true;
    }


    private static void CheckAndThrowIfStreamFinished(ReadResult readResult, ReadOnlySequence<byte> buffer)
    {
        if (readResult.IsCompleted && buffer.IsEmpty)
        {
            throw new StreamFinishedException("Stream finished");
        }
    }


    private static bool IsFrameTypeValid(FrameType frameType)
    {
        bool isValid = frameType != FrameType.Method
                       && frameType != FrameType.Header
                       && frameType != FrameType.Body
                       && frameType != FrameType.HeartBeat;

        return isValid;
    }
}