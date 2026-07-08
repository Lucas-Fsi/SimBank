using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimBank.Api.Data;
using SimBank.Api.DTOs.Auth;
using SimBank.Api.Exceptions;
using SimBank.Api.Models;
using SimBank.Api.Services;

namespace SimBank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar(RegistrarUsuarioDto dto)
    {
        var emailNormalizado = dto.Email.ToLower().Trim();

        var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == emailNormalizado);
        if (emailExiste)
            throw new ConflictException("Já existe um usuário com esse e-mail.");

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

        var usuario = new Usuario
        {
        Nome = dto.Nome,
        Email = emailNormalizado, // salva sempre em minúsculo
        SenhaHash = senhaHash
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Usuário registrado com sucesso." });
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenRespostaDto>> Login(LoginDto dto)
    {
        var emailNormalizado = dto.Email.ToLower().Trim();

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == emailNormalizado);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            throw new BusinessException("E-mail ou senha inválidos.");

        var (token, expiracao) = _tokenService.GerarToken(usuario);

        return Ok(new TokenRespostaDto(token, expiracao, usuario.Nome));
    }
}