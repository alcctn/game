# SPRINT 102 — RP INCOME UX

## 1. Sprint Amacı

Coverage RP gelirini HUD’da göstermek; diversity bonus için toast.

## 2. Kapsam

Dahil:

- `ResearchProgressTracker.LastCoverageRpGranted` + diversity event
- EnergyHud `+RP/tick` etiketi
- NotificationController diversity `+10 RP` toast
- Formüller değişmez (coverage +1, diversity +10 bir kez)
- `RpUxTests`

Dahil değil:

- RP formül dengeleme

## 3. Definition of Done

- Coverage varken HUD `+1/tick` gösterir
- Diversity toast bir kez gelir
- Testler geçer
