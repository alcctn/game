# CURSOR RULES

Bu belge Cursor ile geliştirilecek Unity / C# projesi için çalışma kurallarını tanımlar.

## 1. Genel Çalışma Biçimi

- Bir seferde yalnızca küçük ve test edilebilir bir görevi uygula.
- Büyük özellikleri alt görevlere böl.
- İstenen kapsam dışında dosya değiştirme.
- Kod yazmadan önce mevcut ilgili dosyaları incele.
- Her görev sonunda değişen dosyaları ve test yöntemini özetle.
- Belirsiz durumda mevcut mimari ve isimlendirmeyle uyumlu en sade çözümü seç.

## 2. Dil ve Kod Standardı

- Programlama dili C#.
- Unity LTS ile uyumlu kod yaz.
- Sınıf, metot ve public üyelerde İngilizce isim kullan.
- Kod yorumları kısa ve yalnızca gerekli yerlerde olsun.
- Public API için XML documentation kullanılması tercih edilir.
- Nullable risklerini kontrol et.
- Magic number kullanma; ayar veya sabit tanımla.

## 3. Tasarım İlkeleri

- Single Responsibility Principle uygula.
- Büyük “God Manager” sınıfları oluşturma.
- MonoBehaviour sınıflarını ince tut.
- Saf hesaplama mantığını mümkün olduğunca normal C# sınıflarında tut.
- Veri tanımlarında ScriptableObject kullan.
- Sistemler arasında sıkı bağımlılıktan kaçın.
- Global singleton kullanımını minimumda tut.
- Gereksiz design pattern ekleme.

## 4. Proje Mimarisi

Kodları uygun modül klasörlerine yerleştir:

- Core
- Camera
- Map
- Grid
- Terrain
- Resources
- Buildings
- Placement
- Energy
- Economy
- Research
- Settlements
- Simulation
- SaveLoad
- UI
- Debug

Bir modül başka modüle erişirken açık arayüz veya servis kullanmalıdır.

## 5. Veri Odaklı Tasarım

Aşağıdaki değerleri kod içine sabitleme:

- Bina maliyetleri
- Üretim kapasitesi
- Verim oranları
- Bakım maliyetleri
- Teknoloji gereksinimleri
- Yerleştirme sınırları
- Senaryo hedefleri

Bina, teknoloji, biyom ve senaryo tanımlarını ScriptableObject veya yapılandırma dosyalarından oku.

## 6. Unity Kuralları

- `FindObjectOfType`, `GameObject.Find` ve benzeri pahalı aramaları sürekli kullanma.
- `Update()` içinde tahsis oluşturan koddan kaçın.
- Ağır simülasyon hesaplarını her frame çalıştırma.
- Inspector alanlarında `[SerializeField] private` tercih et.
- Prefab ve scene referanslarını null kontrolüyle doğrula.
- Runtime sırasında hata oluşursa anlaşılır log üret.
- Editör araçları `Editor` klasöründe tutulmalıdır.

## 7. Harita ve Grid Kuralları

- Görsel terrain ile mantıksal grid verisini ayır.
- Aynı seed ve ayarlar aynı sonucu üretmelidir.
- Harita üretim aşamalarını bağımsız sınıflarda uygula.
- Grid koordinat dönüşümlerini tek servis üzerinden yap.
- Dünya koordinatı → grid koordinatı dönüşümünü farklı yerlerde tekrar yazma.
- Hücre verisini erişim servisinden al; doğrudan koleksiyonlara rastgele erişme.

## 8. Bina Yerleştirme Kuralları

- Yerleştirme doğrulamasında tek bir dev metot oluşturma.
- Her kural ayrı bir `PlacementRule` olmalıdır.
- Geçersiz yerleştirmelerde tüm nedenleri döndür.
- Önizleme ile gerçek yerleştirme aynı doğrulama servisini kullanmalıdır.
- Para düşümü yalnızca başarılı yerleştirmeden sonra yapılmalıdır.
- Grid işgali ve prefab oluşturma işlemleri başarısızlık durumunda geri alınabilir olmalıdır.

## 9. Enerji Sistemi Kuralları

- Üretici, tüketici ve depolama davranışlarını arayüzlerle ayır.
- Şebeke grafiğini yalnızca bağlantı değiştiğinde yeniden oluştur.
- Enerji hesaplamaları deterministik olmalıdır.
- Üretim ve tüketim birimlerini açıkça adlandır.
- Negatif enerji veya kapasite değerlerine izin verme.
- Enerji açığı ve fazla üretim ayrı değerler olarak raporlanmalıdır.

## 10. UI Kuralları

- UI doğrudan oyun durumunu değiştirmemelidir.
- UI, servis veya komut çağrısı yapmalıdır.
- Görsel metinleri mümkün olduğunca merkezi lokalizasyon anahtarlarıyla yönetmeye uygun yaz.
- Debug bilgilerini oyuncu arayüzünden ayır.
- Panel açma / kapama mantığını kopyalama.

## 11. Hata Yönetimi

- Sessizce başarısız olma.
- Beklenen kullanıcı hatalarında exception fırlatma; sonuç modeli döndür.
- Beklenmeyen programlama hatalarında açıklayıcı exception veya log kullan.
- Log mesajına sistem adı ve ilgili kimliği ekle.

Örnek:

```text
[Placement] Building 'small_wind_turbine' could not be placed at (12, 8): slope exceeds limit.
```

## 12. Test Kuralları

Yeni saf mantık sınıfları için EditMode testi ekle.

Öncelikli test alanları:

- Seed determinismi
- Grid koordinat dönüşümü
- Eğim hesabı
- Yerleştirme doğrulaması
- Enerji üretim formülü
- Depolama şarj / deşarj mantığı
- Teknoloji kilidi

Her görev sonunda şu bilgileri ver:

1. Değişen dosyalar
2. Eklenen davranış
3. Test veya doğrulama adımları
4. Bilinen sınırlamalar

## 13. Kod Üretiminde Yasaklar

- Tüm oyunu tek prompt ile yazmaya çalışma.
- Çalışmayan sahte implementasyon bırakma.
- `NotImplementedException` bırakma; görev dışıysa arayüzü ekleme.
- Mevcut çalışan sistemi gerekmedikçe yeniden yazma.
- Yeni paket veya üçüncü taraf kütüphane ekleme.
- Kullanıcının onayı olmadan render pipeline değiştirme.
- Kullanıcının onayı olmadan klasör yapısını kökten değiştirme.

## 14. Cursor Görev Şablonu

Her yeni görev aşağıdaki formatta verilmelidir:

```text
Amaç:

Kapsam:

Değiştirilecek veya oluşturulacak dosyalar:

Teknik kurallar:

Kabul kriterleri:

Test adımları:

Kapsam dışı:
```

## 15. İlk Görevlerde Öncelik

1. Proje ve klasör yapısı
2. Kamera kontrolü
3. Mantıksal grid
4. Seed tabanlı height map
5. Terrain üretimi
6. Eğim hesabı
7. Debug katmanları

Enerji sistemi, bina yerleştirme ve ekonomi; arazi altyapısı doğrulanmadan başlatılmamalıdır.
