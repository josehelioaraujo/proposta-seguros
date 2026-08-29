using FluentAssertions;
using Moq;
using PropostaService.Application.UseCases.CriarProposta;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports;
using PropostaService.Domain.Shared;
using PropostaService.Tests.Mocks;

namespace PropostaService.Tests.UseCases;

public class CriarPropostaUseCaseTests
{
    private readonly Mock<IPropostaRepository> _repositoryMock;
    private readonly Mock<IRegraSeguro>        _regraMock;
    private readonly CriarPropostaUseCase      _useCase;
    private readonly CriarPropostaRequestFaker _faker;

    public CriarPropostaUseCaseTests()
    {
        _repositoryMock = new Mock<IPropostaRepository>();
        _regraMock      = new Mock<IRegraSeguro>();
        _faker          = new CriarPropostaRequestFaker();

        // Regra padrao — aceita qualquer valor
        _regraMock.Setup(r => r.Tipo).Returns(TipoSeguro.SeguroVidaFamiliar);
        _regraMock.Setup(r => r.ValorMinimo).Returns(30m);
        _regraMock.Setup(r => r.ValidarValorMinimo(It.IsAny<decimal>())).Returns(true);

        _useCase = new CriarPropostaUseCase(
            _repositoryMock.Object,
            new[] { _regraMock.Object });
    }

    [Fact]
    public async Task ExecuteAsync_DeveCriarProposta_QuandoDadosValidos()
    {
        // Arrange — record nao suporta RuleFor, instanciamos diretamente
        var request = new CriarPropostaRequest(
            NomeCliente: "Joao Silva",
            Cpf:         "529.982.247-25",
            TipoSeguro:  TipoSeguro.SeguroVidaFamiliar,
            Valor:       500m);

        _repositoryMock
            .Setup(r => r.BuscarPorCpfETipoAsync(It.IsAny<string>(), It.IsAny<TipoSeguro>()))
            .ReturnsAsync((Proposta?)null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Proposta>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Created);
        result.Data.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Proposta>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarConflict_QuandoPropostaJaExiste()
    {
        // Arrange
        var request   = _faker.Generate();
        var existente = new PropostaFaker().Generate();

        _repositoryMock
            .Setup(r => r.BuscarPorCpfETipoAsync(It.IsAny<string>(), It.IsAny<TipoSeguro>()))
            .ReturnsAsync(existente);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Proposta>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarUnprocessable_QuandoValorAbaixoDoMinimo()
    {
        // Arrange
        var request = new CriarPropostaRequest(
            NomeCliente: "Maria Souza",
            Cpf:         "529.982.247-25",
            TipoSeguro:  TipoSeguro.SeguroVidaFamiliar,
            Valor:       10m); // abaixo do minimo de 30m

        _repositoryMock
            .Setup(r => r.BuscarPorCpfETipoAsync(It.IsAny<string>(), It.IsAny<TipoSeguro>()))
            .ReturnsAsync((Proposta?)null);

        _regraMock
            .Setup(r => r.ValidarValorMinimo(10m))
            .Returns(false);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.UnprocessableEntity);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Proposta>()), Times.Never);
    }
}
