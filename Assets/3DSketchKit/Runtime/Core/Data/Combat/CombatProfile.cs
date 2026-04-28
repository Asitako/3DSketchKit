using System.Collections.Generic;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEngine;

namespace ThreeDSketchKit.Core.Data.Combat
{
    [CreateAssetMenu(fileName = "CP_CombatProfile", menuName = "3D Sketch Kit/Characters/Combat Profile", order = 12)]
    public sealed class CombatProfile : ScriptableObject
    {
        [SerializeField] float baseDamage = 10f;
        [SerializeField] float attackRange = 1.5f;
        [SerializeField] List<CombatModuleAsset> modules = new();

        public float BaseDamage => baseDamage;
        public float AttackRange => attackRange;
        public IReadOnlyList<CombatModuleAsset> Modules => modules;
    }
}
