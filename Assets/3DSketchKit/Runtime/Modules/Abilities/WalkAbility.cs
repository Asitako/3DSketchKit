using ThreeDSketchKit.Core;
using ThreeDSketchKit.Core.Attributes;
using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Data;
using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Modules.Abilities
{
    [SketchKitAbilityId(SketchKitBuiltInAbilityIds.Walk)]
    public sealed class WalkAbility : ITickableAbility, IAbilityLifecycle
    {
        IAbilityHost _host;
        IMovable _movable;
        AbilityData _config;
        /// <summary>
        /// Typically <see cref="Camera.main"/>: used once, then reused to map movement input onto world XZ
        /// relative to the camera view (avoids calling <c>Camera.main</c> every frame).
        /// </summary>
        Camera _cachedMainCameraForWalkInput;

        public string AbilityName => "Walk";
        public bool IsActive { get; set; } = true;

        public void OnAttached(IAbilityHost host, AbilityData config)
        {
            _host = host;
            _config = config;
            _movable = host.GetDependency<MovementComponent>() as IMovable;
            if (_movable != null && _config != null)
                _movable.MoveSpeed = _config.GetFloat("moveSpeed", _movable.MoveSpeed);
        }

        public void Tick(float deltaTime)
        {
            if (_host == null || _movable == null || !IsActive)
                return;

            var movementInput = _host.MovementInput;
            if (_cachedMainCameraForWalkInput == null)
                _cachedMainCameraForWalkInput = Camera.main;

            var mainCameraForWalkInput = _cachedMainCameraForWalkInput;
            Vector3 forward;
            Vector3 right;
            if (mainCameraForWalkInput != null)
            {
                forward = mainCameraForWalkInput.transform.forward;
                right = mainCameraForWalkInput.transform.right;
            }
            else
            {
                forward = Vector3.forward;
                right = Vector3.right;
            }

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            var worldMoveDirection = forward * movementInput.y + right * movementInput.x;
            if (worldMoveDirection.sqrMagnitude > 1f)
                worldMoveDirection.Normalize();

            _movable.SetDesiredVelocity(worldMoveDirection * _movable.MoveSpeed);
        }

        public void PerformAction()
        {
            // Locomotion is driven by Tick; discrete action unused.
        }
    }
}
