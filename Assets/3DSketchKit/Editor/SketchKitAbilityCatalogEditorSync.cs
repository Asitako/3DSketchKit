using ThreeDSketchKit.Utility;
using UnityEditor;
using ThreeDSketchKit.Editor.Diagnostics;

namespace ThreeDSketchKit.Editor
{
    /// <summary>
    /// Keeps <see cref="AbilityTypeCatalog"/> in sync in the Editor without entering Play Mode (domain reload / script compile).
    /// </summary>
    [InitializeOnLoad]
    static class SketchKitAbilityCatalogEditorSync
    {
        static SketchKitAbilityCatalogEditorSync()
        {
            SketchKitInitTimings.Begin("EditorSync.AbilityTypeCatalog.RefreshDiscoveredAbilities");
            try
            {
                AbilityTypeCatalog.RefreshDiscoveredAbilities();
            }
            finally
            {
                SketchKitInitTimings.End("EditorSync.AbilityTypeCatalog.RefreshDiscoveredAbilities");
            }
        }
    }
}
