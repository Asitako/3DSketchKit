using System;
using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Core.Components
{
    public sealed class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHealth = 100f;
        [SerializeField] float currentHealth = 100f;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        public event Action OnDeath;

        void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || currentHealth <= 0f)
                return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            if (currentHealth <= 0f)
                OnDeath?.Invoke();
        }

        public void Heal(float amount)
        {
            if (amount <= 0f)
                return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        public void SetMaxHealth(float value, bool refill = false)
        {
            maxHealth = Mathf.Max(1f, value);
            if (refill)
                currentHealth = maxHealth;
            else
                currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
    }
}
