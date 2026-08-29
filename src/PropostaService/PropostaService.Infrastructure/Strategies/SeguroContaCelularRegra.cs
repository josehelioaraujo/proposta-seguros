using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports;

namespace PropostaService.Infrastructure.Strategies;

public class SeguroContaCelularRegra : IRegraSeguro
{
    public TipoSeguro Tipo        => TipoSeguro.SeguroContaCelularProtegidos;
    public decimal    ValorMinimo => 10.00m;
    public bool ValidarValorMinimo(decimal valor) => valor >= ValorMinimo;
}
