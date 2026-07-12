# SPRINT 21 — LINE CAPACITY

## 1. Sprint Amacı

Hub başına hat kapasitesi (`LinkCapacity`) ile bileşen içi üretim teslimatını sınırlamak (GDD §8).

## 2. Kapsam

Dahil:

- `BuildingDefinition.LinkCapacity`
- Bileşen throughput = hub kapasiteleri toplamı (`<= 0` = sınırsız)
- Balance: `DeliveredProduction = min(P, C)`
- Congestion HUD / bildirim / inspection
- EditMode testleri

Dahil değil:

- Kenar bazlı flow / max-flow
- Peer-to-peer bağlantı
- Overlay renkleri

## 3. Değerler

| Bina | LinkCapacity |
|------|--------------|
| power_line | 40 |
| distribution_hub | 120 |

## 4. Definition of Done

- Düşük kapasite üretimi kısıtlar ve congestion işaretler
- Yüksek / çoklu hub tam teslimat sağlar
- `LinkCapacity <= 0` sınırsızdır
- Testler geçer
