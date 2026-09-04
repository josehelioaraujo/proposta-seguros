using Microsoft.Extensions.Logging;
using PropostaService.Application.Metrics;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports.Output;
using PropostaService.Domain.Shared;

namespace PropostaService.Application.UseCases.CriarProposta;

public class CriarPropostaUseCase
{
    private readonly IPropostaRepository              _repository;
    private readonly IEnumerable<IRegraSeguro>        _regras;
    private readonly ILogger<CriarPropostaUseCase>    _logger;

    public CriarPropostaUseCase(
        IPropostaRepository           repository,
        IEnumerable<IRegraSeguro>     regras,
        ILogger<CriarPropostaUseCase> logger)
    {
        _repository = repository;
        _regras     = regras;
        _logger     = logger;
    }

    public async Task<Result<PropostaResponse>> ExecuteAsync(CriarPropostaRequest request)
    {
        _logger.LogInformation(
            "Criando proposta — CPF: {Cpf} | Tipo: {Tipo} | Valor: {Valor:C}",
            request.Cpf, request.TipoSeguro, request.Valor);

        var existente = await _repository.BuscarPorCpfETipoAsync(request.Cpf, request.TipoSeguro);
        if (existente is not null)
        {
            _logger.LogWarning(
                "Proposta duplicada — CPF: {Cpf} | Tipo: {Tipo} | ID existente: {Id}",
                request.Cpf, request.TipoSeguro, existente.Id);

            return Result<PropostaResponse>.Conflict(
                "Ja existe uma proposta em analise para este CPF e tipo de seguro.");
        }

        var regra = _regras.FirstOrDefault(r => r.Tipo == request.TipoSeguro);
        if (regra is null)
        {
            _logger.LogError("Tipo de seguro nao suportado — Tipo: {Tipo}", request.TipoSeguro);
            return Result<PropostaResponse>.Fail("Tipo de seguro nao suportado.");
        }

        if (!regra.ValidarValorMinimo(request.Valor))
        {
            _logger.LogWarning(
                "Valor abaixo do minimo — Tipo: {Tipo} | Valor: {Valor:C} | Minimo: {Minimo:C}",
                request.TipoSeguro, request.Valor, regra.ValorMinimo);

            return Result<PropostaResponse>.Unprocessable(
                $"Valor minimo para {request.TipoSeguro} e {regra.ValorMinimo:C}.");
        }

        var proposta = Proposta.Criar(
            request.NomeCliente,
            request.Cpf,
            request.TipoSeguro,
            request.Valor);

        await _repository.AddAsync(proposta);

        PropostaMetrics.PropostasCriadas.Inc();

        _logger.LogInformation(
            "Proposta criada com sucesso — ID: {Id} | CPF: {Cpf} | Tipo: {Tipo}",
            proposta.Id, proposta.Cpf, proposta.TipoSeguro);

        return Result<PropostaResponse>.Created(PropostaResponse.FromEntity(proposta));
    }
}
