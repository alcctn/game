# SPRINT 56 — TRANSMISSION LOSS

## 1. Sprint Amacı

Üreticiden en yakın yük’e Manhattan hop kaybı (teslim faktörü).

## 2. Kapsam

Dahil:

- `deliveryFactor = clamp(1 - 0.05 * hops, 0.25, 1)`
- Balance + Inspection Loss satırı

Dahil değil:

- Placement Expected loss
- Hub LinkCapacity değişikliği

## 3. Definition of Done

- Uzak üretici daha az teslim eder
- Testler geçer
