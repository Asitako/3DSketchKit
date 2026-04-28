using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Data.Combat;
using ThreeDSketchKit.Core.Data.Control;
using ThreeDSketchKit.Core.Data.Equipment;
using ThreeDSketchKit.Core.Data.Interaction;
using ThreeDSketchKit.Core.Data.Inventory;
using ThreeDSketchKit.Core.Data.Locomotion;
using UnityEngine;

namespace ThreeDSketchKit.Core.Data.Characters
{
    [CreateAssetMenu(fileName = "CP_CharacterPreset", menuName = "3D Sketch Kit/Characters/Character Preset", order = 0)]
    public sealed class CharacterPreset : ScriptableObject
    {
        [SerializeField] CharacterEntityRole role = CharacterEntityRole.Neutral;
        [SerializeField] LocomotionProfile locomotionProfile;
        [SerializeField] AbilityLoadout abilityLoadout;
        [SerializeField] CombatProfile combatProfile;
        [SerializeField] InventoryProfile inventoryProfile;
        [SerializeField] EquipmentProfile equipmentProfile;
        [SerializeField] ControlProfile controlProfile;
        [SerializeField] InteractionProfile interactionProfile;

        public CharacterEntityRole Role => role;
        public LocomotionProfile LocomotionProfile => locomotionProfile;
        public AbilityLoadout AbilityLoadout => abilityLoadout;
        public CombatProfile CombatProfile => combatProfile;
        public InventoryProfile InventoryProfile => inventoryProfile;
        public EquipmentProfile EquipmentProfile => equipmentProfile;
        public ControlProfile ControlProfile => controlProfile;
        public InteractionProfile InteractionProfile => interactionProfile;

        public void Configure(
            CharacterEntityRole newRole,
            LocomotionProfile newLocomotionProfile,
            AbilityLoadout newAbilityLoadout,
            CombatProfile newCombatProfile,
            InventoryProfile newInventoryProfile,
            EquipmentProfile newEquipmentProfile,
            ControlProfile newControlProfile,
            InteractionProfile newInteractionProfile)
        {
            role = newRole;
            locomotionProfile = newLocomotionProfile;
            abilityLoadout = newAbilityLoadout;
            combatProfile = newCombatProfile;
            inventoryProfile = newInventoryProfile;
            equipmentProfile = newEquipmentProfile;
            controlProfile = newControlProfile;
            interactionProfile = newInteractionProfile;
        }
    }
}
