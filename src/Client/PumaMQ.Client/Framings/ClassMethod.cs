namespace PumaMQ.Client.Framings;

internal enum ClassMethod : uint
{
    BasicPublish = 0x00_3C_00_28,
    ChannelOpen = 0x00_14_00_0A,
    ChannelOpenOk = 0x_00_14_00_0B
}