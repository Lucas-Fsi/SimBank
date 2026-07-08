using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SimBank.Api.Controllers;
using SimBank.Api.DTOs.Conta;
using SimBank.Api.Exceptions;
using SimBank.Api.Models;
using SimBank.Tests.Helpers;

namespace SimBank.Tests.Tests;

public class OperacoesBancariasTests
{
    private async Task<(ContasController controller, Conta conta)> CriarCenario()
    {
        var context = DbContextFactory.Criar();

        var cliente = new Cliente { Nome = "João Silva", Cpf = "12345678900", Email = "joao@email.com" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var conta = new Conta { ClienteId = cliente.Id, Numero = "123456-7", Tipo = TipoConta.Corrente, Saldo = 1000 };
        context.Contas.Add(conta);
        await context.SaveChangesAsync();

        var controller = new ContasController(context);
        return (controller, conta);
    }

    [Fact]
    public async Task Depositar_ValorValido_DeveAumentarSaldo()
    {
        var (controller, conta) = await CriarCenario();
        var dto = new DepositoSaqueDto(500, "Depósito teste");

        var resultado = await controller.Depositar(conta.Id, dto);

        resultado.Should().BeOfType<OkObjectResult>();
        conta.Saldo.Should().Be(1500);
    }

    [Fact]
    public async Task Depositar_ValorZero_DeveLancarExcecao()
    {
        var (controller, conta) = await CriarCenario();
        var dto = new DepositoSaqueDto(0, null);

        var acao = async () => await controller.Depositar(conta.Id, dto);

        await acao.Should().ThrowAsync<BusinessException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public async Task Sacar_ValorValido_DeveReduzirSaldo()
    {
        var (controller, conta) = await CriarCenario();
        var dto = new DepositoSaqueDto(300, "Saque teste");

        var resultado = await controller.Sacar(conta.Id, dto);

        resultado.Should().BeOfType<OkObjectResult>();
        conta.Saldo.Should().Be(700);
    }

    [Fact]
    public async Task Sacar_SaldoInsuficiente_DeveLancarExcecao()
    {
        var (controller, conta) = await CriarCenario();
        var dto = new DepositoSaqueDto(9999, null);

        var acao = async () => await controller.Sacar(conta.Id, dto);

        await acao.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Saldo insuficiente*");

        conta.Saldo.Should().Be(1000);
    }

    [Fact]
    public async Task Transferir_EntreContas_DeveAtualizarAmbosOsSaldos()
    {
        var context = DbContextFactory.Criar();

        var cliente = new Cliente { Nome = "Maria", Cpf = "98765432100", Email = "maria@email.com" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var contaOrigem = new Conta { ClienteId = cliente.Id, Numero = "111111-1", Tipo = TipoConta.Corrente, Saldo = 1000 };
        var contaDestino = new Conta { ClienteId = cliente.Id, Numero = "222222-2", Tipo = TipoConta.Corrente, Saldo = 500 };
        context.Contas.AddRange(contaOrigem, contaDestino);
        await context.SaveChangesAsync();

        var controller = new ContasController(context);
        var dto = new TransferenciaDto(contaDestino.Id, 300, "Transferência teste");

        var resultado = await controller.Transferir(contaOrigem.Id, dto);

        resultado.Should().BeOfType<OkObjectResult>();
        contaOrigem.Saldo.Should().Be(700);
        contaDestino.Saldo.Should().Be(800);
    }

    [Fact]
    public async Task Transferir_SaldoInsuficiente_DeveLancarExcecao()
    {
        var context = DbContextFactory.Criar();

        var cliente = new Cliente { Nome = "Carlos", Cpf = "11122233300", Email = "carlos@email.com" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var contaOrigem = new Conta { ClienteId = cliente.Id, Numero = "333333-3", Tipo = TipoConta.Corrente, Saldo = 100 };
        var contaDestino = new Conta { ClienteId = cliente.Id, Numero = "444444-4", Tipo = TipoConta.Corrente, Saldo = 0 };
        context.Contas.AddRange(contaOrigem, contaDestino);
        await context.SaveChangesAsync();

        var controller = new ContasController(context);
        var dto = new TransferenciaDto(contaDestino.Id, 9999, null);

        var acao = async () => await controller.Transferir(contaOrigem.Id, dto);

        await acao.Should().ThrowAsync<BusinessException>();
        contaOrigem.Saldo.Should().Be(100);
    }
}