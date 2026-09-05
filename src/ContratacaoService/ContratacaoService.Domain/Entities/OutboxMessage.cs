using System.Text.Json;

namespace ContratacaoService.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Tipo { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; } = DateTime.UtcNow;
    public bool Processado { get; private set; }
    public DateTime? ProcessadoEm { get; private set; }

    public static OutboxMessage Criar<T>(T evento)
    {
        return new OutboxMessage
        {
            Tipo    = typeof(T).Name,
            Payload = JsonSerializer.Serialize(evento)
        };
    }

    public void MarcarProcessado()
    {
        Processado    = true;
        ProcessadoEm  = DateTime.UtcNow;
    }
}
