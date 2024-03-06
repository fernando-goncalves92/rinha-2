namespace Domain;

public enum TransactionType
{
    C,
    D
}

public static class TransactionTypeExtensions
{
    public static string ToLowerName(this TransactionType transactionType)
    {
        return transactionType.ToString().ToLower();
    }
}