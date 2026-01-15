FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ENV NUGET_XMLDOC_MODE=skip
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

RUN dotnet nuget locals all --clear

# 1) Копируем csproj (кэш restore)
COPY source/PortfolioCalculator.Domain/PortfolioCalculator.Domain.csproj source/PortfolioCalculator.Domain/
COPY source/PortfolioCalculator.Application/PortfolioCalculator.Application.csproj source/PortfolioCalculator.Application/
COPY source/PortfolioCalculator.Infrastructure/PortfolioCalculator.Infrastructure.csproj source/PortfolioCalculator.Infrastructure/
COPY source/PortfolioCalculator.ConsoleApp/PortfolioCalculator.ConsoleApp.csproj source/PortfolioCalculator.ConsoleApp/

# 2) restore (создаст obj с project.assets.json внутри контейнера)
RUN dotnet restore source/PortfolioCalculator.ConsoleApp/PortfolioCalculator.ConsoleApp.csproj --force --no-cache

# 3) Копируем остальной код
COPY . .

# ❌ НЕ удаляем obj/bin после restore!
# Если хочешь чистить, то только ДО restore, но это уже не нужно при .dockerignore

# 4) publish
RUN dotnet publish source/PortfolioCalculator.ConsoleApp/PortfolioCalculator.ConsoleApp.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "PortfolioCalculator.ConsoleApp.dll"]