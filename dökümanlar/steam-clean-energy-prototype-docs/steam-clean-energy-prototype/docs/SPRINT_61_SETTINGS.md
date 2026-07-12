# SPRINT 61 — SETTINGS PANEL

## 1. Sprint Amacı

Main Menu ve Pause overlay’de ayar alt paneli: volume, SFX mute, zoom sensitivity.

## 2. Kapsam

Dahil:

- `SettingsService` PlayerPrefs: `ce_master_volume`, `ce_sfx_mute`, `ce_zoom_speed`
- Master volume → `AudioListener.volume`
- SFX mute → Sprint 60
- Zoom → `IsometricCameraController.ZoomSpeed`
- Anında uygula; play scene start’ta restore
- Main Menu + Pause Settings sub-panel

Dahil değil:

- Keybind remap

## 3. Definition of Done

- Prefs yazılır ve hemen uygulanır
- Play sahnesi açılışında camera/audio restore
- Testler geçer
