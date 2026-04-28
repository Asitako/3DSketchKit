# Character System Hosts Architecture

Generated character prefabs use a shell-plus-hosts architecture.

## Shell

`CharacterEntity` owns identity, core references, sockets and typed access to host systems. It must not contain gameplay logic.

## Hosts

- `LocomotionSystem`: movement mode and movement modules.
- `AbilitySystem`: loadout, unlocks and ability activation through `AbilityManager`.
- `CombatSystem`: attacks, hit detection and damage routing.
- `InventorySystem`: item containers and storage rules.
- `EquipmentSystem`: equipment slots and socket binding.
- `ControlSystem`: player input, AI, scripted or passive control.
- `InteractableSystem`: logical interaction layer such as dialogue, trade, loot and combat targeting.

## Boundaries

- AI chooses intent; locomotion executes movement.
- Control requests actions; combat and ability systems execute them.
- Inventory stores items; equipment equips them.
- Interactable exposes interaction options; combat resolves damage.
- Progression unlocks capabilities through public APIs.
- Generator creates structure; runtime systems define behavior.

## Beginner Guide: What To Click After Generation

Think of the prefab as a workspace:

- **`CharacterEntity`**: identity + references + sockets registry.
- **`Systems/*`**: independent knobs you tune per role (player/mob/NPC).

### 1) Locate Host Systems On The Prefab

Open your generated prefab (double-click). Expand:

```text
CharacterRoot/
└── Systems/
```

Each child component (`LocomotionSystem`, `CombatSystem`, ...) is one domain.

### 2) Profiles Drive Variation

Every role variation should primarily differ by:

- `CharacterPreset`
- referenced profiles (`LocomotionProfile`, `CombatProfile`, ...)

Recommended workflow:

1. Duplicate profile assets (`Ctrl+D`) when creating variants.
2. Rename duplicates clearly (`LP_Player`, `LP_Mob`).
3. Assign profiles either:
   - directly on hosts (temporary), or
   - indirectly via `CharacterPreset` referenced by `CharacterEntity`.

### 3) Modules Attach Through Profiles First

Put gameplay behaviors into typed modules referenced by profiles.

Avoid stuffing unrelated behaviors into `CharacterEntity`.

### 4) Practical Wiring Examples

#### Player Character

Typical priorities:

1. `ControlSystem` → player control modules/profiles.
2. `LocomotionSystem` → locomotion profile tuned for responsiveness.
3. `InteractableSystem` → interactions relevant for players.

#### Mob Enemy

Typical priorities:

1. `ControlSystem` → AI-oriented profile/modules (later gameplay packs).
2. `CombatSystem` → targeting/hit logic modules (later gameplay packs).
3. `InteractableSystem` → loot/combat-target hooks.

#### NPC

Typical priorities:

1. `ControlSystem` → passive/scripted.
2. `InteractableSystem` → dialogue/trade/quest hooks.

### 5) Avoid Cross-Wiring Mistakes

If something belongs to combat, keep it under `CombatSystem` profiles/modules—not inside inventory hosts.

See also:

- [`WritingCharacterModules.md`](WritingCharacterModules.md)
- [`CharacterGenerator.md`](CharacterGenerator.md)
