using NUnit.Framework;
using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Data.Characters;
using ThreeDSketchKit.Modules.Abilities;
using ThreeDSketchKit.Modules.Control;
using ThreeDSketchKit.Modules.Interactions;
using ThreeDSketchKit.Modules.Locomotion;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Tests
{
    public sealed class CharacterArchitectureTests
    {
        [Test]
        public void StarterModules_TargetOnlyTheirTypedSystemContracts()
        {
            Assert.IsInstanceOf<LocomotionModuleAsset>(ScriptableObject.CreateInstance<SimpleLocomotionModule>());
            Assert.IsInstanceOf<AbilityModuleAsset>(ScriptableObject.CreateInstance<AbilityLoadoutModule>());
            Assert.IsInstanceOf<ControlModuleAsset>(ScriptableObject.CreateInstance<PassiveControlModule>());
            Assert.IsInstanceOf<InteractableModuleAsset>(ScriptableObject.CreateInstance<BasicInteractableModule>());
        }

        [Test]
        public void CharacterShell_CanRegisterAllDefaultHostSystems()
        {
            var root = new GameObject("CharacterRoot");
            try
            {
                root.AddComponent<CharacterEntity>();
                root.AddComponent<AbilityManager>();
                root.AddComponent<MovementComponent>();
                root.AddComponent<HealthComponent>();

                var systems = new GameObject("Systems");
                systems.transform.SetParent(root.transform, false);
                systems.AddComponent<LocomotionSystem>();
                systems.AddComponent<AbilitySystem>();
                systems.AddComponent<CombatSystem>();
                systems.AddComponent<InventorySystem>();
                systems.AddComponent<EquipmentSystem>();
                systems.AddComponent<ControlSystem>();
                systems.AddComponent<InteractableSystem>();

                var entity = root.GetComponent<CharacterEntity>();
                var report = entity.ValidateSystems();

                Assert.IsFalse(report.HasErrors);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
