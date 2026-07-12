# SPRINT 84 — RESEARCH SYNC

## 1. Sprint Amacı

Disk research asset’ini runtime prototip ağacıyla her setup’ta senkron tutmak.

## 2. Kapsam

Dahil:

- `CreateOrLoadResearch` her zaman `ConfigureGreenValleyPrototype()` çağırır
- Storage + tier-3 düğümleri (storage_basic, battery_cap, hydro_tune, solar_inverter, wind_blade)
- Battery unlock → `storage_basic`

Dahil değil:

- Yeni araştırma dalları

## 3. Definition of Done

- CreateRuntime ağacı beklenen düğümleri içerir
- Battery `storage_basic` ile açılır
- Testler geçer

## 4. Kurulum

Unity’de `Clean Energy/Setup Test Terrain Scene` çalıştır (research asset yeniden yazılır).
