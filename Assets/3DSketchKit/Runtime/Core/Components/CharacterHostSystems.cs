using System.Collections.Generic;
using ThreeDSketchKit.Core.Attributes;
using ThreeDSketchKit.Core.Data.Characters;
using ThreeDSketchKit.Core.Data.Combat;
using ThreeDSketchKit.Core.Data.Control;
using ThreeDSketchKit.Core.Data.Equipment;
using ThreeDSketchKit.Core.Data.Interaction;
using ThreeDSketchKit.Core.Data.Inventory;
using ThreeDSketchKit.Core.Data.Locomotion;
using UnityEngine;

namespace ThreeDSketchKit.Core.Components
{
    public sealed class LocomotionSystem : CharacterSystemHost
    {
        [SerializeField] LocomotionProfile profile;
        [SerializeField] List<LocomotionModuleAsset> modules = new();

        public override CharacterSystemKind SystemKind => CharacterSystemKind.Locomotion;
        public LocomotionProfile Profile => profile;

        protected override void OnInitialize(CharacterEntity entity)
        {
            if (profile == null && entity.Preset != null)
                profile = entity.Preset.LocomotionProfile;
            if (profile != null && entity.Movement != null)
                entity.Movement.MoveSpeed = profile.MoveSpeed;
            foreach (var module in EnumerateModules())
                module.Initialize(entity);
        }

        protected override void OnShutdown()
        {
            foreach (var module in EnumerateModules())
                module.Shutdown();
        }

        public override void ValidateHost(CharacterModuleValidationReport report)
        {
            if (Entity != null && Entity.Movement == null)
                report.Warning($"{nameof(LocomotionSystem)} has no MovementComponent dependency.");
            foreach (var module in EnumerateModules())
                module.Validate(Entity, report);
        }

        IEnumerable<LocomotionModuleAsset> EnumerateModules()
        {
            if (profile != null)
            {
                foreach (var module in profile.Modules)
                    if (module != null)
                        yield return module;
            }

            foreach (var module in modules)
                if (module != null)
                    yield return module;
        }
    }

    public sealed class AbilitySystem : CharacterSystemHost
    {
        [SerializeField] AbilityLoadout loadout;
        [SerializeField] List<AbilityModuleAsset> modules = new();

        public override CharacterSystemKind SystemKind => CharacterSystemKind.Ability;
        public AbilityLoadout Loadout => loadout;
        public AbilityManager AbilityManager => Entity != null ? Entity.AbilityManager : null;

        public void PerformAllAbilities() => AbilityManager?.PerformAllActions();
        public void PerformAbility(string abilityName) => AbilityManager?.PerformByName(abilityName);

        protected override void OnInitialize(CharacterEntity entity)
        {
            if (loadout == null && entity.Preset != null)
                loadout = entity.Preset.AbilityLoadout;
            foreach (var module in EnumerateModules())
                module.Initialize(entity);
        }

        protected override void OnShutdown()
        {
            foreach (var module in EnumerateModules())
                module.Shutdown();
        }

        public override void ValidateHost(CharacterModuleValidationReport report)
        {
            if (Entity != null && Entity.AbilityManager == null)
                report.Warning($"{nameof(AbilitySystem)} has no AbilityManager dependency.");
            foreach (var module in EnumerateModules())
                module.Validate(Entity, report);
        }

        IEnumerable<AbilityModuleAsset> EnumerateModules()
        {
            if (loadout != null)
            {
                foreach (var module in loadout.Modules)
                    if (module != null)
                        yield return module;
            }

            foreach (var module in modules)
                if (module != null)
                    yield return module;
        }
    }

    public sealed class CombatSystem : CharacterSystemHost
    {
        [SerializeField] CombatProfile profile;
        [SerializeField] List<CombatModuleAsset> modules = new();

        public override CharacterSystemKind SystemKind => CharacterSystemKind.Combat;
        public CombatProfile Profile => profile;

        protected override void OnInitialize(CharacterEntity entity)
        {
            if (profile == null && entity.Preset != null)
                profile = entity.Preset.CombatProfile;
            foreach (var module in EnumerateModules())
                module.Initialize(entity);
        }

        protected override void OnShutdown()
        {
            foreach (var module in EnumerateModules())
                module.Shutdown();
        }

        public override void ValidateHost(CharacterModuleValidationReport report)
        {
            foreach (var module in EnumerateModules())
                module.Validate(Entity, report);
        }

        IEnumerable<CombatModuleAsset> EnumerateModules()
        {
            if (profile != null)
            {
                foreach (var module in profile.Modules)
                    if (module != null)
                        yield return module;
            }

            foreach (var module in modules)
                if (module != null)
                    yield return module;
        }
    }

    public sealed class InventorySystem : CharacterSystemHost
    {
        [SerializeField] InventoryProfile profile;
        [SerializeField] List<InventoryModuleAsset> modules = new();

        public override CharacterSystemKind SystemKind => CharacterSystemKind.Inventory;
        public InventoryProfile Profile => profile;

        protected override void OnInitialize(CharacterEntity entity)
        {
            if (profile == null && entity.Preset != null)
                profile = entity.Preset.InventoryProfile;
            foreach (var module in EnumerateModules())
                module.Initialize(entity);
        }

        protected override void OnShutdown()
        {
            foreach (var module in EnumerateModules())
                module.Shutdown();
        }

        public override void ValidateHost(CharacterModuleValidationReport report)
        {
            foreach (var module in EnumerateModules())
                module.Validate(Entity, report);
        }

        IEnumerable<InventoryModuleAsset> EnumerateModules()
        {
            if (profile != null)
            {
                foreach (var module in profile.Modules)
                    if (module != null)
                        yield return module;
            }

            foreach (var module in modules)
                if (module != null)
                    yield return module;
        }
    }

    public sealed class EquipmentSystem : CharacterSystemHost
    {
        [SerializeField] EquipmentProfile profile;
        [SerializeField] List<EquipmentModuleAsset> modules = new();

        public override CharacterSystemKind SystemKind => CharacterSystemKind.Equipment;
        public EquipmentProfile Profile => profile;

        protected override void OnInitialize(CharacterEntity entity)
        {
            if (profile == null && entity.Preset != null)
                profile = entity.Preset.EquipmentProfile;
            foreach (var module in EnumerateModules())
                module.Initialize(entity);
        }

        protected override void OnShutdown()
        {
            foreach (var module in EnumerateModules())
                module.Shutdown();
        }

        public override void ValidateHost(CharacterModuleValidationReport report)
        {
            if (profile != null && Entity != null)
            {
                foreach (var slot in profile.Slots)
                {
                    if (!Entity.TryGetSocket(slot.SocketId, out _))
                        report.Warning($"{nameof(EquipmentSystem)} cannot find socket '{slot.SocketId}' for slot '{slot.SlotId}'.");
                }
            }

            foreach (var module in EnumerateModules())
                module.Validate(Entity, report);
        }

        IEnumerable<EquipmentModuleAsset> EnumerateModules()
        {
            if (profile != null)
            {
                foreach (var module in profile.Modules)
                    if (module != null)
                        yield return module;
            }

            foreach (var module in modules)
                if (module != null)
                    yield return module;
        }
    }

    public sealed class ControlSystem : CharacterSystemHost
    {
        [SerializeField] ControlProfile profile;
        [SerializeField] List<ControlModuleAsset> modules = new();

        public override CharacterSystemKind SystemKind => CharacterSystemKind.Control;
        public ControlProfile Profile => profile;

        protected override void OnInitialize(CharacterEntity entity)
        {
            if (profile == null && entity.Preset != null)
                profile = entity.Preset.ControlProfile;
            foreach (var module in EnumerateModules())
                module.Initialize(entity);
        }

        protected override void OnShutdown()
        {
            foreach (var module in EnumerateModules())
                module.Shutdown();
        }

        public override void ValidateHost(CharacterModuleValidationReport report)
        {
            foreach (var module in EnumerateModules())
                module.Validate(Entity, report);
        }

        IEnumerable<ControlModuleAsset> EnumerateModules()
        {
            if (profile != null)
            {
                foreach (var module in profile.Modules)
                    if (module != null)
                        yield return module;
            }

            foreach (var module in modules)
                if (module != null)
                    yield return module;
        }
    }

    public sealed class InteractableSystem : CharacterSystemHost
    {
        [SerializeField] InteractionProfile profile;
        [SerializeField] List<InteractableModuleAsset> modules = new();

        public override CharacterSystemKind SystemKind => CharacterSystemKind.Interactable;
        public InteractionProfile Profile => profile;
        public string Prompt => profile != null ? profile.Prompt : "Interact";

        protected override void OnInitialize(CharacterEntity entity)
        {
            if (profile == null && entity.Preset != null)
                profile = entity.Preset.InteractionProfile;
            foreach (var module in EnumerateModules())
                module.Initialize(entity);
        }

        protected override void OnShutdown()
        {
            foreach (var module in EnumerateModules())
                module.Shutdown();
        }

        public override void ValidateHost(CharacterModuleValidationReport report)
        {
            foreach (var module in EnumerateModules())
                module.Validate(Entity, report);
        }

        IEnumerable<InteractableModuleAsset> EnumerateModules()
        {
            if (profile != null)
            {
                foreach (var module in profile.Modules)
                    if (module != null)
                        yield return module;
            }

            foreach (var module in modules)
                if (module != null)
                    yield return module;
        }
    }
}
