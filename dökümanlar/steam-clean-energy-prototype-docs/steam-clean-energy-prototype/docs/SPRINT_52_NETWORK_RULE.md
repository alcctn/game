# SPRINT 52 — PLACEMENT NETWORK CONNECTION RULE

## 1. Sprint Amacı

Üretici ve storage yerleştirirken mevcut şebeke düğümüne bağlantı zorunluluğu.

## 2. Kapsam

Dahil:

- `NetworkConnectionRule` (IsProducer / IsStorage)
- Boş harita: yalnızca hub ve village serbest
- Manhattan ≤ max(yerleştirilen, düğüm ConnectionRange)

Dahil değil:

- Runtime izole curtailment değişikliği (S49)

## 3. Definition of Done

- Preview validator aynı kuralı kullanır
- Testler geçer
