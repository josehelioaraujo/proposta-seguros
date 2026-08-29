using FluentValidation;

namespace ContratacaoService.Application.UseCases.ContratarProposta;

public class ContratarPropostaValidator : AbstractValidator<ContratarPropostaRequest>
{
    public ContratarPropostaValidator()
    {
        RuleFor(x => x.PropostaId)
            .NotEmpty().WithMessage("Id da proposta e obrigatorio.");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF e obrigatorio.")
            .Must(CpfValido).WithMessage("CPF invalido.");
    }

    private static bool CpfValido(string cpf)
    {
        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        return digits.Length == 11 && digits.Distinct().Count() > 1;
    }
}
