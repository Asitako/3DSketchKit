using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Core.Data.Characters
{
    public abstract class CharacterModuleAsset : ScriptableObject, ICharacterSystemModule
    {
        [SerializeField] string moduleId;

        public virtual string ModuleId => string.IsNullOrWhiteSpace(moduleId) ? GetType().FullName : moduleId;

        public virtual void Initialize(CharacterEntity entity) {}
        public virtual void Shutdown() {}

        public virtual bool Validate(CharacterEntity entity, CharacterModuleValidationReport report)
        {
            if (entity == null)
            {
                report.Error($"{name}: CharacterEntity is missing.");
                return false;
            }

            return true;
        }
    }

    public abstract class LocomotionModuleAsset : CharacterModuleAsset, ILocomotionModule {}
    public abstract class AbilityModuleAsset : CharacterModuleAsset, IAbilityModule {}
    public abstract class CombatModuleAsset : CharacterModuleAsset, ICombatModule {}
    public abstract class InventoryModuleAsset : CharacterModuleAsset, IInventoryModule {}
    public abstract class EquipmentModuleAsset : CharacterModuleAsset, IEquipmentModule {}
    public abstract class ControlModuleAsset : CharacterModuleAsset, IControlModule {}
    public abstract class InteractableModuleAsset : CharacterModuleAsset, IInteractableModule {}
}
