# SPRINT 107 — REPAIR UNDO

## 1. Sprint Amacı

Son repair batch’lerini (tekli / multi / global) geri al.

## 2. Kapsam

Dahil:

- `RepairUndoService` LIFO stack, depth ≤3
- Refund maliyet + önceki maintenance seviyeleri
- Demolish undo stack ayrı kalır
- Inspection **Undo Repair**

Dahil değil:

- Ctrl+Z ile repair (Ctrl+Z demolish’de kalır)

## 3. Definition of Done

- Single / multi / global repair undo çalışır
- 4. batch en eski grubu düşürür
- Testler geçer
