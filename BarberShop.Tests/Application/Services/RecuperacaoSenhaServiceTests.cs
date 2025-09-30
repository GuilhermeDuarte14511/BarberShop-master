using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace BarberShop.Tests.Application.Services;

public class RecuperacaoSenhaServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IAutenticacaoService> _autenticacaoServiceMock = new();
    private readonly Mock<IClienteRepository> _clienteRepositoryMock = new();
    private readonly RecuperacaoSenhaService _service;

    public RecuperacaoSenhaServiceTests()
    {
        _service = new RecuperacaoSenhaService(
            _usuarioRepositoryMock.Object,
            _emailServiceMock.Object,
            _httpContextAccessorMock.Object,
            _autenticacaoServiceMock.Object,
            _clienteRepositoryMock.Object);
    }

    [Fact]
    public async Task EnviarEmailRecuperacaoSenhaAsync_DeveLancarExcecaoQuandoClienteNaoEncontrado()
    {
        _clienteRepositoryMock
            .Setup(r => r.GetByEmailAsync("teste@email.com"))
            .ReturnsAsync((Cliente)null!);

        await Assert.ThrowsAsync<Exception>(() => _service.EnviarEmailRecuperacaoSenhaAsync("teste@email.com"));
    }

    [Fact]
    public async Task EnviarEmailRecuperacaoSenhaAsync_DeveGerarTokenEEnviarEmail()
    {
        var cliente = new Cliente { ClienteId = 1, Email = "teste@email.com", Nome = "Teste" };
        _clienteRepositoryMock
            .Setup(r => r.GetByEmailAsync(cliente.Email))
            .ReturnsAsync(cliente);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("barbershop.com", 443);
        _httpContextAccessorMock
            .SetupGet(a => a.HttpContext)
            .Returns(httpContext);

        await _service.EnviarEmailRecuperacaoSenhaAsync(cliente.Email);

        cliente.TokenRecuperacaoSenha.Should().NotBeNullOrEmpty();
        cliente.TokenExpiracao.Should().BeAfter(DateTime.UtcNow);
        _clienteRepositoryMock.Verify(r => r.UpdateAsync(cliente), Times.Once);
        _emailServiceMock.Verify(e => e.EnviarEmailRecuperacaoSenhaAsync(
            cliente.Email,
            cliente.Nome,
            It.Is<string>(link => link.StartsWith("https://barbershop.com") && link.Contains("/redefinir-senha") && link.Contains($"clienteId={cliente.ClienteId}"))
        ), Times.Once);
    }

    [Fact]
    public async Task VerificarTokenAsync_DeveRetornarTrueQuandoTokenValido()
    {
        var cliente = new Cliente
        {
            ClienteId = 1,
            TokenRecuperacaoSenha = "token",
            TokenExpiracao = DateTime.UtcNow.AddMinutes(5)
        };
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.ClienteId))
            .ReturnsAsync(cliente);

        var resultado = await _service.VerificarTokenAsync(cliente.ClienteId, "token");

        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task VerificarTokenAsync_DeveRetornarFalseQuandoTokenInvalido()
    {
        var cliente = new Cliente
        {
            ClienteId = 1,
            TokenRecuperacaoSenha = "token",
            TokenExpiracao = DateTime.UtcNow.AddMinutes(5)
        };
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.ClienteId))
            .ReturnsAsync(cliente);

        var resultado = await _service.VerificarTokenAsync(cliente.ClienteId, "outro");

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task RedefinirSenhaAsync_DeveRetornarFalseQuandoClienteNaoExiste()
    {
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Cliente)null!);

        var resultado = await _service.RedefinirSenhaAsync(1, "NovaSenha!");

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task RedefinirSenhaAsync_DeveRetornarFalseQuandoTokenExpirado()
    {
        var cliente = new Cliente { ClienteId = 1, TokenExpiracao = DateTime.UtcNow.AddMinutes(-1) };
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.ClienteId))
            .ReturnsAsync(cliente);

        var resultado = await _service.RedefinirSenhaAsync(cliente.ClienteId, "NovaSenha!");

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task RedefinirSenhaAsync_DeveAtualizarSenhaQuandoDadosValidos()
    {
        var cliente = new Cliente
        {
            ClienteId = 1,
            TokenExpiracao = DateTime.UtcNow.AddMinutes(10),
            TokenRecuperacaoSenha = "token"
        };
        _clienteRepositoryMock
            .Setup(r => r.GetByIdAsync(cliente.ClienteId))
            .ReturnsAsync(cliente);
        _autenticacaoServiceMock
            .Setup(a => a.HashPassword("NovaSenha!"))
            .Returns("hash");

        var resultado = await _service.RedefinirSenhaAsync(cliente.ClienteId, "NovaSenha!");

        resultado.Should().BeTrue();
        cliente.Senha.Should().Be("hash");
        cliente.TokenRecuperacaoSenha.Should().BeNull();
        cliente.TokenExpiracao.Should().BeNull();
        _clienteRepositoryMock.Verify(r => r.UpdateAsync(cliente), Times.Once);
    }
}
