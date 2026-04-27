using System.Collections.Generic;
using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Modules.Zones
{
    public sealed class StealthZoneEffect : IZoneEffect
    {
        readonly int _stealthLayer;
        readonly Dictionary<int, int> _originalLayers = new();

        public StealthZoneEffect(int stealthLayerIndex)
        {
            _stealthLayer = Mathf.Clamp(stealthLayerIndex, 0, 31);
        }

        public void OnMemberEntered(ZoneEffectSubject subject)
        {
            if (subject?.GameObject == null)
                return;

            var subjectGameObject = subject.GameObject;
            var gameObjectInstanceId = subjectGameObject.GetInstanceID();
            if (!_originalLayers.ContainsKey(gameObjectInstanceId))
                _originalLayers[gameObjectInstanceId] = subjectGameObject.layer;
            subjectGameObject.layer = _stealthLayer;
        }

        public void OnMemberStaying(ZoneEffectSubject subject, float deltaTime) { }

        public void OnMemberExited(ZoneEffectSubject subject)
        {
            if (subject?.GameObject == null)
                return;

            var gameObjectInstanceId = subject.GameObject.GetInstanceID();
            if (_originalLayers.TryGetValue(gameObjectInstanceId, out var originalLayerIndex))
            {
                subject.GameObject.layer = originalLayerIndex;
                _originalLayers.Remove(gameObjectInstanceId);
            }
        }
    }
}
