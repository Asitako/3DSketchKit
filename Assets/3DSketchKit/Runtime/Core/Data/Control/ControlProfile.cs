using System.Collections.Generic;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEngine;

namespace ThreeDSketchKit.Core.Data.Control
{
    [CreateAssetMenu(fileName = "CP_ControlProfile", menuName = "3D Sketch Kit/Characters/Control Profile", order = 15)]
    public sealed class ControlProfile : ScriptableObject
    {
        [SerializeField] ControlMode mode = ControlMode.Passive;
        [SerializeField] float perceptionRange = 8f;
        [SerializeField] float chaseRange = 12f;
        [SerializeField] List<ControlModuleAsset> modules = new();

        public ControlMode Mode => mode;
        public float PerceptionRange => perceptionRange;
        public float ChaseRange => chaseRange;
        public IReadOnlyList<ControlModuleAsset> Modules => modules;
    }

    public enum ControlMode
    {
        Passive,
        PlayerInput,
        AI,
        Scripted
    }
}
