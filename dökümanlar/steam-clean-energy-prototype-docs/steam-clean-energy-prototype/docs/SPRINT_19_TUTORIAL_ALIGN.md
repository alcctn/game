# SPRINT 19 — TUTORIAL ALIGN

## 1. Sprint Amacı

Tutorial’ı prototip kurallarıyla hizalamak: batarya always-unlocked; “batarya araştır” adımı yok.

## 2. GDD farkı

GDD §16 adım 7 “Batarya araştır” der. Prototipte `battery` always-unlocked olduğu için akış:

1. Camera → Water → Water Wheel → Power Line  
2. Solar layer → Unlock solar_basic → Small Solar  
3. Place Battery → Sustain demand  

## 3. Kapsam

Dahil:

- Step metinleri (batarya hint net)
- EditMode sıra / no UnlockBattery testleri
- Doküman

Dahil değil:

- Battery research node

## 4. Definition of Done

- Checklist batarya “araştır” demez
- PlaceBattery, PlaceSolar’dan sonra
- Testler geçer
