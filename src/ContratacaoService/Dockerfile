# Stage 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ContratacaoService/ContratacaoService.Domain/ContratacaoService.Domain.csproj             src/ContratacaoService/ContratacaoService.Domain/
COPY src/ContratacaoService/ContratacaoService.Application/ContratacaoService.Application.csproj   src/ContratacaoService/ContratacaoService.Application/
COPY src/ContratacaoService/ContratacaoService.Infrastructure/ContratacaoService.Infrastructure.csproj src/ContratacaoService/ContratacaoService.Infrastructure/
COPY src/ContratacaoService/ContratacaoService.Api/ContratacaoService.Api.csproj                   src/ContratacaoService/ContratacaoService.Api/

RUN dotnet restore src/ContratacaoService/ContratacaoService.Api/ContratacaoService.Api.csproj

COPY src/ContratacaoService/ src/ContratacaoService/

RUN dotnet publish src/ContratacaoService/ContratacaoService.Api/ContratacaoService.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Versioning
ARG BUILD_VERSION=local
ARG BUILD_COMMIT=local
ARG BUILD_DATE=local
ENV APP_VERSION=$BUILD_VERSION
ENV APP_COMMIT=$BUILD_COMMIT
ENV APP_BUILD_DATE=$BUILD_DATE

EXPOSE 5002
ENV ASPNETCORE_URLS=http://+:5002
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "ContratacaoService.Api.dll"]
