# SPRINT 55 — DEMOLISH UNDO

## 1. Sprint Amacı

Son demolish için tek adımlık Undo.

## 2. Kapsam

Dahil:

- Snapshot (definitionId, coordinate, rotation, storedEnergy, maintenanceLevel, refund)
- Inspection **Undo Demolish** (placement aktif değilken)
- Refund geri alma + TryPlaceFromSave
- Yeni place / load / ikinci demolish üzerine yazar

Dahil değil:

- Multi-select undo

## 3. Definition of Done

- Undo refund'u geri alır ve binayı restore eder
- Testler geçer
