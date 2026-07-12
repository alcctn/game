# Clean Energy Strategy Prototype

Bu depo, çevresel koşullara göre temiz enerji üretme, yerleşim talebini karşılama ve teknoloji ağacında ilerleme temelli bir Steam strateji oyununun prototip planını içerir.

## Çalışma Varsayımı

- Oyun motoru: Unity
- Programlama dili: C#
- Görsel yaklaşım: İzometrik 3D
- Harita: Seed tabanlı prosedürel terra üretimi
- İlk senaryo: Ilıman nehir vadisi
- İlk hedef platform: Windows / Steam

## Belgeler

1. [Proje Genel Bakış](docs/PROJECT_OVERVIEW.md)
2. [Prototip Oyun Tasarım Dokümanı](docs/GAME_DESIGN_PROTOTYPE.md)
3. [Teknik Mimari](docs/TECHNICAL_ARCHITECTURE.md)
4. [Cursor Kuralları](docs/CURSOR_RULES.md)
5. [Sprint 01 — Arazi Sistemi](docs/SPRINT_01_TERRAIN.md)
6. [Sprint 02 — Kaynak Katmanları](docs/SPRINT_02_RESOURCES.md)
7. [Sprint 03 — Bina Yerleştirme](docs/SPRINT_03_PLACEMENT.md)
8. [Sprint 04 — Enerji Şebekesi](docs/SPRINT_04_ENERGY.md)
9. [Sprint 05 — Senaryo Hedefleri](docs/SPRINT_05_SCENARIO.md)
10. [Sprint 06 — Araştırma Ağacı](docs/SPRINT_06_RESEARCH.md)
11. [Sprint 07 — Gün Döngüsü](docs/SPRINT_07_DAYCYCLE.md)
12. [Sprint 08 — Kayıt / Yükleme](docs/SPRINT_08_SAVELOAD.md)
13. [Sprint 09 — Eğitim Görevleri](docs/SPRINT_09_TUTORIAL.md)
14. [Sprint 10 — Bakım Sistemi](docs/SPRINT_10_MAINTENANCE.md)
15. [Sprint 11 — Dağıtım Noktası](docs/SPRINT_11_DISTRIBUTION.md)
16. [Sprint 12 — Küçük Hidro Türbin](docs/SPRINT_12_SMALL_HYDRO.md)
17. [Sprint 13 — Periyodik Bakım Gideri](docs/SPRINT_13_UPKEEP.md)
18. [Sprint 14 — Sağ Bilgi Paneli](docs/SPRINT_14_INFO_PANEL.md)
19. [Sprint 15 — Alt Bildirim Alanı](docs/SPRINT_15_NOTIFICATIONS.md)
20. [Sprint 16 — Acil Yardım Kredisi](docs/SPRINT_16_EMERGENCY_CREDIT.md)
21. [Sprint 17 — Senaryo Başarısızlığı](docs/SPRINT_17_SCENARIO_FAIL.md)

## İlk Uygulama Sırası

1. Unity projesini oluştur.
2. `CURSOR_RULES.md` içeriğini `.cursor/rules/project-rules.mdc` dosyasına aktar.
3. `TECHNICAL_ARCHITECTURE.md` içindeki klasör yapısını oluştur.
4. `SPRINT_01_TERRAIN.md` görevlerini sırayla uygula.
5. Her görevden sonra test sahnesinde doğrulama yap.

## Prototip Başarı Kriteri

Oyuncu seed tabanlı oluşturulan bir haritada kaynak katmanlarını inceleyebilmeli, enerji yapıları kurabilmeli, bunları elektrik ağına bağlayabilmeli ve köyün enerji talebini karşılayarak senaryoyu tamamlayabilmelidir.
