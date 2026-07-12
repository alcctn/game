# SPRINT 05 — SCENARIO OBJECTIVES

## 1. Sprint Amacı

Köy enerji talebini karşılama, kaynak çeşitliliği ve batarya hedefleriyle prototip senaryosunu tamamlanabilir hale getirmek.

## 2. Kapsam

Dahil:

- ScenarioDefinition (Yeşil Vadi)
- ScenarioProgressService (tick ilerleme)
- Kazanma: %95 coverage streak + ≥2 kaynak tipi + batarya
- Soft-lose: kesinti → memnuniyet düşüşü + risk uyarısı
- Scenario HUD (checklist + win)
- Generate’de sıfırlama
- EditMode testleri

Dahil değil:

- Araştırma ağacı
- Kayıt / yükleme
- Gün-gece talep eğrisi
- Sert kaybetme
- Tutorial quest zinciri

## 3. Kazanma

1. Demand coverage ≥ %95, kesintisiz N tick
2. En az iki aktif üretici tipi (water_wheel / small_solar / small_wind)
3. Ağa bağlı en az bir batarya

Hepsi sağlanınca senaryo kazanılır; simülasyon duraklar.

## 4. Definition of Done

- Checklist HUD’da güncellenir
- Win paneli görünür
- Generate senaryoyu sıfırlar
- Testler geçer
