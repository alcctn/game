# SPRINT 91 — WEATHER AMBIENT DIM

## 1. Sprint Amacı

Cloudy hava olayında ambient ışığı karartmak.

## 2. Kapsam

Dahil:

- `DayCycleLighting`: Cloudy → ambient ×0.7; WindGust lighting değiştirmez
- Pause lighting’i dondurur (S72)
- `WeatherEventService` aktif olaya hook
- `WeatherDimTests`

Dahil değil:

- Volumetric clouds / shadow art

## 3. Definition of Done

- Cloudy ambient dimlenir
- Pause’ta dim uygulanmaz
- Testler geçer
