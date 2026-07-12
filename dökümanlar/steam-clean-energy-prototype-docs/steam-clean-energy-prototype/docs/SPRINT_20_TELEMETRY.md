# SPRINT 20 — SESSION TELEMETRY

## 1. Sprint Amacı

GDD §17 dengeleme metriklerini play oturumunda sayaç olarak tutmak.

## 2. Kapsam

Dahil:

- SessionTelemetryService
- TelemetryController hooks
- TelemetryHudUI (sol alt, her zaman)
- Generate reset
- EditMode testleri

Dahil değil:

- Save persist
- Analytics backend

## 3. Metrikler

- Time to first building / production
- Invalid placement attempts
- Preferred debug layer
- Avg shortage ratio
- Scenario end elapsed + fail reason

## 4. Definition of Done

- HUD sayaçları gösterir
- Generate sıfırlar
- Testler geçer
