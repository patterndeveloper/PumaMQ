using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace PumaMQ.Server.Persistences;

internal class DbConnectionFactory
{
    private readonly string _connectionString;
    private readonly DbOption _dbOption;


    public DbConnectionFactory(IOptions<DbOption> options)
    {
        _dbOption = options.Value;
        _connectionString = BuildConnectionString();
    }


    private string BuildConnectionString()
    {
        SqlConnectionStringBuilder builder = new()
        {
            DataSource = _dbOption.Server,
            InitialCatalog = _dbOption.Database,

            Encrypt = _dbOption.Encrypt,
            TrustServerCertificate = _dbOption.TrustServerCertificate,

            ConnectTimeout = _dbOption.ConnectionTimeout,

            Pooling = _dbOption.Pooling.Enabled,
            MinPoolSize = _dbOption.Pooling.MinPoolSize,
            MaxPoolSize = _dbOption.Pooling.MaxPoolSize,
            LoadBalanceTimeout = _dbOption.Pooling.LoadBalanceTimeout,

            ApplicationName = _dbOption.ApplicationName
        };

        if (_dbOption.IntegratedSecurity)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = _dbOption.UserName;
            builder.Password = _dbOption.Password;
        }

        return builder.ConnectionString;
    }


    internal async Task<SqlConnection> CreateAsync(CancellationToken cancellationToken = default)
    {
        SqlConnection dbConnection = new SqlConnection(_connectionString);
        try
        {
            await dbConnection.OpenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            dbConnection.Dispose();
            throw;
        }

        return dbConnection;
    }
}