using System.Collections.Immutable;
using Domain;
using FluentResults;
using Npgsql;

namespace Infrastructure;

public class TransactionRepository
{
    // managed by uow
    private readonly NpgsqlConnection _conn;
    
    public TransactionRepository(NpgsqlConnection conn)
    {
        _conn = conn;
    }
    
    public async Task<Result<ImmutableList<Transaction>>> GetLast10(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            var sql = @"
                      select
                         id
                        ,customerId
                        ,amount
                        ,transactionType
                        ,description
                        ,createdAt
                      from
                         transaction
                      where
                          customerId = $1
                      order by 
                          createdAt desc
                      limit 10
                      ";

            await using var command = _conn.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue(customerId);
            
            var transactions = new List<Transaction>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var id = reader.GetInt32(reader.GetOrdinal("id"));
                var amount = reader.GetInt32(reader.GetOrdinal("amount"));
                var transactionType = Enum.Parse<TransactionType>(reader["transactionType"].ToString()!, true);
                var description = reader.GetString(reader.GetOrdinal("description"));
                var createdAt = reader.GetDateTime(reader.GetOrdinal("createdAt"));
                var transaction = Transaction.From(id, customerId, amount, transactionType, description, createdAt);
            
                transactions.Add(transaction);
            }
            
            return transactions.ToImmutableList();
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
    
    public async Task<Result<Balance>> AddTransactionAndUpdateBalance(Customer customer, Transaction transaction, CancellationToken cancellationToken)
    {
        try
        {
            var sql = "select id_balance, new_balance, has_error from AddTransaction($1, $2, $3, $4, $5)";

            await using var command = _conn.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue(customer.Id);
            command.Parameters.AddWithValue(customer.Limit);
            command.Parameters.AddWithValue(transaction.TransactionType.ToLowerName());
            command.Parameters.AddWithValue(transaction.Amount);
            command.Parameters.AddWithValue(transaction.Description);

            var idBalance = 0;
            var newBalance = 0;
            var error = false;
            
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                idBalance = reader.GetInt32(reader.GetOrdinal("id_balance"));
                newBalance = reader.GetInt32(reader.GetOrdinal("new_balance"));
                error = reader.GetBoolean(reader.GetOrdinal("has_error"));
            }

            return error 
                ? Result.Fail("Invalid Transaction") 
                : Result.Ok(Balance.From(idBalance, customer.Id, newBalance));
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