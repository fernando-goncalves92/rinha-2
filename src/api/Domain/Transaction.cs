namespace Domain;

public class Transaction
{
    private Transaction(
        long id, 
        int customerId, 
        int amount, 
        TransactionType transactionType, 
        string description,
        DateTime createdAt)
    {
        Id = id;
        CustomerId = customerId;
        Amount = amount;
        TransactionType = transactionType;
        Description = description;
        CreatedAt = createdAt;
    }
    
    public long Id { get; }
    public int CustomerId { get; }
    public int Amount { get; }
    public TransactionType TransactionType { get; }
    public string Description { get; }
    public DateTime CreatedAt { get; }
    
    public static Transaction From(
        long id,
        int customerId,
        int amount,
        TransactionType transactionType,
        string description,
        DateTime createdAt) => new(
        id,
        customerId,
        amount, 
        transactionType, 
        description, 
        createdAt);
    
    public static Transaction From(
        int customerId,
        int amount,
        TransactionType transactionType,
        string description) => new(
        0,
        customerId,
        amount, 
        transactionType, 
        description, 
        DateTime.Now);
}