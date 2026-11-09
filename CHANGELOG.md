# Changes

## Unreleased

API level 1.3 (`KSPAssembly("ModularFlightIntegrator", 1, 3)`); `AssemblyVersion` stays 1.0.0.0, so mods built against 1.2 keep loading. Misspelled names from 1.2 still work and are marked `[Obsolete]`.

- `ModularVesselPrecalculate` is public, so its hooks can be registered from another mod; it was `internal` (#16, supersedes #8). The `RunFirst` override is registered with `RegisterRunFirstOverride`, and every precalc hook has a `Base*` method.
- `RegisterUpdateConvectionOverride` works: the `UpdateConvection` override it feeds had been commented out since KSP 1.0.5 (#17).
- `MainPhysics` runs once per physics step keyed on the universal time; the `float` `Time.fixedTime` key skipped about one step in three after 73 hours of game session (#18).
- Registration: `Unregister*` for every hook, the same delegate registered twice is accepted once, a refused registration names both mods in the log, and a registration outside the Space Centre is logged but accepted (#19).
- `UpdateThermodynamics` and `CalculateSunBodyFlux` Pre/Post handlers run one by one, each in a `try/catch`: one failing handler no longer skips the others and the rest of the vessel's physics step (#20).
- Consistent method names (`Register...Override`, `BaseFI...`) and correct conflict messages (#19, #21).
- `BaseFI*` methods for the seven hooks that had none (#22).
- New hooks: `ThermalIntegrationPass`, `PrecalcConduction`, `PrecalcConvection`, `PrecalcRadiation`, `UnifySkinTemp`, `SetSkinThermalMass`, `SmoothVelocity`, `ThermoPrecalculate`, the four `CalculateDragValue_*` models, and on the precalc `SetLandedPosRot`, `StartEasing`, `StopEasing` (#23).
- XML docs say when the Pre/Post handlers run (#24). No more VesselModule list in the log for every vessel start (#28).
- README, root LICENSE (#29).
- Project: SDK-style .NET Framework 4.8 project, references through `KSPDIR`, portable pdb, nothing copied without `KSPDIR`; UTF-8 BOMs removed. GitHub Actions: `build.yml` builds every push and PR (zip as artifact and PR comment), `create-release` builds, bumps, tags and publishes a release with the zip and the KSP-AVC version file. The version file points at GitHub releases and declares KSP 1.11.0 to 1.13.99.

## 1.2.10.0 (2021-08-09)

- The integrator runs at execution order 10 instead of 8: order 9 is the stock FlightIntegrator's, and running next to it caused execution-order problems with FAR and Principia.
- The version file declares KSP 1.12 (1.11.0 to 1.12.90).

## 1.2.9.0 (2021-02-06)

- `ModularVesselPrecalculate` runs at execution order -102, before the vessel, like the stock precalc.
