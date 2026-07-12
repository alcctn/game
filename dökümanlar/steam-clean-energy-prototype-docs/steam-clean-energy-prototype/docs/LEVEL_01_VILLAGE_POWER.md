# LEVEL 01 - VILLAGE POWER

Bu belge Cursor'a verilecek uygulama gorevidir. Amac, oyunun ilk levelini oynanabilir bir hedef dongusune cevirmektir.

## Amac

Oyuncu, mevcut bir koyun enerji ihtiyacini karsilamak icin su ve ruzgar uretimi kurar.

Level 1'de:

- Koy enerji tuketiminin en az %95'i belirli sure karsilanmali.
- Oyuncu en az 1 su uretim yapisi kurmali.
- Oyuncu en az 1 ruzgar turbini kurmali.
- Oyuncu en az 1 Engineer olusturmali.
- Oyuncu en az 1 Technician olusturmali.
- Yapilar aktif koyden cok uzakta kurulamamali.
- Enerji nakil hatlari otomatik olusmali.
- Oyuncuya manuel enerji hatti kur gorevi verilmemeli.

Bu level, sonraki levellar icin temel sablon olacak.

## Tasarim Karari

Oyun yapisi su sekilde dusunulmeli:

```text
Scenario = bolge / harita
Settlement = aktif koy, kasaba veya sehir
Level = aktif yerlesimin hedef asamasi
```

Ornek:

```text
Scenario: Green Valley
  Settlement: Dere Koyu
    Level 1: Koyu Aydinlat
    Level 2: Bakim Ekibi
    Level 3: Kasabaya Gecis
```

Oyuncu ayni koyu sonsuza kadar buyutmemeli. Her yerlesim birkac level boyunca gelismeli, sonra yeni yerlesim veya yeni bolge acilmali.

Level 1 sadece ilk koyu kapsar.

## Level 1: Koyu Aydinlat

### Tema

Oyuncudan var olan kucuk bir koy icin enerji uretmesi istenir.

### Ana hedef

Koyun enerji tuketiminin en az %95 kismi belirli sure boyunca karsilanmali.

```text
requiredCoverageRatio: 0.95
requiredCoverageTicks: 60
```

Bu gelisim ekranda genel bir level progress bar ile gosterilmeli:

```text
Level 1 Progress: 0% -> 100%
```

Progress bar, tek bir genel level ilerlemesini temsil eder.

## Level 1 Hedefleri

Level 1 tamamlanmasi icin sunlar gerekli:

```text
1x water producer
1x small wind turbine
1x engineer
1x technician
95% coverage objective
```

Detay:

```text
Water objective:
- En az 1 Water Wheel veya Small Hydro aktif olmali.

Wind objective:
- En az 1 Small Wind Turbine aktif olmali.

Engineer objective:
- En az 1 Engineer olusturulmali.

Technician objective:
- En az 1 Technician olusturulmali.

Coverage objective:
- Koy enerji talebinin en az %95'i 60 tick boyunca karsilanmali.
```

## Level Progress Hesabi

Progress bar agirliklari:

```text
Water objective: 20%
Wind objective: 20%
Engineer objective: 15%
Technician objective: 15%
Coverage objective: 30%
Total: 100%
```

Progress mantigi:

```text
Su uretim hedefi tamamlandiysa +20
Ruzgar uretim hedefi tamamlandiysa +20
Engineer hedefi tamamlandiysa +15
Technician hedefi tamamlandiysa +15
Coverage streak hedefi tamamlandiysa +30
```

Coverage streak kademeli gosterilmeli:

```text
coverageProgress = coverageStreakTicks / requiredCoverageTicks
coverageContribution = coverageProgress * 30
```

Boylece oyuncu %95 karsilama hedefinde ilerledikce bar yavas yavas dolar.

## Yerlesim Etki Alani Sistemi

Her aktif yerlesimin bir yapi kurma radius degeri olmali.

Level 1 icin:

```text
activeSettlementType: Village
placementRadius: 10 grid cell
```

Kural:

```text
Yapi aktif yerlesimin radius alani disinda kurulamaz.
```

Gecersiz yerlestirme nedeni acik gosterilmeli:

```text
Aktif yerlesimden cok uzak.
```

veya Ingilizce UI icin:

```text
Too far from active settlement.
```

## Otomatik Enerji Baglantisi

Oyuncuya enerji hatti kurdurulmamali.

Yeni kural:

```text
Gecerli bir enerji yapisi kuruldugunda, aktif yerlesime veya en yakin uygun sebeke noktasina otomatik enerji baglantisi olusur.
```

Oyuncu sadece yapiyi kurar. Hat otomatik hesaplanir.

### Baglanti maliyeti

Otomatik baglanti ucretsiz olmamali.

Onerilen formul:

```text
autoConnectionCost = distanceToSettlementOrGrid * connectionCostPerCell
```

Level 1 icin:

```text
connectionCostPerCell: 4
```

Placement preview panelinde goster:

```text
Build Cost: 150
Auto Grid Connection: 36
Total Cost: 186
```

Turkce UI icin:

```text
Insa Maliyeti: 150
Otomatik Sebeke Baglantisi: 36
Toplam: 186
```

### Gorsel baglanti

Yapi kurulduktan sonra koy ile yapi arasinda otomatik ince enerji hatti gorseli cizilebilir.

Ilk implementasyonda gercek model zorunlu degil. Debug line veya simple line renderer yeterli.

## Yeni Personel Tipleri

Yeni personel tipleri:

```text
Engineer
Technician
```

Bunlar bina degilse bile baslangicta basit veri modeli olarak eklenebilir. Mevcut mimariye daha uygunsa `WorkerType`, `WorkerPool`, `StaffService` gibi ayri sistem olustur.

### Engineer

Rol:

```text
Gelismis yapilarin kurulmasi icin gerekli teknik personel.
```

Level 1 hedefi:

```text
Engineer count >= 1
```

Ornek insa sartlari:

```text
Small Wind Turbine requires 1 Engineer
Small Hydro requires 1 Engineer
Distribution Hub requires 2 Engineers
```

Level 1'de en azindan Small Wind Turbine icin Engineer sarti uygulanmali.

### Technician

Rol:

```text
Bakim gorevlerini ustlenen personel.
```

Level 1 hedefi:

```text
Technician count >= 1
```

Ilk implementasyonda bakim sistemine tam baglamak zorunlu degilse bile veri olarak tutulmali. Bakim sistemine baglanabiliyorsa:

```text
Her Technician belirli sayida yapinin bakim kaybini azaltir veya manuel repair kapasitesi saglar.
```

## Onerilen Veri Modeli

Yeni level sistemi icin basit bir yapi eklenebilir.

```text
LevelDefinition
- levelId
- displayName
- requiredCoverageRatio
- requiredCoverageTicks
- placementRadius
- connectionCostPerCell
- requiredBuildingObjectives
- requiredWorkerObjectives
- progressWeights
```

Ornek:

```text
levelId: level_01_village_power
displayName: Koyu Aydinlat

requiredCoverageRatio: 0.95
requiredCoverageTicks: 60
placementRadius: 10
connectionCostPerCell: 4

requiredBuildingObjectives:
- water_producer: 1
- small_wind: 1

requiredWorkerObjectives:
- engineer: 1
- technician: 1

progressWeights:
- waterObjective: 20
- windObjective: 20
- engineerObjective: 15
- technicianObjective: 15
- coverageObjective: 30
```

## Yerlestirme Kurallari

Herhangi bir yapi kurmak icin sartlar:

```text
1. Aktif yerlesimin etki alani icinde olmali.
2. Arazi insa edilebilir olmali.
3. Egim sinirini asmamali.
4. Kaynak potansiyeli yeterli olmali.
5. Gerekli teknoloji acilmis olmali.
6. Gerekli personel mevcut olmali.
7. Para toplam maliyeti karsilamali.
8. Otomatik baglanti mesafesi hesaplanmali.
```

Placement validation tum basarisiz nedenleri dondurmeli.

Ornek hata nedenleri:

```text
- Aktif yerlesimden cok uzak.
- Yetersiz Engineer.
- Yetersiz para.
- Ruzgar potansiyeli dusuk.
- Egim cok yuksek.
```

## UI Gereksinimleri

Level 1 icin ekranda gorunmeli:

```text
Level 1: Koyu Aydinlat
Progress: X%
```

Alt hedef listesi:

```text
[ ] Su uretimi kur
[ ] Ruzgar turbini kur
[ ] 1 Engineer olustur
[ ] 1 Technician olustur
[ ] Koy talebinin %95'ini 60 tick karsila
```

Placement panelinde gorunmeli:

```text
Build Cost
Auto Connection Cost
Total Cost
Required Engineers
Required Technicians
Distance to Settlement
```

## Degistirilecek veya Olusturulacak Dosyalar

Mevcut mimariye gore isimler degisebilir, ama yaklasik dosyalar:

```text
Assets/_Game/Scripts/Scenario/LevelDefinition.cs
Assets/_Game/Scripts/Scenario/LevelObjectiveState.cs
Assets/_Game/Scripts/Scenario/LevelProgressService.cs
Assets/_Game/Scripts/Scenario/SettlementDefinition.cs
Assets/_Game/Scripts/Workers/WorkerType.cs
Assets/_Game/Scripts/Workers/WorkerPool.cs
Assets/_Game/Scripts/Workers/WorkerService.cs
Assets/_Game/Scripts/Placement/SettlementRadiusRule.cs
Assets/_Game/Scripts/Placement/WorkerRequirementRule.cs
Assets/_Game/Scripts/Placement/AutoConnectionCost.cs
Assets/_Game/Scripts/UI/LevelProgressHudUI.cs
```

Asset onerileri:

```text
Assets/_Game/Data/Levels/level_01_village_power.asset
Assets/_Game/Data/Settlements/green_valley_village.asset
```

## Teknik Kurallar

- Mevcut sistemleri bozmadan genislet.
- Enerji hatti manuel bina olarak oyuncuya zorunlu kilinmamali.
- Mevcut `power_line` sistemi varsa otomatik baglanti gorseli veya maliyet hesabi icin kullanilabilir.
- UI dogrudan oyun state degistirmemeli; servislerden veri okumali.
- Placement validation tek buyuk metot olmamali; kural siniflariyla genisletilmeli.
- Yeni personel sistemi ileride buyuyebilir olmali.
- Ilk level basit ama veri odakli olmali.
- Magic number kullanma; LevelDefinition veya ayar asset'inden oku.

## Kabul Kriterleri

- Oyuncu koyden cok uzaga yapi kuramaz.
- Yapi kuruldugunda enerji baglantisi otomatik hesaplanir.
- Oyuncuya manuel enerji hatti kur gorevi verilmez.
- Auto connection cost toplam maliyete eklenir.
- Small Wind Turbine kurmak icin en az 1 Engineer gerekir.
- Level 1'de en az 1 Engineer ve 1 Technician hedefi vardir.
- Level progress bar yuzde olarak ilerler.
- Su uretimi, ruzgar uretimi, personel ve coverage hedefleri level progress'e katkı verir.
- Level progress %100 oldugunda Level 1 tamamlanir.
- Mevcut Green Valley akisi bozulmaz.

## Test Adimlari

1. Yeni oyun baslat.
2. Koyden uzak bir alana yapi kurmayi dene.
3. "Aktif yerlesimden cok uzak" hatasi goruldugunu dogrula.
4. Engineer olmadan Small Wind Turbine kurmayi dene.
5. "Yetersiz Engineer" hatasi goruldugunu dogrula.
6. Engineer olustur.
7. Small Wind Turbine kur.
8. Su uretim yapisi kur.
9. Technician olustur.
10. Koyun enerji talebini %95 ustunde karsila.
11. Progress bar'in hedeflere gore arttigini dogrula.
12. Progress %100 oldugunda Level 1 tamamlanmali.

## Kapsam Disi

- Level 2 ve sonrasi
- Yeni sehir/kasaba gecis sistemi
- Gelismis calisan animasyonlari
- Detayli personel ekonomisi
- Manuel enerji hatti editoru
- Yeni gorsel prefab entegrasyonu
