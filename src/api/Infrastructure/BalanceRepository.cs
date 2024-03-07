using Domain;
using FluentResults;
using Npgsql;

namespace Infrastructure;

public class BalanceRepository
{
    private readonly string _connectionString;
    
    public BalanceRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<Result<Balance>> GetByCustomerId(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await using var command = connection.CreateCommand();
            command.CommandText = "select id, customerId, amount, updatedAt from balance where customerId = $1;";
            command.Parameters.AddWithValue(customerId);

            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            Balance balance = null;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var id = reader.GetInt32(reader.GetOrdinal("id"));
                var amount = reader.GetInt32(reader.GetOrdinal("amount"));
                var updatedAt = reader.GetDateTime(reader.GetOrdinal("updatedAt"));

                balance = Balance.From(id, customerId, amount, updatedAt);
            }

            return Result.Ok(balance);
        }
        catch (Exception e)
        {
            var error = e.InnerException is not null
                ? e.InnerException.Message
                : e.Message; 
            
            Console.WriteLine(error);
            
            return Result.Fail(error);
        }
    }
}