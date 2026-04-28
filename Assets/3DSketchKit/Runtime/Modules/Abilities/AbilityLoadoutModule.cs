using ThreeDSketchKit.Core.Attributes;
using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEngine;

namespace ThreeDSketchKit.Modules.Abilities
{
    [CreateAssetMenu(fileName = "AM_AbilityLoadout", menuName = "3D Sketch Kit/Modules/Abilities/Ability Loadout", order = 120)]
    [CharacterSystemModule(CharacterSystemKind.Ability, "Ability Loadout", Description = "Starter module that validates AbilityManager availability.")]
    public sealed class AbilityLoadoutModule : AbilityModuleAsset
    {
        public override bool Validate(CharacterEntity entity, CharacterModuleValidationReport report)
        {
            base.Validate(entity, report);
            if (entity != null && entity.AbilityManager == null)
                report.Warning($"{name}: Ability loadout requires AbilityManager.");
            return !report.HasErrors;
        }
    }
}
