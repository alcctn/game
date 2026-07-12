# SPRINT 07 — DAY CYCLE

## 1. Sprint Amacı

Hızlandırılmış gün döngüsü ile köy talep eğrisini ve güneş daylight çarpanını bağlamak; bataryayı akşam/gece için anlamlı kılmak.

## 2. Kapsam

Dahil:

- DayPhase + DayCycleService (48 tick/gün)
- SimulationContext day alanları
- Talep çarpanı (VillageConsumer)
- Solar daylight çarpanı
- HUD phase göstergesi
- Generate’de day reset
- EditMode testleri

Dahil değil:

- Kayıt / yükleme
- Bakım
- Hava durumu
- Tutorial

## 3. Çarpanlar

| Phase | Demand | Daylight |
|-------|--------|----------|
| Morning | 1.0 | 0.55 |
| Noon | 0.75 | 1.0 |
| Evening | 1.45 | 0.35 |
| Night | 0.55 | 0.0 |

## 4. Definition of Done

- HUD’da phase döner
- Akşam talep yükselir, gece solar düşer
- Generate günü sıfırlar
- Testler geçer
