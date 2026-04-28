using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Diagnostics
{
    /// <summary>
    /// TEMP (development-only): logs how long after Unity start the asset finished initializing (Editor domain).
    /// Remove before release if you don't want editor console noise.
    /// </summary>
    [InitializeOnLoad]
    public static class SketchKitLoadTimeLogger
    {
        const string SessionKey = "ThreeDSketchKit.LoadTimeLogger.Logged";

        static SketchKitLoadTimeLogger()
        {
            // Log once per editor session (prevents spam on domain reloads).
            if (SessionState.GetBool(SessionKey, false))
                return;
            SessionState.SetBool(SessionKey, true);

            // Defer to the next tick so Editor timeSinceStartup is stable.
            EditorApplication.delayCall += Log;
        }

        static void Log()
        {
            var secondsSinceEditorStart = EditorApplication.timeSinceStartup;
            Debug.Log($"[3D Sketch Kit] Время загрузки (инициализации) ассета: {secondsSinceEditorStart:0.000} сек (EditorApplication.timeSinceStartup).");
        }
    }
}

