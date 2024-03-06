using Domain;
using FluentResults;
using Npgsql;

namespace Infrastructure;

public class BalanceRepository
{
    // managed by uow
    private readonly NpgsqlConnection _conn;
    
    public BalanceRepository(NpgsqlConnection conn)
    {
        _conn = conn;
    }
    
    public async Task<Result<Balance>> GetByCustomerId(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            var sql = "select id, customerId, amount, updatedAt from balance where customerId = $1;";
            
            await using var command = _conn.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue(customerId);

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