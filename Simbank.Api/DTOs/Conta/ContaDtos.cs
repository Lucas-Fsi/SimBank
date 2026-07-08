using SimBank.Api.Models;

namespace SimBank.Api.DTOs.Conta;

public record CriarContaDto(int ClienteId, TipoConta Tipo);

public record ContaRespostaDto(int Id, string Numero, TipoConta Tipo, decimal Saldo, int ClienteId);

public record TransacaoRespostaDto(
    int Id, 
    string Tipo,
    decimal Valor,
    DateTime DataHora,
    string? Descricao
);

public record ExtratoRespostaDto(
    string NumeroConta,
    decimal SaldoAtual,
    List<TransacaoRespostaDto> Transacoes
);

public record DepositoSaqueDto(decimal Valor, string? Descricao);

public record TransferenciaDto(int ContaDestinoId, decimal Valor, string? Descricao);