# SPRINT 08 — SAVE / LOAD

## 1. Sprint Amacı

JSON tabanlı tek slot kayıt ile seed, yapılar, ekonomi, araştırma ve senaryo durumunu kalıcı hale getirmek.

## 2. Kapsam

Dahil:

- GameSaveData + SaveGameService
- SaveLoadController (Save / Load)
- Slot: `persistentDataPath/saves/slot1.json`
- Seed’den harita regenerate + bina restore
- IMGUI Save / Load
- EditMode testleri

Dahil değil:

- Çoklu slot UI
- Cloud save
- Bakım / tutorial

## 3. Load sırası

1. Seed set + Generate
2. Research / RP restore
3. Binaları ücretsiz yerleştir + batarya
4. Para / senaryo / clock restore
5. Energy network rebuild

## 4. Definition of Done

- Save sonrası Load aynı seed ve binaları getirir
- RP, unlock, batarya, senaryo korunur
- Testler geçer
