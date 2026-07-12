# SPRINT 01 — TERRAIN FOUNDATION

## 1. Sprint Amacı

Seed tabanlı, tekrar üretilebilir bir 3D arazi oluşturmak ve bu arazinin altında çalışan 64 × 64 mantıksal grid altyapısını hazırlamak.

Sprint sonunda oyuncu haritada gezinebilmeli, yeni seed ile arazi oluşturabilmeli ve yükseklik / eğim debug katmanlarını görüntüleyebilmelidir.

## 2. Sprint Süresi

Önerilen süre:

- 1–2 hafta

## 3. Sprint Kapsamı

Dahil:

- Unity proje kurulumu
- Git deposu
- Klasör yapısı
- Cursor kuralları
- Test sahnesi
- İzometrik kamera
- Mantıksal grid
- Seed sistemi
- Noise tabanlı yükseklik haritası
- 3D terrain üretimi
- Eğim hesabı
- Yeniden üretme aracı
- Yükseklik ve eğim debug görünümü
- Temel EditMode testleri

Dahil değil:

- Nehir üretimi
- Güneş ve rüzgâr potansiyeli
- Bina yerleştirme
- Enerji sistemi
- Ekonomi
- Araştırma
- Kayıt / yükleme

## 4. Teknik Varsayımlar

- Unity LTS
- URP
- C#
- Unity Terrain veya prosedürel mesh seçeneklerinden biri
- 64 × 64 grid
- Seed tabanlı deterministik üretim

İlk uygulama için Unity Terrain önerilir. Daha sonra ihtiyaç halinde mesh tabanlı sisteme geçiş değerlendirilebilir.

## 5. User Story’ler

### US-01 — Proje altyapısı

Bir geliştirici olarak düzenli bir klasör yapısına ve test sahnesine sahip olmak istiyorum; böylece sistemleri kontrollü şekilde geliştirebilirim.

Kabul kriterleri:

- Proje açıldığında hata vermemeli.
- Önerilen klasör yapısı oluşturulmuş olmalı.
- `Test_Terrain` sahnesi bulunmalı.
- Git deposu ve `.gitignore` hazırlanmış olmalı.
- Cursor kural dosyası projeye eklenmiş olmalı.

### US-02 — Kamera kontrolü

Bir oyuncu olarak haritayı rahatça incelemek istiyorum.

Kabul kriterleri:

- WASD ile hareket.
- Fare tekerleği ile zoom.
- Q / E ile dönüş.
- Kamera harita dışına çıkmamalı.
- Hareket farklı frame rate değerlerinde tutarlı olmalı.

### US-03 — Mantıksal grid

Bir sistem geliştiricisi olarak arazinin her bölümüne ait veriyi hücre bazında saklamak istiyorum.

Kabul kriterleri:

- Grid boyutu ayarlardan alınmalı.
- Her hücrenin koordinatı ve dünya merkezi olmalı.
- Dünya konumu grid koordinatına çevrilebilmeli.
- Grid koordinatı dünya konumuna çevrilebilmeli.
- Sınır dışı erişimler güvenli sonuç döndürmeli.

### US-04 — Seed sistemi

Bir oyuncu olarak aynı seed ile aynı haritayı tekrar oluşturmak istiyorum.

Kabul kriterleri:

- Seed metin veya sayı olarak girilebilmeli.
- Aynı seed ve ayarlar aynı height map’i üretmeli.
- Rastgele seed oluşturma düğmesi bulunmalı.
- Aktif seed arayüzde görünmeli.

### US-05 — Height map üretimi

Bir geliştirici olarak doğal görünümlü arazi şekilleri oluşturmak istiyorum.

Kabul kriterleri:

- Çok katmanlı noise kullanılmalı.
- Noise ölçeği ayarlanabilir olmalı.
- Octave, persistence ve lacunarity değerleri ayarlanabilir olmalı.
- Yükseklikler normalize edilmeli.
- Harita kenarlarında ciddi kopma veya NaN oluşmamalı.

### US-06 — Terrain oluşturma

Bir oyuncu olarak oluşturulan height map’i 3D arazi olarak görmek istiyorum.

Kabul kriterleri:

- Terrain height map verisini kullanmalı.
- Grid ve terrain koordinatları hizalı olmalı.
- Terrain boyutu ayarlardan alınmalı.
- Yeniden üretimde eski terrain temizlenmeli veya güncellenmeli.

### US-07 — Eğim hesabı

Bir geliştirici olarak her hücrenin eğimini bilmek istiyorum; böylece ileride yerleştirme kuralları uygulanabilir.

Kabul kriterleri:

- Her hücre için eğim değeri hesaplanmalı.
- Eğim derece veya normalize değer olarak açıkça tanımlanmalı.
- Düz bölgeler düşük, dik bölgeler yüksek değer üretmeli.
- Kenar hücreleri güvenli şekilde hesaplanmalı.

### US-08 — Debug katmanları

Bir geliştirici olarak harita verilerini görsel olarak doğrulamak istiyorum.

Kabul kriterleri:

- Normal görünüm.
- Yükseklik görünümü.
- Eğim görünümü.
- Görünüm modu runtime sırasında değiştirilebilmeli.
- Hücre seçildiğinde yükseklik ve eğim bilgisi gösterilmeli.

## 6. Görev Listesi

### T01 — Unity projesini oluştur

Çıktı:

- URP Unity projesi
- Temel kalite ayarları
- `Test_Terrain` sahnesi

### T02 — Git yapılandırması

Çıktı:

- Unity uyumlu `.gitignore`
- İlk commit
- Branch yapısı başlangıcı

### T03 — Klasör yapısını oluştur

Çıktı:

- `TECHNICAL_ARCHITECTURE.md` ile uyumlu klasörler

### T04 — Cursor kural dosyasını ekle

Önerilen yol:

```text
.cursor/rules/project-rules.mdc
```

İçerik:

- `CURSOR_RULES.md` kurallarının Cursor formatına uyarlanmış hali

### T05 — Harita ayar verisini oluştur

Dosya önerileri:

- `MapGenerationSettings.cs`
- `MapGenerationSettings.asset`

Alanlar:

- Grid genişliği
- Grid yüksekliği
- Terrain dünya boyutu
- Maksimum yükseklik
- Noise scale
- Octaves
- Persistence
- Lacunarity
- Seed

### T06 — GridCoordinate oluştur

Dosya:

- `GridCoordinate.cs`

Kabul:

- Immutable veya readonly yapı
- Eşitlik karşılaştırması
- Debug için `ToString()`

### T07 — GridCellData oluştur

Dosya:

- `GridCellData.cs`

İlk sprint alanları:

- X
- Y
- WorldPosition
- Elevation
- Slope
- IsBuildable — şimdilik eğime göre

### T08 — GridService oluştur

Dosya:

- `GridService.cs`

Sorumluluklar:

- Grid oluşturma
- Hücre erişimi
- Dünya / grid koordinat dönüşümü
- Sınır kontrolü

### T09 — HeightMapGenerator oluştur

Dosya:

- `HeightMapGenerator.cs`

Sorumluluklar:

- Seed ve ayarlara göre normalize `float[,]` üretmek
- Unity bileşenlerine bağımlı olmadan test edilebilir olmak

### T10 — TerrainBuilder oluştur

Dosya:

- `TerrainBuilder.cs`

Sorumluluklar:

- Height map’i Unity Terrain’e uygulamak
- Terrain boyutunu ayarlamak
- Yeniden üretimi desteklemek

### T11 — SlopeCalculator oluştur

Dosya:

- `SlopeCalculator.cs`

Sorumluluklar:

- Komşu yüksekliklerden hücre eğimi hesaplamak
- Sonucu GridCellData’ya yazmak

### T12 — MapGenerator orkestrasyonu

Dosya:

- `MapGenerator.cs`

Akış:

1. Ayarları doğrula
2. Height map üret
3. Terrain oluştur
4. Grid oluştur
5. Hücre yüksekliklerini doldur
6. Eğimleri hesapla
7. `MapGenerated` olayı yayınla

### T13 — Kamera sistemi

Dosyalar:

- `IsometricCameraController.cs`
- `CameraBounds.cs`

### T14 — Debug overlay sistemi

Dosyalar:

- `MapDebugOverlay.cs`
- `DebugViewMode.cs`

Yaklaşım seçenekleri:

- Terrain material parametreleri
- Hücre üstü renkli quad / mesh

Prototipte performansı yeterli olan en sade yaklaşım seçilmelidir.

### T15 — Debug UI

İçerik:

- Seed giriş alanı
- Random seed düğmesi
- Generate düğmesi
- Normal / yükseklik / eğim seçimleri
- Seçili hücre bilgi paneli

### T16 — EditMode testleri

Testler:

- Aynı seed aynı height map
- Farklı seed farklı height map
- Height map değerleri 0–1 aralığında
- Grid dönüşümleri tutarlı
- Düz haritada eğim sıfıra yakın
- Dik örnek veride eğim yüksek

## 7. Önerilen Uygulama Sırası

1. T01
2. T02
3. T03
4. T04
5. T05
6. T06
7. T07
8. T08
9. T09
10. T16’nın height map testleri
11. T10
12. T11
13. T16’nın eğim testleri
14. T12
15. T13
16. T14
17. T15
18. Genel kabul testi

## 8. Definition of Done

Bir görev tamamlanmış sayılırsa:

- Kod derleniyor.
- Console’da hata yok.
- Kabul kriterleri karşılanıyor.
- İlgili testler geçiyor.
- Yeni inspector alanları anlaşılır isimlendirilmiş.
- Değişen dosyalar özetlenmiş.
- Bilinen sınırlamalar belgelenmiş.

## 9. Sprint Kabul Testi

### Test 1 — Deterministik harita

1. Seed değerini `12345` yap.
2. Haritayı oluştur.
3. Ekran görüntüsü veya belirli hücre değerlerini kaydet.
4. Farklı seed ile üret.
5. Tekrar `12345` ile üret.

Beklenen:

- İlk `12345` haritasıyla aynı sonuç.

### Test 2 — Kamera

1. Haritanın dört kenarına hareket et.
2. Maksimum ve minimum zoom dene.
3. Kamerayı döndür.

Beklenen:

- Kamera harita dışında kaybolmaz.
- Hareket akıcıdır.

### Test 3 — Grid doğruluğu

1. Farklı noktalara tıkla.
2. Grid koordinatını görüntüle.
3. Kenar hücrelerini kontrol et.

Beklenen:

- Koordinatlar doğru.
- Sınır dışı hata yok.

### Test 4 — Yükseklik katmanı

Beklenen:

- Düşük ve yüksek bölgeler görsel olarak ayırt edilir.
- Hücre bilgi değeri terrain yüksekliğiyle uyumludur.

### Test 5 — Eğim katmanı

Beklenen:

- Düz alanlar düşük değer.
- Dağ yamaçları yüksek değer.
- NaN veya sonsuz değer yok.

## 10. Sprint Sonu Çıktıları

- Çalışan Unity prototip sahnesi
- Seed tabanlı prosedürel terrain
- 64 × 64 mantıksal grid
- Hücre yükseklik ve eğim verileri
- İzometrik kamera
- Debug UI
- Yükseklik / eğim overlay
- EditMode testleri
- Sprint 02 için hazır altyapı

## 11. Sprint 02 Ön İzleme

Sonraki sprintte planlananlar:

- Su akış yönü
- Akış birikimi
- Akarsu üretimi
- Su debisi katmanı
- Güneş potansiyeli
- Rüzgâr potansiyeli
- Biyom atama
- İnşa edilebilirlik kurallarının genişletilmesi
