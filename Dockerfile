# syntax=docker/dockerfile:1

# Targets:
#   final (default) - Release build on the slim ASP.NET runtime image
#   debug           - Debug build, used by compose.debug.yaml and the VS Code
#                     "Docker" launch config. vsdbg is not baked in: compose
#                     mounts the C# extension's own copy at /vsdbg so its
#                     version always matches the extension.

ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Restore first, using only the files that affect it, so the layer is cached
# until a project or package version changes.
COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/PokeViewer.Domain/PokeViewer.Domain.csproj src/PokeViewer.Domain/
COPY src/PokeViewer.Application/PokeViewer.Application.csproj src/PokeViewer.Application/
COPY src/PokeViewer.Infrastructure/PokeViewer.Infrastructure.csproj src/PokeViewer.Infrastructure/
COPY src/PokeViewer.Web/PokeViewer.Web.csproj src/PokeViewer.Web/
RUN dotnet restore src/PokeViewer.Web/PokeViewer.Web.csproj

COPY src/ src/

FROM build AS publish
ARG CONFIGURATION=Release
RUN dotnet publish src/PokeViewer.Web/PokeViewer.Web.csproj \
    --configuration $CONFIGURATION \
    --output /app/publish \
    /p:UseAppHost=false

FROM base AS debug
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "PokeViewer.Web.dll"]

FROM base AS final
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "PokeViewer.Web.dll"]
