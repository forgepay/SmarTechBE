# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base stage for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CryptoOnRamp.API/CryptoOnRamp.API.csproj", "CryptoOnRamp.API/"]
COPY ["CryptoOnRamp.BLL/CryptoOnRamp.BLL.csproj", "CryptoOnRamp.BLL/"]
COPY ["CryptoOnRamp.DAL/CryptoOnRamp.DAL.csproj", "CryptoOnRamp.DAL/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "CryptoOnRamp.API/CryptoOnRamp.API.csproj"
COPY . .
WORKDIR "/src/CryptoOnRamp.API"
RUN dotnet build "CryptoOnRamp.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CryptoOnRamp.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage for production
FROM base AS final
ENV ASPNETCORE_ENVIRONMENT=Production
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CryptoOnRamp.API.dll"]
