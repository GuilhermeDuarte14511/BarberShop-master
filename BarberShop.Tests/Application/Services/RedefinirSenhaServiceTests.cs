using BarberShop.Application.DTOs;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

#pragma warning disable CS8625
namespace BarberShop.Tests.Application.Services;

public class RedefinirSenhaServiceTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock = new();
    private readonly Mock<IAutenticacaoService> _autenticacaoServiceMock = new();
    private readonly Mock<IBarbeariaRepository> _barbeariaRepositoryMock = new();
    private readonly RedefinirSenhaService _service;

    public RedefinirSenhaServiceTests()
    {
        _service = new RedefinirSenhaService(
            _clienteRepositoryMock.Object,
            _autenticacaoServiceMock.Object,
            _barbeariaRepositoryMock.Object);
    }

    [Fact]
    public async Task ValidarTokenAsync_DeveRetornarTrueQuandoTokenValido()
    {
        var cliente = new Cliente
        {
            ClienteId = 1,
            TokenRecuperacaoSenha = "token",
            TokenExpiracao = DateTime.UtcNow.AddMinutes(10)
        };
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.ClienteId))
            .ReturnsAsync(cliente);

        var resultado = await _service.ValidarTokenAsync(cliente.ClienteId, "token");

        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task ValidarTokenAsync_DeveRetornarFalseQuandoTokenExpiradoOuInvalido()
    {
        var cliente = new Cliente
        {
            ClienteId = 1,
            TokenRecuperacaoSenha = "token",
            TokenExpiracao = DateTime.UtcNow.AddMinutes(-1)
        };
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.ClienteId))
            .ReturnsAsync(cliente);

        var expirado = await _service.ValidarTokenAsync(cliente.ClienteId, "token");
        var invalido = await _service.ValidarTokenAsync(cliente.ClienteId, "outro");

        expirado.Should().BeFalse();
        invalido.Should().BeFalse();
    }

    [Fact]
    public async Task RedefinirSenhaAsync_DeveRetornarFalseQuandoClienteNaoExiste()
    {
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Cliente)null!);

        var dto = new RedefinirSenhaDto { ClienteId = 1, NovaSenha = "NovaSenha123" };

        var resultado = await _service.RedefinirSenhaAsync(dto);

        resultado.Should().BeFalse();
        _clienteRepositoryMock.Verify(r => r.AtualizarSenhaAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Never);
    }

    [Fact]
    public async Task RedefinirSenhaAsync_DeveAtualizarSenhaQuandoClienteExiste()
    {
        var cliente = new Cliente { ClienteId = 1 };
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.ClienteId))
            .ReturnsAsync(cliente);
        _autenticacaoServiceMock
            .Setup(a => a.HashPassword("NovaSenha123"))
            .Returns("hash");

        var dto = new RedefinirSenhaDto { ClienteId = cliente.ClienteId, NovaSenha = "NovaSenha123" };

        var resultado = await _service.RedefinirSenhaAsync(dto);

        resultado.Should().BeTrue();
        _clienteRepositoryMock.Verify(r => r.AtualizarSenhaAsync(cliente.ClienteId, "hash", null, null), Times.Once);
    }

    [Fact]
    public async Task ObterUrlRedirecionamentoAsync_DeveRetornarUrlDaBarbearia()
    {
        var cliente = new Cliente { ClienteId = 5, BarbeariaId = 2 };
        var barbearia = new Barbearia { BarbeariaId = 2, UrlSlug = "meu-salao" };
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.ClienteId))
            .ReturnsAsync(cliente);
        _barbeariaRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.BarbeariaId))
            .ReturnsAsync(barbearia);

        var url = await _service.ObterUrlRedirecionamentoAsync(cliente.ClienteId);

        url.Should().Be("/meu-salao/Login");
    }

    [Fact]
    public async Task ObterUrlRedirecionamentoAsync_DeveRetornarLoginPadraoQuandoBarbeariaNaoEncontrada()
    {
        var cliente = new Cliente { ClienteId = 5, BarbeariaId = 3 };
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.ClienteId))
            .ReturnsAsync(cliente);
        _barbeariaRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.BarbeariaId))
            .ReturnsAsync((Barbearia)null!);

        var url = await _service.ObterUrlRedirecionamentoAsync(cliente.ClienteId);

        url.Should().Be("/Login");
    }
}
#pragma warning restore CS8625
