using System.Collections.Generic;
using ThreeDSketchKit.Core.Data;
using ThreeDSketchKit.Core.Interfaces;
using UnityEngine;

namespace ThreeDSketchKit.Core.Components
{
    /// <summary>
    /// Default <see cref="IEffectable"/> implementation for prototyping buff zones.
    /// </summary>
    public sealed class EffectReceiverComponent : MonoBehaviour, IEffectable
    {
        readonly List<EffectData> _active = new();

        public IReadOnlyList<EffectData> ActiveEffects => _active;

        public void ApplyEffect(EffectData data)
        {
            if (data == null)
                return;
            _active.Add(data.Clone());
        }

        public void RemoveEffect(EffectData data)
        {
            if (data == null)
                return;
            _active.RemoveAll(activeEffect => activeEffect.EffectId == data.EffectId);
        }
    }
}
