namespace SimBank.Api.DTOs.Cliente;

public record CriarClienteDto(string Nome, string Cpf, string Email);

public record ClienteRespostaDto(int Id, string Nome, string Cpf, string Email, DateTime DataCAdastro);