# Pinned to the SDK global.json requires: the floating 10.0 tag lags it and restore then fails.
FROM mcr.microsoft.com/dotnet/sdk:10.0.400 AS build
WORKDIR /src

# Restore against the project files alone, so a source-only change keeps the cached package layer.
COPY global.json ./
COPY backend/Directory.Build.props backend/Directory.Packages.props backend/
COPY backend/BikeRepairShop.Domain/BikeRepairShop.Domain.csproj backend/BikeRepairShop.Domain/
COPY backend/BikeRepairShop.Database/BikeRepairShop.Database.csproj backend/BikeRepairShop.Database/
COPY backend/BikeRepairShop.Api/BikeRepairShop.Api.csproj backend/BikeRepairShop.Api/
RUN dotnet restore backend/BikeRepairShop.Api/BikeRepairShop.Api.csproj

COPY backend/BikeRepairShop.Domain/ backend/BikeRepairShop.Domain/
COPY backend/BikeRepairShop.Database/ backend/BikeRepairShop.Database/
COPY backend/BikeRepairShop.Api/ backend/BikeRepairShop.Api/
RUN dotnet publish backend/BikeRepairShop.Api/BikeRepairShop.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
# The Debian tags carry tz data, which TimeZoneInfo needs for the shop's configured zone.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app ./
USER $APP_UID
ENTRYPOINT ["dotnet", "BikeRepairShop.Api.dll"]
