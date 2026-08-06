# AGENTS.md - FasterDealers Mod for Schedule 1

## Project Overview

Unity game mod using MelonLoader and S1API to make drug dealer NPCs walk faster. It targets both Mono and IL2CPP versions of Schedule 1 and has no direct Harmony dependency.

## Build And Validation

```bash
# Build cross-compatible version (DEFAULT / SHIPPING TARGET)
dotnet build FasterDealers.csproj -c CrossCompat

# Build for Mono (debugging only)
dotnet build FasterDealers.csproj -c Mono

# Build for IL2CPP (debugging only)
dotnet build FasterDealers.csproj -c Il2cpp
```

`CrossCompat` is the shipping target. Its output is `FasterDealers.dll`; `Mono` and `Il2cpp` produce `FasterDealers_Mono.dll` and `FasterDealers_Il2cpp.dll` for platform-specific debugging.

There is currently no automated test project. Validate behavioral changes with the narrowest applicable build, then use the CrossCompat build before release.

## Build Configuration

Build paths and deployment settings are managed via [`local.build.props`](local.build.props) (imported automatically by `.csproj`). Copy [`example.build.props`](example.build.props) to `local.build.props` and set your local game installation paths. This file is **git-ignored** - never commit it.

Key properties in `local.build.props`:

- `AutomateLocalDeployment` - when `true`, built DLLs auto-copy to the configured game's `Mods/` folder after build.
- `LocalMonoDeploymentPath` / `LocalIl2CppDeploymentPath` - game installation roots used to derive managed-assembly locations.
- `UseLocalS1APIForked` - when `true`, uses `LocalS1APIForkedPath` instead of the NuGet package.
- `UseLocalMelonLoaderReferences` - when `true`, uses the MelonLoader assemblies from the configured game installation. This optional property is supported by the project file but is not shown in `example.build.props`.

`Mono` requires `LocalMonoDeploymentPath`; `Il2cpp` requires `LocalIl2CppDeploymentPath`. Build failures for missing paths are intentional configuration checks. CrossCompat uses the Mono Unity assemblies when that path is configured.

## Architecture

- **`Core.cs`** - Main `MelonMod` entry point. It loads preferences in `OnInitializeMelon`, subscribes to `GameLifecycle.OnLoadComplete`, then applies the configured multiplier directly to the six explicitly listed dealer NPC types through S1API.
- **`Utils/Constants.cs`** - Mod metadata, preference category, and shared constants.

## Key Conventions

- **Multi-platform code**: Use `#if MONO`, `#elif IL2CPP`, and `#elif CROSS_COMPAT` where APIs differ. CrossCompat defines both `CROSS_COMPAT` and `MONO`; it is a Mono-compatible shipping build and still relies on Unity assemblies.
- **Dependencies**: By default, the project restores S1API.Forked `3.1.9` and MelonLoader `0.7.0` from NuGet. Add Harmony only when introducing a concrete patch that requires it.
- **S1API lifecycle**: Subscribe to `GameLifecycle.OnLoadComplete` before retrieving dealers with `NPC.Get<T>()`. Keep null-safe application because an NPC can be unavailable at load completion.
- **Constants**: Keep IDs, version strings, preference names, and log tags in `Utils/Constants.cs`.
- **Preferences**: MelonPreferences persist in `UserData/MelonPreferences.cfg`. Category: `FasterDealers`; entries are `Enabled` (bool) and `SpeedMultiplier` (float, default `3.0`).

## Pitfalls

- **`local.build.props` is git-ignored** - copy from `example.build.props` and configure local paths before building platform-specific targets. Never commit it.
- **NPC timing**: A dealer may still be unavailable when `OnLoadComplete` runs. Preserve null-safe application and investigate the lifecycle before adding polling or retries.
- **Compatibility boundaries**: `Assembly-CSharp` is only referenced by the Mono and Il2cpp configurations. CrossCompat may use the Mono Unity references but should avoid APIs that require direct game assemblies.

## External References

The S1API GitHub repository is configured as an MCP server for code and documentation lookups. Use it to explore S1API types, methods, and available APIs when working with game-specific functionality.

### Framework Documentation

- [S1API Documentation](https://ifbars.github.io/S1API/) - Game API reference for Schedule 1 modding
- [MelonLoader Wiki](https://melonwiki.xyz/#/) - Mod loader documentation, lifecycle hooks, and preferences API

## Documentation

- [README.md](README.md) - Installation, configuration, and credits
- [README-NexusMods.md](README-NexusMods.md) - Nexus Mods release description
- [CHANGELOG.md](CHANGELOG.md) - Release history
- [manifest.json](manifest.json) - Thunderstore package version and dependencies
