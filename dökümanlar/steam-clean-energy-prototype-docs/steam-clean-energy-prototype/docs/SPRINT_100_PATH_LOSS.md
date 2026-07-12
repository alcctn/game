# SPRINT 100 — PATH-BASED TRANSMISSION HOPS

## 1. Sprint Amacı

Transmission loss hop sayısını Manhattan yerine network edge BFS ile hesaplamak.

## 2. Kapsam

Dahil:

- `TransmissionLoss` BFS hop (üretici → en yakın consumer/storage)
- Placement preview aynı kural (occupancy graph + hub edges)
- `LossPerHop` / `MinDeliveryFactor` aynı
- `PathLossTests`

Dahil değil:

- Max-flow
- Kapasite tabanlı routing

## 3. Definition of Done

- Uzak Manhattan ama kısa path → düşük kayıp
- Placement aynı hop kuralını kullanır
- Testler geçer
