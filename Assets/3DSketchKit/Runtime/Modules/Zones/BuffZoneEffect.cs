using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Data;
using ThreeDSketchKit.Core.Interfaces;

namespace ThreeDSketchKit.Modules.Zones
{
    public sealed class BuffZoneEffect : IZoneEffect
    {
        readonly EffectData _template;

        public BuffZoneEffect(EffectData template) => _template = template;

        public void OnMemberEntered(ZoneEffectSubject subject)
        {
            var effectReceiver = subject?.GetComponent<EffectReceiverComponent>();
            if (effectReceiver == null || _template == null)
                return;
            effectReceiver.ApplyEffect(_template.Clone());
        }

        public void OnMemberStaying(ZoneEffectSubject subject, float deltaTime) { }

        public void OnMemberExited(ZoneEffectSubject subject)
        {
            var effectReceiver = subject?.GetComponent<EffectReceiverComponent>();
            if (effectReceiver == null || _template == null)
                return;
            effectReceiver.RemoveEffect(_template);
        }
    }
}
