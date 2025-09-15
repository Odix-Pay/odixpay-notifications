FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first for better layer caching
COPY ["src/OdixPay.Notifications.API/OdixPay.Notifications.API.csproj", "src/OdixPay.Notifications.API/"]
COPY ["src/OdixPay.Notifications.Application/OdixPay.Notifications.Application.csproj", "src/OdixPay.Notifications.Application/"]
COPY ["src/OdixPay.Notifications.Domain/OdixPay.Notifications.Domain.csproj", "src/OdixPay.Notifications.Domain/"]
COPY ["src/OdixPay.Notifications.Infrastructure/OdixPay.Notifications.Infrastructure.csproj", "src/OdixPay.Notifications.Infrastructure/"]
COPY ["src/OdixPay.Notifications.Realtime.Contracts/OdixPay.Notifications.Realtime.Contracts.csproj", "src/OdixPay.Notifications.Realtime.Contracts/"]

# Restore dependencies
RUN dotnet restore "src/OdixPay.Notifications.API/OdixPay.Notifications.API.csproj"

# Copy all source code and configuration files
COPY . .

# Replace appsettings.json with appsettings.Development.json for development environment
RUN rm -f src/OdixPay.Notifications.API/appsettings.json
RUN mv src/OdixPay.Notifications.API/appsettings.Development.json src/OdixPay.Notifications.API/appsettings.json

# Build the application
WORKDIR "/src/src/OdixPay.Notifications.API"
RUN dotnet build "OdixPay.Notifications.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OdixPay.Notifications.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Copy .pem files from OdixPay.Notifications.API folder
COPY --from=build /src/src/OdixPay.Notifications.API/*.pem ./

# Copy any other configuration files needed from OdixPay.Notifications.API folder
# COPY --from=build /src/src/OdixPay.Notifications.API/appsettings.Development.json ./
COPY --from=build /src/src/OdixPay.Notifications.API/*.xml ./

# Create non-root user for security (optional but recommended)
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "OdixPay.Notifications.API.dll"]
