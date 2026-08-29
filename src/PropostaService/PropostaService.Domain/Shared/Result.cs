namespace PropostaService.Domain.Shared;

public record Result<T>
{
    public bool         Success { get; init; }
    public T?           Data    { get; init; }
    public string?      Error   { get; init; }
    public ResultStatus Status  { get; init; }

    public static Result<T> Ok(T data) => new()
    {
        Success = true,
        Data    = data,
        Status  = ResultStatus.Ok
    };

    public static Result<T> Created(T data) => new()
    {
        Success = true,
        Data    = data,
        Status  = ResultStatus.Created
    };

    public static Result<T> Fail(string error) => new()
    {
        Success = false,
        Error   = error,
        Status  = ResultStatus.BadRequest
    };

    public static Result<T> NotFound(string error) => new()
    {
        Success = false,
        Error   = error,
        Status  = ResultStatus.NotFound
    };

    public static Result<T> Conflict(string error) => new()
    {
        Success = false,
        Error   = error,
        Status  = ResultStatus.Conflict
    };

    public static Result<T> Unprocessable(string error) => new()
    {
        Success = false,
        Error   = error,
        Status  = ResultStatus.UnprocessableEntity
    };
}
