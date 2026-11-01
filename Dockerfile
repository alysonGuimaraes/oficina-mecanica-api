# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER 1654
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project files of the API and its project references, so the restore
# layer is cached until a dependency changes. Paths mirror src/ for the relative references.
COPY ["src/AutoGarage.Api/AutoGarage.Api.csproj", "AutoGarage.Api/"]
COPY ["src/AutoGarage.Application/AutoGarage.Application.csproj", "AutoGarage.Application/"]
COPY ["src/AutoGarage.Infrastructure/AutoGarage.Infrastructure.csproj", "AutoGarage.Infrastructure/"]
COPY ["src/AutoGarage.Domain/AutoGarage.Domain.csproj", "AutoGarage.Domain/"]
COPY ["src/Shared/AutoGarage.Communication/AutoGarage.Communication.csproj", "Shared/AutoGarage.Communication/"]

# Restore NuGet packages for the API project (test projects are not part of the image)
RUN dotnet restore "AutoGarage.Api/AutoGarage.Api.csproj"

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