namespace PumaMQ.Server.Models;

internal class Binding
{
    internal int Id { get; set; }
    internal int QueueId { get; set; }
    internal int ExchangeId { get; set; }
    internal string RoutingKey { get; set; } = default!;
}
