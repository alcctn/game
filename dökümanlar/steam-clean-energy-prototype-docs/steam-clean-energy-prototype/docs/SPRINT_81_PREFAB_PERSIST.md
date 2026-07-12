# SPRINT 81 — PREFAB PERSIST

## 1. Sprint Amacı

Dokuz bina için placeholder prefab’ları diske yazıp `BuildingDefinition` alanına bağlamak.

## 2. Kapsam

Dahil:

- `BuildingPrefabIds.All` — 9 id
- Editor setup: `Assets/_Game/Prefabs/Buildings/{id}.prefab` + `SetPrefab`
- Wind/hydro placeholder’larında `Spin` child
- `ASSET_PREFAB_REPLACEMENT.md` notu

Dahil değil:

- Gerçek art mesh
- Animasyon mantığı (S82)

## 3. Definition of Done

- 9 id listesi test edilir
- `SetPrefab` atanır
- Testler geçer

## 4. Kurulum

Unity’de `Clean Energy/Setup Test Terrain Scene` veya `Create Building Placeholder Prefabs` çalıştır.
