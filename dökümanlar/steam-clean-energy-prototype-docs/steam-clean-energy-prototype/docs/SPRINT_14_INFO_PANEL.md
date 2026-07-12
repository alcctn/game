# SPRINT 14 — INFO PANEL

## 1. Sprint Amacı

Sağ bilgi paneli: seçili hücre / bina, verimlilik, bakım ve şebeke durumu.

## 2. Kapsam

Dahil:

- Normal modda sol tık hücre seçimi
- `InspectionPanelUI` (sağ IMGUI)
- Network status helper
- EditMode testleri

Dahil değil:

- Alt bildirim kuyruğu
- Seçim highlight mesh
- UGUI

## 3. Şebeke durumları

- No building
- Not in network
- Isolated (tek düğüm bileşen)
- Connected (bileşende başka düğüm var)

## 4. Definition of Done

- Normal view’da tıklayınca panel doluyor
- Bina / bakım / şebeke görünüyor
- Testler geçer
