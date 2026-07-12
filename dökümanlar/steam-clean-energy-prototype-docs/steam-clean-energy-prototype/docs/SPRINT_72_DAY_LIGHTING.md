# SPRINT 72 — DAY-CYCLE LIGHTING

## 1. Sprint Amacı

Gün fazına göre directional + ambient lighting.

## 2. Kapsam

Dahil:

- `DayCycleLighting` sabit tablo: Dawn (`Morning`) / Noon / Evening / Night
- Directional intensity/color + `RenderSettings.ambientLight`
- Sim pause lighting’i dondurur

Dahil değil:

- Volumetric fog / shadows art pass
- Weather cloud darkening

## 3. Definition of Done

- Faz değişince ışık güncellenir
- Pause’ta son faz korunur
- Testler geçer
