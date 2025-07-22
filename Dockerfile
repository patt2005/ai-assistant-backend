# Base runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy CSPROJ file separately to leverage Docker caching
COPY ["QwenChatBackend.csproj", "./"]

# Restore dependencies
RUN dotnet restore "QwenChatBackend.csproj"

# Copy everything else after restore (better caching)
COPY . .

# Ensure correct environment variable expansion
RUN dotnet build "QwenChatBackend.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "QwenChatBackend.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set entrypoint
ENTRYPOINT ["dotnet", "QwenChatBackend.dll"]
