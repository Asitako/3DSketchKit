using ThreeDSketchKit.Utility;
using UnityEngine;

namespace ThreeDSketchKit.Core.Bootstrap
{
    /// <summary>
    /// Ensures <see cref="AbilityTypeCatalog"/> is populated in player builds before scene objects wake.
    /// </summary>
    static class SketchKitAbilityCatalogBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDiscoveredAbilities()
        {
            AbilityTypeCatalog.RefreshDiscoveredAbilities();
        }
    }
}
