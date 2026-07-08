using System.ComponentModel;
using Microsoft.AspNetCore.SignalR;

namespace SimBank.Api.Models;

public class Cliente
 {
    public int Id {get; set; }
    public string Nome {get; set; } = string.Empty;
    public string Cpf {get; set; } = string.Empty;
    public string Email {get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public List<Conta> Contas {get; set;} = new();

}