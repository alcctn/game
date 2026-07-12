# SPRINT 108 — SECOND EMERGENCY CREDIT

## 1. Sprint Amacı

Borç tamamen ödendikten sonra bir ikinci acil kredi.

## 2. Kapsam

Dahil:

- `creditUses` 0–2 (save)
- 1. grant 200; 2. grant 150 (yalnızca debt=0 iken)
- Faiz aynı kural: `ceil(debt * 0.01)` min 1 (yeni principal)
- 3. kredi yok

Dahil değil:

- Steam Cloud (S109)

## 3. Definition of Done

- Repay sonrası ikinci grant çalışır; üçüncü reddedilir
- Save `creditUses` round-trip
- Testler geçer
