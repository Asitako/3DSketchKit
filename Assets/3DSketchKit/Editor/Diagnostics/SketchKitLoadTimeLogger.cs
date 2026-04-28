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
            SketchKitInitTimings.Begin("EditorInit.LoadTimeLogger");
            // Log once per editor session (prevents spam on domain reloads).
            if (SessionState.GetBool(SessionKey, false))
            {
                SketchKitInitTimings.End("EditorInit.LoadTimeLogger");
                return;
            }
            SessionState.SetBool(SessionKey, true);

            // Defer to the next tick so Editor timeSinceStartup is stable.
            EditorApplication.delayCall += Log;
            SketchKitInitTimings.End("EditorInit.LoadTimeLogger");
        }

        static void Log()
        {
            var secondsSinceEditorStart = EditorApplication.timeSinceStartup;
            var sketchKitInitTotal = SketchKitInitTimings.GetTotalSeconds();
            var sketchKitInitSummed = SketchKitInitTimings.GetSummedModuleSeconds();

            Debug.Log(
                "[3D Sketch Kit] Unity startup time: " + secondsSinceEditorStart.ToString("0.000") + "s (EditorApplication.timeSinceStartup)\n" +
                "[3D Sketch Kit] Sketch Kit init time: " + sketchKitInitTotal.ToString("0.000") + "s (init window), " +
                sketchKitInitSummed.ToString("0.000") + "s (summed modules)\n" +
                SketchKitInitTimings.BuildReport());
        }
    }
}

