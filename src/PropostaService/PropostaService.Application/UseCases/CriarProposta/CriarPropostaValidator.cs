using FluentValidation;

namespace PropostaService.Application.UseCases.CriarProposta;

public class CriarPropostaValidator : AbstractValidator<CriarPropostaRequest>
{
    public CriarPropostaValidator()
    {
        RuleFor(x => x.NomeCliente)
            .NotEmpty().WithMessage("Nome do cliente e obrigatorio.")
            .MaximumLength(200).WithMessage("Nome pode ter no maximo 200 caracteres.");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF e obrigatorio.")
            .Must(CpfValido).WithMessage("CPF invalido.");

        RuleFor(x => x.TipoSeguro)
            .IsInEnum().WithMessage("Tipo de seguro invalido.");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero.");
    }

    private static bool CpfValido(string cpf)
    {
        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        return digits.Length == 11 && digits.Distinct().Count() > 1;
    }
}
