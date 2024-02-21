using System.Collections.Immutable;
using Domain;
using System.Data.SqlClient;
using FluentResults;

namespace Infrastructure;

public class TransactionRepository
{
    private readonly string _connectionString;
    
    public TransactionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<Result> Add(Transaction transaction, CancellationToken cancellationToken)
    {
        try
        {
            var sql = @"
                      insert into [transaction]
                      (
                         customerId
                        ,amount
                        ,transactionType
                        ,description
                        ,createdAt
                      )
                      values
                      (
                         @customerId
                        ,@amount
                        ,@transactionType
                        ,@description
                        ,@createdAt
                      )
                      ";

            await using var con = new SqlConnection(_connectionString);
            await using var command = con.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@customerId", transaction.CustomerId);
            command.Parameters.AddWithValue("@amount", transaction.Amount);
            command.Parameters.AddWithValue("@transactionType", transaction.TransactionType.GetLowerName());
            command.Parameters.AddWithValue("@description", transaction.Description);
            command.Parameters.AddWithValue("@createdAt", transaction.CreatedAt);

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

    public async Task<Result<ImmutableList<Transaction>>> GetLast10(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            var sql = @"
                      select top 10
                         id
                        ,customerId
                        ,amount
                        ,transactionType
                        ,description
                        ,createdAt
                      from
                         [transaction]
                      where
                          customerId = @customerId
                      order by 
                          createdAt desc
                      ";

            await using var con = new SqlConnection(_connectionString);
            await using var command = con.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@customerId", customerId);

            await con.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            var transactions = new List<Transaction>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken))
            {
                var id = reader.GetInt32(reader.GetOrdinal("id"));
                var amount = reader.GetInt32(reader.GetOrdinal("amount"));
                var transactionType = Enum.Parse<TransactionType>(reader["transactionType"].ToString()!, true);
                var description = reader.GetString(reader.GetOrdinal("description"));
                var createdAt = reader.GetDateTime(reader.GetOrdinal("createdAt"));
                var transaction = Transaction.From(id, customerId, amount, transactionType, description!, createdAt);
            
                transactions.Add(transaction);
            }
            
            return transactions.ToImmutableList();
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