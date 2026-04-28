using ThreeDSketchKit.Core.Components;

namespace ThreeDSketchKit.Core.Interfaces
{
    public interface ICharacterSystemModule
    {
        string ModuleId { get; }
        void Initialize(CharacterEntity entity);
        void Shutdown();
        bool Validate(CharacterEntity entity, CharacterModuleValidationReport report);
    }

    public interface ILocomotionModule : ICharacterSystemModule {}
    public interface IAbilityModule : ICharacterSystemModule {}
    public interface ICombatModule : ICharacterSystemModule {}
    public interface IInventoryModule : ICharacterSystemModule {}
    public interface IEquipmentModule : ICharacterSystemModule {}
    public interface IControlModule : ICharacterSystemModule {}
    public interface IInteractableModule : ICharacterSystemModule {}
}
