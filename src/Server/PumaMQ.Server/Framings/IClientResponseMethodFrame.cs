namespace PumaMQ.Server.Framings;

internal interface IClientResponseMethodFrame
{
    int GetPayloadLen();

    int SerializeAndWrite(Span<byte> buffer);
}