using System;
using System.Collections.Generic;
using ThreeDSketchKit.Core.Attributes;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Integration
{
    public static class CharacterModuleValidation
    {
        public static CharacterModuleTypeValidationResult Validate(Type type)
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var attribute = (CharacterSystemModuleAttribute)Attribute.GetCustomAttribute(type, typeof(CharacterSystemModuleAttribute), inherit: false);

            if (!typeof(CharacterModuleAsset).IsAssignableFrom(type))
                errors.Add("Type must inherit CharacterModuleAsset.");
            if (!typeof(ScriptableObject).IsAssignableFrom(type))
                errors.Add("Type must be a ScriptableObject module asset.");
            if (attribute == null)
                warnings.Add("Missing CharacterSystemModuleAttribute. Discovery still works, but editor metadata is incomplete.");
            if (type.GetConstructor(Type.EmptyTypes) == null)
                errors.Add("Type must have a public parameterless constructor for ScriptableObject asset creation.");

            var systemKind = attribute != null ? attribute.SystemKind : InferSystemKind(type);
            var displayName = attribute != null ? attribute.DisplayName : ObjectNames.NicifyVariableName(type.Name);
            var description = attribute != null ? attribute.Description : "";

            return new CharacterModuleTypeValidationResult(systemKind, displayName, description, errors, warnings);
        }

        static CharacterSystemKind InferSystemKind(Type type)
        {
            if (typeof(LocomotionModuleAsset).IsAssignableFrom(type))
                return CharacterSystemKind.Locomotion;
            if (typeof(AbilityModuleAsset).IsAssignableFrom(type))
                return CharacterSystemKind.Ability;
            if (typeof(CombatModuleAsset).IsAssignableFrom(type))
                return CharacterSystemKind.Combat;
            if (typeof(InventoryModuleAsset).IsAssignableFrom(type))
                return CharacterSystemKind.Inventory;
            if (typeof(EquipmentModuleAsset).IsAssignableFrom(type))
                return CharacterSystemKind.Equipment;
            if (typeof(ControlModuleAsset).IsAssignableFrom(type))
                return CharacterSystemKind.Control;
            return CharacterSystemKind.Interactable;
        }
    }

    public sealed class CharacterModuleTypeValidationResult
    {
        public CharacterModuleTypeValidationResult(
            CharacterSystemKind systemKind,
            string displayName,
            string description,
            IReadOnlyList<string> errors,
            IReadOnlyList<string> warnings)
        {
            SystemKind = systemKind;
            DisplayName = displayName;
            Description = description;
            Errors = errors;
            Warnings = warnings;
        }

        public CharacterSystemKind SystemKind { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public IReadOnlyList<string> Errors { get; }
        public IReadOnlyList<string> Warnings { get; }
    }
}
