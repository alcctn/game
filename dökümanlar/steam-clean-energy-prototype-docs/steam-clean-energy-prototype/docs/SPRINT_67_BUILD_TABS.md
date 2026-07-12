# SPRINT 67 — BUILD MENU CATEGORY TABS

## 1. Sprint Amacı

Build menüsünü `BuildingCategory` sekmelerine ayırmak; son sekmeyi kalıcı tutmak.

## 2. Kapsam

Dahil:

- Sekmeler: Energy / Network / Storage / Settlement / Service
- Aktif sekme yalnızca o kategorideki unlocked binaları listeler
- `PlayerPrefs` anahtarı `ce_build_tab`

Dahil değil:

- UGUI rebuild
- Kilitli binaları sekmede gösterme (locked satır opsiyonel; filtre unlocked)

## 3. Definition of Done

- Sekme değişince liste filtrelenir
- Restart sonrası son sekme geri gelir
- Testler geçer
