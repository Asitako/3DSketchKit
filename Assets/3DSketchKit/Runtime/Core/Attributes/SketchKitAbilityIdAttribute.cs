using System;

namespace ThreeDSketchKit.Core.Attributes
{
    /// <summary>
    /// Marks an <see cref="ThreeDSketchKit.Core.Interfaces.IAbility"/> implementation with a stable id for
    /// <see cref="ThreeDSketchKit.Utility.AbilityTypeCatalog"/> discovery (use with IL2CPP: register or preserve your assembly).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SketchKitAbilityIdAttribute : Attribute
    {
        public SketchKitAbilityIdAttribute(string abilityId)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
                throw new ArgumentException("Ability id must be a non-empty string.", nameof(abilityId));
            AbilityId = abilityId.Trim();
        }

        public string AbilityId { get; }
    }
}
