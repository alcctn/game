# SPRINT 75 — MULTI-SELECT DEMOLISH UNDO

## 1. Sprint Amacı

Shift+click çoklu seçim ve grup demolish undo.

## 2. Kapsam

Dahil:

- Shift+click add/remove (max 8)
- Inspection **Demolish Selected**
- Tek undo grubu (N snapshot); Ctrl+Z grubu geri alır
- Place/load temizler

Dahil değil:

- Çok seviyeli undo stack

## 3. Definition of Done

- Grup undo tek Ctrl+Z ile geri gelir
- Testler geçer
