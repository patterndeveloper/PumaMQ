namespace PumaMQ.Server.Persistences;

internal class DbOption
{
    public string Server { get; init; } = default!;

    public string Database { get; init; } = default!;

    public bool IntegratedSecurity { get; init; }

    public string UserName { get; init; } = default!;

    public string Password { get; init; } = default!;

    public bool Encrypt { get; init; }

    public bool TrustServerCertificate { get; init; }

    public string ApplicationName { get; init; } = default!;

    public int ConnectionTimeout { get; init; }

    public int CommandTimeout { get; init; }

    public PoolingOption Pooling { get; init; } = new();


    internal class PoolingOption
    {
        public bool Enabled { get; init; }

        public int MinPoolSize { get; init; }

        public int MaxPoolSize { get; init; }

        public int LoadBalanceTimeout { get; init; }
    }
}
