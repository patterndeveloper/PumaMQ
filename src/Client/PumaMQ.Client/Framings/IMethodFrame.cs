namespace PumaMQ.Client.Framings;

internal interface IMethodFrame
{
    int GetPayloadLen();

    int SerializeAndWrite(Span<byte> buffer);
}