# SPRINT 96 — PAUSE SLOT DELETE

## 1. Sprint Amacı

Pause overlay’de aktif save slot’u confirm ile silmek.

## 2. Kapsam

Dahil:

- Delete Slot butonu (aktif slot)
- Confirm diyaloğu; Esc/Back iptal
- `SaveLoadController.DeleteSlot`

Dahil değil:

- Steam Cloud
- Slot metadata polish (S104)

## 3. Definition of Done

- Confirm olmadan silinmez
- Main Menu confirm ile karşılıklı exclusive
- Testler geçer
