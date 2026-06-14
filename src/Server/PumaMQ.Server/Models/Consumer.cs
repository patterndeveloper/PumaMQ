namespace PumaMQ.Server.Models;

internal class Consumer
{
    internal int Id { get; set; }
    internal int QueueId { get; set; }
    internal int ChannelNo { get; set; }
    internal Guid ConnectionId { get; set; }
}
