using System.Collections.Generic;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEngine;

namespace ThreeDSketchKit.Core.Data.Locomotion
{
    [CreateAssetMenu(fileName = "LP_LocomotionProfile", menuName = "3D Sketch Kit/Characters/Locomotion Profile", order = 10)]
    public sealed class LocomotionProfile : ScriptableObject
    {
        [SerializeField] float moveSpeed = 4f;
        [SerializeField] float rotationSpeed = 540f;
        [SerializeField] float acceleration = 24f;
        [SerializeField] bool useRootMotion;
        [SerializeField] List<LocomotionModuleAsset> modules = new();

        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public float Acceleration => acceleration;
        public bool UseRootMotion => useRootMotion;
        public IReadOnlyList<LocomotionModuleAsset> Modules => modules;
    }
}
