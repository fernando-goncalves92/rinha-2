using Api;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:Postgres"];

builder.Services.AddSingleton<CustomerRepository>();
builder.Services.AddScoped(_ => new BalanceRepository(connectionString));
builder.Services.AddScoped(_ => new TransactionRepository(connectionString));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("clientes/{id:int}/extrato", async (
    int id, 
    CancellationToken cancellationToken,
    [FromServices]CustomerRepository customerRepository,
    [FromServices]TransactionRepository transactionRepository,
    [FromServices]BalanceRepository balanceRepository) =>
{
    var customerResult = customerRepository.GetById(id);
    if (customerResult.Value is null)
        return Results.NotFound("Customer not found");
    
    var transactionResult = await transactionRepository.GetLast10(id, cancellationToken);
    if (transactionResult.IsFailed)
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    
    var balanceResult = await balanceRepository.GetByCustomerId(id, cancellationToken);
    if (balanceResult.IsFailed)
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    
    return Results.Ok
    (
        new
        {
            saldo = new
            {
                total = balanceResult.Value.Amount,
                data_extrato = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                limite = customerResult.Value.Limit
            },
            ultimas_transacoes = transactionResult.Value.Select(t => new
            {
                valor = t.Amount,
                tipo = t.TransactionType.ToLowerName(),
                descricao = t.Description,
                realizada_em = t.CreatedAt
            }) 
        }
    );
});

app.MapPost("clientes/{id:int}/transacoes", async (
    int id,
    CancellationToken cancellationToken,
    [FromBody]TransactionPost request,
    [FromServices]CustomerRepository customerRepository,
    [FromServices]TransactionRepository transactionRepository) =>
{
    if (!int.TryParse(request.Amount.ToString(), out var amount))
        return Results.StatusCode(StatusCodes.Status422UnprocessableEntity);
    if (!Enum.TryParse<TransactionType>(request.TransactionType.ToString(), true, out var transactionType))
        return Results.StatusCode(StatusCodes.Status422UnprocessableEntity);
    if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 10)
        return Results.StatusCode(StatusCodes.Status422UnprocessableEntity);

    var customerResult = customerRepository.GetById(id);
    if (customerResult.Value is null)
        return Results.NotFound("Customer not found");
        
    var transaction = Transaction.From(id, amount, transactionType, request.Description);
    var transactionResult = await transactionRepository.AddTransactionAndUpdateBalance(
        customerResult.Value,
        transaction, 
        cancellationToken);
    if (transactionResult.IsFailed)
        return Results.StatusCode(StatusCodes.Status422UnprocessableEntity);

    return Results.Ok
    (
        new
        {
            limite = customerResult.Value.Limit,
            saldo = transactionResult.Value.Amount
        }
    );
});

app.Run();