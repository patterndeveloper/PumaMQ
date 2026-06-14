namespace PumaMQ.Server.Models;

internal class RoutedMessage
{
    internal int Id { get; set; }
    internal int MessageId { get; set; }
    internal int QueueId { get; set; }
    internal RoutedMessageStatus Status { get; set; }
    internal DateTime RoutedAt { get; set; }
}
