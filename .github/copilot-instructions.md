# GitHub Copilot Instructions for Eii.Ecopath.Runner

## Project overview
Eii.Ecopath.Runner is a headless, JSON-driven runner for the EwE (Ecopath with Ecosim) marine
ecosystem modelling platform. It automates sequential runs of Ecopath, Ecosim, Ecospace and
Ecotracer without the GUI. It is available in two forms:
- **EwERunConsole** — a command-line executable driven by a JSON configuration file.
- **EwERunApi** — an ASP.NET Core Web API that accepts the same configuration as a POST body.

## Solution structure

| Project | Description |
|---|---|
| `Eii.Ecopath.Runner.Console` | CLI entry point (`EwERunConsole.csproj`) |
| `Eii.Ecopath.Runner.Api` | ASP.NET Core Web API wrapper (`EwERunApi.csproj`) |
| `Eii.Ecopath.Runner.Services` | Runtime engine, modifiers, and automation node tree |
| `Eii.Ecopath.Runner.Datamodel` | Plain data containers deserialised from JSON |

## Technology stack
- Language: C# 13
- Target framework: .NET 10 (`net10.0`)
- Publish target: Windows (`win-x64`), framework-dependent
- Key NuGet dependencies:
  - `Eii.Ecopath.EwECore` 6.7.x — the EwE simulation engine
  - `Eii.Ecopath.Bridge` 1.0.x — plug-in bridge for runtime callbacks (e.g. `cEcospaceBridgePlugin`)
  - `CommandLineParser` 2.9.1 — CLI argument parsing (Console only)
  - `Serilog` + sinks — file logging (Console only)
  - `Microsoft.Extensions.Logging` 10.x — logging abstraction
  - `Microsoft.AspNetCore.OpenApi` 10.x — OpenAPI support (API only)

## Code conventions
- Class names are prefixed with a lowercase `c` (e.g. `cEwEEngine`, `cRuntimeModifier`).
- Interfaces are prefixed with `I` (e.g. `IModelRunInstructions`).
- Do not use the `this.` qualifier unless needed to resolve an ambiguity.
- Use C# 12+ collection expressions (`[]`) for empty collection initialisers.
- XML doc comments use the `// ---` divider style already present in the file being edited.
- Use `#region` / `#endregion` blocks to group related members, consistent with existing files.
- Follow the comment and formatting style of the file being edited.
- Do not use `// ---` divider comment lines; use blank lines between members instead.

## Architecture

### Console entry point
- `Program.cs` — configures Serilog, parses CLI args via `CommandLineParser`, reads the JSON
  run-info file, and delegates to `cEwEEngine`.
- `CommandLineParmOptions.cs` — defines the CLI flags (`--runinfo`, `--output`, `--showtree`,
  `--showcommands`).

### API entry point
- `EwERunController.cs` — single `POST /eweRun` endpoint; accepts `cEwERunInstructions` as a
  JSON body, resolves paths, and delegates to `cEwEEngine`.

### Datamodel (`Eii.Ecopath.Runner.Datamodel`)
Plain data containers deserialised from JSON via `System.Text.Json`:
- `cEwERunInstructions` — top-level wrapper; top-level JSON keys are `Configuration`,
  `EcopathRun`, `EcosimRun`, `EcospaceRun`, `EcotracerRun`.
- `cEwEConfiguration` — global settings (model file path, scenario indices, run years).
- `cEcopathRunInstructions`, `cEcosimRunInstructions`, `cEcospaceRunInstructions`,
  `cEcotracerRunInstructions` — per-model settings and modification schedules.
- `cModificationsAtT` — parameter changes to inject at a specific date or time step.
- `IModelRunInstructions` — interface implemented by all per-model instruction classes.
- `JsonStrictConverter` — applied to each section; unknown JSON properties cause a reported error.
- `cConsoleCopy` — tees console output to a log file. **Note:** `cConsoleCopy` does NOT work reliably in Kubernetes. For writing console output to a log file in Kubernetes, use a different approach (e.g. a custom TextWriter or direct file writing) without `cConsoleCopy`.

### Services / runtime (`Eii.Ecopath.Runner.Services`)
- `cEwEEngine` (`cEwEEngine.cs`) — instantiates `cCore`, loads plug-ins, and orchestrates the
  Ecopath → Ecosim → Ecospace pipeline by delegating to the concrete modifier classes.
- `cRuntimeModifier` — abstract base; handles change scheduling, date→timestep conversion,
  and plug-in lookup. Concrete subclasses override `Run()`.
- `cEcopathModifier`, `cEcosimModifier`, `cEcospaceModifier` — concrete per-model runners.
- `Automation/` — `cEwERootNode` and its child nodes form a command/object tree used to apply
  dynamic parameter changes at runtime.

## JSON configuration
- The run-info JSON file maps directly onto `cEwERunInstructions`.
- Top-level keys: `Configuration`, `EcopathRun`, `EcosimRun`, `EcospaceRun`, `EcotracerRun`.
- `JsonStrictConverter` is applied to each section — unknown JSON properties cause a reported error.
- File paths inside the JSON are relative to the folder containing the JSON file.
- Scenario indices are one-based; `-1` means "skip this model".

## CI/CD
- Workflow: `.github/workflows/release.yml`, triggered on every push to `master`.
- Versioning: GitVersion 6.x with `workflow=TrunkBased/preview1`.
  - Patch is auto-incremented on every commit to `master`.
  - Use `+semver: minor` or `+semver: major` in a commit/merge message to force a higher bump.
- Private NuGet feed: `github-Official-EwE` at
  `https://nuget.pkg.github.com/Official-EwE/index.json`, authenticated via `GITHUB_TOKEN`.
- Publishes a `win-x64` framework-dependent binary, zipped and attached to a GitHub Release.
- See https://github.com/Official-EwE/.github/blob/master/profile/gitversion.md

## Docker (API project)
- The API project (`EwERunApi`) is published as a Docker image to `ghcr.io/official-ewe/`.
- The build-and-push script is at `Eii.Ecopath.Runner.Api/tools/build-push-docker.ps1`.
- The script requires a **persistent user-level environment variable** `DOCKER_BUILD_GITHUB_TOKEN`
  set to a GitHub Personal Access Token (PAT) with `read:packages` scope (to pull the private
  NuGet feed during the Docker build) and `write:packages` scope (to push the image to GHCR).
- Set the variable once on your machine before running the script:
  ```powershell
  [System.Environment]::SetEnvironmentVariable('DOCKER_BUILD_GITHUB_TOKEN', 'your-token', 'User')
  ```
- The token is passed to the Docker build as `--build-arg GITHUB_TOKEN` and is also used to
  authenticate `docker login ghcr.io` before pushing.
- Never commit the token value to source control.
- For Docker launch profiles in Visual Studio, use `containerRunArguments` to pass extra `docker run` arguments (such as volume bind mounts), NOT `additionalDockerRunArguments`.

## What to avoid
- Do not use `this.` unless resolving a name collision.
- Do not add new NuGet packages without a clear reason.
- Do not remove or alter the `// ToDo` stubs in `cEcopathModifier` — they are known placeholders.
- Do not change the hardcoded `return 1` in `Program.Main` without also wiring up proper exit codes
  throughout `ParseInstructions`.
- Do not change `workflow=TrunkBased/preview1` in `release.yml` — it is the only valid
  trunk-based workflow name embedded in GitVersion 6.x.

