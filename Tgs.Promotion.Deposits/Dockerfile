FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.Promotion.Deposits/Tgs.Promotion.Deposits.csproj", "Tgs.Promotion.Deposits/"]
COPY ["DepositPromotion.Core/DepositPromotion.Core.csproj", "DepositPromotion.Core/"]
COPY ["DepositPromotion.Repositories/DepositPromotion.Repositories.csproj", "DepositPromotion.Repositories/"]
RUN dotnet restore "./Tgs.Promotion.Deposits/Tgs.Promotion.Deposits.csproj"
COPY . .
WORKDIR "/src/Tgs.Promotion.Deposits"
RUN dotnet build "./Tgs.Promotion.Deposits.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.Promotion.Deposits.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM hub.lumigame.com/base/dotnet/core/aspnet:8.0-alpine-24h1.1 AS base
USER app
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Promotion.Deposits.dll"]