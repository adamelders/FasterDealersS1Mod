# AGENTS.md — FasterDealers Mod for Schedule 1

## Project Overview
Unity game mod using **MelonLoader** + **Harmony** to make drug dealer NPCs walk faster. Targets both Mono and IL2CPP versions of Schedule 1.

## Build Commands
```bash
# Build cross-compatible version (DEFAULT / SHIPPING TARGET)
dotnet build FasterDealers/FasterDealers.csproj -c CrossCompat

# Build for Mono (debugging only)
dotnet build FasterDealers/FasterDealers.csproj -c Mono

# Build for IL2CPP (debugging only)
dotnet build FasterDealers/FasterDealers.csproj -c Il2cpp
```

> **Important:** `CrossCompat` is always the shipping target. The released DLL should come from this build. `Mono` and `Il2cpp` targets exist solely for platform-specific debugging during development.

## Architecture
- **`Core.cs`** — Main `MelonMod` entry point. Handles config loading, coroutine-based dealer speed setting on Main scene load.
- **`Integrations/HarmonyPatches.cs`** — Harmony patch stubs. Uses `#if MONO / #elif IL2CPP` preprocessor directives for platform-specific namespaces.
- **`Utils/Constants.cs`** — Mod metadata (name, version, author) and config defaults/constraints.

## Key Conventions
- **Multi-platform builds**: Use `#if MONO`, `#elif IL2CPP`, `#elif CROSS_COMPAT` preprocessor directives for platform-specific code. Namespace prefixes differ (`ScheduleOne.` vs `Il2CppScheduleOne.`).
- **S1API NPCs**: All 6 dealers are typed via S1API (`S1API.Entities.NPCs.<Region>.<Name>`). Use `NPC.Get<T>()` to retrieve instances.
- **Config**: MelonPreferences stored in `UserData/MelonPreferences.cfg`. Category: `FasterDealers`, entries: `Enabled` (bool), `SpeedMultiplier` (float, default 3.0).
- **Thread safety**: Core uses a `_lock` object for shared state (`_speedsBeingSet`, `_completedDealers`, `_waitingDealers`).

## Pitfalls
- **Game paths in `.csproj`** are hardcoded to local machine paths. Update `<GamePath>`, `<ManagedPath>`, `<MelonLoaderPath>` before building on a new machine.
- **NPC timing**: Dealers may not exist when scene loads. Use coroutine polling (`WaitAndSetDealerSpeed<T>`) with `yield return new WaitForSeconds(0.5f)`.
- **CrossCompat** builds exclude `Assembly-CSharp` reference — don't reference game-specific types directly in cross-compatible code.

## External References
The S1API GitHub repository is configured as an MCP server for code and documentation lookups. Use it to explore S1API types, methods, and available APIs when working with game-specific functionality.

### Framework Documentation
- [S1API Documentation](https://ifbars.github.io/S1API/) — Game API reference for Schedule 1 modding
- [MelonLoader Wiki](https://melonwiki.xyz/#/) — Mod loader documentation, lifecycle hooks, preferences API
- [Harmony Guide](https://harmony.pardeike.net/) — Patching framework overview and patterns
- [Harmony API Reference](https://harmony.pardeike.net/api/index.html) — Full Harmony API documentation

## Documentation
- [README.md](README.md) — Installation, config, credits
- [CHANGELOG.md](CHANGELOG.md) — Version history
- [manifest.json](manifest.json) — Thunderstore package metadata (version, dependencies)
