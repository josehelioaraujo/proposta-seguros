using FluentAssertions;
using Moq;
using ContratacaoService.Application.UseCases.ContratarProposta;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports.Output;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ContratacaoService.Domain.Shared;
using ContratacaoService.Tests.Mocks;

namespace ContratacaoService.Tests.UseCases;

public class ContratarPropostaUseCaseTests
{
    private readonly Mock<IContratacaoRepository>         _repositoryMock;
    private readonly Mock<IOutboxRepository>              _outboxMock;
    private readonly Mock<ILogger<ContratarPropostaUseCase>> _loggerMock;
    private readonly Mock<IPropostaServiceClient>         _clientMock;
    private readonly IConfiguration _config;
    private readonly ContratarPropostaUseCase             _useCase;
    private readonly ContratarPropostaRequestFaker        _faker;

    public ContratarPropostaUseCaseTests()
    {
        _repositoryMock = new Mock<IContratacaoRepository>();
        _outboxMock     = new Mock<IOutboxRepository>();
        _loggerMock     = new Mock<ILogger<ContratarPropostaUseCase>>();
        _clientMock     = new Mock<IPropostaServiceClient>();
        _config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Features:UsarBancoDados"] = "false", ["Features:UsarKafka"] = "false", ["Features:UsarRabbitMQ"] = "false" }).Build();
        _faker          = new ContratarPropostaRequestFaker();

        
        _useCase = new ContratarPropostaUseCase(
            _repositoryMock.Object,
            _outboxMock.Object,
            _clientMock.Object,
            _config,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveContratarProposta_QuandoPropostaAprovada()
    {
        var request  = _faker.Generate();
        var proposta = new PropostaDto(request.PropostaId, "Aprovada");

        _repositoryMock.Setup(r => r.GetByPropostaIdAsync(request.PropostaId)).ReturnsAsync((Contratacao?)null);
        _clientMock.Setup(c => c.ObterPropostaAsync(request.PropostaId)).ReturnsAsync(proposta);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Contratacao>())).Returns(Task.CompletedTask);

        var result = await _useCase.ExecuteAsync(request);

        result.Success.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Created);
        result.Data.Should().NotBeNull();
        result.Data!.PropostaId.Should().Be(request.PropostaId);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarConflict_QuandoPropostaJaContratada()
    {
        var request     = _faker.Generate();
        var contratacao = new ContratacaoFaker().Generate();

        _repositoryMock.Setup(r => r.GetByPropostaIdAsync(request.PropostaId)).ReturnsAsync(contratacao);

        var result = await _useCase.ExecuteAsync(request);

        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Contratacao>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarNotFound_QuandoPropostaNaoEncontrada()
    {
        var request = _faker.Generate();

        _repositoryMock.Setup(r => r.GetByPropostaIdAsync(request.PropostaId)).ReturnsAsync((Contratacao?)null);
        _clientMock.Setup(c => c.ObterPropostaAsync(request.PropostaId)).ReturnsAsync((PropostaDto?)null);

        var result = await _useCase.ExecuteAsync(request);

        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Contratacao>()), Times.Never);
    }

    [Theory]
    [InlineData("EmAnalise")]
    [InlineData("Rejeitada")]
    public async Task ExecuteAsync_DeveRetornarUnprocessable_QuandoPropostaNaoAprovada(string status)
    {
        var request  = _faker.Generate();
        var proposta = new PropostaDto(request.PropostaId, status);

        _repositoryMock.Setup(r => r.GetByPropostaIdAsync(request.PropostaId)).ReturnsAsync((Contratacao?)null);
        _clientMock.Setup(c => c.ObterPropostaAsync(request.PropostaId)).ReturnsAsync(proposta);

        var result = await _useCase.ExecuteAsync(request);

        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.UnprocessableEntity);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Contratacao>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarUnprocessable_QuandoPropostaEmAnalise()
    {
        var request  = _faker.Generate();
        var proposta = new PropostaDto(request.PropostaId, "EmAnalise");

        _repositoryMock.Setup(r => r.GetByPropostaIdAsync(request.PropostaId)).ReturnsAsync((Contratacao?)null);
        _clientMock.Setup(c => c.ObterPropostaAsync(request.PropostaId)).ReturnsAsync(proposta);

        var result = await _useCase.ExecuteAsync(request);

        result.Success.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.UnprocessableEntity);
    }
}


