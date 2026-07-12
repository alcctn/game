using CleanEnergy.Art;
using UnityEditor;
using UnityEngine;

namespace CleanEnergy.Editor
{
    /// <summary>
    /// One-shot wire of TerrainArtCatalog after domain reload (no import loop).
    /// </summary>
    [InitializeOnLoad]
    public static class TerrainArtCatalogAutoWire
    {
        private const string SessionKey = "ce_terrain_art_autowire_done";

        static TerrainArtCatalogAutoWire()
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
                var catalog = AssetDatabase.LoadAssetAtPath<TerrainArtCatalog>(TerrainArtCatalog.DefaultAssetPath);
                if (catalog != null && catalog.HasTerrainLayers())
                {
                    return;
                }

                TestTerrainSceneSetup.EnsureTerrainArtCatalog();
                AssetDatabase.SaveAssets();
                Debug.Log("[TerrainArt] Catalog auto-wired (one-shot).");
            };
        }
    }
}
