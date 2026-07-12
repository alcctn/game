# SPRINT 59 — MULTI-CELL FOOTPRINT

## 1. Sprint Amacı

Binaların birden fazla hücre kaplaması (footprint); ilk içerik `small_solar` = 2×1.

## 2. Kilit Kararlar

- Occupancy / validator / preview / demolish / save: tüm footprint hücreleri (anchor = min X/Y = `BuildingInstance.Coordinate`)
- Rotation R: Size eksen takası `(w,h)→(h,w)` tek rotasyonlarda; offset hücreler
- NetworkConnectionRule / mesafe maliyeti / yield: yalnızca anchor hücre
- Multi-cell demolish: tek undo snapshot (tüm footprint)

## 3. Kapsam

Dahil:

- `BuildingFootprint` helper
- `GridOccupancyService` çok hücreli occupy/release
- Placement kuralları footprint üzerinde
- Preview tüm hücreler
- `small_solar` Size=(2,1)

## 4. Definition of Done

- Solar 2×1 iki hücreyi işgal eder
- Dönüşte boyut eksenleri değişir
- Overlap engellenir
- Testler geçer
