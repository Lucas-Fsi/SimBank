namespace SimBank.Api.Models;

public enum TipoTransacao
{
    Deposito,
    Saque,
    TransferenciaEnviada,
    TransferenciaRecebida
}

public class Transacao
{
    public int Id { get; set; }
    public TipoTransacao Tipo { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public string? Descricao { get; set; }


    public int ContaId { get; set; }
    public Conta? Conta { get; set; }
}