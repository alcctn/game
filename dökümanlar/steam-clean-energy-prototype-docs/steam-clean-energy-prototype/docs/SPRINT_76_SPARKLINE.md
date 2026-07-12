# SPRINT 76 — PRODUCTION SPARKLINE

## 1. Sprint Amacı

Inspection’da son 20 tick üretim mini grafiği.

## 2. Kapsam

Dahil:

- Ring buffer per instanceId (max 32 bina, 20 sample)
- IMGUI bar strip
- Save’e yazılmaz

Dahil değil:

- Persist / CSV export

## 3. Definition of Done

- Inspection producer sparkline gösterir
- Testler geçer
