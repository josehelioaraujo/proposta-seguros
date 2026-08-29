using FluentAssertions;
using Moq;
using PropostaService.Application.UseCases.AlterarStatus;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports;
using PropostaService.Domain.Shared;
using PropostaService.Tests.Mocks;

namespace PropostaService.Tests.UseCases;

public class AlterarStatusUseCaseTests
{
    private readonly Mock<IPropostaRepository> _repositoryMock;
    private readonly AlterarStatusUseCase      _useCase;
    private readonly PropostaFaker             _faker;

    public AlterarStatusUseCaseTests()
    {
        _repositoryMock = new Mock<IPropostaRepository>();
        _faker          = new PropostaFaker();
        _useCase        = new AlterarStatusUseCase(_repositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveAlterarStatus_QuandoPropostaEmAnalise()
    {
        // Arrange
        var proposta = _faker.Generate(); // EmAnalise por padrao
        var request  = new AlterarStatusRequest(proposta.Id, PropostaStatus.Aprovada);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(proposta.Id))
            .ReturnsAsync(proposta);

        _repositoryMock
            .Setup(r => r.UpdateAsync(proposta))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("Aprovada");
        _repositoryMock.Verify(r => r.UpdateAsync(proposta), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarNotFound_QuandoPropostaNaoExiste()
    {
        // Arrange
        var request = new AlterarStatusRequest(Guid.NewGuid(), PropostaStatus.Aprovada);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Entities.Proposta?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Theory]
    [InlineData(PropostaStatus.Aprovada)]
    [InlineData(PropostaStatus.Rejeitada)]
    public async Task ExecuteAsync_DeveRetornarUnprocessable_QuandoStatusFinal(PropostaStatus statusFinal)
    {
        // Arrange
        var proposta = _faker.ComStatus(statusFinal);
        var request  = new AlterarStatusRequest(proposta.Id, PropostaStatus.EmAnalise);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(proposta.Id))
            .ReturnsAsync(proposta);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.UnprocessableEntity);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.Proposta>()), Times.Never);
    }
}
