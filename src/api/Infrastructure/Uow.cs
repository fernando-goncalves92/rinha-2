using Npgsql;

namespace Infrastructure;

public class Uow : IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    
    private BalanceRepository _balanceRepository;
    private CustomerRepository _customerRepository;
    private TransactionRepository _transactionRepository;
    private NpgsqlConnection _connection;
    private NpgsqlTransaction _transaction;
    
    public Uow(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public BalanceRepository BalanceRepository => _balanceRepository ??= new BalanceRepository(_connection);
    public CustomerRepository CustomerRepository => _customerRepository ??= new CustomerRepository();
    public TransactionRepository TransactionRepository => _transactionRepository ??= new TransactionRepository(_connection);

    public async Task OpenConnectionWithTransactionAsync()
    {
        _connection = _dataSource.CreateConnection();
        
        await _connection.OpenAsync().ConfigureAwait(false);
        
        _transaction = await _connection.BeginTransactionAsync();
    }
    
    public Task OpenConnection()
    {
        _connection = _dataSource.CreateConnection();
        
        return _connection.OpenAsync();
    }

    public Task Commit()
    {
        return _transaction.CommitAsync();
    }
    
    public Task Rollback()
    {
        return _transaction.RollbackAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null) 
            await _transaction.DisposeAsync().ConfigureAwait(false);

        await _connection.DisposeAsync().ConfigureAwait(false);
    }
}