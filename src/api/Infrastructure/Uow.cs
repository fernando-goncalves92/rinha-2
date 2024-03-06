using Npgsql;

namespace Infrastructure;

public class Uow : IDisposable, IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;
    
    public Uow(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        BalanceRepository = new BalanceRepository(_connection);
        CustomerRepository = new CustomerRepository();
        TransactionRepository = new TransactionRepository(_connection);
    }

    public BalanceRepository BalanceRepository { get; }
    public CustomerRepository CustomerRepository { get; }
    public TransactionRepository TransactionRepository { get; }
   
    public Task OpenConnection(CancellationToken cancellationToken)
    {
        return _connection.OpenAsync(cancellationToken);
    }
    
    public void Dispose()
    {
        if (_connection != null) _connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null) await _connection.DisposeAsync().ConfigureAwait(false);
    }
}