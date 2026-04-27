# 3D Sketch Kit (MVP)

Modular Unity prototyping kit: attach plain `IAbility` modules via `AbilityManager`, group objects under `Room`, and drive zone effects from `ZoneEffectData` + `ZoneTrigger`.

## Layout

- `Runtime/` — player build (`ThreeDSketchKit.Runtime` assembly)
- `Editor/` — `Window > 3D Sketch Kit` and custom inspectors (`ThreeDSketchKit.Editor`)
- `_Demo/` — optional sample wiring (legacy `Input` axes)
- `Documentation/` — reserved for PDF/HTML user docs
- `Prefabs/` — place packaged prefabs here as you author them
- `link.xml` — IL2CPP: preserves the runtime assembly (see Extension below)

## Quick start

1. Add `AbilityManager` to a character. Prefer **stable ability ids** (see `SketchKitBuiltInAbilityIds`) or use the **3D Sketch Kit** window to append slots (fills `abilityId` + assembly-qualified name when the type has `SketchKitAbilityIdAttribute`).
2. Add `MovementComponent` / `HealthComponent` as needed; optional `EffectReceiverComponent` for buff zones.
3. For rooms: select block roots → **Create Room From Selection** in the Rooms tab.
4. For zones: add `ZoneTrigger` + trigger collider; assign `ZoneEffectData` assets.

### Built-in ability ids (`SketchKitBuiltInAbilityIds`)

| Constant | Value |
|----------|--------|
| `Walk` | `com.3dsketchkit.abilities.walk` |
| `Jump` | `com.3dsketchkit.abilities.jump` |
| `MeleeAttack` | `com.3dsketchkit.abilities.melee_attack` |

Legacy: you can still leave `abilityId` empty and use only the assembly-qualified type string; `AbilityTypeCatalog.TryResolveType` falls back to `Type.GetType`.

## Extension (Asset Store–style)

1. **Reference** `ThreeDSketchKit.Runtime` from your game assembly (asmdef → Assembly Definition References).
2. Implement `IAbility` with a **parameterless constructor** (any accessibility).
3. Choose one (or combine):
   - **`[SketchKitAbilityId("com.yourstudio.abilities.foo")]`** on the class — it is picked up when `AbilityTypeCatalog.RefreshDiscoveredAbilities()` runs (automatically at subsystem registration in players, and on script compile in the Editor via `SketchKitAbilityCatalogEditorSync`).
   - **`AbilityTypeCatalog.Register("your.id", typeof(MyAbility))`** (or `Register<MyAbility>(...)`) from your bootstrap, e.g. `[RuntimeInitializeOnLoadMethod]` after assemblies load — safest for IL2CPP if you do not rely on attribute scanning alone.
4. **IL2CPP / stripping**: this package ships `link.xml` preserving **`ThreeDSketchKit.Runtime`**. For **your** assembly, add your own `link.xml` or `[Preserve]` / static references so types activated only by string are not stripped.

## Conventions

- **Managers** — `MonoBehaviour` under `Runtime/Core/Components`
- **Modules** — plain classes under `Runtime/Modules/*`
- **Shared settings** — `ScriptableObject` under `Runtime/Core/Data`
