# 3D Sketch Kit (MVP)

Modular Unity prototyping kit: attach plain `IAbility` modules via `AbilityManager`, group objects under `Room`, and drive zone effects from `ZoneEffectData` + `ZoneTrigger`.

## Layout

- `Runtime/` — player build (`ThreeDSketchKit.Runtime` assembly)
- `Editor/` — `Window > 3D Sketch Kit` and custom inspectors (`ThreeDSketchKit.Editor`)
- `_Demo/` — optional sample wiring (legacy `Input` axes)
- `Documentation/` — reserved for PDF/HTML user docs
- `Prefabs/` — place packaged prefabs here as you author them

## Quick start

1. Add `AbilityManager` to a character; use the **3D Sketch Kit** window (Abilities tab) or Inspector to add slots with assembly-qualified types, for example:
   - `ThreeDSketchKit.Modules.Abilities.WalkAbility, ThreeDSketchKit.Runtime`
   - `ThreeDSketchKit.Modules.Abilities.JumpAbility, ThreeDSketchKit.Runtime`
   - `ThreeDSketchKit.Modules.Abilities.MeleeAttackAbility, ThreeDSketchKit.Runtime`
2. Add `MovementComponent` / `HealthComponent` as needed; optional `EffectReceiverComponent` for buff zones.
3. For rooms: select block roots → **Create Room From Selection** in the Rooms tab.
4. For zones: add `ZoneTrigger` + trigger collider; assign `ZoneEffectData` assets.

## Extension

Implement `IAbility`, `IZoneEffect`, or domain interfaces in `Runtime/Core/Interfaces` from your own assembly; reference `ThreeDSketchKit.Runtime` and keep editor-only code under your `Editor` folder with `ThreeDSketchKit.Editor` patterns if you ship tooling.

## Conventions

- **Managers** — `MonoBehaviour` under `Runtime/Core/Components`
- **Modules** — plain classes under `Runtime/Modules/*`
- **Shared settings** — `ScriptableObject` under `Runtime/Core/Data`
