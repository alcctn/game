# SPRINT 32 — WIND WAKE EFFICIENCY

## 1. Sprint Amacı

Aynı tip rüzgar türbinleri arasında soft wake / verim cezası (Sprint 24 spacing sonrası).

## 2. Kapsam

Dahil:

- `WindWakeFactor` (Chebyshev ≤ MinSameTypeSpacing, 0.12 / komşu, min 0.4)
- Production formülüne wake çarpanı
- Inspection `WakeFactor` satırı

Dahil değil:

- Çapraz tip wake
- Hava durumu / turbulence
- Spacing kuralı değişikliği

## 3. Definition of Done

- Yoğun türbinler daha düşük üretim verir
- Testler geçer
