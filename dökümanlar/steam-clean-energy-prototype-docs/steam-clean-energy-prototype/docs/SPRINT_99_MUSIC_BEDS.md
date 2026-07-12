# SPRINT 99 — DAY/NIGHT MUSIC BEDS

## 1. Sprint Amacı

Gündüz/gece için isteğe bağlı müzik bed’leri.

## 2. Kapsam

Dahil:

- `MusicService` dayClip + nightClip
- `DayPhase.Night` → night, aksi halde day; null → mevcut loopClip
- DayCycle / SimulationClock hook
- `MusicBedsTests`

Dahil değil:

- Crossfade
- Spatial 3D music

## 3. Definition of Done

- Phase’e göre clip seçilir; null fallback loop
- Testler geçer
