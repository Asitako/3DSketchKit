using System;

namespace ThreeDSketchKit.Core.Interfaces
{
    public interface IDamageable
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        void TakeDamage(float amount);
        event Action OnDeath;
    }
}
