# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_TOKEN
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["Eii.Ecopath.Runner.RunProcess/EwERunProcess.csproj", "Eii.Ecopath.Runner.RunProcess/"]
COPY ["Eii.Ecopath.Runner.Datamodel/Eii.Ecopath.Runner.Datamodel.csproj", "Eii.Ecopath.Runner.Datamodel/"]
COPY ["Eii.Ecopath.Runner.Services/Eii.Ecopath.Runner.Services.csproj", "Eii.Ecopath.Runner.Services/"]

# Add authentication for GitHub Packages using add source with credentials
RUN if [ ! -z "$GITHUB_TOKEN" ]; then \
    dotnet nuget remove source github-Official-EwE || true; \
    dotnet nuget add source https://nuget.pkg.github.com/Official-EwE/index.json \
    --name github-Official-EwE \
    --username github \
    --password $GITHUB_TOKEN \
    --store-password-in-clear-text \
    --configfile NuGet.config; \
    fi

RUN dotnet nuget locals all --clear     # Clear local NuGet cache to ensure we get the latest packages, especially important when using tokens for private feeds
RUN dotnet restore "Eii.Ecopath.Runner.RunProcess/EwERunProcess.csproj"

COPY . .
WORKDIR "/src/Eii.Ecopath.Runner.RunProcess"
RUN dotnet build "EwERunProcess.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EwERunProcess.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Remove local test directories that may have been copied in during publish — not needed in the image
RUN rm -rf testconfig testdata testoutput
ENTRYPOINT ["dotnet", "EwERunProcess.dll"]