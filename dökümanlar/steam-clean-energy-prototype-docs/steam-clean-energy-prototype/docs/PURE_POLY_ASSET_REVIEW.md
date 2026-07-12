# PURE POLY — ASSET REVIEW

Ücretsiz Pure Poly doğa paketi envanteri. Entegrasyon (tile spawn / prop scatter) ayrı sprint bandıdır; bu doküman yalnızca inceleme.

## Konum

`Assets/Pure Poly/Free Low Poly Nature Pack/`

| Klasör | İçerik |
|--------|--------|
| Prefabs / Meshes | **34** hazır prefab + eş FBX |
| Materials | `PP_Standard_Material`, `PP_Ground`, `PP_Water` (+ Built-in dönüşüm package) |
| Terrain | Unity Terrain texture/layer’lar (10 layer) |
| Scenes | `Demo_01`, `Demo_List`, `Demo_Terrain` |
| Docs | `Read Me.pdf` |

## Prefab grupları

- **Zemin / karo:** `PP_Floor_Tile_05/06/15/16`, `PP_Meadow_07/08`, `PP_Meadow_Path_05`, `PP_Cemetery_Pebbles_03/09`, `PP_Lake_Ground_04`
- **Yükseklik:** `PP_Forest_Mountain_Moss_01/02`
- **Doğa dekor:** ağaç (`PP_Tree_*`, `PP_Birch_Tree_*`), çimen, çiçek, mantar, kaya yığınları
- **Props:** `PP_Bridge_15_*`, `PP_Small_Fence_*`

Prefab yapısı: tek GameObject + MeshFilter/Renderer + collider; scale `1,1,1`. Enerji binası yok.

## Oyuna uyum

| İhtiyaç | Bu paket |
|---------|----------|
| Low poly arazi karoları | **Kısmen** — meadow/floor/lake/mountain/path örnekleri; tam tile set değil, ücretsiz alt küme |
| Bina mesh (solar/wind/hub…) | **Yok** — bkz. `ASSET_PREFAB_REPLACEMENT.md` |
| Seed grid harita | Doğrudan uyumlu değil; catalog + hücre spawn gerekir |
| URP | Materyaller mevcut; gerekirse URP convert |

Mevcut oyun araziyi heightmesh ile üretir (`MapGenerator`). Bu paket **overlay / tile / prop** olarak bağlanır; sim logic değiştirmez.

## Sonuç

- Paket doğru yerde import edilmiş.
- Tam harita tile paketi değil — dekor + sınırlı zemin varyantı.
- **Entegrasyon (S111–118):** `PurePolyCatalog` + `NatureVisualSpawner` — `MapGeneratedEvent` sonrası biome scatter (max 512). Unity Terrain korunur; sim değişmez; bina mesh yok.

Kurulum: Unity menü **Clean Energy → Setup Pure Poly Catalog** (veya Setup Test Terrain Scene).

## Görünürlük notu (play)

- Proje **URP** kullanır; `TerrainBuilder` artık URP Terrain Lit malzeme atar (aksi halde arazi boş/kareli görünür).
- Catalog boşsa prop spawn olmaz — Setup menüsü veya auto-wire `Assets/_Game/Data/Art/PurePolyCatalog.asset` doldurur.
- Play sonrası kamera `FitToMapBounds` ile haritaya hizalanır.
