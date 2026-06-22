namespace PumaMQ.Server.Models;

internal class Connection
{
    internal int Id { get; set;}
    internal DateTime ConnectedAt { get; set; }
    internal string VirtualHost { get; set; } = default!;
}
