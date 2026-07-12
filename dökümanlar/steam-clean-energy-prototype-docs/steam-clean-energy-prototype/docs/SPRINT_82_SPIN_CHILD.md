# SPRINT 82 — SPIN CHILD

## 1. Sprint Amacı

`RotatingVisual` dönüş hedefini `Spin` child’a yönlendirmek.

## 2. Kapsam

Dahil:

- Child adı `Spin` varsa o transform döner; yoksa root
- `ResolveSpinTarget` + Configure cache
- RPM tablosu aynı (wind 40, hydro/wheel 25)

Dahil değil:

- Blade mesh art
- Diğer bina id’leri

## 3. Definition of Done

- Spin varken child döner, parent sabit
- Spin yokken root döner
- Testler geçer
