# SPRINT 06 — RESEARCH TREE

## 1. Sprint Amacı

Araştırma puanı ile teknoloji ağacını açmak; güneş/rüzgâr kilidini ve verimlilik upgrade’lerini oyuna bağlamak.

## 2. Kapsam

Dahil:

- ResearchPointWallet
- 3 dal × 2 düğüm (hidro / güneş / rüzgâr)
- Yerleştirme kilidi
- Efficiency bonus
- Research HUD
- Generate reset
- EditMode testleri

Dahil değil:

- 3. seviye düğümler
- Yeni bina asset’leri
- Senaryo win’e research
- Kayıt / yükleme

## 3. Ağaç

| Dal | Node 1 | Node 2 |
|-----|--------|--------|
| Hydro | water_wheel (start) | +0.1 efficiency |
| Solar | unlock small_solar (15 RP) | +0.1 efficiency (25 RP) |
| Wind | unlock small_wind (20 RP) | +0.1 efficiency (25 RP) |

## 4. RP

- Coverage ≥ %95 tick: +1 RP
- İlk ≥2 aktif kaynak tipi: +10 RP (bir kez)

## 5. Definition of Done

- Solar/wind kilitli başlar; unlock sonrası yerleştirilir
- Efficiency üretimi artırır
- Generate research’i sıfırlar
- Testler geçer
