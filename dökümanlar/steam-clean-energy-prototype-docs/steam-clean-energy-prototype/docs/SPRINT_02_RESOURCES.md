# SPRINT 02 — RESOURCE LAYERS

## 1. Sprint Amacı

Sprint 01 arazi ve grid altyapısının üzerine su akışı, güneş / rüzgâr potansiyeli, biyom ve genişletilmiş inşa edilebilirlik katmanlarını eklemek.

Sprint sonunda oyuncu Normal / Yükseklik / Eğim / Su / Güneş / Rüzgâr debug katmanları arasında geçiş yapabilmelidir.

## 2. Kapsam

Dahil:

- `GridCellData` kaynak alanları
- D8 su akış yönü ve birikimi
- Akarsu / göl işaretleme
- Güneş ve rüzgâr potansiyeli
- Biyom atama
- İnşa edilebilirlik kurallarının genişletilmesi
- Debug görünümleri ve EditMode testleri

Dahil değil:

- Bina yerleştirme
- Enerji şebekesi
- Ekonomi / araştırma
- Görsel art / prefab paketleri

## 3. Boru Hattı Sırası

1. Height map + terrain + grid (Sprint 01)
2. Eğim + bakı (aspect)
3. Su akış yönü (D8)
4. Akış birikimi ve debi
5. Akarsu / göl hücreleri
6. Güneş potansiyeli
7. Rüzgâr potansiyeli
8. Biyom
9. İnşa edilebilirlik
10. Debug katmanları

## 4. Definition of Done

- Aynı seed aynı kaynak katmanlarını üretir.
- Su / güneş / rüzgâr değerleri NaN üretmez ve makul aralıktadır.
- Su hücreleri inşa edilemez.
- `Test_Terrain` sahnesinde katmanlar runtime’da değiştirilebilir.
- İlgili EditMode testleri geçer.
