# SPRINT 74 — EMERGENCY CREDIT INTEREST

## 1. Sprint Amacı

Acil kredi sonrası faiz ve manuel Repay.

## 2. Kapsam

Dahil:

- `remainingDebt` = 200 kredi sonrası
- Her tick `ceil(debt * 0.01)` min 1 faiz (principal sabit)
- Inspection/HUD Debt + Repay
- Save `creditDebt`

Dahil değil:

- İkinci kredi

## 3. Definition of Done

- Faiz principal’ı değiştirmez; Repay borcu sıfırlar
- Testler geçer
