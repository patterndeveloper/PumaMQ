using Dapper;
using Microsoft.Data.SqlClient;
using PumaMQ.Server.Models;

namespace PumaMQ.Server.Persistences;

internal class ChannelRepository
{
    private readonly DbConnectionFactory _dbConnectionFactory;


    public ChannelRepository(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }


    internal async Task<int> CreateAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        SqlConnection sqlConnection = default!;

        try
        {
            sqlConnection = await _dbConnectionFactory.CreateAsync().ConfigureAwait(false);

            string commandText = @"INSERT INTO [PumaMQ].[dbo].[Channel] ([TcpConnectionId], [ChannelNo])
                                   OUTPUT INSERTED.Id
                                   VALUES (@TcpConnectionId, @ChannelNo);";

            object param = new { TcpConnectionId = channel.ConnectionId, ChannelNo = (short) channel.ChannelNo };

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