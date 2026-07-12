# SPRINT 94 — UI SCALE

## 1. Sprint Amacı

IMGUI HUD ölçeğini ayarlanabilir yapmak.

## 2. Kapsam

Dahil:

- `SettingsService` `ce_ui_scale`: 0.85 / 1.0 / 1.25 (en yakına snap), default 1.0
- `SettingsPanelUI` kontrolü
- `GuiScale.ApplyGuiScale()` — ana HUD `OnGUI` girişleri
- `UiScaleTests`

Dahil değil:

- Canvas Scaler / TextMeshPro

## 3. Definition of Done

- Ölçek prefs’e yazılır
- HUD `GUI.matrix` ile ölçeklenir
- Testler geçer
