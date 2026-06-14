namespace PumaMQ.Server.Framings;

internal class L1Frame
{
    internal FrameType Type { get; set; }
    internal ushort Channel { get; set; }
    internal int Length { get; set; }

    internal RentedMemory Payload { get; set; }
}