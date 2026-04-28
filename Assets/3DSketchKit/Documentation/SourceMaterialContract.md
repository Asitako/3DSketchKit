# Character Source Material Contract

The Character Prefab Generator creates a shell prefab from prepared Unity assets. It does not repair arbitrary broken models.

## Required

- A Unity model asset or prefab, usually `FBX` or `GLB`.
- At least one `Renderer` or `SkinnedMeshRenderer`.
- For skinned characters: bones assigned on every `SkinnedMeshRenderer`.
- A valid Humanoid Avatar or a stable Generic rig.
- A material, or a base texture that can be converted into a simple material.

## Recommended Layout

```text
Assets/UserContent/Characters/<CharacterName>/
├── Model/
├── Materials/
├── Textures/
├── Animations/
└── Profiles/
```

## Orientation

- Default forward axis: Unity `+Z`.
- Default up axis: Unity `+Y`.
- If the model faces another direction, correct it in import settings or use a wrapper correction in the generator.

## Animation

The generator can create a shell without clips. For a ready locomotion setup, prepare at least:

- `Idle`
- `Walk`
- `Run`

## Sockets

Recommended socket or bone names:

- `RightHand`
- `LeftHand`
- `WeaponSocket`
- `BackSocket`
- `HeadSocket`
- `VfxRoot`
- `CameraTarget`
- `InteractionPoint`

If these are not found, the generator creates fallback sockets under `CharacterRoot/Sockets`.

## Beginner Guide: Prepare Everything Yourself In Unity

This section is written for designers who are new to Unity. Follow it top-to-bottom once per character.

### 0) Create A Clean Folder For Your Character

Use the recommended layout:

```text
Assets/UserContent/Characters/<CharacterName>/
├── Model/
├── Materials/
├── Textures/
├── Animations/
└── Profiles/
```

Unity imports assets from disk paths. Keeping everything together prevents broken references later.

### 1) Import The Model (`FBX` / `GLB`)

1. Copy your model file into `Assets/UserContent/Characters/<CharacterName>/Model/`.
2. Wait for Unity to import it (progress bar bottom-right).
3. Select the imported model asset in the Project window.
4. In the Inspector, verify:
   - **Rig** tab is configured correctly (next section).
   - **Materials** tab does not reference missing shaders (pink materials).

### 2) Rig Type: Humanoid vs Generic

Most third-party characters are Humanoid-friendly.

#### Humanoid (recommended if available)

1. Open the model importer **Rig** tab.
2. Set **Animation Type** to **Humanoid**.
3. Click **Apply**.
4. Open **Avatar Configuration**:
   - If bones map correctly: click **Done**.
   - If mapping fails: click **Configure...** and assign bones manually (hips/spine/chest/arms/legs).

#### Generic (fallback)

Use Generic when Humanoid mapping is impossible or undesired.

1. Set **Animation Type** to **Generic**.
2. Choose **Avatar** behavior appropriate for your skeleton (often **Create From This Model**).
3. Click **Apply**.

Humanoid vs Generic affects animation retargeting and tooling expectations. Pick one workflow and stay consistent across animation clips.

### 3) Renderer Check (Mesh vs Skinned Mesh)

Open the imported model prefab (double-click the model asset’s prefab icon if Unity shows one).

Verify:

- There is at least one **Mesh Renderer** or **Skinned Mesh Renderer**.
- For skinned meshes, each `SkinnedMeshRenderer` lists **bones** (not empty).

If bones are empty, your FBX export likely lost skin weights or skeleton references.

### 4) Materials And Textures

You have three practical options:

#### Option A: Assign your own Material

1. Put textures into `Textures/`.
2. Create a Material in `Materials/` (`Create → Material`).
3. Assign maps depending on your render pipeline:
   - URP Lit uses `_BaseMap` / `_BaseColor`.
   - Built-in Standard uses `_MainTex` / `_Color`.

#### Option B: Texture-only input for the generator

If you pass only a base texture into the Character Prefab Generator, it will create a simple Lit material automatically (best-effort). This is OK for prototyping, not final art.

#### Option C: Texture embedded in FBX

Sometimes textures import automatically. Still verify in the Inspector that materials are not pink.

### 5) Animations And Clips

Animations may arrive:

- Inside the same FBX as the mesh (common), or
- As separate `.anim` assets.

Minimum locomotion starter set:

- `Idle`
- `Walk`
- `Run`

If you have zero clips, the shell prefab can still be generated, but nothing will animate until you wire an Animator Controller.

### 6) Animator Controller (Minimal)

Create:

1. `Create → Animator Controller`.
2. Open the Animator window.
3. Drag animation clips into states (Idle as default).
4. Add transitions if needed.

Even a single-state Animator is enough to validate rendering + animation playback.

### 7) Avatar Assignment Rules

Humanoid rigs generally produce an Avatar automatically.

If `Animator.avatar` is missing:

- Ensure Rig import succeeded.
- Re-open Avatar Configuration.

### 8) Prefab Variant Workflow (Recommended)

Instead of duplicating meshes:

1. Generate the Character Shell prefab once.
2. Create **Prefab Variants** for Player/Mob/NPC that differ only by profiles/modules/control setup.

### 9) Validation Checklist Before Running The Generator

- Model renders in Scene view (not invisible).
- Materials are not pink.
- Animator plays Idle in Play Mode (optional but strongly recommended).
- Scale looks reasonable (human roughly 1.6–2.0m tall in Unity units unless stylized).

### 10) Common Problems

- **Pink materials**: shader mismatch (HDRP material in URP project or vice versa).
- **T-pose frozen**: Animator Controller missing or no clips assigned.
- **Invisible mesh**: wrong layer/culling rarely; usually missing renderer or disabled object.
- **Broken sockets**: rename bones consistently or rely on generator fallback sockets under `CharacterRoot/Sockets`.

See also:

- [`CharacterGenerator.md`](CharacterGenerator.md)
- [`WritingCharacterModules.md`](WritingCharacterModules.md)
