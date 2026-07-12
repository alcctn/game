# TECHNICAL ARCHITECTURE

## 1. Teknoloji Tercihi

### Oyun motoru

Unity LTS sürümü

### Dil

C#

### Render yaklaşımı

URP önerilir.

### Hedef mimari

- Veri odaklı
- Modüler
- Test edilebilir
- Büyük merkezi yönetici sınıflarından kaçınan
- ScriptableObject tabanlı tanım dosyaları kullanan

## 2. Temel Teknik İlkeler

1. Oyun dengesi kod içine sabit yazılmamalıdır.
2. Bina tanımları ScriptableObject üzerinden yönetilmelidir.
3. Harita hücresi verisi görsel terrain’den bağımsız tutulmalıdır.
4. Simülasyon sabit zaman adımıyla çalışmalıdır.
5. Sistemler olaylar veya açık servis arayüzleri üzerinden haberleşmelidir.
6. UI, doğrudan oyun verisini değiştirmemelidir.
7. Kayıt sistemi oyun durumunun seri hale getirilebilir modellerini kullanmalıdır.

## 3. Önerilen Klasör Yapısı

```text
Assets/
  _Game/
    Art/
      Materials/
      Models/
      Textures/
      VFX/
    Audio/
      Music/
      SFX/
    Data/
      Buildings/
      Biomes/
      Technologies/
      Scenarios/
      Balance/
    Prefabs/
      Buildings/
      Environment/
      UI/
    Scenes/
      Bootstrap.unity
      MainMenu.unity
      Prototype.unity
      Test_Terrain.unity
      Test_Grid.unity
      Test_Energy.unity
    Scripts/
      Core/
      Camera/
      Map/
      Grid/
      Terrain/
      Resources/
      Buildings/
      Placement/
      Energy/
      Economy/
      Research/
      Settlements/
      Simulation/
      SaveLoad/
      UI/
      Debug/
    Tests/
      EditMode/
      PlayMode/
```

## 4. Sahne Yapısı

### Bootstrap

Sorumlulukları:

- Kalıcı servisleri başlatmak
- Ana menü veya prototip sahnesine geçmek
- Kayıt sistemi ve ayarları hazırlamak

### Prototype

Önerilen kök nesneler:

```text
PrototypeScene
  GameRoot
    GameState
    SimulationClock
    EventBus
  MapRoot
    TerrainRoot
    GridRoot
    WaterRoot
    EnvironmentRoot
  BuildingRoot
  NetworkRoot
  SettlementRoot
  CameraRoot
  UIRoot
  DebugRoot
```

## 5. Çekirdek Veri Modelleri

### GridCoordinate

```csharp
public readonly struct GridCoordinate
{
    public int X { get; }
    public int Y { get; }
}
```

### GridCellData

Önerilen alanlar:

```csharp
[Serializable]
public sealed class GridCellData
{
    public int X;
    public int Y;
    public float Elevation;
    public float Slope;
    public Vector2 Aspect;
    public float WaterFlow;
    public float SolarPotential;
    public float WindPotential;
    public BiomeType Biome;
    public bool IsWater;
    public bool IsBuildable;
    public string OccupyingBuildingId;
}
```

Not: Runtime model ile kayıt modeli ilerleyen aşamada ayrılabilir.

### BuildingDefinition

ScriptableObject alanları:

- Kimlik
- Görünen ad
- Açıklama
- Kategori
- Prefab
- Boyut
- Maliyet
- Bakım maliyeti
- Kurulu güç
- Verim
- Gerekli teknoloji
- Yerleştirme kuralları
- Şebeke bağlantı noktaları

### BuildingInstance

Runtime alanları:

- Benzersiz kimlik
- BuildingDefinition referansı
- Grid konumu
- Rotasyon
- Durum
- Bakım seviyesi
- Şebeke kimliği
- Mevcut üretim

### SettlementData

- Kimlik
- Nüfus
- Temel talep
- Anlık talep
- Memnuniyet
- Karşılanan talep oranı
- Kesinti süresi

## 6. Harita Üretim Boru Hattı

Önerilen işlem sırası:

1. Seed oluştur / yükle
2. Temel height map üret
3. Büyük ölçekli kara şekli maskesi uygula
4. Noise katmanlarını birleştir
5. İsteğe bağlı erozyon veya yumuşatma uygula
6. Terrain mesh / Unity Terrain oluştur
7. Grid hücre yüksekliklerini örnekle
8. Eğim ve bakı hesapla
9. Su akış yönünü hesapla
10. Su birikimi ve debiyi hesapla
11. Akarsu hücrelerini belirle
12. Güneş potansiyelini hesapla
13. Rüzgâr potansiyelini hesapla
14. Biyomları ata
15. İnşa edilebilirlik durumunu belirle
16. Debug katmanlarını hazırla

## 7. Harita Üretim Bileşenleri

Önerilen sınıflar:

- `MapGenerationSettings`
- `MapGenerator`
- `HeightMapGenerator`
- `TerrainBuilder`
- `GridGenerator`
- `SlopeCalculator`
- `WaterFlowCalculator`
- `SolarPotentialCalculator`
- `WindPotentialCalculator`
- `BiomeGenerator`
- `BuildabilityCalculator`

Her sınıf tek bir ana sorumluluğa sahip olmalıdır.

## 8. Su Akışı Yaklaşımı

Prototip için basitleştirilmiş D8 benzeri yöntem kullanılabilir.

Her hücre:

1. Sekiz komşusunu kontrol eder.
2. Kendisinden en düşük komşuya akar.
3. Akış yönü saklanır.
4. Yüksekten düşüğe sıralanmış hücrelerde akış birikimi hesaplanır.
5. Eşik üstündeki hücreler akarsu kabul edilir.

Çukur alanlar için prototip seçenekleri:

- Basit doldurma
- En yakın düşük noktaya yapay çıkış
- Göl hücresi olarak işaretleme

İlk sprintte su sistemi zorunlu değildir; ikinci sprintte eklenebilir.

## 9. Güneş Potansiyeli

Basit prototip modeli:

```text
SolarPotential = BaseClimateSolar
               × AspectFactor
               × SlopeFactor
               × TreeCoverFactor
               × CloudFactor
```

İlk prototipte mevsim sabit olabilir.

## 10. Rüzgâr Potansiyeli

Basit prototip modeli:

```text
WindPotential = BaseWind
              + ElevationBonus
              + RidgeBonus
              - ObstaclePenalty
```

Hakim rüzgâr yönü harita üretim ayarından alınabilir.

## 11. Bina Yerleştirme Mimarisi

Önerilen bileşenler:

- `PlacementController`
- `PlacementPreview`
- `PlacementValidator`
- `PlacementRule`
- `BuildingFactory`
- `GridOccupancyService`

### PlacementRule yaklaşımı

Her yerleştirme kuralı ayrı nesne veya strateji olarak uygulanmalıdır.

Örnek kurallar:

- `MaxSlopeRule`
- `MinWaterFlowRule`
- `MinSolarPotentialRule`
- `MinWindPotentialRule`
- `GridOccupancyRule`
- `TechnologyUnlockedRule`
- `AffordabilityRule`

Sonuç modeli:

```csharp
public sealed class PlacementValidationResult
{
    public bool IsValid;
    public IReadOnlyList<string> FailureReasons;
}
```

## 12. Enerji Sistemi

Önerilen arayüzler:

```csharp
public interface IEnergyProducer
{
    float GetAvailableProduction(SimulationContext context);
}

public interface IEnergyConsumer
{
    float GetDemand(SimulationContext context);
}

public interface IEnergyStorage
{
    float StoredEnergy { get; }
    float Capacity { get; }
}
```

Ana servisler:

- `EnergyNetworkService`
- `EnergyNetworkGraph`
- `EnergyBalanceCalculator`
- `StorageDispatchService`

Şebeke grafiği yalnızca bağlantı değiştiğinde yeniden hesaplanmalıdır.

## 13. Simülasyon Saati

`SimulationClock` aşağıdaki hızları desteklemelidir:

- Duraklat
- 1×
- 2×
- 4×

Simülasyon tick örneği:

- Her oyun dakikası veya her 0.5 gerçek saniye

Sistemler doğrudan `Update()` içinde ağır hesap yapmamalıdır.

## 14. Olay Sistemi

Örnek olaylar:

- `MapGeneratedEvent`
- `BuildingPlacedEvent`
- `BuildingRemovedEvent`
- `NetworkChangedEvent`
- `EnergyShortageEvent`
- `TechnologyUnlockedEvent`
- `ScenarioCompletedEvent`

Basit, tip güvenli bir event bus kullanılabilir. Gereksiz karmaşık framework eklenmemelidir.

## 15. UI Mimarisi

Önerilen yaklaşım:

- View: MonoBehaviour
- Presenter / Controller: UI mantığı
- Model: Salt okunur oyun verisi veya view model

UI doğrudan bina oluşturmaz. Komut veya servis çağrısı yapar.

## 16. Kayıt Sistemi

Prototipte JSON tabanlı kayıt yeterlidir.

Kaydedilecekler:

- Seed
- Harita üretim ayarları
- Oyun zamanı
- Para
- Araştırma durumu
- Yapı örnekleri
- Şebeke bağlantıları
- Batarya durumları
- Köy durumu
- Senaryo hedefleri

Terrain verisinin tamamını kaydetmek yerine seed ve üretim sürümü saklanmalıdır.

## 17. Test Stratejisi

### EditMode testleri

- Noise deterministik mi?
- Aynı seed aynı yükseklik değerlerini üretiyor mu?
- Eğim hesaplaması doğru mu?
- Yerleştirme kuralları doğru sonuç veriyor mu?
- Enerji dengesi matematiği doğru mu?

### PlayMode testleri

- Harita oluşturuluyor mu?
- Kamera sınırlar içinde hareket ediyor mu?
- Yapı yerleştirme akışı çalışıyor mu?
- Şebekeye bağlanan üretici talebi karşılıyor mu?
- Kayıt yükleme sonrası yapı konumları korunuyor mu?

## 18. Performans Hedefleri

Prototip hedefleri:

- 64 × 64 grid
- En az 200 aktif yapı
- 60 FPS hedefi
- Harita üretimi sırasında kısa yükleme ekranı kabul edilebilir
- Kaynak katmanı geçişi gecikmesiz olmalı

## 19. Sürümleme ve Git

Önerilen branch yapısı:

- `main`: çalışır sürüm
- `develop`: entegrasyon
- `feature/terrain-generation`
- `feature/building-placement`
- `feature/energy-network`

Commit örnekleri:

- `feat: add seeded height map generation`
- `fix: prevent placement on occupied cells`
- `test: add slope calculator tests`
- `docs: update terrain sprint acceptance criteria`

## 20. Teknik Borç Kuralları

- Geçici çözüm yorumunda `TODO` ve açıklama bulunmalıdır.
- Prototip kodu olsa bile bilinmeyen sabit sayılar kullanılmamalıdır.
- Çalışmayan kod yorum satırına alınarak bırakılmamalıdır.
- Aynı mantık iki yerde çoğaltılmamalıdır.
- Yeni bağımlılık eklenmeden önce gerekçesi belgelenmelidir.
