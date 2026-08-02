# AGENTS.md — FasterDealers Mod for Schedule 1

## Project Overview
Unity game mod using **MelonLoader** + **Harmony** to make drug dealer NPCs walk faster. Targets both Mono and IL2CPP versions of Schedule 1.

## Build Commands
```bash
# Build cross-compatible version (DEFAULT / SHIPPING TARGET)
dotnet build FasterDealers.csproj -c CrossCompat

# Build for Mono (debugging only)
dotnet build FasterDealers.csproj -c Mono

# Build for IL2CPP (debugging only)
dotnet build FasterDealers.csproj -c Il2cpp
```

> **Important:** `CrossCompat` is always the shipping target. The released DLL should come from this build. `Mono` and `Il2cpp` targets exist solely for platform-specific debugging during development.

## Build Configuration
Build paths and deployment settings are managed via [`local.build.props`](local.build.props) (imported automatically by `.csproj`). Copy [`example.build.props`](example.build.props) to `local.build.props` and set your local game installation paths. This file is **git-ignored** — never commit it.

Key properties in `local.build.props`:
- `AutomateLocalDeployment` — When `true`, built DLLs auto-copy to the game's `Mods/` folder after build.
- `LocalMonoDeploymentPath` / `LocalIl2CppDeploymentPath` — Game installation directories for each platform.
- `UseLocalS1APIForked` — When `true`, uses a local S1API DLL instead of the NuGet package (set `LocalS1APIForkedPath`).

## Architecture
- **`Core.cs`** — Main `MelonMod` entry point. Handles config loading, coroutine-based dealer speed setting on Main scene load.
- **`Integrations/HarmonyPatches.cs`** — Harmony patch stubs. Uses `#if MONO / #elif IL2CPP` preprocessor directives for platform-specific namespaces.
- **`Utils/Constants.cs`** — Mod metadata (name, version, author) and config defaults/constraints.

## Key Conventions
- **Multi-platform builds**: Use `#if MONO`, `#elif IL2CPP`, `#elif CROSS_COMPAT` preprocessor directives for platform-specific code. Namespace prefixes differ (`ScheduleOne.` vs `Il2CppScheduleOne.`).
- **Dependencies via NuGet**: S1API.Forked, MelonLoader, and HarmonyX are pulled from NuGet by default. Set `UseLocalS1APIForked=true` in `local.build.props` to reference a local build instead.
- **S1API registration**: Register S1API items, NPCs, quests, saveables, and shop data from `GameLifecycle.OnPreLoad`.
- **Harmony patches**: Put all Harmony patch classes under `Integrations/`.
- **Constants**: Keep IDs, version strings, config names, and log tags in `Utils/Constants.cs`.
- **CrossCompat rules**: Keep CrossCompat code on S1API wrappers and public abstractions. If a file needs direct game types, guard it with `#if MONO` / `#if IL2CPP` or keep it out of CrossCompat.
- **S1API NPCs**: All 6 dealers are typed via S1API (`S1API.Entities.NPCs.<Region>.<Name>`). Use `NPC.Get<T>()` to retrieve instances.
- **Config**: MelonPreferences stored in `UserData/MelonPreferences.cfg`. Category: `FasterDealers`, entries: `Enabled` (bool), `SpeedMultiplier` (float, default 3.0).
- **Thread safety**: Core uses a `_lock` object for shared state (`_speedsBeingSet`, `_completedDealers`, `_waitingDealers`).

## Pitfalls
- **`local.build.props` is git-ignored** — Always copy from `example.build.props` and set local paths before building. The `.csproj` will emit build errors if required paths are missing.
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
