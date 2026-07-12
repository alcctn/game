using UnityEditor;
using UnityEngine;

namespace CleanEnergy.Editor
{
    /// <summary>
    /// Ensures PurePolyCatalog prefab slots are filled after scripts reload.
    /// </summary>
    [InitializeOnLoad]
    public static class PurePolyCatalogAutoWire
    {
        static PurePolyCatalogAutoWire()
        {
            EditorApplication.delayCall += () =>
            {
                TestTerrainSceneSetup.EnsurePurePolyCatalog();
                AssetDatabase.SaveAssets();
            };
        }
    }
}
