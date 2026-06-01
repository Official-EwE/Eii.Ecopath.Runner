# GitHub Copilot Instructions for EwERunConsole

## Project overview
EwERunConsole is a headless, JSON-driven command-line runner for the EwE (Ecopath with Ecosim)
marine ecosystem modelling platform. It automates sequential runs of Ecopath, Ecosim, Ecospace
and Ecotracer without the GUI, driven by a JSON configuration file.

## Technology stack
- Language: C# 12
- Target framework: .NET 8 (`net8.0`)
- Platform: Windows (`win-x64`)
- Key NuGet dependencies:
  - `Eii.Ecopath.EwECore` — the EwE simulation engine
  - `Eii.Ecopath.Bridge` — plug-in bridge for runtime callbacks (e.g. `cEcospaceBridgePlugin`)
  - `CommandLineParser` — CLI argument parsing

## Code conventions
- Class names are prefixed with a lowercase `c` (e.g. `cEcospaceModifier`, `cRuntimeModifier`).
- Interfaces are prefixed with `I` (e.g. `IModelRunInstructions`).
- Do not use the `this.` qualifier unless needed to resolve an ambiguity.
- Use C# 12 collection expressions (`[]`) for empty collection initialisers.
- XML doc comments use the `// ---` divider style already present in the file being edited.
- Follow the comment and formatting style of the file being edited.

## Architecture
- `Program.cs` — entry point; parses CLI args, reads the JSON run-info file, delegates to `cEwEEngine`.
- `RunInstructions/` — plain data containers deserialised from the JSON file:
  - `cEwERunInstructions` — top-level wrapper
  - `cEwEConfiguration` — global settings (model file, scenario indices, run years)
  - `cEcosimRunInstructions`, `cEcospaceRunInstructions`, `cEcotracerRunInstructions` — per-model settings
  - `cModificationsAtT` — parameter changes to inject at a specific date or time step
- `Runtime/` — execution layer:
  - `cEwEEngine` (`cEwEWrapper.cs`) — orchestrates the Ecopath → Ecosim → Ecospace pipeline
  - `cRuntimeModifier` — abstract base; handles change scheduling, date→timestep conversion, plug-in lookup
  - `cEcopathModifier`, `cEcosimModifier`, `cEcospaceModifier` — concrete per-model runners
- `Automation/` — command/object tree (`cEwERootNode`) used to apply dynamic parameter changes at runtime.

## JSON configuration
- The run-info JSON file maps directly onto `cEwERunInstructions`.
- Top-level keys: `Configuration`, `EcopathRun`, `EcosimRun`, `EcospaceRun`, `EcotracerRun`.
- `JsonStrictConverter` is applied to each section — unknown JSON properties cause a reported error.
- File paths inside the JSON are relative to the folder containing the JSON file.
- Scenario indices are one-based; `-1` means "skip this model".

## What to avoid
- Do not use `this.` unless resolving a name collision.
- Do not add new NuGet packages without a clear reason.
- Do not remove or alter the `// ToDo` stubs in `cEcopathModifier` — they are known placeholders.
- Do not change the hardcoded `return 1` in `Program.Main` without also wiring up proper exit codes
  throughout `ParseInstructions`.
