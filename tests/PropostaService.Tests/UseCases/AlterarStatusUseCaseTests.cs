using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PropostaService.Application.UseCases.AlterarStatus;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports.Output;
using PropostaService.Domain.Shared;
using PropostaService.Tests.Mocks;

namespace PropostaService.Tests.UseCases;

public class AlterarStatusUseCaseTests
{
    private readonly Mock<IPropostaRepository>             _repositoryMock;
    private readonly Mock<ILogger<AlterarStatusUseCase>>   _loggerMock;
    private readonly AlterarStatusUseCase                  _useCase;
    private readonly PropostaFaker                         _faker;

    public AlterarStatusUseCaseTests()
    {
        _repositoryMock = new Mock<IPropostaRepository>();
        _loggerMock     = new Mock<ILogger<AlterarStatusUseCase>>();
        _faker          = new PropostaFaker();
        _useCase        = new AlterarStatusUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveAlterarStatus_QuandoPropostaEmAnalise()
    {
        var proposta = _faker.Generate();
        var request  = new AlterarStatusRequest(proposta.Id, PropostaStatus.Aprovada);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(proposta.Id))
            .ReturnsAsync(proposta);

        _repositoryMock
            .Setup(r => r.UpdateAsync(proposta))
            .Returns(Task.CompletedTask);

        var result = await _useCase.ExecuteAsync(request);

        result.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("Aprovada");
        _repositoryMock.Verify(r => r.UpdateAsync(proposta), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarNotFound_QuandoPropostaNaoExiste()
    {
        var request = new AlterarStatusRequest(Guid.NewGuid(), PropostaStatus.Aprovada);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Entities.Proposta?)null);

        var result = await _useCase.ExecuteAsync(request);

        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Theory]
    [InlineData(PropostaStatus.Aprovada)]
    [InlineData(PropostaStatus.Rejeitada)]
    public async Task ExecuteAsync_DeveRetornarUnprocessable_QuandoStatusFinal(PropostaStatus statusFinal)
    {
        var proposta = _faker.ComStatus(statusFinal);
        var request  = new AlterarStatusRequest(proposta.Id, PropostaStatus.EmAnalise);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(proposta.Id))
            .ReturnsAsync(proposta);

        var result = await _useCase.ExecuteAsync(request);

        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.UnprocessableEntity);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Entities.Proposta>()), Times.Never);
    }
}
