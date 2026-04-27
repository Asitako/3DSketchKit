using UnityEngine;

namespace ThreeDSketchKit.Core.Data
{
    public enum ZoneEffectKind
    {
        DamageOverTime,
        StealthLayer,
        Buff
    }

    [CreateAssetMenu(fileName = "ZoneEffectData", menuName = "3D Sketch Kit/Zone Effect Data", order = 1)]
    public class ZoneEffectData : ScriptableObject
    {
        [SerializeField] ZoneEffectKind effectKind = ZoneEffectKind.DamageOverTime;
        [SerializeField] float tickInterval = 0.5f;
        [SerializeField] float strength = 10f;
        [SerializeField] float duration = -1f;
        [SerializeField] GameObject visualPrefab;
        [SerializeField] EffectData buffTemplate;
        [SerializeField] int stealthLayerIndex = 8;

        public ZoneEffectKind EffectKind => effectKind;
        public float TickInterval => tickInterval;
        public float Strength => strength;
        public float Duration => duration;
        public GameObject VisualPrefab => visualPrefab;
        public EffectData BuffTemplate => buffTemplate;
        public int StealthLayerIndex => stealthLayerIndex;
    }
}
