# SPRINT 73 — GLOBAL MAP REPAIR

## 1. Sprint Amacı

Depot olmadan tüm producer’ları atomik toplu onar.

## 2. Kapsam

Dahil:

- `MaintenanceService.TryGlobalRepairAllProducers`
- HUD **Repair All**
- Atomic spend (yetersiz parada hiçbiri onarılmaz)

Dahil değil:

- Multi-select repair

## 3. Definition of Done

- Depot gerekmez; maliyet = sum(manual repair)
- Testler geçer
