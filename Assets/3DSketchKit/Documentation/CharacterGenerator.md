# Character Prefab Generator

Open the generator from `3D Sketch Kit/Characters/Character Prefab Generator`.

## Inputs

- Model prefab or model asset.
- Optional material.
- Optional base texture.
- Optional Animator Controller.
- Template: `NeutralShell`, `PlayerReady`, `MobReady`, `NpcReady`.
- Output prefab folder.
- Generated profile folder.
- Prefab name.

## Output

The generator creates a prefab with:

- `CharacterEntity`
- `AbilityManager`
- `MovementComponent`
- `HealthComponent`
- `Model`
- `Sockets`
- `Systems`
- `Colliders`

It also creates minimal profile assets and links them through `CharacterPreset`.

The source mesh and rig are referenced, not duplicated.

## Beginner Guide: Full Walkthrough From Empty Folder To Shell Prefab

### Step 1 — Prepare Art Imports

Follow [`SourceMaterialContract.md`](SourceMaterialContract.md):

- Import FBX/GLB into `Assets/UserContent/Characters/<Name>/Model/`.
- Fix Rig + Avatar issues before generating gameplay shells.

Optional sanity check:

- Drag the imported model into an empty Scene.
- Confirm it renders and scale looks sane.

### Step 2 — Create Supporting Assets

Minimum recommended:

1. **Textures** in `Textures/`.
2. **Material** in `Materials/` (assign textures).
3. **Animator Controller** with at least `Idle` clip.
4. (Optional) Separate animation FBX files in `Animations/`.

You can skip Animator only if you accept a non-animated prototype shell.

### Step 3 — Open The Generator Window

Menu:

`3D Sketch Kit → Characters → Character Prefab Generator`

Fields:

- **Model Prefab/Asset**: pick the imported model (or a prefab variant you made from it).
- **Material / Base Texture**: optional; use if you want the generator to wire a material for you.
- **Animator Controller**: optional but strongly recommended.
- **Template**: choose a role hint (`NeutralShell` is safest default).
- **Output Folder**: defaults to `Assets/3DSketchKit/Prefabs/Characters/Shells` (you may change it).
- **Profile Folder**: defaults to generated profile storage under runtime data paths; you may redirect to `Assets/UserContent/.../Profiles/`.

### Step 4 — Validate, Then Generate

1. Click **Validate** and read warnings/errors.
2. Fix pink materials / missing renderers first.
3. Click **Generate Character Shell Prefab**.

The tool creates:

- A shell prefab (with hosts and core components)
- Minimal `CharacterPreset` + related profile assets

### Step 5 — Open The Resulting Prefab And Wire Gameplay Data

Open the created prefab and confirm:

- `CharacterEntity` has sane `displayName`/role depending on template.
- `Model` references your imported instance (no duplicate mesh asset).
- Profiles exist and are referenced (via `CharacterPreset` + host fallbacks).

### Step 6 — Create Player/Mob/NPC Variants Without Duplicating Art

Recommended:

1. Right click the shell prefab → `Create → Prefab Variant`.
2. Rename variant (`PF_Goblin_Player`, `PF_Goblin_Mob`).
3. Swap **profiles/modules** only.

### Step 7 — Add Modules Safely

Use:

`3D Sketch Kit → Characters → Integrate Character Module`

Attach modules through profiles (`LocomotionProfile`, etc.), not by scattering logic across random components.

See also:

- [`WritingCharacterModules.md`](WritingCharacterModules.md)
- [`SystemHostsArchitecture.md`](SystemHostsArchitecture.md)
