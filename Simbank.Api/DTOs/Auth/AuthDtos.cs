namespace SimBank.Api.DTOs.Auth;

public record RegistrarUsuarioDto(string Nome, string Email, string Senha);
public record LoginDto(string Email, string Senha);
public record TokenRespostaDto(string Token, DateTime Expiracao, string Nome);