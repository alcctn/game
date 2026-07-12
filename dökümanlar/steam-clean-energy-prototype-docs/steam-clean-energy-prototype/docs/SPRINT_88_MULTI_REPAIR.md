# SPRINT 88 — MULTI-SELECT REPAIR

## 1. Sprint Amacı

Multi-select ile seçili producer’ları atomik onar.

## 2. Kapsam

Dahil:

- Inspection **Repair Selected**
- Max 8 hücre (demolish ile aynı)
- Maliyet = sum(manual repair); atomic spend; depot gerekmez

Dahil değil:

- Repair undo

## 3. Definition of Done

- Yalnızca seçili producer’lar onarılır
- Yetersiz parada hiçbiri onarılmaz
- Testler geçer
