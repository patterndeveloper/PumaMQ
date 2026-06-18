namespace PumaMQ.Server.Framings;

internal enum ClassMethod : uint
{
    BasicPublish = 0x00_3C_00_28,
    ChannelOpen = 0x00_14_00_0A,
    ChannelOpenOk = 0x00_14_00_0B,
    BasicConsume = 0x00_3C_00_14,
    BasicConsumeOk = 0x00_0C_00_15,
    BasicDeliver = 0x00_3C_00_3C,
    BasicAck = 0x00_3C_00_50
}