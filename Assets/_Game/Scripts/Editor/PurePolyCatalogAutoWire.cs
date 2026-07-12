using CleanEnergy.Art;
using UnityEditor;
using UnityEngine;

namespace CleanEnergy.Editor
{
    /// <summary>
    /// One-shot wire of PurePolyCatalog after domain reload (no import loop).
    /// </summary>
    [InitializeOnLoad]
    public static class PurePolyCatalogAutoWire
    {
        private const string SessionKey = "ce_purepoly_autowire_done";

        static PurePolyCatalogAutoWire()
        {
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (SessionState.GetBool(SessionKey, false))
                {
                    return;
                }

                SessionState.SetBool(SessionKey, true);
                var catalog = AssetDatabase.LoadAssetAtPath<PurePolyCatalog>(PurePolyCatalog.DefaultAssetPath);
                if (catalog != null && catalog.AssignedSlotCount() > 0)
                {
                    return;
                }

                TestTerrainSceneSetup.EnsurePurePolyCatalog();
                AssetDatabase.SaveAssets();
                Debug.Log("[PurePoly] Catalog auto-wired (one-shot).");
            };
        }
    }
}
