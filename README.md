# ModularFlightIntegrator

A KSP plugin that lets other mods replace or extend parts of the stock `FlightIntegrator` (aerodynamics, thermodynamics, mass statistics) and of the stock `VesselPrecalculate` (gravity, orbit update, physics statistics) without each of them shipping its own subclass of the integrator, which would not work for more than one mod at a time.

MFI itself changes nothing in the game. It replaces the two stock components with subclasses whose virtual methods forward to a delegate when a mod has registered one, and to the stock code otherwise.

Licence: MIT. For KSP 1.12 to 1.13 (built against 1.12.5).

## Installation

Download the zip of the [latest release](https://github.com/sarbian/ModularFlightIntegrator/releases/latest) and copy its `ModularFlightIntegrator` folder into `GameData`. Mods that need MFI list it as a dependency; it is also on CKAN. The `ModularFlightIntegrator.version` file in the folder lets KSP-AVC check for updates against the latest GitHub release. `CHANGELOG.md` lists the changes of each version.

## Using it from a mod

Reference `ModularFlightIntegrator.dll` (do not copy it into your own folder), then register your methods once, from the Space Centre scene:

```csharp
using ModularFI;

[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
class MyRegistration : MonoBehaviour
{
    void Awake()
    {
        if (!ModularFlightIntegrator.RegisterUpdateAerodynamicsOverride(MyAero))
            Debug.Log("[MyMod] another mod already overrides UpdateAerodynamics");
        ModularFlightIntegrator.RegisterUpdateThermodynamicsPre(MyPreThermo);
    }

    static void MyAero(ModularFlightIntegrator fi, Part part)
    {
        fi.BaseFIUpdateAerodynamics(part); // the stock behaviour, if you want it
        // ... then change what you need
    }

    static void MyPreThermo(ModularFlightIntegrator fi) { }
}
```

Rules:

- **One owner per override.** Each `Register*Override` slot takes the first delegate registered and returns `true`; a later one from another mod gets `false` and a log line naming both. Registering the same delegate again is accepted silently, so an addon that runs on every Space Centre visit is fine. `Unregister*Override` hands the slot back; the stock code runs again.
- **`Pre` and `Post` lists take any number of handlers**, each once, run in registration order, each in its own `try/catch`: a handler that throws is skipped for that step and logged (in full the first time, then every 1000 failures). The `UpdateThermodynamics` lists run on **every FixedUpdate** of the integrator, not only on the steps where the stock code integrates the thermodynamics. The `CalculateSunBodyFlux` lists run once per frame, from `Update`.
- **`BaseFI*` / `Base*` methods** call the stock implementation from inside an override, so you can decorate it instead of replacing it.
- The scene is not enforced, but registering from the Space Centre means every vessel of the game sees the same hooks from its first frame.
- The `ModularFlightIntegrator` instance passed to a delegate exposes the stock protected fields as properties (`PartRef`, `CurrentMainBody`, `DensityThermalLerp`, `PartCount`, `PartThermalDataList`, `RecreateThermalGraph`, `PartThermalDataCount`, `FDeltaTime`, `FDeltaTimeRecip`, `DeltaTime`, `FTimeSinceThermo`, `FTimeSinceThermoRecip`, `WasMachConvectionEnabled`, `IntegratorTransform`, static `SunLayerMask`). The ones with a setter are rewritten by the stock `FixedUpdate` every step.

`Example.cs` in this repository is a complete, commented example.

## Hooks

### `ModularFlightIntegrator` (replaces `FlightIntegrator`)

Delegate types: `voidDelegate(fi)`, `voidBoolDelegate(fi, bool)`, `doubleDelegate(fi)`, `doubleDoubleDelegate(fi, double)`, `voidPartDelegate(fi, Part)`, `doublePartDelegate(fi, Part)`, `voidThermalDataDelegate(fi, PartThermalData)`, `doubleThermalDataDelegate(fi, PartThermalData)`, `IntegratePhysicalObjectsDelegate(fi, List<physicalObject>, double)` (its `double` result is ignored).

| Hook | Delegate | Extras |
|---|---|---|
| `FixedUpdate` | `voidDelegate` | replaces the whole stock step |
| `UpdateMassStats` | `voidDelegate` | `BaseFIUpdateMassStats`, `BaseFIGetPhysicslessChildMass` |
| `SmoothVelocity` | `voidDelegate` | `BaseFISmoothVelocity` |
| `CalculatePressure` | `voidDelegate` | `BaseFICalculatePressure` |
| `CalculateConstantsVacuum`, `CalculateConstantsAtmosphere` | `voidDelegate` | `BaseFI*` |
| `CalculateDensityThermalLerp` | `doubleDelegate` | `BaseFI*` |
| `CalculateBackgroundRadiationTemperature` | `doubleDoubleDelegate` | `BaseFI*` |
| `CalculateShockTemperature`, `CalculateAnalyticTemperature` | `doubleDelegate` | `BaseFI*` |
| `CalculateConvectiveCoefficient`, `...Newtonian`, `...Mach` | `doubleDelegate` | `BaseFI*` |
| `ThermoPrecalculate` | `voidDelegate` | `BaseFI*`; per frame |
| `CalculateSunBodyFlux` | `voidDelegate` | `BaseFI*`, `Pre`, `Post`; per frame |
| `UpdateOcclusion` | `voidBoolDelegate` | `BaseFI*` |
| `UpdateThermodynamics` | `voidDelegate` | `BaseFI*`, `Pre`, `Post` |
| `ThermalIntegrationPass` | `voidBoolDelegate` | `BaseFI*` |
| `UpdateThermalGraph`, `UpdateCompoundParts`, `UpdateConduction` | `voidDelegate` | `BaseFI*` |
| `PrecalcConduction`, `PrecalcConvection`, `PrecalcRadiation` | `voidThermalDataDelegate` | `BaseFI*` |
| `UpdateConvection`, `UpdateRadiation`, `UnifySkinTemp`, `SetSkinProperties` | `voidThermalDataDelegate` | `BaseFI*` |
| `GetSunArea`, `GetBodyArea` | `doubleThermalDataDelegate` | `BaseFI*` |
| `SetSkinThermalMass` | `voidPartDelegate` | `BaseFI*` |
| `Integrate` | `voidPartDelegate` | `BaseFIIntegrate` |
| `IntegratePhysicalObjects` | `IntegratePhysicalObjectsDelegate` | `BaseFI*` |
| `UpdateAerodynamics` | `voidPartDelegate` | `BaseFI*` |
| `CalculateDragValue`, `CalculateDragValue_Spherical`, `_Cylindrical`, `_Conic`, `_Cube` | `doublePartDelegate` | `BaseFI*` |
| `CalculateAerodynamicArea`, `CalculateAreaRadiative`, `CalculateAreaExposed` | `doublePartDelegate` | `BaseFI*` |

### `ModularVesselPrecalculate` (replaces `VesselPrecalculate`)

All take `Action`, except `MainPhysics` which takes `Action<bool>`; each has a `Base*` wrapper.

`RunFirst`, `MainPhysics`, `ApplyVelocityCorrection`, `GoOnRails`, `GoOffRails`, `CalculateGravity`, `CalculatePhysicsStats`, `SetLandedPosRot`, `StartEasing`, `StopEasing`.

`MainPhysics` runs at most once per physics step (keyed on the universal time), so a mod that calls `vessel.precalc.MainPhysics` itself before Unity does (Principia) gets one run, not two.

## Execution order

MFI sets `DefaultExecutionOrder(10)` on the integrator and `DefaultExecutionOrder(-102)` on the precalc. The stock integrator runs at 9; 9 raced with FAR and Principia, hence 10. The precalc runs early, before `Vessel`, like the stock one.

## Known users

FAR, Kopernicus, RealHeat, RP-1 (`ModuleNonReentryRated`), KerbalWeatherProject, AdvancedAtmosphereTools / CPWE, Blueshift, FlyingSaucers; Principia does not register anything but relies on the once-per-step rule of `MainPhysics`. FAR, KerbalWeatherProject and AdvancedAtmosphereTools all want the `UpdateAerodynamics` slot: the first one started wins, check your return value.

## Building

A .NET Framework 4.8 class library built with the .NET SDK. Set the `KSPDIR` environment variable to the KSP install (or pass `-p:KSPDIR=<path>`): the references are read from its `KSP_x64_Data\Managed`, and a build copies the DLL and its pdb into `GameData\ModularFlightIntegrator`. With `KSPDIR` empty nothing is copied.

```
dotnet build -c Debug
```

## Releases

GitHub Actions builds every push and pull request (`build.yml`, the zip is a workflow artifact and a comment on the PR). A release is made by running the `create-release` workflow with the version (`major.minor.patch.build`): it builds that version, commits the bump to `AssemblyFileVersion` in `Properties/AssemblyInfo.cs`, names the CHANGELOG's `Unreleased` section after it, tags it `v<version>`, and publishes the release with `ModularFlightIntegrator-<version>.zip` and the KSP-AVC `ModularFlightIntegrator.version` next to it. The version file is generated from `ModularFlightIntegrator/ModularFlightIntegrator.version.in` with the KSP range set in `build.yml` (1.12.0 to 1.13.99, built against 1.12.5).

`AssemblyVersion` stays 1.0.0.0 on purpose: other mods are compiled against this assembly. The API level a mod can require with `KSPAssemblyDependency` is the `KSPAssembly` attribute (1.3); raise it by hand when the API changes.

## History worth knowing

- 1.2.6 to 1.2.7 added the `UpdateMassStats` hook. Kopernicus shipped a recompile of 1.2.6 for a while because it seemed to fix landing gear sinking through terrain on modded planets ([Kopernicus #459](https://github.com/Kopernicus/Kopernicus/issues/459), [#12](https://github.com/sarbian/ModularFlightIntegrator/pull/12)); the code difference cannot explain it, so the cause was the build or timing. 1.2.8 and 1.2.9 moved the scheduling to `DefaultExecutionOrder`.
- 1.3.0 made `ModularVesselPrecalculate` public (it was `internal`, so its hooks were unreachable), restored the `UpdateConvection` hook (commented out since 2015), fixed the once-per-step guard for long sessions, added `Unregister*`, exception isolation of the `Pre`/`Post` lists, the missing `BaseFI*` wrappers and the hooks listed above. Misspelled method names from earlier versions still work and are marked `[Obsolete]`.
