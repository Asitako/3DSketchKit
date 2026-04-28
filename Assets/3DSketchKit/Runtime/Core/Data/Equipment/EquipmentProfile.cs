using System;
using System.Collections.Generic;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEngine;

namespace ThreeDSketchKit.Core.Data.Equipment
{
    [CreateAssetMenu(fileName = "EP_EquipmentProfile", menuName = "3D Sketch Kit/Characters/Equipment Profile", order = 14)]
    public sealed class EquipmentProfile : ScriptableObject
    {
        [SerializeField] List<EquipmentSlotDefinition> slots = new();
        [SerializeField] List<EquipmentModuleAsset> modules = new();

        public IReadOnlyList<EquipmentSlotDefinition> Slots => slots;
        public IReadOnlyList<EquipmentModuleAsset> Modules => modules;
    }

    [Serializable]
    public sealed class EquipmentSlotDefinition
    {
        [SerializeField] string slotId = "Weapon";
        [SerializeField] string socketId = "WeaponSocket";
        [SerializeField] string allowedCategory = "Weapon";

        public string SlotId => slotId;
        public string SocketId => socketId;
        public string AllowedCategory => allowedCategory;
    }
}
