namespace ThreeDSketchKit.Core.Interfaces
{
    /// <summary>
    /// Optional per-frame hook for abilities (e.g. locomotion). Managed by <see cref="Components.AbilityManager"/>.
    /// </summary>
    public interface ITickableAbility : IAbility
    {
        void Tick(float deltaTime);
    }
}
