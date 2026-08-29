using PropostaService.Domain.Enums;

namespace PropostaService.Domain.Ports.Output;

public interface IRegraSeguro
{
    TipoSeguro Tipo         { get; }
    decimal    ValorMinimo  { get; }
    bool       ValidarValorMinimo(decimal valor);
}
