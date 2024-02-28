using Domain;
using FluentResults;
using Npgsql;

namespace Infrastructure;

public class BalanceRepository
{
    // managed by uow, doesn't need to be disposed here
    private readonly NpgsqlConnection _conn;
    
    public BalanceRepository(NpgsqlConnection conn)
    {
        _conn = conn;
    }
    
    public async Task<Result> Update(Balance balance, CancellationToken cancellationToken)
    {
        try
        {
            var sql = "update balance set amount = $1, updatedAt = $2 where id = $3";

            await using var command = _conn.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue(balance.Amount);
            command.Parameters.AddWithValue(balance.UpdatedAt);
            command.Parameters.AddWithValue(balance.Id);
            
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return Result.Ok();
        }
        catch (Exception e)
        {
            var error = e.InnerException is not null
                ? e.InnerException.Message
                : e.Message; 
            
            return Result.Fail(error);
        }
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
            
            return Result.Fail(error);
        }
    }
    
    public async Task<Result> LockTableInExclusiveMode(CancellationToken cancellationToken)
    {
        try
        {
            var sql = "lock table balance in access exclusive mode;";

            await using var command = _conn.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return Result.Ok();
        }
        catch (Exception e)
        {
            var error = e.InnerException is not null
                ? e.InnerException.Message
                : e.Message; 
            
            return Result.Fail(error);
        }
    } 
}