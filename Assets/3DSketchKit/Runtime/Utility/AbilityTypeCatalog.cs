using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThreeDSketchKit.Core.Attributes;
using ThreeDSketchKit.Core.Interfaces;

namespace ThreeDSketchKit.Utility
{
    /// <summary>
    /// Resolves and registers <see cref="IAbility"/> types by stable id, full name, or assembly-qualified name.
    /// Built-ins are discovered via <see cref="SketchKitAbilityIdAttribute"/> on startup; games can call <see cref="Register"/> for custom abilities.
    /// </summary>
    public static class AbilityTypeCatalog
    {
        static readonly Dictionary<string, Type> ResolutionTable = new(StringComparer.Ordinal);

        /// <summary>
        /// Clears the table and re-registers all types in loaded assemblies that carry <see cref="SketchKitAbilityIdAttribute"/>.
        /// Safe to call from editor after script reloads.
        /// </summary>
        public static void RefreshDiscoveredAbilities()
        {
            ResolutionTable.Clear();
            RegisterTypesWithSketchKitAbilityIdAttribute();
        }

        /// <summary>
        /// Registers a concrete ability type under a stable id and indexes common string keys (id, <see cref="Type.FullName"/>, <see cref="Type.AssemblyQualifiedName"/>).
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the type cannot be instantiated as an ability.</exception>
        public static void Register(string abilityId, Type abilityType)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
                throw new ArgumentException("Ability id must not be empty.", nameof(abilityId));
            ValidateAbilityType(abilityType);
            IndexTypeUnderAllKeys(abilityId.Trim(), abilityType);
        }

        /// <summary>
        /// Registers <typeparamref name="TAbility"/> under <paramref name="abilityId"/>.
        /// </summary>
        public static void Register<TAbility>(string abilityId) where TAbility : class, IAbility =>
            Register(abilityId, typeof(TAbility));

        /// <summary>
        /// Resolves a type from a slot key: registered id / indexed names first, then <see cref="Type.GetType(string)"/> for legacy assembly-qualified strings.
        /// </summary>
        public static bool TryResolveType(string resolutionKey, out Type abilityType)
        {
            abilityType = null;
            if (string.IsNullOrWhiteSpace(resolutionKey))
                return false;

            var trimmedKey = resolutionKey.Trim();
            if (ResolutionTable.TryGetValue(trimmedKey, out abilityType) &&
                abilityType != null &&
                typeof(IAbility).IsAssignableFrom(abilityType))
                return true;

            abilityType = Type.GetType(trimmedKey, throwOnError: false, ignoreCase: false);
            return abilityType != null &&
                   abilityType.IsClass &&
                   !abilityType.IsAbstract &&
                   typeof(IAbility).IsAssignableFrom(abilityType);
        }

        /// <summary>
        /// Distinct ability types currently indexed (for editor pickers).
        /// </summary>
        public static IReadOnlyList<Type> GetDistinctRegisteredAbilityTypes() =>
            ResolutionTable.Values.Distinct().OrderBy(t => t.FullName, StringComparer.Ordinal).ToList();

        static void RegisterTypesWithSketchKitAbilityIdAttribute()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }

                foreach (var candidateType in types)
                {
                    if (!candidateType.IsClass || candidateType.IsAbstract || candidateType.IsInterface)
                        continue;
                    if (!typeof(IAbility).IsAssignableFrom(candidateType))
                        continue;

                    var idAttribute = candidateType.GetCustomAttribute<SketchKitAbilityIdAttribute>(inherit: false);
                    if (idAttribute == null)
                        continue;
                    if (!HasParameterlessConstructor(candidateType))
                        continue;

                    IndexTypeUnderAllKeys(idAttribute.AbilityId, candidateType);
                }
            }
        }

        static void IndexTypeUnderAllKeys(string primaryAbilityId, Type abilityType)
        {
            ValidateAbilityType(abilityType);
            ResolutionTable[primaryAbilityId] = abilityType;

            var assemblyQualifiedName = abilityType.AssemblyQualifiedName;
            if (!string.IsNullOrEmpty(assemblyQualifiedName))
                ResolutionTable[assemblyQualifiedName] = abilityType;

            var fullName = abilityType.FullName;
            if (!string.IsNullOrEmpty(fullName))
                ResolutionTable[fullName] = abilityType;
        }

        static void ValidateAbilityType(Type abilityType)
        {
            if (abilityType == null ||
                !abilityType.IsClass ||
                abilityType.IsAbstract ||
                !typeof(IAbility).IsAssignableFrom(abilityType))
                throw new ArgumentException("Type must be a concrete class implementing IAbility.", nameof(abilityType));

            if (!HasParameterlessConstructor(abilityType))
                throw new ArgumentException("Type must declare a parameterless constructor (public or non-public).", nameof(abilityType));
        }

        static bool HasParameterlessConstructor(Type type)
        {
            return type.GetConstructor(
                       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                       binder: null,
                       types: Type.EmptyTypes,
                       modifiers: null)
                   != null;
        }
    }
}
