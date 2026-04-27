using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThreeDSketchKit.Core.Interfaces;

namespace ThreeDSketchKit.Utility
{
    /// <summary>
    /// Finds concrete <see cref="IAbility"/> types for editor tooling and extension discovery.
    /// </summary>
    public static class AbilityTypeDiscovery
    {
        public static IReadOnlyList<Type> FindAbilityTypes()
        {
            var results = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (!type.IsClass || type.IsAbstract || type.IsInterface)
                        continue;
                    if (!typeof(IAbility).IsAssignableFrom(type))
                        continue;
                    if (!HasParameterlessConstructor(type))
                        continue;
                    results.Add(type);
                }
            }

            foreach (var catalogType in AbilityTypeCatalog.GetDistinctRegisteredAbilityTypes())
            {
                if (!results.Contains(catalogType))
                    results.Add(catalogType);
            }

            return results.OrderBy(candidateType => candidateType.FullName, StringComparer.Ordinal).ToList();
        }

        static bool HasParameterlessConstructor(Type type)
        {
            return type.GetConstructor(
                       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                       null,
                       Type.EmptyTypes,
                       null)
                   != null;
        }
    }
}
