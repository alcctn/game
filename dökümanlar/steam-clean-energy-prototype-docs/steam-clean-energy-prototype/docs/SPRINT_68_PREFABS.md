# SPRINT 68 — PREFAB STATIC PASS

## 1. Sprint Amacı

Statik yapılar için placeholder prefab bağlamak; Factory Instantiation yolunu kullanmak.

## 2. Kapsam

Dahil:

- Prefab: `village`, `distribution_hub`, `battery`, `small_solar`, `power_line`
- `Assets/_Game/Prefabs/Buildings/` + `BuildingDefinition.SetPrefab`
- Editor setup (`CreateOrLoadBuildings` / menu) ile `PrefabUtility.SaveAsPrefabAsset`
- Primitive fallback korunur

Dahil değil:

- Animasyon (S70)
- Wind / hydro mesh art

## 3. Definition of Done

- Prefab atanmış tanımlar Instantiate edilir
- Prefab null iken primitive fallback çalışır
- Testler geçer

## 4. Kurulum

Unity’de `Clean Energy/Setup Test Terrain Scene` veya `Create Building Placeholder Prefabs` çalıştır.
