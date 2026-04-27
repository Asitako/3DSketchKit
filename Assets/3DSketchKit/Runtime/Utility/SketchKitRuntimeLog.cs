using System.Diagnostics;
using UnityEngine;

namespace ThreeDSketchKit.Utility
{
    /// <summary>
    /// Editor / Development-only diagnostics. Calls are stripped from non-development player builds
    /// (see Asset Store guidance: avoid <see cref="UnityEngine.Debug.LogWarning(object)"/> on hot production paths).
    /// </summary>
    internal static class SketchKitRuntimeLog
    {
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void InvalidAbilityType(MonoBehaviour context, string assemblyQualifiedTypeName)
        {
            UnityEngine.Debug.LogWarning($"[3D Sketch Kit] Unknown or invalid ability type: {assemblyQualifiedTypeName}", context);
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void AbilityCreationFailed(MonoBehaviour context, string typeName, string message)
        {
            UnityEngine.Debug.LogWarning($"[3D Sketch Kit] Could not create ability {typeName}: {message}", context);
        }
    }
}
