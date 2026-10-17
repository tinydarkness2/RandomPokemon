# PokeViewer

A small Blazor Web App that shows a random Pokémon from Generations 1–4
(name, types, height, weight and official artwork) each time you press a
button. Data comes from [PokeAPI](https://pokeapi.co/).

It's a practice project for production-style .NET architecture: clean
architecture layers, the Options pattern, a typed `HttpClient` with
resilience, in-memory caching, and tests for every layer.

## Getting started

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
dotnet build
dotnet test
dotnet run --project src/PokeViewer.Web
```

Then open the URL printed in the console and press **Show me a Pokémon!**

## Solution layout

```
src/
  PokeViewer.Domain          Entities and value objects (no dependencies)
  PokeViewer.Application     Use case handler, interfaces, DTOs, options
  PokeViewer.Infrastructure  PokeAPI client, memory cache, DI registration
  PokeViewer.Web             Blazor UI (Interactive Server) and composition root
tests/
  One xUnit project per src project (bUnit for the Web components)
```

Dependencies point inward only: Web → Infrastructure → Application → Domain.
The UI calls `GetRandomPokemonHandler`, never `HttpClient` directly.

### How a request flows

1. `Home.razor` calls `GetRandomPokemonHandler.HandleAsync`.
2. The handler picks an ID in the configured range via `IRandomIdProvider`.
3. `CachedPokemonRepository` returns the Pokémon from `IMemoryCache`, or asks
   `PokeApiClient` for it and caches the result. Failures are never cached.
4. `PokeApiClient` calls `GET pokemon/{id}` through the standard resilience
   handler (retries, timeouts, circuit breaker) and maps the JSON to Domain.
5. Any API failure becomes a `PokemonUnavailableException`, which the handler
   turns into a friendly error message for the UI.

## Configuration

Settings live in the `PokeApi` section of
`src/PokeViewer.Web/appsettings.json` and are validated at startup:

| Key             | Default                      | Meaning                                  |
| --------------- | ---------------------------- | ---------------------------------------- |
| `BaseUrl`       | `https://pokeapi.co/api/v2/` | PokeAPI root                             |
| `MinId`         | `1`                          | Lowest Pokédex number to pick            |
| `MaxId`         | `493`                        | Highest Pokédex number (493 = Arceus)    |
| `CacheDuration` | `01:00:00`                   | How long a fetched Pokémon stays cached  |

For example, to stick to Generation 1, set `MaxId` to `151`, or use an
environment variable: `PokeApi__MaxId=151`.

## Testing

```bash
dotnet test
```

- **Domain / Application:** plain xUnit, with hand-written fakes.
- **Infrastructure:** a fake `HttpMessageHandler` returns canned PokeAPI JSON,
  so tests never call the live API.
- **Web:** bUnit tests cover the hint, loading, success and error states.

## Conventions

- .NET 10, nullable enabled, warnings as errors (`Directory.Build.props`)
- Central package management (`Directory.Packages.props`)
- Code style in `.editorconfig`
- Async throughout, with `CancellationToken` passed all the way down

## License

See [LICENSE](LICENSE).
