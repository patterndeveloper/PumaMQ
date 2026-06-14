using System.Buffers.Binary;
using PumaMQ.Client.Exceptions;
using PumaMQ.Client.Framings;

namespace PumaMQ.Client.Parsers;

internal class L2Parser
{
    private L2ParserState _state;
    private readonly L2Frame _l2Frame;

    private const ushort _headerClassId = 0x3C;
    private const uint _maxBodyLen = 1024 * 1024;
    private const int _maxHeaderFrameLenWithNoProp = 14;


    public L2Parser()
    {
        _state = L2ParserState.ExpectingMethod;
        _l2Frame = new L2Frame();
    }

    internal L2Frame? Parse(L1Frame l1Frame)
    {
        switch (_state)
        {
            case L2ParserState.ExpectingMethod:
                ParseMethodFrame(l1Frame);
                break;
            case L2ParserState.ExpectingHeader:
                ParseHeaderFrame(l1Frame);
                break;
            case L2ParserState.ExpectingBody:
                ParseBodyFrame(l1Frame);
                break;
        }

        if (_state != L2ParserState.Complete)
        {
            return null;
        }

        _state = L2ParserState.ExpectingMethod;
        return _l2Frame;
    }


    private void ParseMethodFrame(L1Frame l1Frame)
    {
        int currentIndex = 0;
        _l2Frame.ClassMethod = (ClassMethod)BinaryPrimitives.ReadUInt32BigEndian(l1Frame.Payload.Memory.Span);
        currentIndex += MethodFrameLen.Method + MethodFrameLen.Class;

        RentedMemory unCommonMethodPayload = l1Frame.Payload.Takeover(currentIndex);
        _l2Frame.Method = unCommonMethodPayload;

        if (_l2Frame.ClassMethod == ClassMethod.BasicPublish)
        {
            _state = L2ParserState.ExpectingHeader;
        }
        else
        {
            _state = L2ParserState.Complete;
        }
    }


    private void ParseHeaderFrame(L1Frame l1Frame)
    {
        if (l1Frame.Type != FrameType.Header)
        {
            string message = $"Expecting frame of type: {FrameType.Header} but receiving frame of type: {l1Frame.Type}";
            throw new UnExpectedFrameException(message);
        }

        int currentIndex = 0;
        ReadOnlySpan<byte> classIdSpan = l1Frame.Payload.Memory.Span;
        ushort classId = BinaryPrimitives.ReadUInt16BigEndian(classIdSpan);
        if (classId != _headerClassId)
        {
            string message = $"Expecting classId: {_headerClassId}, but receiving classId: {classId}";
            throw new UnKnownClassOrMethodException(message);
        }
        currentIndex += HeaderFrameLen.Class + HeaderFrameLen.Weight;

        ReadOnlySpan<byte> bodySizeSpan = l1Frame.Payload.Memory.Slice(currentIndex).Span;
        ulong bodyLen = BinaryPrimitives.ReadUInt64BigEndian(bodySizeSpan);
        currentIndex += HeaderFrameLen.BodySize;

        if (bodyLen > _maxBodyLen)
        {
            string message = $"Frame body size: {bodyLen} exceeds max body size: {_maxBodyLen}";
            throw new MalformedFrameException(message);
        }

        int maxHeaderFrameLenWithNoProp = 14;

        if (l1Frame.Payload.Memory.Length <= maxHeaderFrameLenWithNoProp + 1)
        {
            l1Frame.Payload.Dispose();
        }
        else
        {
            _l2Frame.Header = l1Frame.Payload.Takeover(currentIndex);
        }

        if (bodyLen > 0)
        {
            _state = L2ParserState.ExpectingBody;
        }
        else
        {
            _state = L2ParserState.Complete;
        }
    }


    private void ParseBodyFrame(L1Frame l1Frame)
    {
        if (l1Frame.Type != FrameType.Body)
        {
            string message = $"Expecting frame of type: {FrameType.Body} but receiving frame of type: {l1Frame.Type}";
            throw new UnExpectedFrameException(message);
        }

        _l2Frame.Body = l1Frame.Payload.Takeover(0);
        _state = L2ParserState.Complete;
    }
}