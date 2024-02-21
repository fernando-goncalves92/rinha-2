using FluentResults;
using Domain;

namespace Infrastructure;

public class CustomerRepository
{
    private readonly IReadOnlyList<Customer> _customers = new List<Customer>()
    {
        Customer.From(1, 100_000),
        Customer.From(2, 80_000),
        Customer.From(3, 1_000_000),
        Customer.From(4, 10_000_000),
        Customer.From(5, 500_000),
    };

    public Result<Customer> GetById(int id)
    {
        return Result.Ok(_customers.FirstOrDefault(c => c.Id == id));
    }
}