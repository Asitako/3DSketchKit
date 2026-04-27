using UnityEngine;

namespace ThreeDSketchKit.Core.Interfaces
{
    public interface IMovable
    {
        float MoveSpeed { get; set; }
        /// <summary>World-space velocity in units per second for the motor.</summary>
        void SetDesiredVelocity(Vector3 velocity);
    }
}
