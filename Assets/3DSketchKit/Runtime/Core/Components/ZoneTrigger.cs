using System.Collections.Generic;
using ThreeDSketchKit.Core.Data;
using ThreeDSketchKit.Core.Interfaces;
using ThreeDSketchKit.Utility;
using UnityEngine;

namespace ThreeDSketchKit.Core.Components
{
    [RequireComponent(typeof(Collider))]
    public sealed class ZoneTrigger : MonoBehaviour
    {
        [SerializeField] List<ZoneEffectData> effectDefinitions = new();
        [SerializeField] bool rebuildOnEnable = true;

        readonly List<IZoneEffect> _effects = new();

        void OnEnable()
        {
            var zoneCollider = GetComponent<Collider>();
            zoneCollider.isTrigger = true;
            if (rebuildOnEnable)
                RebuildEffects();
        }

        public void RebuildEffects()
        {
            _effects.Clear();
            foreach (var effectDefinition in effectDefinitions)
            {
                if (effectDefinition == null)
                    continue;
                _effects.Add(ZoneEffectFactory.Create(effectDefinition));
            }
        }

        public IReadOnlyList<IZoneEffect> Effects => _effects;

        void OnTriggerEnter(Collider otherCollider)
        {
            var zoneSubject = new ZoneEffectSubject(otherCollider.attachedRigidbody != null ? otherCollider.attachedRigidbody.gameObject : otherCollider.gameObject, otherCollider);
            foreach (var zoneEffect in _effects)
                zoneEffect.OnMemberEntered(zoneSubject);
        }

        void OnTriggerStay(Collider otherCollider)
        {
            var deltaTime = Time.deltaTime;
            var zoneSubject = new ZoneEffectSubject(otherCollider.attachedRigidbody != null ? otherCollider.attachedRigidbody.gameObject : otherCollider.gameObject, otherCollider);
            foreach (var zoneEffect in _effects)
                zoneEffect.OnMemberStaying(zoneSubject, deltaTime);
        }

        void OnTriggerExit(Collider otherCollider)
        {
            var zoneSubject = new ZoneEffectSubject(otherCollider.attachedRigidbody != null ? otherCollider.attachedRigidbody.gameObject : otherCollider.gameObject, otherCollider);
            foreach (var zoneEffect in _effects)
                zoneEffect.OnMemberExited(zoneSubject);
        }
    }
}
