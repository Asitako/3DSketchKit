using System.Collections.Generic;
using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Modules.Zones
{
    public sealed class DamageZoneEffect : IZoneEffect
    {
        readonly float _tickInterval;
        readonly float _damagePerTick;
        readonly Dictionary<int, float> _nextTickTime = new();

        public DamageZoneEffect(float tickInterval, float damagePerTick)
        {
            _tickInterval = Mathf.Max(0.01f, tickInterval);
            _damagePerTick = Mathf.Max(0f, damagePerTick);
        }

        public void OnMemberEntered(ZoneEffectSubject subject) => ResetTick(subject);

        public void OnMemberStaying(ZoneEffectSubject subject, float deltaTime)
        {
            if (subject?.GameObject == null)
                return;

            var gameObjectInstanceId = subject.GameObject.GetInstanceID();
            if (!_nextTickTime.TryGetValue(gameObjectInstanceId, out var nextDamageAllowedTime))
                nextDamageAllowedTime = Time.time;

            if (Time.time < nextDamageAllowedTime)
                return;

            var healthComponent = subject.GetComponent<HealthComponent>();
            if (healthComponent != null)
                healthComponent.TakeDamage(_damagePerTick);

            _nextTickTime[gameObjectInstanceId] = Time.time + _tickInterval;
        }

        public void OnMemberExited(ZoneEffectSubject subject)
        {
            if (subject?.GameObject == null)
                return;
            _nextTickTime.Remove(subject.GameObject.GetInstanceID());
        }

        void ResetTick(ZoneEffectSubject subject)
        {
            if (subject?.GameObject == null)
                return;
            _nextTickTime[subject.GameObject.GetInstanceID()] = Time.time;
        }
    }
}
