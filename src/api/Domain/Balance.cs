namespace Domain;

public class Balance
{
    private Balance(int id, int customerId, int amount, DateTime updatedAt)
    {
        Id = id;
        CustomerId = customerId;
        Amount = amount;
        UpdatedAt = updatedAt;
    }

    public int Id { get; }
    public int CustomerId { get; }
    public int Amount { get; }
    public DateTime UpdatedAt { get; }

    public static Balance From(int id, int customerId, int amount, DateTime updatedAt) => new (id, customerId, amount, updatedAt);
    public static Balance From(int id, int customerId, int amount) => new (id, customerId, amount, DateTime.Now);
}