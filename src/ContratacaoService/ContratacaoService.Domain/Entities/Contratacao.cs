namespace ContratacaoService.Domain.Entities;

public class Contratacao
{
    public Guid     Id               { get; private set; }
    public Guid     PropostaId       { get; private set; }
    public string   Cpf              { get; private set; } = string.Empty;
    public DateTime DataContratacao  { get; private set; }
    public DateTime CriadoEm        { get; private set; }

    private Contratacao() { }

    public static Contratacao Criar(Guid propostaId, string cpf) => new()
    {
        Id              = Guid.NewGuid(),
        PropostaId      = propostaId,
        Cpf             = cpf,
        DataContratacao = DateTime.UtcNow,
        CriadoEm       = DateTime.UtcNow
    };

    public static Contratacao Reconstituir(
        Guid     id,
        Guid     propostaId,
        string   cpf,
        DateTime dataContratacao,
        DateTime criadoEm) => new()
    {
        Id              = id,
        PropostaId      = propostaId,
        Cpf             = cpf,
        DataContratacao = dataContratacao,
        CriadoEm       = criadoEm
    };
}
