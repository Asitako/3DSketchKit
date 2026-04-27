using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Data;
using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Modules.Abilities
{
    public sealed class JumpAbility : IAbility, IAbilityLifecycle
    {
        IAbilityHost _host;
        float _impulse = 6f;

        public string AbilityName => "Jump";
        public bool IsActive { get; set; } = true;

        public void OnAttached(IAbilityHost host, AbilityData config)
        {
            _host = host;
            if (config != null)
                _impulse = config.GetFloat("jumpImpulse", 6f);
        }

        public void PerformAction()
        {
            if (_host == null || !IsActive)
                return;

            var rigidbody = _host.GetDependency<Rigidbody>();
            if (rigidbody != null && !rigidbody.isKinematic)
            {
                var updatedLinearVelocity = rigidbody.linearVelocity;
                updatedLinearVelocity.y = _impulse;
                rigidbody.linearVelocity = updatedLinearVelocity;
                return;
            }

            var movableMotor = _host.GetDependency<MovementComponent>() as IMovable;
            movableMotor?.SetDesiredVelocity(Vector3.up * _impulse);
        }
    }
}
