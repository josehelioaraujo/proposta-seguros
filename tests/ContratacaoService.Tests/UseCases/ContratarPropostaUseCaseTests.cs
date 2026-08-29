using FluentAssertions;
using Moq;
using ContratacaoService.Application.UseCases.ContratarProposta;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports.Output;
using Microsoft.Extensions.Logging;
using ContratacaoService.Domain.Shared;
using ContratacaoService.Tests.Mocks;

namespace ContratacaoService.Tests.UseCases;

public class ContratarPropostaUseCaseTests
{
    private readonly Mock<IContratacaoRepository> _repositoryMock;
    private readonly Mock<ILogger<ContratarPropostaUseCase>> _loggerMock;
    private readonly Mock<IPropostaServiceClient> _clientMock;
    private readonly ContratarPropostaUseCase     _useCase;
    private readonly ContratarPropostaRequestFaker _faker;

    public ContratarPropostaUseCaseTests()
    {
        _repositoryMock = new Mock<IContratacaoRepository>();
        _loggerMock     = new Mock<ILogger<ContratarPropostaUseCase>>();
        _clientMock     = new Mock<IPropostaServiceClient>();
        _faker          = new ContratarPropostaRequestFaker();

        _useCase = new ContratarPropostaUseCase(
            _repositoryMock.Object,
            _clientMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveContratarProposta_QuandoPropostaAprovada()
    {
        // Arrange
        var request  = _faker.Generate();
        var proposta = new PropostaDto(request.PropostaId, "Aprovada");

        _repositoryMock
            .Setup(r => r.GetByPropostaIdAsync(request.PropostaId))
            .ReturnsAsync((Contratacao?)null);

        _clientMock
            .Setup(c => c.ObterPropostaAsync(request.PropostaId))
            .ReturnsAsync(proposta);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Contratacao>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Created);
        result.Data.Should().NotBeNull();
        result.Data!.PropostaId.Should().Be(request.PropostaId);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Contratacao>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarConflict_QuandoPropostaJaContratada()
    {
        // Arrange
        var request      = _faker.Generate();
        var contratacao  = new ContratacaoFaker().Generate();

        _repositoryMock
            .Setup(r => r.GetByPropostaIdAsync(request.PropostaId))
            .ReturnsAsync(contratacao);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Contratacao>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarNotFound_QuandoPropostaNaoEncontrada()
    {
        // Arrange
        var request = _faker.Generate();

        _repositoryMock
            .Setup(r => r.GetByPropostaIdAsync(request.PropostaId))
            .ReturnsAsync((Contratacao?)null);

        _clientMock
            .Setup(c => c.ObterPropostaAsync(request.PropostaId))
            .ReturnsAsync((PropostaDto?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Contratacao>()), Times.Never);
    }

    [Theory]
    [InlineData("EmAnalise")]
    [InlineData("Rejeitada")]
    public async Task ExecuteAsync_DeveRetornarUnprocessable_QuandoPropostaNaoAprovada(string status)
    {
        // Arrange
        var request  = _faker.Generate();
        var proposta = new PropostaDto(request.PropostaId, status);

        _repositoryMock
            .Setup(r => r.GetByPropostaIdAsync(request.PropostaId))
            .ReturnsAsync((Contratacao?)null);

        _clientMock
            .Setup(c => c.ObterPropostaAsync(request.PropostaId))
            .ReturnsAsync(proposta);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.UnprocessableEntity);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Contratacao>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarUnprocessable_QuandoPropostaEmAnalise()
    {
        // Arrange
        var request  = _faker.Generate();
        var proposta = new PropostaDto(request.PropostaId, "EmAnalise");

        _repositoryMock
            .Setup(r => r.GetByPropostaIdAsync(request.PropostaId))
            .ReturnsAsync((Contratacao?)null);

        _clientMock
            .Setup(c => c.ObterPropostaAsync(request.PropostaId))
            .ReturnsAsync(proposta);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.UnprocessableEntity);
    }
}

