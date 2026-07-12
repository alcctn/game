# SPRINT 13 — UPKEEP ECONOMY

## 1. Sprint Amacı

Yerleştirilmiş binaların `MaintenanceCost` toplamının her tick cüzdandan düşülmesi.

## 2. Kapsam

Dahil:

- `UpkeepService` (toplam + ödeme)
- EnergySimulationDriver tick entegrasyonu
- HUD upkeep satırı + soft uyarı
- EditMode testleri

Dahil değil:

- Borç / acil kredi
- Sağ bilgi paneli
- Upkeep × MaintenanceLevel

## 3. Kurallar

- `totalUpkeep = Σ MaintenanceCost` (unique instance)
- `TrySpend` başarısızsa para 0; `CouldNotAffordFullUpkeep = true`
- Surplus satışından sonra uygulanır

## 4. Definition of Done

- Tick’te para düşer
- HUD upkeep gösterir
- Yetersiz paradа clamp + uyarı
- Testler geçer
