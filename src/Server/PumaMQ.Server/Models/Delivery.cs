namespace PumaMQ.Server.Models;

internal class Delivery
{
    internal int Id { get; set; }
    internal int RoutedMessageId { get; set; } = default!;
    internal int DeliveryCount { get; set; }
    internal DeliveryStatus Status { get; set; }
    internal DateTime TimeoutAt { get; set; }
}
