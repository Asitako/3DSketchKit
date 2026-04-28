using ThreeDSketchKit.Core.Attributes;
using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEngine;

namespace ThreeDSketchKit.Modules.Locomotion
{
    [CreateAssetMenu(fileName = "LM_SimpleLocomotion", menuName = "3D Sketch Kit/Modules/Locomotion/Simple Locomotion", order = 100)]
    [CharacterSystemModule(CharacterSystemKind.Locomotion, "Simple Locomotion", Description = "Starter locomotion module that validates MovementComponent availability.")]
    public sealed class SimpleLocomotionModule : LocomotionModuleAsset
    {
        public override bool Validate(CharacterEntity entity, CharacterModuleValidationReport report)
        {
            base.Validate(entity, report);
            if (entity != null && entity.Movement == null)
                report.Warning($"{name}: Simple locomotion requires MovementComponent.");
            return !report.HasErrors;
        }
    }
}
