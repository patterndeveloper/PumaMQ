namespace PumaMQ.Server.Models;

internal class Consumer
{
    internal int Id { get; set; }
    internal int QueueId { get; set; }
    internal int ChannelId { get; set; }
    internal string Tag { get; set; } = default!;
}