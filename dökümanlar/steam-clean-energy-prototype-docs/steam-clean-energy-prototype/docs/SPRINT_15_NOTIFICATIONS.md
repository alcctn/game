# SPRINT 15 — NOTIFICATIONS

## 1. Sprint Amacı

Alt bildirim feed’i: unlock, shortage, battery full, bakım ihtiyacı.

## 2. Kapsam

Dahil:

- NotificationService (max 6, 5 sn)
- NotificationHudUI + Controller
- EnergyCharged alanı
- EditMode testleri

Dahil değil:

- Ses / UGUI / save

## 3. Kurallar

- Max 6 satır; eski düşer
- Lifetime 5 sn
- Battery full: 3 sn cooldown

## 4. Definition of Done

- Olaylar altta görünür
- Cap / expire / cooldown testleri geçer
