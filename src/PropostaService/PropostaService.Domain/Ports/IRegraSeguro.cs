using PropostaService.Domain.Enums;

namespace PropostaService.Domain.Ports;

public interface IRegraSeguro
{
    TipoSeguro Tipo         { get; }
    decimal    ValorMinimo  { get; }
    bool       ValidarValorMinimo(decimal valor);
}
