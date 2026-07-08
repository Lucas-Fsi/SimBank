using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimBank.Api.Data;
using SimBank.Api.DTOs.Conta;
using SimBank.Api.Models;
using SimBank.Api.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace SimBank.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ContasController : ControllerBase
{
    private readonly AppDbContext _context;

    public ContasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContaRespostaDto>>> GetAll()
    {
        var contas = await _context.Contas
            .Select(c => new ContaRespostaDto(c.Id, c.Numero, c.Tipo, c.Saldo, c.ClienteId))
            .ToListAsync();

        return Ok(contas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContaRespostaDto>> GetById(int id)
    {
        var conta = await _context.Contas.FindAsync(id);
        if (conta is null)
            throw new NotFoundException($"Cliente com id {id} não encontrado.");

        return Ok(new ContaRespostaDto(conta.Id, conta.Numero, conta.Tipo, conta.Saldo, conta.ClienteId));
    }

    [HttpPost]
    public async Task<ActionResult<ContaRespostaDto>> Create(CriarContaDto dto)
    {
        var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == dto.ClienteId);
        if (!clienteExiste)
            throw new NotFoundException($"Cliente com id {dto.ClienteId} não encontrado.");

        var conta = new Conta
        {
            ClienteId = dto.ClienteId,
            Tipo = dto.Tipo,
            Numero = GerarNumeroConta(),
            Saldo = 0
        };

        _context.Contas.Add(conta);
        await _context.SaveChangesAsync();

        var resposta = new ContaRespostaDto(conta.Id, conta.Numero, conta.Tipo, conta.Saldo, conta.ClienteId);
        return CreatedAtAction(nameof(GetById), new { id = conta.Id }, resposta);
    }

    private static string GerarNumeroConta()
    {
        var numero = Random.Shared.Next(100000, 999999);
        var digito = Random.Shared.Next(0, 9);
        return $"{numero}-{digito}";
    }

    [HttpGet("{id}/extrato")]
    public async Task<ActionResult<ExtratoRespostaDto>> Extrato(
    int id,
    [FromQuery] DateTime? de,
    [FromQuery] DateTime? ate)
    {
    var conta = await _context.Contas.FindAsync(id);
    if (conta is null)
        throw new NotFoundException($"Cliente com id {id} não encontrado.");

    var query = _context.Transacoes
    .Where(t => t.ContaId == id)
    .AsQueryable();

    if (de.HasValue)
        query = query.Where(t => t.DataHora >= de.Value);

    if (ate.HasValue)
        query = query.Where(t => t.DataHora <= ate.Value);

    var transacoes = await query
        .OrderByDescending(t => t.DataHora)
        .Select(t => new TransacaoRespostaDto(
            t.Id,
            t.Tipo.ToString(),
            t.Valor,
            t.DataHora,
            t.Descricao))
        .ToListAsync();

    var extrato = new ExtratoRespostaDto(
        conta.Numero,
        conta.Saldo,
        transacoes
    );

    return Ok(extrato);
    }

    [HttpPost("{id}/depositar")]
public async Task<IActionResult> Depositar(int id, DepositoSaqueDto dto)
{
    if (dto.Valor <= 0)
        throw new BusinessException("O valor do depósito deve ser maior que zero.");

    var conta = await _context.Contas.FindAsync(id);
    if (conta is null)
        throw new NotFoundException("Conta não encontrada.");

    conta.Saldo += dto.Valor;

    _context.Transacoes.Add(new Transacao
    {
        ContaId = conta.Id,
        Tipo = TipoTransacao.Deposito,
        Valor = dto.Valor,
        Descricao = dto.Descricao ?? "Depósito"
    });

    await _context.SaveChangesAsync();

    return Ok(new { conta.Id, conta.Numero, SaldoAtual = conta.Saldo });
}

[HttpPost("{id}/sacar")]
public async Task<IActionResult> Sacar(int id, DepositoSaqueDto dto)
{
    if (dto.Valor <= 0)
        throw new BusinessException("O valor do saque deve ser maior que zero.");

    var conta = await _context.Contas.FindAsync(id);
    if (conta is null)
        throw new NotFoundException("Conta não encontrada.");

    if (conta.Saldo < dto.Valor)
        throw new BusinessException("Saldo insuficiente.");

    conta.Saldo -= dto.Valor;

    _context.Transacoes.Add(new Transacao
    {
        ContaId = conta.Id,
        Tipo = TipoTransacao.Saque,
        Valor = dto.Valor,
        Descricao = dto.Descricao ?? "Saque"
    });

    await _context.SaveChangesAsync();

    return Ok(new { conta.Id, conta.Numero, SaldoAtual = conta.Saldo });
}

[HttpPost("{id}/transferir")]
public async Task<IActionResult> Transferir(int id, TransferenciaDto dto)
{
    if (dto.Valor <= 0)
        throw new BusinessException("O valor da transferência deve ser maior que zero.");

    if (id == dto.ContaDestinoId)
        throw new BusinessException("Não é possível transferir para a mesma conta.");

    var contaOrigem = await _context.Contas.FindAsync(id);
    if (contaOrigem is null)
        throw new NotFoundException("Conta de origem não encontrada.");

    var contaDestino = await _context.Contas.FindAsync(dto.ContaDestinoId);
    if (contaDestino is null)
        throw new NotFoundException("Conta de destino não encontrada.");

    if (contaOrigem.Saldo < dto.Valor)
        throw new BusinessException("Saldo insuficiente.");

    using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        contaOrigem.Saldo -= dto.Valor;
        contaDestino.Saldo += dto.Valor;

        _context.Transacoes.Add(new Transacao
        {
            ContaId = contaOrigem.Id,
            Tipo = TipoTransacao.TransferenciaEnviada,
            Valor = dto.Valor,
            Descricao = dto.Descricao ?? $"Transferência para conta {contaDestino.Numero}"
        });

        _context.Transacoes.Add(new Transacao
        {
            ContaId = contaDestino.Id,
            Tipo = TipoTransacao.TransferenciaRecebida,
            Valor = dto.Valor,
            Descricao = dto.Descricao ?? $"Transferência recebida da conta {contaOrigem.Numero}"
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw new BusinessException("Erro ao processar a transferência. Nenhum valor foi movimentado.");
    }

    return Ok(new
    {
        ContaOrigem = new { contaOrigem.Id, SaldoAtual = contaOrigem.Saldo },
        ContaDestino = new { contaDestino.Id, SaldoAtual = contaDestino.Saldo }
    });
}

}