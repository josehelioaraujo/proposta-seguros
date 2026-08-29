using FluentValidation;

namespace PropostaService.Application.UseCases.AlterarStatus;

public class AlterarStatusValidator : AbstractValidator<AlterarStatusRequest>
{
    public AlterarStatusValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id da proposta e obrigatorio.");

        RuleFor(x => x.NovoStatus)
            .IsInEnum().WithMessage("Status invalido.");
    }
}
