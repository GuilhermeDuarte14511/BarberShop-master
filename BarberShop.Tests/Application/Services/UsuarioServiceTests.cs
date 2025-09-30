using BarberShop.Application.Services;
using BarberShop.Domain.Entities;
using BarberShop.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace BarberShop.Tests.Application.Services;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _service = new UsuarioService(_usuarioRepositoryMock.Object);
    }

    [Fact]
    public async Task ListarUsuariosPorBarbeariaAsync_DeveRetornarSomenteUsuariosDaBarbearia()
    {
        var usuarios = new List<Usuario>
        {
            new() { UsuarioId = 1, BarbeariaId = 10 },
            new() { UsuarioId = 2, BarbeariaId = 11 },
            new() { UsuarioId = 3, BarbeariaId = 10 }
        };
        _usuarioRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(usuarios);

        var resultado = await _service.ListarUsuariosPorBarbeariaAsync(10);

        resultado.Should().HaveCount(2)
            .And.OnlyContain(u => u.BarbeariaId == 10);
        _usuarioRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CriarUsuarioAsync_DeveDefinirDataCriacaoESalvar()
    {
        var usuario = new Usuario { Nome = "Carlos", Email = "carlos@email.com" };
        _usuarioRepositoryMock
            .Setup(r => r.AddAsync(usuario))
            .ReturnsAsync(usuario);

        var criado = await _service.CriarUsuarioAsync(usuario);

        criado.Should().BeSameAs(usuario);
        usuario.DataCriacao.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _usuarioRepositoryMock.Verify(r => r.AddAsync(usuario), Times.Once);
        _usuarioRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData(null, "email@email.com")]
    [InlineData("Nome", null)]
    [InlineData("", "")]
    public async Task CriarUsuarioAsync_DeveLancarArgumentExceptionQuandoDadosInvalidos(string? nome, string? email)
    {
        var usuario = new Usuario { Nome = nome!, Email = email! };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarUsuarioAsync(usuario));
        _usuarioRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task AtualizarUsuarioAsync_DeveLancarArgumentExceptionQuandoUsuarioInvalido()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AtualizarUsuarioAsync(null!));
    }

    [Fact]
    public async Task AtualizarUsuarioAsync_DeveLancarKeyNotFoundQuandoUsuarioNaoExiste()
    {
        var usuario = new Usuario { UsuarioId = 99, Nome = "Ana" };
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(usuario.UsuarioId))
            .ReturnsAsync((Usuario)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AtualizarUsuarioAsync(usuario));
    }

    [Fact]
    public async Task AtualizarUsuarioAsync_DeveAtualizarDadosQuandoUsuarioExiste()
    {
        var usuarioExistente = new Usuario { UsuarioId = 1, Nome = "Antigo", Email = "antigo@email.com", Status = 0 };
        var usuarioAtualizado = new Usuario
        {
            UsuarioId = 1,
            Nome = "Novo",
            Email = "novo@email.com",
            Telefone = "123",
            Role = "Admin",
            Status = 1
        };
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(usuarioAtualizado.UsuarioId))
            .ReturnsAsync(usuarioExistente);

        var retorno = await _service.AtualizarUsuarioAsync(usuarioAtualizado);

        retorno.Should().BeSameAs(usuarioExistente);
        usuarioExistente.Nome.Should().Be("Novo");
        usuarioExistente.Email.Should().Be("novo@email.com");
        usuarioExistente.Telefone.Should().Be("123");
        usuarioExistente.Role.Should().Be("Admin");
        usuarioExistente.Status.Should().Be(1);
        _usuarioRepositoryMock.Verify(r => r.UpdateAsync(usuarioExistente), Times.Once);
        _usuarioRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeletarUsuarioAsync_DeveLancarKeyNotFoundQuandoUsuarioNaoExiste()
    {
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync((Usuario)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeletarUsuarioAsync(5));
    }

    [Fact]
    public async Task DeletarUsuarioAsync_DeveRemoverUsuarioQuandoExiste()
    {
        var usuario = new Usuario { UsuarioId = 7 };
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(usuario.UsuarioId))
            .ReturnsAsync(usuario);

        var resultado = await _service.DeletarUsuarioAsync(usuario.UsuarioId);

        resultado.Should().BeTrue();
        _usuarioRepositoryMock.Verify(r => r.DeleteAsync(usuario), Times.Once);
        _usuarioRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ObterUsuarioPorIdAsync_DeveRetornarUsuarioQuandoEncontrado()
    {
        var usuario = new Usuario { UsuarioId = 3 };
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(usuario.UsuarioId))
            .ReturnsAsync(usuario);

        var resultado = await _service.ObterUsuarioPorIdAsync(usuario.UsuarioId);

        resultado.Should().Be(usuario);
    }

    [Fact]
    public async Task ObterUsuarioPorIdAsync_DeveLancarKeyNotFoundQuandoNaoEncontrado()
    {
        _usuarioRepositoryMock
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Usuario)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ObterUsuarioPorIdAsync(99));
    }

    [Fact]
    public async Task ObterUsuariosPorEmailOuTelefoneAsync_DeveDelegarParaRepositorio()
    {
        var usuarios = new List<Usuario> { new() { UsuarioId = 1 } };
        _usuarioRepositoryMock
            .Setup(r => r.ObterPorEmailOuTelefoneAsync("email@email.com", "123"))
            .ReturnsAsync(usuarios);

        var resultado = await _service.ObterUsuariosPorEmailOuTelefoneAsync("email@email.com", "123");

        resultado.Should().BeEquivalentTo(usuarios);
    }
}
