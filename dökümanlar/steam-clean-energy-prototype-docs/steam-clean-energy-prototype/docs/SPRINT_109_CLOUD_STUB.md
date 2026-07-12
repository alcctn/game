# SPRINT 109 — STEAM CLOUD PATH STUB

## 1. Sprint Amacı

Steam Cloud için path soyutlaması (SDK yok).

## 2. Kapsam

Dahil:

- `ICloudSaveStore`
- `LocalCloudSaveStore` passthrough
- `SteamCloudSaveStoreStub` (no-op, Steamworks yok)
- `GameServices` + Bootstrap local register

Dahil değil:

- Gerçek Steamworks paketi / sync

## 3. Definition of Done

- Bootstrap local store kaydeder
- Stub cloud unavailable
- Testler geçer
