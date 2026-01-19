FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ENV NUGET_XMLDOC_MODE=skip
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

COPY source/PortfolioCalculator.Domain/PortfolioCalculator.Domain.csproj source/PortfolioCalculator.Domain/
COPY source/PortfolioCalculator.Application/PortfolioCalculator.Application.csproj source/PortfolioCalculator.Application/
COPY source/PortfolioCalculator.Infrastructure/PortfolioCalculator.Infrastructure.csproj source/PortfolioCalculator.Infrastructure/
COPY source/PortfolioCalculator.WebApi/PortfolioCalculator.WebApi.csproj source/PortfolioCalculator.WebApi/

RUN dotnet restore source/PortfolioCalculator.WebApi/PortfolioCalculator.WebApi.csproj --force --no-cache

COPY . .

RUN dotnet publish source/PortfolioCalculator.WebApi/PortfolioCalculator.WebApi.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PortfolioCalculator.WebApi.dll"]