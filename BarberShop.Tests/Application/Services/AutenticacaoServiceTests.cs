using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BarberShop.Tests.Application.Services;

public class AutenticacaoServiceTests
{
    private readonly AutenticacaoService _service = new();

    [Fact]
    public void AutenticarCliente_DeveRetornarNullQuandoClienteNulo()
    {
        var principal = _service.AutenticarCliente(null!, "slug");

        principal.Should().BeNull();
    }

    [Fact]
    public void AutenticarCliente_DeveCriarClaimsEsperados()
    {
        var cliente = new Cliente
        {
            ClienteId = 10,
            Nome = "Joao",
            Email = "joao@email.com",
            Telefone = "+5511999999999",
            Role = "Cliente"
        };

        var principal = _service.AutenticarCliente(cliente, "barbearia-slug");

        principal.Should().NotBeNull();
        principal.Identity.Should().NotBeNull();
        principal.Identity!.AuthenticationType.Should().Be("Cookies");
        principal.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "10");
        principal.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.Email && c.Value == cliente.Email);
        principal.Claims.Should().ContainSingle(c => c.Type == "Telefone" && c.Value == cliente.Telefone);
        principal.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.Role && c.Value == cliente.Role);
        principal.Claims.Should().ContainSingle(c => c.Type == "BarbeariaUrl" && c.Value == "barbearia-slug");
    }

    [Fact]
    public void AutenticarCliente_DeveUtilizarTelefoneQuandoEmailVazio()
    {
        var cliente = new Cliente
        {
            ClienteId = 42,
            Nome = "Maria",
            Email = null!,
            Telefone = "+5511988887777",
            Role = "Cliente"
        };

        var principal = _service.AutenticarCliente(cliente, "slug");

        principal.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.Email && c.Value == cliente.Telefone);
    }

    [Fact]
    public void HashPassword_DeveRetornarHashDeterministico()
    {
        const string senha = "Senha@123";
        var expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(senha)));

        var hash = _service.HashPassword(senha);

        hash.Should().Be(expected);
    }

    [Fact]
    public void VerifyPassword_DeveRetornarTrueQuandoHashCorreto()
    {
        const string senha = "MinhaSenha!";
        var hash = _service.HashPassword(senha);

        var result = _service.VerifyPassword(senha, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_DeveRetornarFalseQuandoHashDiferente()
    {
        const string senha = "Senha1";
        var hash = _service.HashPassword("Senha2");

        var result = _service.VerifyPassword(senha, hash);

        result.Should().BeFalse();
    }
}
