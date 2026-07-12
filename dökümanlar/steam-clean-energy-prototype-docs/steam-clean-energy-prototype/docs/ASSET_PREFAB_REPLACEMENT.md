# ASSET PREFAB REPLACEMENT

Bu rehber, prototipteki gecici primitive bina modellerini gercek Unity prefab'lari ile degistirmek icin kullanilir.

## 1. Mevcut sistem nasil calisir?

Bina gorunumu `BuildingDefinition` asset'i uzerindeki `prefab` alanindan gelir.

- `prefab` doluysa `BuildingFactory` bu prefab'i sahneye koyar.
- `prefab` bossa `BuildingFactory` gecici cube / cylinder model uretir.

Ilgili dosyalar:

```text
Assets/_Game/Scripts/Buildings/BuildingDefinition.cs
Assets/_Game/Scripts/Buildings/BuildingFactory.cs
Assets/_Game/Data/Buildings/
Assets/_Game/Prefabs/Buildings/
```

## 2. Onerilen klasor yapisi

Import edilen model, materyal ve prefab'lari asagidaki yerlere koy:

```text
Assets/_Game/Art/Models/Buildings/
Assets/_Game/Art/Materials/
Assets/_Game/Art/Textures/
Assets/_Game/Prefabs/Buildings/
```

Hazir prefab isimleri:

```text
Assets/_Game/Prefabs/Buildings/SmallSolar.prefab
Assets/_Game/Prefabs/Buildings/SmallWind.prefab
Assets/_Game/Prefabs/Buildings/WaterWheel.prefab
Assets/_Game/Prefabs/Buildings/SmallHydro.prefab
Assets/_Game/Prefabs/Buildings/Battery.prefab
Assets/_Game/Prefabs/Buildings/DistributionHub.prefab
Assets/_Game/Prefabs/Buildings/PowerLine.prefab
Assets/_Game/Prefabs/Buildings/MaintenanceDepot.prefab
Assets/_Game/Prefabs/Buildings/Village.prefab
```

## 3. Unity'de prefab olusturma adimlari

1. Modeli Unity'ye import et.
2. Modeli sahneye surukle.
3. Olcegi 1 grid hucresine okunabilir olacak sekilde ayarla.
4. Koku zemine oturt:
   - Root object position: `0, 0, 0`
   - Gorsel child gerekiyorsa yukariya tasinabilir.
5. Gereksiz collider veya animation componentlerini temizle.
6. Root objeye anlamli isim ver.
7. Objeyi `Assets/_Game/Prefabs/Buildings/` icine surukleyerek prefab yap.
8. Sahneden test objesini sil.

## 4. BuildingDefinition asset'ine baglama

Unity Project penceresinde ilgili asset'i ac:

```text
Assets/_Game/Data/Buildings/small_solar.asset
Assets/_Game/Data/Buildings/small_wind.asset
Assets/_Game/Data/Buildings/water_wheel.asset
Assets/_Game/Data/Buildings/small_hydro.asset
Assets/_Game/Data/Buildings/battery.asset
Assets/_Game/Data/Buildings/distribution_hub.asset
Assets/_Game/Data/Buildings/power_line.asset
Assets/_Game/Data/Buildings/maintenance_depot.asset
Assets/_Game/Data/Buildings/village.asset
```

Inspector'da `Prefab` alanina ilgili prefab'i surukle.

Ornek:

```text
small_solar.asset -> Prefab: SmallSolar.prefab
small_wind.asset  -> Prefab: SmallWind.prefab
water_wheel.asset -> Prefab: WaterWheel.prefab
```

## 5. Pivot ve olcek kurali

Prefab zemine `Y = 0` kondugunda dogru gorunmelidir.

Dogru:

```text
Prefab root pivot zeminde.
BuildingFactory prefab'i cell.WorldPosition'a koyar.
Model terrain uzerine oturur.
```

Yanlis:

```text
Prefab pivot modelin ortasinda.
Modelin yarisi zeminin altinda kalir.
```

Bu olursa prefab icinde gorsel mesh'i child objeye al ve child'i yukari tasi. Root pivotu zeminde kalsin.

## 6. Ilk degistirilecek yapilar

Once kolay ve statik yapilarla basla:

1. `small_solar`
2. `battery`
3. `distribution_hub`
4. `village`
5. `power_line`

**Sprint 68:** Ilk bes id icin `Assets/_Game/Prefabs/Buildings/{id}.prefab` placeholder'lari editor setup ile olusturulur ve ilgili `BuildingDefinition.prefab` alanina atanir. Gercek art ile degistirmek icin ayni path'i overwrite etmek yeterlidir; data baglantisi korunur.

**Sprint 81:** Editor setup artik dokuz bina icin placeholder persist eder: `village`, `distribution_hub`, `battery`, `small_solar`, `small_wind`, `water_wheel`, `small_hydro`, `power_line`, `maintenance_depot`. Wind/hydro placeholder'larinda `Spin` child bulunur (Sprint 82).

Daha sonra hareketli veya yon duyarliligi olan yapilar:

1. `small_wind`
2. `water_wheel`
3. `small_hydro`

## 7. Animasyon notlari

Ilk geciste animasyon zorunlu degildir. Prefab oyunda dogru yerde gorunuyorsa yeterlidir.

Sonraki iyilestirme:

- Ruzgar turbin kanatlari kendi child objesi olarak ayrilmali.
- Su carki kendi child objesi olarak ayrilmali.
- Donen parcalar icin basit bir `RotatingVisual` script'i eklenebilir (Sprint 70).

## 8. Test kontrol listesi

Her prefab baglandiktan sonra `Test_Terrain` sahnesinde kontrol et:

- Yapi seciliyor mu?
- Placement preview dogru calisiyor mu?
- Yapi terrain ustune oturuyor mu?
- Olcek grid hucresine uygun mu?
- Rotation ile model dogru donuyor mu?
- Inspector panelinde bina bilgileri gorunuyor mu?
- Save / load sonrasinda ayni prefab geri geliyor mu?
- Prefab alanini bosaltinca gecici primitive fallback calisiyor mu?

## 9. Sik sorunlar

### Model gorunmuyor

- Prefab alaninin dolu oldugunu kontrol et.
- Mesh renderer kapali olabilir.
- Materyal shader'i URP ile uyumsuz olabilir.
- Model cok kucuk veya cok buyuk olabilir.

### Model zeminin altinda kaliyor

- Root pivot zeminde degildir.
- Mesh'i child objeye al ve child'i yukari tasi.

### Model yan donuk geliyor

- Prefab icinde gorsel child rotation'ini duzelt.
- Root rotation'i `0, 0, 0` birak.

### Placement dogru ama goruntu yanlis

- Bu genelde data sorunu degil, prefab pivot / scale / material sorunudur.
- `BuildingDefinition` maliyet, enerji ve kurallari tutar; goruntu prefab icinde duzeltilmelidir.

## 10. Kabul kriteri

Gecici modelden gercek prefab'a gecis tamam sayilirsa:

- Ilgili `BuildingDefinition` asset'indeki `prefab` alani doludur.
- Yapi oyun icinde prefab ile olusur.
- Prefab terrain ustune dogru oturur.
- Save / load sonrasinda goruntu korunur.
- `prefab` alani bos kalirsa primitive fallback hala calisir.
