using System.Collections.Generic;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEngine;

namespace ThreeDSketchKit.Core.Data.Interaction
{
    [CreateAssetMenu(fileName = "IP_InteractionProfile", menuName = "3D Sketch Kit/Characters/Interaction Profile", order = 16)]
    public sealed class InteractionProfile : ScriptableObject
    {
        [SerializeField] string prompt = "Interact";
        [SerializeField] float interactionRadius = 2f;
        [SerializeField] List<InteractableModuleAsset> modules = new();

        public string Prompt => prompt;
        public float InteractionRadius => interactionRadius;
        public IReadOnlyList<InteractableModuleAsset> Modules => modules;
    }
}
