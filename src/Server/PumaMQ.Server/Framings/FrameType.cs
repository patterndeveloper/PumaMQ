namespace PumaMQ.Server.Framings;

internal enum FrameType : byte
{
    None = 0x00,
    Method = 0x01,
    Header = 0x02,
    Body = 0x03,
    HeartBeat = 0x08
}