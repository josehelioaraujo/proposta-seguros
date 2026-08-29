using FluentValidation;
using FluentValidation.AspNetCore;
using ContratacaoService.Application.Extensions;
using ContratacaoService.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Camadas — Hexagonal
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "Contratacao de Seguros API",
        Version     = "v1",
        Description = "API para contratacao de propostas de seguro — Arquitetura Hexagonal"
    });
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contratacao de Seguros API v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
