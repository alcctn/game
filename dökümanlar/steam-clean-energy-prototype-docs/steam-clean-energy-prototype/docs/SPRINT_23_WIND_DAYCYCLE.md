# SPRINT 23 — WIND DAY CYCLE

## 1. Sprint Amacı

Rüzgâr üretimini gün fazına bağlamak; solar ile tamamlayıcı gece/öğlen eğrisi (GDD §7.4).

## 2. Kapsam

Dahil:

- `GetWindFactor` / `SimulationContext.WindFactor`
- `small_wind` × WindFactor
- HUD Wind etiketi
- EditMode testleri

Dahil değil:

- Hava durumu
- Turbulence noise

## 3. Çarpanlar

| Phase | WindFactor |
|-------|------------|
| Morning | 0.85 |
| Noon | 0.55 |
| Evening | 1.15 |
| Night | 1.35 |

## 4. Definition of Done

- Gece wind > öğlen wind
- Solar gece 0
- HUD Wind çarpanı gösterir
- Testler geçer
