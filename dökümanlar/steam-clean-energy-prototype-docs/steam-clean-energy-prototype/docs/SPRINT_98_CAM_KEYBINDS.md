# SPRINT 98 — CAMERA KEYBIND REMAP

## 1. Sprint Amacı

Kamera hareket/rotate tuşlarını remappable keybind’lere bağlamak.

## 2. Kapsam

Dahil:

- `RemappableAction` CamForward/Back/Left/Right + CamRotateLeft/Right (default WASD + Q/E)
- `IsometricCameraController` `KeybindService` okur
- Settings keybind satırları
- F1–F10 hâlâ yasak
- `CamKeybindTests`

Dahil değil:

- Zoom remap
- Gamepad

## 3. Definition of Done

- Kamera defaults WASD/QE
- Remap prefs’e yazılır; F-keys reddedilir
- Testler geçer
