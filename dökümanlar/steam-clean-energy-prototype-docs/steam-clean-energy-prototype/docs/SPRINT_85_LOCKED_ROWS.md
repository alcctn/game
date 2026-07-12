# SPRINT 85 — LOCKED BUILD ROWS

## 1. Sprint Amacı

Build menüsünde kilitli binaları gri satır + reason ile göstermek.

## 2. Kapsam

Dahil:

- Aktif sekmede locked defs görünür (`Requires: {displayName|id}`)
- Click no-op; unlocked davranış aynı
- `FilterForTab` locked dahil

Dahil değil:

- UGUI rebuild
- Research HUD değişiklikleri

## 3. Definition of Done

- Filtre locked satırları içerir
- Reason formatı test edilir
- Unlocked seçilebilir; locked seçilemez
- Testler geçer
