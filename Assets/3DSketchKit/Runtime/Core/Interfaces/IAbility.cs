namespace ThreeDSketchKit.Core.Interfaces
{
    /// <summary>
    /// Behavioural module attached to an object; does not inherit MonoBehaviour.
    /// </summary>
    public interface IAbility
    {
        string AbilityName { get; }
        bool IsActive { get; set; }
        void PerformAction();
    }
}
