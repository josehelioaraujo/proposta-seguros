using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports.Output;

namespace PropostaService.Infrastructure.Strategies;

public class SeguroFGTSProtegidoRegra : IRegraSeguro
{
    public TipoSeguro Tipo        => TipoSeguro.SeguroFGTSProtegido;
    public decimal    ValorMinimo => 50.00m;
    public bool ValidarValorMinimo(decimal valor) => valor >= ValorMinimo;
}
