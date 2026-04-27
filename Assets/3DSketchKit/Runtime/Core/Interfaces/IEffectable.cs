using ThreeDSketchKit.Core.Data;

namespace ThreeDSketchKit.Core.Interfaces
{
    public interface IEffectable
    {
        void ApplyEffect(EffectData data);
        void RemoveEffect(EffectData data);
    }
}
