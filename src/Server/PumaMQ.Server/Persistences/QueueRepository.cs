using Dapper;
using Microsoft.Data.SqlClient;
using PumaMQ.Server.Models;

namespace PumaMQ.Server.Persistences;

internal class QueueRepository
{
    private readonly DbConnectionFactory _connectionFactory;


    public QueueRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    
    internal async Task<Queue?> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        SqlConnection sqlConnection = default!;

        try
        {
            sqlConnection = await _connectionFactory.CreateAsync(cancellationToken).ConfigureAwait(false);

            string commandText = @"SELECT
                                     [Id]
                                    ,[Name]
                                   FROM [Queue]
                                   WHERE [Name] = @Name";

            object param = new { Name = name };

            CommandDefinition commandDefinition = new CommandDefinition(commandText, param, cancellationToken: cancellationToken);
            Queue? queue = await sqlConnection.QuerySingleOrDefaultAsync<Queue>(commandDefinition).ConfigureAwait(false);
            return queue;
        }
        finally
        {
            sqlConnection?.Dispose();
        }
    }
}