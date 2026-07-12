# SPRINT 70 — ROTATING VISUAL

## 1. Sprint Amacı

Rüzgâr / hidro yapılara Y ekseni dönüş animasyonu.

## 2. Kapsam

Dahil:

- `RotatingVisual` RPM: `small_wind` 40, `water_wheel` / `small_hydro` 25
- `SimulationSpeed.Paused` iken durur
- `BuildingFactory` ilgili id’lere component ekler

Dahil değil:

- Ayrı blade child mesh art
- Diğer bina id’leri

## 3. Definition of Done

- Wind/hydro create sonrası component + doğru RPM
- Pause’ta rotasyon donar
- Testler geçer
