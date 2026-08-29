using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports;
using PropostaService.Domain.Shared;

namespace PropostaService.Application.UseCases.CriarProposta;

public class CriarPropostaUseCase
{
    private readonly IPropostaRepository _repository;
    private readonly IEnumerable<IRegraSeguro> _regras;

    public CriarPropostaUseCase(
        IPropostaRepository repository,
        IEnumerable<IRegraSeguro> regras)
    {
        _repository = repository;
        _regras     = regras;
    }

    public async Task<Result<PropostaResponse>> ExecuteAsync(CriarPropostaRequest request)
    {
        // Idempotencia — mesmo CPF + mesmo tipo nao pode ter duas propostas EmAnalise
        var existente = await _repository.BuscarPorCpfETipoAsync(request.Cpf, request.TipoSeguro);
        if (existente is not null)
            return Result<PropostaResponse>.Conflict(
                "Ja existe uma proposta em analise para este CPF e tipo de seguro.");

        // Regra de negocio — valor minimo por tipo (Strategy Pattern)
        var regra = _regras.FirstOrDefault(r => r.Tipo == request.TipoSeguro);
        if (regra is null)
            return Result<PropostaResponse>.Fail("Tipo de seguro nao suportado.");

        if (!regra.ValidarValorMinimo(request.Valor))
            return Result<PropostaResponse>.Unprocessable(
                $"Valor minimo para {request.TipoSeguro} e {regra.ValorMinimo:C}.");

        // Factory method — unica forma de criar proposta valida
        var proposta = Proposta.Criar(
            request.NomeCliente,
            request.Cpf,
            request.TipoSeguro,
            request.Valor);

        await _repository.AddAsync(proposta);

        return Result<PropostaResponse>.Created(PropostaResponse.FromEntity(proposta));
    }
}
