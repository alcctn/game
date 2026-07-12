using UnityEngine;

namespace CleanEnergy.Art
{
    /// <summary>
    /// Typed Pure Poly prefab slots for nature decoration (visual only).
    /// </summary>
    [CreateAssetMenu(fileName = "PurePolyCatalog", menuName = "Clean Energy/Pure Poly Catalog")]
    public sealed class PurePolyCatalog : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/_Game/Data/Art/PurePolyCatalog.asset";
        public const string PrefabFolder = "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs";

        [SerializeField] private GameObject meadow;
        [SerializeField] private GameObject[] forestTrees = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject lakeGround;
        [SerializeField] private GameObject mountain;
        [SerializeField] private GameObject path;
        [SerializeField] private GameObject[] rocks = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] flowers = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] grasses = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] bridges = System.Array.Empty<GameObject>();

        public GameObject Meadow => meadow;
        public GameObject[] ForestTrees => forestTrees;
        public GameObject LakeGround => lakeGround;
        public GameObject Mountain => mountain;
        public GameObject Path => path;
        public GameObject[] Rocks => rocks;
        public GameObject[] Flowers => flowers;
        public GameObject[] Grasses => grasses;
        public GameObject[] Bridges => bridges;

        public void Configure(
            GameObject meadowPrefab,
            GameObject[] forestTreePrefabs,
            GameObject lakeGroundPrefab,
            GameObject mountainPrefab,
            GameObject pathPrefab,
            GameObject[] rockPrefabs,
            GameObject[] flowerPrefabs,
            GameObject[] grassPrefabs,
            GameObject[] bridgePrefabs = null)
        {
            meadow = meadowPrefab;
            forestTrees = forestTreePrefabs ?? System.Array.Empty<GameObject>();
            lakeGround = lakeGroundPrefab;
            mountain = mountainPrefab;
            path = pathPrefab;
            rocks = rockPrefabs ?? System.Array.Empty<GameObject>();
            flowers = flowerPrefabs ?? System.Array.Empty<GameObject>();
            grasses = grassPrefabs ?? System.Array.Empty<GameObject>();
            bridges = bridgePrefabs ?? System.Array.Empty<GameObject>();
        }

        public int AssignedSlotCount()
        {
            var n = 0;
            if (meadow != null) n++;
            if (lakeGround != null) n++;
            if (mountain != null) n++;
            if (path != null) n++;
            n += CountNonNull(forestTrees);
            n += CountNonNull(rocks);
            n += CountNonNull(flowers);
            n += CountNonNull(grasses);
            n += CountNonNull(bridges);
            return n;
        }

        public static GameObject Pick(GameObject[] options, int hash)
        {
            if (options == null || options.Length == 0)
            {
                return null;
            }

            var valid = 0;
            for (var i = 0; i < options.Length; i++)
            {
                if (options[i] != null)
                {
                    valid++;
                }
            }

            if (valid == 0)
            {
                return null;
            }

            var index = Mathf.Abs(hash) % valid;
            for (var i = 0; i < options.Length; i++)
            {
                if (options[i] == null)
                {
                    continue;
                }

                if (index == 0)
                {
                    return options[i];
                }

                index--;
            }

            return null;
        }

        private static int CountNonNull(GameObject[] items)
        {
            if (items == null)
            {
                return 0;
            }

            var n = 0;
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] != null)
                {
                    n++;
                }
            }

            return n;
        }
    }
}
