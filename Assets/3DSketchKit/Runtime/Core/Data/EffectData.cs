using System;
using UnityEngine;

namespace ThreeDSketchKit.Core.Data
{
    /// <summary>
    /// Runtime payload for <see cref="Interfaces.IEffectable"/>; distinct from zone ScriptableObjects.
    /// </summary>
    [Serializable]
    public class EffectData
    {
        [SerializeField] string effectId = "default";
        [SerializeField] float duration = 1f;
        [SerializeField] float magnitude = 1f;

        public string EffectId
        {
            get => effectId;
            set => effectId = value;
        }

        public float Duration
        {
            get => duration;
            set => duration = value;
        }

        public float Magnitude
        {
            get => magnitude;
            set => magnitude = value;
        }

        public EffectData Clone()
        {
            return new EffectData
            {
                effectId = effectId,
                duration = duration,
                magnitude = magnitude
            };
        }
    }
}
