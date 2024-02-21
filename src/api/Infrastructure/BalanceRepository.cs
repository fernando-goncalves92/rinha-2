using Domain;
using System.Data.SqlClient;
using FluentResults;

namespace Infrastructure;

public class BalanceRepository
{
    private readonly string _connectionString;
    
    public BalanceRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<Result> Update(Balance balance, CancellationToken cancellationToken)
    {
        try
        {
            var sql = "update balance set amount = @amount, updatedAt = @updatedAt where id = @id";

            await using var con = new SqlConnection(_connectionString);
            await using var command = con.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@id", balance.Id);
            command.Parameters.AddWithValue("@amount", balance.Amount);
            command.Parameters.AddWithValue("@updatedAt", balance.UpdatedAt);

            await con.OpenAsync(cancellationToken).ConfigureAwait(false);
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
    
    public async Task<Result<Balance>> GetByCustomerId(int customerId , CancellationToken cancellationToken)
    {
        try
        {
            var sql = "select id, customerId, amount, updatedAt from balance with(tablockx) where customerId = @customerId";

            await using var con = new SqlConnection(_connectionString);
            await using var command = con.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@customerId", customerId);
            
            await con.OpenAsync(cancellationToken).ConfigureAwait(false);

            Balance balance = null;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken))
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
}