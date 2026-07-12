# SPRINT 69 — MUSIC + AMBIENCE

## 1. Sprint Amacı

Döngüsel müzik / ambience ve ayrı müzik ses seviyesi.

## 2. Kapsam

Dahil:

- `MusicService` loop clip (null = no-op)
- Settings Music volume 0–1, `ce_music_volume`
- SFX mute müziği etkilemez
- `SettingsPanelUI` + `SettingsService.ApplyAll`

Dahil değil:

- Day/night ayrı clip setleri
- Spatial 3D music

## 3. Definition of Done

- Volume prefs yazılır ve ApplyAll restore eder
- Mute SFX açıkken müzik çalmaya devam eder
- Testler geçer
