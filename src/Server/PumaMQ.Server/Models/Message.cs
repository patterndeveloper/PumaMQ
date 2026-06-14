namespace PumaMQ.Server.Models;

internal class Message
{
    internal int Id { get; set; }
    internal int ExchangeId { get; set; }
    internal string RoutingKey { get; set; } = default!;
    internal string Content { get; set; } = default!;
    internal DateTime ReceivedAt { get; set; }
}
