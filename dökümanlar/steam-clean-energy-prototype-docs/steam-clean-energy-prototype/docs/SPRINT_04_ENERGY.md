# SPRINT 04 — ENERGY NETWORK

## 1. Sprint Amacı

Yerleştirilen yapıları enerji düğümlerine bağlamak; tick tabanlı üretim, talep, depolama ve şebeke dengesi çalıştırmak.

## 2. Kapsam

Dahil:

- SimulationClock (pause / 1x / 2x / 4x)
- IEnergyProducer / IEnergyConsumer / IEnergyStorage
- EnergyNetworkGraph + Service
- EnergyBalanceCalculator + StorageDispatch
- village, battery, power_line
- Energy HUD
- EditMode testleri

Dahil değil:

- Araştırma ağacı
- Kayıt / yükleme
- Senaryo kazanma / kaybetme
- Gün-gece döngüsü

## 3. Tick Akışı

1. Bileşen üretimi hesapla
2. Talebi karşıla
3. Fazlayı bataryaya şarj et
4. Batarya doluysa fazla sat
5. Açığı bataryadan kapat; kalırsa kesinti

## 4. Definition of Done

- Köy + üretici + hat ile HUD güncellenir
- Batarya fazla / açığı yumuşatır
- Generate ağı sıfırlar
- Testler geçer
