namespace ThreeDSketchKit.Core.Interfaces
{
    /// <summary>
    /// Pure zone logic invoked by <see cref="Components.ZoneTrigger"/>.
    /// </summary>
    public interface IZoneEffect
    {
        void OnMemberEntered(ZoneEffectSubject subject);
        void OnMemberStaying(ZoneEffectSubject subject, float deltaTime);
        void OnMemberExited(ZoneEffectSubject subject);
    }
}
