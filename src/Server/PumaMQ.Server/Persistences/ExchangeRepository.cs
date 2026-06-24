using Dapper;
using Microsoft.Data.SqlClient;
using PumaMQ.Server.Models;

namespace PumaMQ.Server.Persistences;

internal class ExchangeRepository
{
    private readonly DbConnectionFactory _dbConnectionFactory;


    public ExchangeRepository(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }


    internal async Task<int> CreateAsync(Exchange exchange, CancellationToken cancellationToken = default)
    {
        SqlConnection sqlConnection = default!;

        try
        {
            sqlConnection = await _dbConnectionFactory.CreateAsync(cancellationToken).ConfigureAwait(false);

            string commandText = @"INSERT INTO dbo.[Exchange] ([Name])
                                   OUTPUT INSERTED.[Id]
                                   VALUES (@Name);";

            object param = new {Name = exchange.Name};

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