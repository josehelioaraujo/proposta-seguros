using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports.Output;

namespace PropostaService.Infrastructure.Strategies;

public class SeguroCartaoProtegidoRegra : IRegraSeguro
{
    public TipoSeguro Tipo        => TipoSeguro.SeguroCartaoProtegido;
    public decimal    ValorMinimo => 15.00m;
    public bool ValidarValorMinimo(decimal valor) => valor >= ValorMinimo;
}
