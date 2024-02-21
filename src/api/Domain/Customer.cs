namespace Domain;

public class Customer
{
    private Customer(int id, int limit)
    {
        Id = id;
        Limit = limit;
    }
    
    public int Id { get; }
    public int Limit { get; }

    public static Customer From(int id, int limit) => new(id, limit);
}