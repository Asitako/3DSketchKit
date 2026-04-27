using ThreeDSketchKit.Utility;
using UnityEditor;

namespace ThreeDSketchKit.Editor
{
    /// <summary>
    /// Keeps <see cref="AbilityTypeCatalog"/> in sync in the Editor without entering Play Mode (domain reload / script compile).
    /// </summary>
    [InitializeOnLoad]
    static class SketchKitAbilityCatalogEditorSync
    {
        static SketchKitAbilityCatalogEditorSync() => AbilityTypeCatalog.RefreshDiscoveredAbilities();
    }
}
