namespace Domain;

public enum TransactionType
{
    C,
    D
}

public static class TransactionTypeExtensions
{
    public static string GetLowerName(this TransactionType transactionType)
    {
        return transactionType.ToString().ToLower();
    }
}