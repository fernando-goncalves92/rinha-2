using Api;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(_ => new Uow(builder.Configuration["ConnectionStrings:Postgres"]));

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
    [FromServices]Uow uow) =>
{
    await using (uow)
    {
        await uow.OpenConnection(cancellationToken);
    
        var customerResult = uow.CustomerRepository.GetById(id);
        if (customerResult.Value is null)
            return Results.NotFound("Customer not found");
    
        var transactionResult = await uow.TransactionRepository.GetLast10(id, cancellationToken);
        if (transactionResult.IsFailed)
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
    
        var balanceResult = await uow.BalanceRepository.GetByCustomerId(id, cancellationToken);
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
    }
});

app.MapPost("clientes/{id:int}/transacoes", async (
    int id,
    CancellationToken cancellationToken,
    [FromBody]TransactionPost request,
    [FromServices]Uow uow) =>
{
    if (!int.TryParse(request.Amount.ToString(), out var amount))
        return Results.StatusCode(StatusCodes.Status422UnprocessableEntity);
    if (!Enum.TryParse<TransactionType>(request.TransactionType.ToString(), true, out var transactionType))
        return Results.StatusCode(StatusCodes.Status422UnprocessableEntity);
    if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 10)
        return Results.StatusCode(StatusCodes.Status422UnprocessableEntity);

    await using (uow)
    {
        await uow.OpenConnection(cancellationToken);
        
        var customerResult = uow.CustomerRepository.GetById(id);
        if (customerResult.Value is null)
            return Results.NotFound("Customer not found");
        
        var transaction = Transaction.From(id, amount, transactionType, request.Description);
        var transactionResult = await uow.TransactionRepository.AddTransactionAndUpdateBalance(
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
    }
});

app.Run();