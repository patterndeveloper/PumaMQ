namespace PumaMQ.Server.Models;

internal enum DeliveryStatus : byte
{
    Acked,
    Nacked,
}