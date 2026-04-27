using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Core.Components
{
    /// <summary>
    /// Motor: Rigidbody (non-kinematic), else CharacterController, else Transform.
    /// </summary>
    public sealed class MovementComponent : MonoBehaviour, IMovable
    {
        [SerializeField] float moveSpeed = 4f;
        CharacterController _controller;
        Rigidbody _rigidbody;
        Vector3 _desiredVelocity;

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void SetDesiredVelocity(Vector3 velocity) => _desiredVelocity = velocity;

        void FixedUpdate()
        {
            if (_rigidbody != null && !_rigidbody.isKinematic)
            {
                var combinedVelocity = _desiredVelocity;
                combinedVelocity.y = _rigidbody.linearVelocity.y;
                _rigidbody.linearVelocity = combinedVelocity;
            }
            else if (_controller != null && _controller.enabled)
            {
                _controller.Move(_desiredVelocity * Time.fixedDeltaTime);
            }
            else
            {
                transform.position += _desiredVelocity * Time.fixedDeltaTime;
            }

            _desiredVelocity = Vector3.zero;
        }
    }
}
