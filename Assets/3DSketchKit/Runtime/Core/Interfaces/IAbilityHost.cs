using UnityEngine;

namespace ThreeDSketchKit.Core.Interfaces
{
    /// <summary>
    /// Dependency bridge from <see cref="Components.AbilityManager"/> into plain ability modules.
    /// </summary>
    public interface IAbilityHost
    {
        GameObject Owner { get; }
        T GetDependency<T>() where T : class;
        Vector2 MovementInput { get; }
    }
}
