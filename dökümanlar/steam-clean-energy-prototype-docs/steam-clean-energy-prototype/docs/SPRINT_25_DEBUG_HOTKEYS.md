# SPRINT 25 — DEBUG LAYER HOTKEYS

## 1. Sprint Amacı

Kaynak katmanları arasında F1–F6 klavye kısayolları (GDD §3).

## 2. Kapsam

Dahil:

- `DebugViewHotkeys.TryMapKey`
- MapDebugUI Update
- EditMode testleri

Dahil değil:

- Kamera odaklanma
- F7/F8 yeni katmanlar
- Rebind UI

## 3. Mapping

| Key | Mode |
|-----|------|
| F1 | Normal |
| F2 | Height |
| F3 | Slope |
| F4 | Water |
| F5 | Solar |
| F6 | Wind |

## 4. Definition of Done

- F-tuşları katman değiştirir
- SelectionGrid senkron
- Testler geçer
