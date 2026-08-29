using PropostaService.Domain.Enums;
using PropostaService.Domain.ValueObjects;

namespace PropostaService.Domain.Entities;

public class Proposta
{
    public Guid           Id          { get; private set; }
    public string         NomeCliente { get; private set; } = string.Empty;
    public string         Cpf         { get; private set; } = string.Empty;
    public TipoSeguro     TipoSeguro  { get; private set; }
    public decimal        Valor       { get; private set; }
    public PropostaStatus Status      { get; private set; }
    public DateTime       CriadoEm   { get; private set; }
    public DateTime?      AtualizadoEm { get; private set; }

    // Construtor privado — EF/Dapper
    private Proposta() { }

    // Factory method — unica forma de criar uma proposta valida
    public static Proposta Criar(
        string    nomeCliente,
        string    cpf,
        TipoSeguro tipoSeguro,
        decimal   valor)
    {
        return new Proposta
        {
            Id          = Guid.NewGuid(),
            NomeCliente = nomeCliente,
            Cpf         = cpf,
            TipoSeguro  = tipoSeguro,
            Valor       = valor,
            Status      = PropostaStatus.EmAnalise,
            CriadoEm   = DateTime.UtcNow
        };
    }

    // Comportamento encapsulado — status so muda aqui
    public void AlterarStatus(PropostaStatus novoStatus)
    {
        if (Status == PropostaStatus.Aprovada || Status == PropostaStatus.Rejeitada)
            throw new InvalidOperationException(
                $"Proposta com status {Status} nao pode ser alterada.");

        Status       = novoStatus;
        AtualizadoEm = DateTime.UtcNow;
    }

    public bool StatusFinal =>
        Status == PropostaStatus.Aprovada ||
        Status == PropostaStatus.Rejeitada;
}
