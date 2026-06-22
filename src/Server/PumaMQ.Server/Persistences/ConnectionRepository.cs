using Dapper;
using Microsoft.Data.SqlClient;
using PumaMQ.Server.Models;

namespace PumaMQ.Server.Persistences;

internal class ConnectionRepository
{
    private readonly DbConnectionFactory _dbConnectionFactory;


    public ConnectionRepository(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }


    internal async Task<int> CreateAsync(Connection connection, CancellationToken cancellationToken = default)
    {
        SqlConnection sqlConnection = default!;

        try
        {
            sqlConnection = await _dbConnectionFactory.CreateAsync().ConfigureAwait(false);

            string commandText = @"INSERT INTO [TcpConnection] ([VirtualHost])
                                   OUTPUT INSERTED.Id
                                   VALUES (N'@VirtualHost');";

            object param = new { VirtualHost = connection.VirtualHost };

            CommandDefinition commandDefinition = new(commandText, param, cancellationToken: cancellationToken);
            int id = await sqlConnection.ExecuteScalarAsync<int>(commandDefinition).ConfigureAwait(false);
            return id;
        }
        finally
        {
            sqlConnection?.Dispose();
        }
    }
}