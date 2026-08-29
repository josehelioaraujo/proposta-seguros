namespace ContratacaoService.Domain.Shared;

public enum ResultStatus
{
    Ok                  = 200,
    Created             = 201,
    BadRequest          = 400,
    NotFound            = 404,
    Conflict            = 409,
    UnprocessableEntity = 422
}
