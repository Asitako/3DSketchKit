using System;

namespace ThreeDSketchKit.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CharacterSystemModuleAttribute : Attribute
    {
        public CharacterSystemModuleAttribute(CharacterSystemKind systemKind, string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name must be a non-empty string.", nameof(displayName));

            SystemKind = systemKind;
            DisplayName = displayName.Trim();
        }

        public CharacterSystemKind SystemKind { get; }
        public string DisplayName { get; }
        public string Description { get; set; }
    }

    public enum CharacterSystemKind
    {
        Locomotion,
        Ability,
        Combat,
        Inventory,
        Equipment,
        Control,
        Interactable
    }
}
