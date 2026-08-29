using FluentValidation;
using FluentValidation.AspNetCore;
using PropostaService.Application.Extensions;
using PropostaService.Infrastructure.Extensions;

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
        Title       = "Proposta de Seguros API",
        Version     = "v1",
        Description = "API para gerenciamento de propostas de seguro — Arquitetura Hexagonal"
    });
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Proposta de Seguros API v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
