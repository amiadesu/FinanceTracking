# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["backend/FinanceTracking.API/FinanceTracking.API.csproj", "backend/FinanceTracking.API/"]
COPY ["backend/FinanceTracking.Contracts/FinanceTracking.Contracts.csproj", "backend/FinanceTracking.Contracts/"]
RUN dotnet restore "backend/FinanceTracking.API/FinanceTracking.API.csproj"
COPY . .
WORKDIR "/src/backend/FinanceTracking.API"
RUN dotnet build "FinanceTracking.API.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "FinanceTracking.API.csproj" -c Release -o /app/publish

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceTracking.API.dll"]