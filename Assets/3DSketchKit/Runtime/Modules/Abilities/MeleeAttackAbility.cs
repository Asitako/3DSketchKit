using System.Collections.Generic;
using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Data;
using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Modules.Abilities
{
    public sealed class MeleeAttackAbility : IAbility, IAbilityLifecycle
    {
        IAbilityHost _host;
        float _radius = 1.5f;
        float _damage = 15f;
        LayerMask _mask = ~0;

        public string AbilityName => "MeleeAttack";
        public bool IsActive { get; set; } = true;

        public void OnAttached(IAbilityHost host, AbilityData config)
        {
            _host = host;
            if (config == null)
                return;
            _radius = config.GetFloat("radius", _radius);
            _damage = config.GetFloat("damage", _damage);
        }

        public void PerformAction()
        {
            if (_host == null || !IsActive)
                return;

            var attackOrigin = _host.Owner.transform.position;
            var overlappingColliders = Physics.OverlapSphere(attackOrigin, _radius, _mask, QueryTriggerInteraction.Ignore);
            var damagedInstanceIds = new HashSet<int>();
            foreach (var hitCollider in overlappingColliders)
            {
                var healthComponent = hitCollider.GetComponentInParent<HealthComponent>();
                if (healthComponent == null)
                    continue;
                var healthInstanceId = healthComponent.GetInstanceID();
                if (!damagedInstanceIds.Add(healthInstanceId))
                    continue;
                if (healthComponent.gameObject == _host.Owner)
                    continue;
                healthComponent.TakeDamage(_damage);
            }
        }
    }
}
