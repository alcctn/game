# SPRINT 92 — ENVIRONMENTAL SCORE (REAL)

## 1. Sprint Amacı

Çevresel yoğunluk skorunu ekonomik upkeep’e bağlamak.

## 2. Kapsam

Dahil:

- `EnvironmentalImpact` hücre yoğunluğu 0–1 (F10 aynı skor)
- Yoğunluk > 0.6 → o hücredeki producer upkeep ×1.15
- `UpkeepService` hesaplamasına bağlama
- `EnvScoreTests`

Dahil değil:

- Global pollution meter UI

## 3. Definition of Done

- Yüksek yoğunlukta upkeep artar
- Overlay skoru ekonomi ile uyumlu
- Testler geçer
