using ThreeDSketchKit.Core.Data;

namespace ThreeDSketchKit.Core.Interfaces
{
    /// <summary>
    /// Optional setup hook after the ability instance is created.
    /// </summary>
    public interface IAbilityLifecycle
    {
        void OnAttached(IAbilityHost host, AbilityData config);
    }
}
