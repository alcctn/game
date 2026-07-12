# GAME DESIGN — PROTOTYPE

## 1. Tasarım Hedefi

Prototipin ana amacı eğlenceli ve anlaşılır bir “arazi analizi → doğru yatırım → enerji üretimi → gelişim” döngüsünü doğrulamaktır.

Simülasyon gerçekçi hissedilmeli ancak mühendislik yazılımı kadar karmaşık olmamalıdır.

## 2. Oyuncu Deneyimi İlkeleri

- Oyuncu kararlarının sonucunu hızlı görmelidir.
- Verimlilik değerlerinin nedeni açıklanmalıdır.
- Harita yalnızca dekor değil, oyun sisteminin merkezidir.
- Farklı enerji kaynakları birbirini tamamlamalıdır.
- Başarısızlık cezalandırıcı olmaktan çok öğretici olmalıdır.

## 3. Kamera ve Kontroller

### Kamera

- İzometrik 3D görünüm
- Döndürme
- Yakınlaştırma / uzaklaştırma
- Harita üzerinde sürükleme
- Yapı seçildiğinde odaklanma

### Önerilen kontroller

- WASD: kamera hareketi
- Orta fare: kamera sürükleme
- Fare tekerleği: zoom
- Q / E: kamera döndürme
- Sol tık: seçim / yerleştirme
- Sağ tık veya Escape: iptal
- R: yapı döndürme
- F1–F8: kaynak katmanları

## 4. Harita Modeli

Harita görsel olarak 3D terrain, sistemsel olarak mantıksal hücrelerden oluşur.

Her hücre aşağıdaki temel verileri taşır:

- Koordinat
- Yükseklik
- Eğim
- Bakı yönü
- Su varlığı
- Su debisi
- Güneş potansiyeli
- Rüzgâr potansiyeli
- Biyom
- Ağaç yoğunluğu
- İnşa edilebilirlik
- Üzerindeki yapı
- Elektrik ağı bağlantısı

## 5. Kaynak Katmanları

Oyuncu aşağıdaki görünüm modları arasında geçiş yapabilir:

1. Normal görünüm
2. Yükseklik
3. Eğim
4. Su akışı / debi
5. Güneş potansiyeli
6. Rüzgâr potansiyeli
7. Elektrik ağı
8. Enerji üretimi
9. Enerji talebi
10. Çevresel etki — sonraki sürüm

Renk skalaları prototipte basit tutulmalıdır:

- Düşük potansiyel
- Orta potansiyel
- Yüksek potansiyel

## 6. Enerji Üretim Modeli

Temel formül:

`Gerçek Üretim = Kurulu Güç × Kaynak Potansiyeli × Yapı Verimi × Durum Katsayısı`

Örnek katsayılar:

- Kaynak potansiyeli: 0.00–1.00
- Yapı verimi: 0.50–0.95
- Durum katsayısı: 0.00–1.00

Durum katsayısını etkileyenler:

- Bakım durumu
- Şebeke kapasitesi
- Depolama doluluğu
- Geçici çevre koşulu

Prototipte gerçek dünya birimleri yerine dengeli oyun birimleri kullanılabilir.

## 7. Enerji Yapıları

### 7.1 Su Çarkı

Rol:

- Başlangıç hidro teknolojisi
- Düşük maliyet
- Düşük ve sürekli üretim

Yerleştirme şartları:

- Akarsu hücresine bitişik
- Minimum su debisi
- Uygun kıyı eğimi

Avantaj:

- Ucuz
- Kararlı üretim

Dezavantaj:

- Düşük kapasite
- Sadece belirli nehir noktalarında kurulabilir

### 7.2 Küçük Hidro Türbin

Rol:

- Orta seviye hidro üretimi

Yerleştirme şartları:

- Yüksek su debisi
- Belirli kot farkı veya eğim
- Daha pahalı bağlantı

Avantaj:

- Su çarkından daha verimli

Dezavantaj:

- Daha yüksek yatırım ve bakım maliyeti

### 7.3 Küçük Güneş Paneli Dizisi

Rol:

- Kolay kurulan, gündüz üretim yapan enerji kaynağı

Yerleştirme şartları:

- İnşa edilebilir arazi
- Düşük gölgelenme
- Kabul edilebilir eğim

Avantaj:

- Hızlı kurulum
- Az bakım

Dezavantaj:

- Gece üretmez
- Gölge ve bakı yönünden etkilenir

### 7.4 Küçük Rüzgâr Türbini

Rol:

- Yüksek ve açık alanlarda üretim

Yerleştirme şartları:

- Minimum rüzgâr potansiyeli
- Yakında engel yoğunluğunun düşük olması
- Türbinler arası minimum mesafe

Avantaj:

- Günün farklı saatlerinde üretim

Dezavantaj:

- Dalgalı üretim
- Yanlış konumda ciddi verim kaybı

## 8. Destek Yapıları

### Elektrik Hattı

- Üretim ve tüketim yapılarını şebekeye bağlar.
- Uzun bağlantılar daha pahalıdır.
- Prototipte kapasite limiti uygulanabilir.

### Dağıtım Noktası

- Yerel şebeke merkezi görevi görür.
- Yakın yapıların bağlantısını kolaylaştırır.

### Batarya

- Fazla üretimi depolar.
- Talep üretimden yüksek olduğunda enerji verir.
- Kapasite ve şarj / deşarj oranı vardır.

### Bakım Binası

- Belirli yarıçaptaki yapıların bakım kaybını azaltır.
- Prototipte otomatik etki uygulayabilir.

### Köy

- Sürekli ve değişken enerji talebi oluşturur.
- Talep karşılandıkça gelir ve memnuniyet üretir.

## 9. Yerleştirme Sistemi

Yerleştirme akışı:

1. Oyuncu yapı seçer.
2. Hayalet önizleme görünür.
3. Geçerli hücreler yeşil, geçersiz hücreler kırmızı görünür.
4. Bilgi paneli beklenen üretimi ve maliyeti gösterir.
5. Oyuncu onaylar.
6. Para düşülür ve yapı oluşturulur.

Geçersiz yerleştirme nedenleri açıkça listelenir:

- Eğim çok yüksek
- Su debisi yetersiz
- Güneş potansiyeli düşük
- Rüzgâr potansiyeli düşük
- Başka yapıyla çakışıyor
- Arazi inşa edilemez
- Yetersiz para
- Teknoloji kilitli

## 10. Elektrik Ağı

Prototip için ağ mantığı:

- Her yapı bir ağ düğümüdür.
- Elektrik hatları düğümler arasında bağlantı kurar.
- Bağlantılı yapılar aynı şebeke bileşenine ait olur.
- Üretim, depolama ve talep aynı bileşen içinde hesaplanır.

Her simülasyon adımında:

1. Toplam üretim hesaplanır.
2. Önce anlık talep karşılanır.
3. Fazla enerji bataryaya gönderilir.
4. Batarya doluysa fazla enerji satılır veya boşa gider.
5. Üretim yetmezse batarya devreye girer.
6. Hâlâ açık varsa kesinti oluşur.

## 11. Ekonomi

### Para kazanımı

- Yerleşime sağlanan enerji
- Şebekeye satılan fazla enerji
- Görev ödülleri

### Giderler

- Yapı inşaat maliyeti
- Elektrik hattı maliyeti
- Periyodik bakım maliyeti
- Araştırma yatırımı — opsiyonel

### Prototip kaynakları

- Para
- Enerji
- Araştırma puanı

## 12. Araştırma Sistemi

Araştırma puanları şu yollarla kazanılabilir:

- Belirli süre kesintisiz enerji sağlama
- Verimli tesis kurma
- Görev tamamlama
- Enerji çeşitliliği sağlama

### Hidro ağacı

1. Su çarkı
2. Gelişmiş su çarkı
3. Küçük hidro türbin

### Güneş ağacı

1. Basit güneş dizisi
2. Verimli panel
3. Akıllı inverter

### Rüzgâr ağacı

1. Küçük rüzgâr türbini
2. Gelişmiş kanat
3. Akıllı yönlendirme

Prototipte her dal için iki veya üç düğüm yeterlidir.

## 13. Köy Talebi

Köyün temel değişkenleri:

- Nüfus
- Temel talep
- Anlık talep
- Karşılanan enerji yüzdesi
- Memnuniyet
- Kesinti süresi

Talep basit bir günlük eğriyle değişebilir:

- Sabah orta
- Öğlen düşük / orta
- Akşam yüksek
- Gece düşük

Prototipte gün döngüsü hızlandırılmış olmalıdır.

## 14. Kazanma ve Kaybetme

### Kazanma koşulları

- Köy talebinin en az %95’ini belirli süre boyunca karşıla.
- İki farklı enerji kaynağını aktif kullan.
- Batarya sistemini kur.
- Hedef araştırma seviyesine ulaş.

### Kaybetme koşulları

Prototipte sert kaybetme yerine uyarı sistemi önerilir.

- Para sıfırın altına düşerse acil yardım kredisi
- Uzun kesintide memnuniyet kaybı
- Çok düşük memnuniyette senaryo başarısız

## 15. Kullanıcı Arayüzü

### Üst bar

- Para
- Anlık üretim
- Anlık talep
- Depolanan enerji
- Araştırma puanı
- Oyun hızı

### Sol yapı menüsü

- Enerji üretimi
- Şebeke
- Depolama
- Hizmet

### Sağ bilgi paneli

- Seçili hücre bilgisi
- Seçili yapı bilgisi
- Verimlilik açıklaması
- Bakım durumu
- Şebeke bağlantısı

### Alt bildirim alanı

- Teknoloji açıldı
- Enerji açığı oluştu
- Batarya doldu
- Yapı bakım istiyor

## 16. Eğitim Akışı

İlk senaryoda görevler sırayla açılır:

1. Kamera kullanımını öğren.
2. Su potansiyeli katmanını aç.
3. Su çarkı kur.
4. Köye elektrik hattı çek.
5. Güneş katmanını incele.
6. Güneş paneli kur.
7. Batarya araştır.
8. Batarya kur.
9. Talebi belirli süre karşıla.

## 17. Dengeleme İçin İzlenecek Veriler

- İlk yapının kurulma süresi
- İlk enerji üretimine kadar geçen süre
- Yanlış yerleştirme sayısı
- Oyuncunun en çok kullandığı kaynak katmanı
- Ortalama kesinti oranı
- Senaryo bitirme süresi
- Başarısızlık nedeni
