# Stage 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/PropostaService/PropostaService.Domain/PropostaService.Domain.csproj             src/PropostaService/PropostaService.Domain/
COPY src/PropostaService/PropostaService.Application/PropostaService.Application.csproj   src/PropostaService/PropostaService.Application/
COPY src/PropostaService/PropostaService.Infrastructure/PropostaService.Infrastructure.csproj src/PropostaService/PropostaService.Infrastructure/
COPY src/PropostaService/PropostaService.Api/PropostaService.Api.csproj                   src/PropostaService/PropostaService.Api/

RUN dotnet restore src/PropostaService/PropostaService.Api/PropostaService.Api.csproj

COPY src/PropostaService/ src/PropostaService/

RUN dotnet publish src/PropostaService/PropostaService.Api/PropostaService.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "PropostaService.Api.dll"]
