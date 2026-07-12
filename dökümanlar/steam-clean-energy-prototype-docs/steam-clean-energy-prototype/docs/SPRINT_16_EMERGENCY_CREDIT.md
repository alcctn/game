# SPRINT 16 — EMERGENCY CREDIT

## 1. Sprint Amacı

Para upkeep sonrası 0 olduğunda bir kez acil yardım kredisi (+200).

## 2. Kapsam

Dahil:

- EmergencyCreditService
- Driver tick (upkeep sonrası)
- Bildirim
- Save/Load + Generate reset
- EditMode testleri

Dahil değil:

- Faiz / ikinci credit / borç

## 3. Kurallar

- Money <= 0 ve unused → +200, used=true, notify
- İkinci kez → no-op
- Generate → flag reset; Load → restore

## 4. Definition of Done

- Credit bir kez çalışır
- Bildirim görünür
- Save korur
- Testler geçer
