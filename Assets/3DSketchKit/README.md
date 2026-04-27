# 3D Sketch Kit (MVP)

Modular Unity prototyping kit: attach plain `IAbility` modules via `AbilityManager`, group objects under `Room`, and drive zone effects from `ZoneEffectData` + `ZoneTrigger`.

## Layout

- `Runtime/` — player build (`ThreeDSketchKit.Runtime` assembly)
- `Editor/` — `Window > 3D Sketch Kit` and custom inspectors (`ThreeDSketchKit.Editor`)
- `_Demo/` — optional sample wiring (legacy `Input` axes)
- `Documentation/` — reserved for PDF/HTML user docs
- `Prefabs/` — place packaged prefabs here as you author them  
  - **Building blocks:** menu **3D Sketch Kit → Generate Building Block Prefabs** creates `Floor`, `Wall`, `Pillar`, `Beam`, `Door`, `Ramp` under `Prefabs/BuildingBlocks/` (URP Lit materials in `Materials/`).
  - **Ramp mesh (Blender / etc.):** export **FBX** or **OBJ** into `Prefabs/BuildingBlocks/Source/` as **`RampModel.fbx`** or **`Ramp.fbx`** (also `RampModel.obj` / `Ramp.obj`, or `.blend` if you keep blends in the project). The generator picks the first mesh in the file, preferring a submesh whose name contains **`Ramp`** (rename the mesh object in Blender so it matches, e.g. `Ramp`). If no source file exists, a small built-in wedge is saved to `Prefabs/BuildingBlocks/Meshes/Ramp_Mesh.asset` and **updated in place** on each generate (the asset is no longer deleted every time, so references stay valid). For **MeshCollider** on imported models, select the asset in the Project window → **Model** tab → enable **Read/Write** if the collider does not work.
- `link.xml` — IL2CPP: preserves the runtime assembly (see Extension below)

## Quick start

1. Add `AbilityManager` to a character. Prefer **stable ability ids** (see `SketchKitBuiltInAbilityIds`) or use the **3D Sketch Kit** window to append slots (fills `abilityId` + assembly-qualified name when the type has `SketchKitAbilityIdAttribute`).
2. Add `MovementComponent` / `HealthComponent` as needed; optional `EffectReceiverComponent` for buff zones.
3. For rooms: select block roots → **Create Room From Selection** in the Rooms tab; to undo that grouping, select the Room or any member → **Unpack room** (members reparent to the scene root, or under the parent **Room** if this room was nested inside one). A GameObject that is already a member of a room cannot be added to a new room until you unpack it, except when you select **two or more** objects that are **all** members of the **same** room and **none** of them is a `Room` root: then a new room is created **under** that owner and the selection becomes the new room’s members. You need at least a block, two **Room** roots, or a mix — not a single **Room** alone. After creation, a branch may contain at most **three** `Room` components (this limit applies to nested “same owner” as well, counting the owner chain), deeper is rejected.
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
