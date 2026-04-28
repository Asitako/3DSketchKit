using System.Collections.Generic;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEngine;

namespace ThreeDSketchKit.Core.Data.Inventory
{
    [CreateAssetMenu(fileName = "IP_InventoryProfile", menuName = "3D Sketch Kit/Characters/Inventory Profile", order = 13)]
    public sealed class InventoryProfile : ScriptableObject
    {
        [SerializeField] InventoryMode mode = InventoryMode.Slots;
        [SerializeField] int capacity = 16;
        [SerializeField] float maxWeight = 50f;
        [SerializeField] List<InventoryModuleAsset> modules = new();

        public InventoryMode Mode => mode;
        public int Capacity => capacity;
        public float MaxWeight => maxWeight;
        public IReadOnlyList<InventoryModuleAsset> Modules => modules;
    }

    public enum InventoryMode
    {
        Slots,
        Grid,
        WeightOnly
    }
}
