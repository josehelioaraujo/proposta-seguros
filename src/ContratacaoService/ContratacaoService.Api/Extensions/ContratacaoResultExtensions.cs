using Microsoft.AspNetCore.Mvc;
using ContratacaoService.Domain.Shared;

namespace ContratacaoService.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result) =>
        result.Status switch
        {
            ResultStatus.Created             => new ObjectResult(result.Data) { StatusCode = 201 },
            ResultStatus.Ok                  => new OkObjectResult(result.Data),
            ResultStatus.NotFound            => new NotFoundObjectResult(new { error = result.Error }),
            ResultStatus.Conflict            => new ConflictObjectResult(new { error = result.Error }),
            ResultStatus.UnprocessableEntity => new UnprocessableEntityObjectResult(new { error = result.Error }),
            _                                => new BadRequestObjectResult(new { error = result.Error })
        };
}
