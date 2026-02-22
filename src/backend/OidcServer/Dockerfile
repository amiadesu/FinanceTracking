# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["backend/OidcServer/OidcServer.csproj", "backend/OidcServer/"]
RUN dotnet restore "backend/OidcServer/OidcServer.csproj"
COPY . .
WORKDIR "/src/backend/OidcServer"
RUN dotnet build "OidcServer.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "OidcServer.csproj" -c Release -o /app/publish

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OidcServer.dll"]