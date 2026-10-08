# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the solution and project files to restore dependencies
COPY ["src/AutoGarage.slnx", "./"]
COPY ["src/AutoGarage.Api/AutoGarage.Api.csproj", "AutoGarage.Api/"]

# Restore NuGet packages
RUN dotnet restore "AutoGarage.slnx"

# Copy the remaining source code
COPY src/ .

# Build the project
WORKDIR "/src/AutoGarage.Api"
RUN dotnet build "AutoGarage.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AutoGarage.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final production image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Define the entry point for the container
ENTRYPOINT ["dotnet", "AutoGarage.Api.dll"]