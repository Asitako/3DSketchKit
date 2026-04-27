using ThreeDSketchKit.Core.Data;
using ThreeDSketchKit.Core.Interfaces;
using ThreeDSketchKit.Modules.Zones;

namespace ThreeDSketchKit.Utility
{
    public static class ZoneEffectFactory
    {
        public static IZoneEffect Create(ZoneEffectData data)
        {
            if (data == null)
                return new DamageZoneEffect(0.5f, 0f);

            switch (data.EffectKind)
            {
                case ZoneEffectKind.DamageOverTime:
                    return new DamageZoneEffect(data.TickInterval, data.Strength);
                case ZoneEffectKind.StealthLayer:
                    return new StealthZoneEffect(data.StealthLayerIndex);
                case ZoneEffectKind.Buff:
                    return new BuffZoneEffect(data.BuffTemplate != null ? data.BuffTemplate.Clone() : new EffectData());
                default:
                    return new DamageZoneEffect(data.TickInterval, data.Strength);
            }
        }
    }
}
