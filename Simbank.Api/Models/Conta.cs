namespace SimBank.Api.Models;

public enum TipoConta
{
    Corrente,
    Poupanca
}

public class Conta
{
    public int Id {get; set; }
    public string Numero {get; set; } = string.Empty;
    public TipoConta Tipo {get; set; } 
    public decimal Saldo {get; set; } = 0;
    public DateTime DataAbertura {get; set; } = DateTime.UtcNow;

    public int ClienteId {get; set; }
    public Cliente? Cliente {get; set; }

    public List<Transacao> Transacoes {get; set; } = new();
 }