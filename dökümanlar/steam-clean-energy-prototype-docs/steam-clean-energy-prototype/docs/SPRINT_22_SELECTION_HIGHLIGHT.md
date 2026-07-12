# SPRINT 22 — SELECTION HIGHLIGHT

## 1. Sprint Amacı

Seçili hücreyi dünya üzerinde amber highlight mesh ile göstermek (GDD §3; Sprint 14 borcu).

## 2. Kapsam

Dahil:

- `SelectionHighlight` (tek hücre quad)
- `MapDebugOverlay.SelectionChanged`
- Generate / Clear temizliği
- EditMode testleri

Dahil değil:

- Kamera odaklanma
- UGUI
- Çoklu seçim

## 3. Görünüm

- Renk: RGBA(1, 0.75, 0.15, 0.55)
- Y offset: overlay + 0.15
- Tüm view mode’larda görünür

## 4. Definition of Done

- Tıklanınca highlight + inspection
- Generate sonrası highlight yok
- Testler geçer
