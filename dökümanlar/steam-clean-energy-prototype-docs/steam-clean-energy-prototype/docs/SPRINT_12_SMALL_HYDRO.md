# SPRINT 12 — SMALL HYDRO

## 1. Sprint Amacı

Su çarkından sonraki hidro teknolojisi: `small_hydro` (yüksek debi, daha yüksek üretim).

## 2. Kapsam

Dahil:

- `small_hydro` binası
- `hydro_turbine` araştırma düğümü (hydro_basic sonrası, 30 RP)
- Hidro üretim örneklemesi
- Senaryo diversity listesine ekleme
- EditMode testleri

Dahil değil:

- Yeni slope/elevation SO alanı
- Periyodik MaintenanceCost ekonomisi
- Tutorial adımı

## 3. Kurallar

| | water_wheel | small_hydro |
|--|-------------|-------------|
| Cost | 80 | 220 |
| Power | 8 | 18 |
| Efficiency | 0.8 | 0.85 |
| Min water | 8 | 20 |
| Unlock | always | hydro_turbine |

## 4. Definition of Done

- Kilitli iken yerleştirilemez
- Düşük debide fail, yüksek debide pass
- Üretim hidro potansiyeli ile ölçeklenir
- Testler geçer
