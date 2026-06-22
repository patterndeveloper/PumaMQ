using Dapper;
using Microsoft.Data.SqlClient;
using PumaMQ.Server.Models;

namespace PumaMQ.Server.Persistences;

internal class ConsumerRepository
{
    private readonly DbConnectionFactory _connectionFactory;


    public ConsumerRepository(DbConnectionFactory dbConnectionFactory)
    {
        _connectionFactory = dbConnectionFactory;
    }


    internal async Task<int> CreateAsync(Consumer consumer, CancellationToken cancellationToken = default)
    {
        SqlConnection sqlConnection = default!;
        try
        {
            sqlConnection = await _connectionFactory.CreateAsync().ConfigureAwait(false);

            string commandText = @"INSERT INTO [Consumer] ([ChannelId], [QueueId], [Tag])
                                   OUTPUT INSERTED.Id
                                   VALUES (@ChannelId, @QueueId, @Tag);";

            object param = new { ChannelId = consumer.ChannelId, QueueId = consumer.QueueId, Tag = consumer.Tag };

            CommandDefinition commandDefinition = new CommandDefinition(commandText, param, cancellationToken: cancellationToken);
            int id = await sqlConnection.ExecuteScalarAsync<int>(commandDefinition).ConfigureAwait(false);
            return id;
        }
        finally
        {
            sqlConnection?.Dispose();
        }
    }
}