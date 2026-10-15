# Base runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PortfolioBackend/PortfolioBackend.csproj", "PortfolioBackend/"]
RUN dotnet restore "./PortfolioBackend/PortfolioBackend.csproj"
COPY . .
WORKDIR "/src/PortfolioBackend"
RUN dotnet build "./PortfolioBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PortfolioBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final production stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Explicitly tell ASP.NET Core to bind HTTP to 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "PortfolioBackend.dll"]