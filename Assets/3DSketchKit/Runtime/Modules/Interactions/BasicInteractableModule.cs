using ThreeDSketchKit.Core.Attributes;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEngine;

namespace ThreeDSketchKit.Modules.Interactions
{
    [CreateAssetMenu(fileName = "IM_BasicInteractable", menuName = "3D Sketch Kit/Modules/Interactions/Basic Interactable", order = 130)]
    [CharacterSystemModule(CharacterSystemKind.Interactable, "Basic Interactable", Description = "Starter interaction module for generated shell prefabs.")]
    public sealed class BasicInteractableModule : InteractableModuleAsset
    {
    }
}
