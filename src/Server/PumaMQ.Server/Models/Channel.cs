namespace PumaMQ.Server.Models;

internal class Channel
{
    internal int Id { get; set; }
    internal int ConnectionId { get; set; }
    internal ushort ChannelNo { get; set; }
}