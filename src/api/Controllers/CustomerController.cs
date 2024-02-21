using System.Transactions;
using Api.Models;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
public class CustomerController : ControllerBase
{
    private readonly CustomerRepository _customerRepository;
    private readonly BalanceRepository _balanceRepository;
    private readonly TransactionRepository _transactionRepository;

    public CustomerController(CustomerRepository customerRepository, BalanceRepository balanceRepository, TransactionRepository transactionRepository)
    {
        _customerRepository = customerRepository;
        _balanceRepository = balanceRepository;
        _transactionRepository = transactionRepository;
    }

    [HttpPost("clientes/{id:int}/transacoes")]
    public async Task<ActionResult> PostTransaction(int id, CancellationToken cancellationToken, TransactionPost request)
    {
        if (!int.TryParse(request.Amount.ToString(), out var amount))
            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        if (!Enum.TryParse<TransactionType>(request.TransactionType.ToString(), true, out var transactionType))
            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 10)
            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        
        var customerResult = _customerRepository.GetById(id);
        if (customerResult.IsFailed)
            return StatusCode(StatusCodes.Status500InternalServerError, customerResult.Errors);
        if (customerResult.Value is null)
            return NotFound("Customer not found");

        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            
        var balanceResult = await _balanceRepository.GetByCustomerId(id, cancellationToken);
        if (balanceResult.IsFailed)
            return StatusCode(StatusCodes.Status500InternalServerError, balanceResult.Errors);

        if (transactionType == TransactionType.D &&
            balanceResult.Value.Amount - amount < -customerResult.Value.Limit)
            return StatusCode(StatusCodes.Status422UnprocessableEntity);
            
        var transaction = Domain.Transaction.From(id, amount, transactionType, request.Description);
        var transactionResult = await _transactionRepository.Add(transaction, cancellationToken);
        if (transactionResult.IsFailed)
            return StatusCode(StatusCodes.Status500InternalServerError, transactionResult.Errors);
            
        var newBalanceAmount = transactionType == TransactionType.D
            ? balanceResult.Value.Amount - amount
            : balanceResult.Value.Amount + amount;
        
        var updatedBalance = Balance.From(balanceResult.Value.Id, id, newBalanceAmount);
        var newBalanceResult = await _balanceRepository.Update(updatedBalance, cancellationToken);
        if (newBalanceResult.IsFailed)
            return StatusCode(StatusCodes.Status500InternalServerError, newBalanceResult.Errors);
        
        transactionScope.Complete();
            
        return Ok
        (
            new
            {
                limite = customerResult.Value.Limit,
                saldo =  updatedBalance.Amount
            }
        );
    }

    [HttpGet("clientes/{id:int}/extrato")]
    public async Task<ActionResult> GetExtract(int id, CancellationToken cancellationToken)
    {
        var customerResult = _customerRepository.GetById(id);
        if (customerResult.Value is null)
            return NotFound("Customer not found");
    
        var transactionResult = await _transactionRepository.GetLast10(id, cancellationToken);
        if (transactionResult.IsFailed)
            return StatusCode(StatusCodes.Status500InternalServerError, transactionResult.Errors);
            
        var balanceResult = await _balanceRepository.GetByCustomerId(id, cancellationToken);
        if (balanceResult.IsFailed)
            return StatusCode(StatusCodes.Status500InternalServerError, balanceResult.Errors);
            
        return Ok
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
                    tipo = t.TransactionType.GetLowerName(),
                    descricao = t.Description,
                    realizada_em = t.CreatedAt
                }) 
            }
        );
    }
}