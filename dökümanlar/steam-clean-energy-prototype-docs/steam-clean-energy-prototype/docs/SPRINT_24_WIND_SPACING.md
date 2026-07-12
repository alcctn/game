# SPRINT 24 — WIND SPACING

## 1. Sprint Amacı

Aynı tip rüzgâr türbinleri arasında minimum Manhattan mesafesi (GDD §7.4).

## 2. Kapsam

Dahil:

- `BuildingDefinition.MinSameTypeSpacing`
- `MinSameTypeSpacingRule`
- `small_wind` = 3
- EditMode testleri

Dahil değil:

- Wake / verim cezası
- Farklı tip binalar arası mesafe

## 3. Kural

`MinSameTypeSpacing <= 0` → atla.  
Aksi halde occupancy’de aynı `Id` için Manhattan &lt; spacing → fail.

## 4. Definition of Done

- Bitişik wind fail
- Mesafe ≥ 3 pass
- Solar yan yana OK
- Testler geçer
