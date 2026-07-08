using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimBank.Api.Data;
using SimBank.Api.DTOs.Cliente;
using SimBank.Api.Models;
using SimBank.Api.Exceptions;
using Microsoft.AspNetCore.Authorization;


namespace SimBank.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteRespostaDto>>> GetAll()
    {
        var clientes = await _context.Clientes
            .Select(c => new ClienteRespostaDto(c.Id, c.Nome, c.Cpf, c.Email, c.DataCadastro))
            .ToListAsync();

        return Ok(clientes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteRespostaDto>> GetById(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente is null)
            throw new NotFoundException($"Cliente com id {id} não encontrado.");

        return Ok(new ClienteRespostaDto(cliente.Id, cliente.Nome, cliente.Cpf, cliente.Email, cliente.DataCadastro));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteRespostaDto>> Create(CriarClienteDto dto)
    {
       var cpfExiste = await _context.Clientes.AnyAsync(c => c.Cpf == dto.Cpf);
        if (cpfExiste)
            throw new ConflictException("Já existe um cliente cadastrado com esse CPF.");

        var cliente = new Cliente { Nome = dto.Nome, Cpf = dto.Cpf, Email = dto.Email };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        var resposta = new ClienteRespostaDto(cliente.Id, cliente.Nome, cliente.Cpf, cliente.Email, cliente.DataCadastro);
        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, resposta);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
            return NotFound();

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}