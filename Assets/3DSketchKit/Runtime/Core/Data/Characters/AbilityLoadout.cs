using System.Collections.Generic;
using UnityEngine;

namespace ThreeDSketchKit.Core.Data.Characters
{
    [CreateAssetMenu(fileName = "AL_AbilityLoadout", menuName = "3D Sketch Kit/Characters/Ability Loadout", order = 11)]
    public sealed class AbilityLoadout : ScriptableObject
    {
        [SerializeField] List<AbilityData> startingAbilities = new();
        [SerializeField] List<AbilityData> unlockableAbilities = new();
        [SerializeField] List<AbilityModuleAsset> modules = new();

        public IReadOnlyList<AbilityData> StartingAbilities => startingAbilities;
        public IReadOnlyList<AbilityData> UnlockableAbilities => unlockableAbilities;
        public IReadOnlyList<AbilityModuleAsset> Modules => modules;
    }
}
