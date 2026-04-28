using ThreeDSketchKit.Core.Attributes;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEngine;

namespace ThreeDSketchKit.Modules.Control
{
    [CreateAssetMenu(fileName = "CM_PassiveControl", menuName = "3D Sketch Kit/Modules/Control/Passive Control", order = 110)]
    [CharacterSystemModule(CharacterSystemKind.Control, "Passive Control", Description = "Starter control module for NPCs or neutral entities that do not drive themselves.")]
    public sealed class PassiveControlModule : ControlModuleAsset
    {
    }
}
