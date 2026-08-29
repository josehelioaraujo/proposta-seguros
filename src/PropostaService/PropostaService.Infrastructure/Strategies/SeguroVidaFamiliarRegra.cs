using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports;

namespace PropostaService.Infrastructure.Strategies;

public class SeguroVidaFamiliarRegra : IRegraSeguro
{
    public TipoSeguro Tipo        => TipoSeguro.SeguroVidaFamiliar;
    public decimal    ValorMinimo => 30.00m;
    public bool ValidarValorMinimo(decimal valor) => valor >= ValorMinimo;
}
