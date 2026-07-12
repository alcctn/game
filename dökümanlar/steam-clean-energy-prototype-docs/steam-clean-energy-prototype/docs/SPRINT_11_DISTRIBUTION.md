# SPRINT 11 — DISTRIBUTION HUB

## 1. Sprint Amacı

Uzun menzilli yerel şebeke merkezi (`distribution_hub`) eklemek; kısa `power_line` rölesinden ayırmak.

## 2. Kapsam

Dahil:

- `distribution_hub` binası (Network, IsNetworkHub, range 10)
- Catalog / research always-unlock / scene wiring
- Hub tespiti yalnızca `IsNetworkHub` (power_line hardcode yok)
- EditMode testleri

Dahil değil:

- Hat kapasite limiti
- Peer-to-peer bağlantı
- Yerleştirmede şebeke zorunluluğu
- Tutorial adımı

## 3. Kurallar

| Bina | Menzil | Cost |
|------|--------|------|
| power_line | 5 | 40 |
| distribution_hub | 10 | 120 |

Bağlantı: Hub → enerji düğümü (Manhattan ≤ hub range); Hub → hub (`max` range).

## 4. Definition of Done

- Uzak düğümler hub ile tek bileşende birleşir
- Kısa menzil aynı mesafede birleştirmez
- Testler geçer
