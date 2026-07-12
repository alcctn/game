# SPRINT 77 — WEATHER MICRO-EVENTS

## 1. Sprint Amacı

Kısa deterministik hava olayları (Cloudy / WindGust).

## 2. Kapsam

Dahil:

- Her 120 tick %25 şans
- Cloudy: solar×0.5 (30 tick); WindGust: wind×1.4 (20 tick)
- `SimulationContext` çarpanları + notification + HUD badge
- RNG: `tickIndex ^ seedHash`

Dahil değil:

- Full seasons

## 3. Definition of Done

- Aynı seed + tick aynı olayı üretir
- Testler geçer
