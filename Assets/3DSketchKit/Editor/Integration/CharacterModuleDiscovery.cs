using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDSketchKit.Core.Attributes;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEditor;

namespace ThreeDSketchKit.Editor.Integration
{
    public static class CharacterModuleDiscovery
    {
        public static List<CharacterModuleDescriptor> Discover()
        {
            var descriptors = new List<CharacterModuleDescriptor>();
            foreach (var type in TypeCache.GetTypesDerivedFrom<CharacterModuleAsset>())
            {
                if (type.IsAbstract || type.IsGenericType)
                    continue;

                var validation = CharacterModuleValidation.Validate(type);
                descriptors.Add(new CharacterModuleDescriptor(
                    type,
                    validation.SystemKind,
                    validation.DisplayName,
                    validation.Description,
                    validation.Errors,
                    validation.Warnings));
            }

            return descriptors
                .OrderBy(d => d.SystemKind)
                .ThenBy(d => d.DisplayName, StringComparer.Ordinal)
                .ToList();
        }
    }

    public sealed class CharacterModuleDescriptor
    {
        public CharacterModuleDescriptor(
            Type type,
            CharacterSystemKind systemKind,
            string displayName,
            string description,
            IReadOnlyList<string> errors,
            IReadOnlyList<string> warnings)
        {
            Type = type;
            SystemKind = systemKind;
            DisplayName = displayName;
            Description = description;
            Errors = errors;
            Warnings = warnings;
        }

        public Type Type { get; }
        public CharacterSystemKind SystemKind { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public IReadOnlyList<string> Errors { get; }
        public IReadOnlyList<string> Warnings { get; }
        public bool IsValid => Errors.Count == 0;
    }
}
