# SPRINT 17 — SCENARIO FAIL

## 1. Sprint Amacı

Memnuniyet 0 olunca senaryo başarısız: overlay, pause, bildirim.

## 2. Kapsam

Dahil:

- HasLost state + ScenarioFailedEvent
- HUD lose overlay
- Clock pause
- Notification
- Save/Load
- EditMode testleri

Dahil değil:

- Restart butonu (Generate yeterli)
- Research win objective

## 3. Kurallar

- Satisfaction <= 0 → HasLost
- Risk banner (<= 30) soft uyarı olarak kalır
- Lost / Won sonrası Evaluate no-op

## 4. Definition of Done

- Fail overlay görünür
- Pause çalışır
- Save korur
- Testler geçer
