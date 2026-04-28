# Writing Character Modules

Character modules are typed `ScriptableObject` assets. They must target exactly one host system.

## Contract

- Locomotion modules inherit `LocomotionModuleAsset`.
- Ability modules inherit `AbilityModuleAsset`.
- Combat modules inherit `CombatModuleAsset`.
- Inventory modules inherit `InventoryModuleAsset`.
- Equipment modules inherit `EquipmentModuleAsset`.
- Control modules inherit `ControlModuleAsset`.
- Interaction modules inherit `InteractableModuleAsset`.

Add `CharacterSystemModuleAttribute` so the integration window can show a clear name and host group.

```csharp
[CharacterSystemModule(CharacterSystemKind.Combat, "Melee Combat")]
public sealed class MeleeCombatModule : CombatModuleAsset
{
}
```

## Rules

- Do not write one module that owns several systems.
- Do not edit another host's serialized/private state.
- Use public host APIs for cross-system communication.
- Validate required sockets, profiles and dependencies.
- Clean up runtime objects and event subscriptions in `Shutdown`.
- Prefer small modules over a large all-purpose module.

## Beginner Guide: Use Modules Without Writing Code

Most designers should start with asset-backed modules shipped with the kit or created from templates.

### A) Understand Where Modules Live

Modules are `ScriptableObject` assets created under paths like:

```text
Assets/3DSketchKit/Runtime/Modules/
```

Your own experimental modules can live under:

```text
Assets/UserContent/Characters/<CharacterName>/Modules/
```

Keep user experiments outside core folders when possible.

### B) Create A Module Asset (Two Ways)

#### Method 1: Unity Create Menu

Some modules expose `CreateAssetMenu`. Use:

`Create → 3D Sketch Kit → Modules → ...`

This creates a `.asset` module instance.

#### Method 2: Integration Window

Open:

`3D Sketch Kit → Characters → Integrate Character Module`

This lists discovered concrete module types and can create module assets into a chosen folder.

### C) Attach Modules To Profiles (Recommended Data Flow)

Profiles own lists of modules:

- `LocomotionProfile` → locomotion modules
- `CombatProfile` → combat modules
- `InteractionProfile` → interaction modules

Workflow:

1. Select your profile asset (`*.asset`) in Project window.
2. Add module references to the profile list fields.

### D) Attach Modules Directly To Hosts (Fallback)

Each host component may also carry additional inline module lists for quick experiments.

Prefer profiles for reusable presets across multiple prefabs.

### E) Validate In Inspector

Select the generated prefab root that contains `CharacterEntity`.

Use **Validate Character Systems** (custom inspector helper) to print warnings/errors about missing sockets or dependencies.

### F) When You Finally Need Code

Create a new class:

1. Pick exactly one typed base (`CombatModuleAsset`, etc.).
2. Add `[CharacterSystemModule(CharacterSystemKind..., "Nice Name")]`.
3. Implement validation logic (`Validate(...)`) rather than embedding gameplay cross-talk.

Keep gameplay orchestration inside host systems or dedicated orchestrators, not inside unrelated modules.

See also:

- [`SystemHostsArchitecture.md`](SystemHostsArchitecture.md)
