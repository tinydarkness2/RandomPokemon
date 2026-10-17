# PokeViewer

Blazor Web App that shows a random Pokémon (name, types, height,
weight, sprite) from https://pokeapi.co/api/v2 when a button is clicked.
Built as a practice project for production-style architecture.

## Solution layout (clean architecture)
- src/PokeViewer.Domain: entities and value objects, no dependencies
- src/PokeViewer.Application: use cases, interfaces, DTOs (refs Domain)
- src/PokeViewer.Infrastructure: PokeAPI client, caching (refs Application)
- src/PokeViewer.Web: Blazor UI and composition root (DI wiring only)
- tests/: one test project per src project

Dependencies point inward only. Domain and Application never
reference Infrastructure or Web.

## Stack and conventions
- .NET 10 (LTS), C# with nullable enabled, warnings as errors
- Blazor Web App, Interactive Server render mode
- Solution file in .slnx format
- Central package management (Directory.Packages.props)
- Shared settings in Directory.Build.props, plus .editorconfig
- Typed HttpClient via IHttpClientFactory, with a resilience handler
- PokeAPI base URL and ID range via the Options pattern
- IMemoryCache for API responses
- Async everywhere, pass CancellationToken through
- Records for DTOs, ILogger for logging
- Plain handler classes for use cases (no MediatR)

## Commands
- Build: dotnet build
- Test: dotnet test
- Run: dotnet run --project src/PokeViewer.Web

## Rules
- Infrastructure implements interfaces defined in Application
- The Blazor UI calls Application use cases, never HttpClient directly
- Show friendly loading and error states in the UI
- xUnit for unit tests, bUnit for components, fake HttpMessageHandler
  for the API client
- Tests never call the live API
