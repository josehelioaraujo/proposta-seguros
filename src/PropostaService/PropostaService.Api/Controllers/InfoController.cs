using Microsoft.AspNetCore.Mvc;

namespace PropostaService.Api.Controllers;

[ApiController]
[Route("info")]
public class InfoController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public InfoController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            service     = "PropostaService",
            version     = Environment.GetEnvironmentVariable("APP_VERSION")    ?? "local",
            commit      = Environment.GetEnvironmentVariable("APP_COMMIT")     ?? "local",
            builtAt     = Environment.GetEnvironmentVariable("APP_BUILD_DATE") ?? "local",
            serverTime  = DateTime.UtcNow.ToString("o"),
            serverName  = Environment.MachineName,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
        });
    }
}
