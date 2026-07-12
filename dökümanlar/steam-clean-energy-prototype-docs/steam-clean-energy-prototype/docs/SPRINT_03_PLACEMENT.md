# SPRINT 03 — BUILDING PLACEMENT

## 1. Sprint Amacı

Kaynak katmanları üzerine kural tabanlı bina yerleştirme, hayalet önizleme ve basit para sistemini eklemek.

## 2. Kapsam

Dahil:

- BuildingDefinition ScriptableObject
- Su çarkı, küçük güneş, küçük rüzgâr
- PlacementRule seti ve PlacementValidator
- GridOccupancyService
- PlacementPreview / PlacementController
- Wallet
- Prosedürel primitif görseller
- EditMode testleri

Dahil değil:

- Elektrik şebekesi
- Enerji dengesi simülasyonu
- Batarya / köy / hat
- Teknoloji ağacı kilidi (kural hazır, bu sprintte her zaman açık)

## 3. Yerleştirme Akışı

1. Oyuncu yapı seçer.
2. Hayalet önizleme (yeşil / kırmızı).
3. Sol tık ile yerleştirme dener.
4. Tüm kurallar çalışır; başarısızsa nedenler listelenir.
5. Başarıda para düşer, hücre işgal edilir, primitif oluşturulur.

## 4. Definition of Done

- Üç yapı türü seçilip yerleştirilebilir.
- Geçersiz konumlarda tüm nedenler görünür.
- Yeniden Generate yapıları temizler.
- EditMode testleri geçer.
