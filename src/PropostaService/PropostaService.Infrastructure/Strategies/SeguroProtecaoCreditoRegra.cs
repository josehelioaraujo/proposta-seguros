using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports.Output;

namespace PropostaService.Infrastructure.Strategies;

public class SeguroProtecaoCreditoRegra : IRegraSeguro
{
    public TipoSeguro Tipo        => TipoSeguro.ProtecaoCreditoTrabalhador;
    public decimal    ValorMinimo => 25.00m;
    public bool ValidarValorMinimo(decimal valor) => valor >= ValorMinimo;
}
