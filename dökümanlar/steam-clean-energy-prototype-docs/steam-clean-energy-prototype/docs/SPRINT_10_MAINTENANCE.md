# SPRINT 10 — MAINTENANCE

## 1. Sprint Amacı

Üretici bakım seviyesinin zamanla düşmesi ve `maintenance_depot` aura’sı ile onarılması.

## 2. Kapsam

Dahil:

- maintenance_depot binası
- Decay / repair tick mantığı
- Üretim × MaintenanceLevel
- Save persistence
- HUD low-maintenance uyarısı
- EditMode testleri

Dahil değil:

- Manuel repair butonu
- Dağıtım noktası
- Tutorial adımı

## 3. Kurallar

- Menzil dışı: −0.005/tick (min 0.4)
- Depot menzilinde: +0.01/tick (max 1)
- Menzil: ConnectionRange (varsayılan 5)

## 4. Definition of Done

- Bakım düşer / depot onarır
- Üretim ölçeklenir
- Save/Load korur
- Testler geçer
