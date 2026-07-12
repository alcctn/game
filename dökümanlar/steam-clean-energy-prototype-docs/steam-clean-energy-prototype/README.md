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

## İlk Uygulama Sırası

1. Unity projesini oluştur.
2. `CURSOR_RULES.md` içeriğini `.cursor/rules/project-rules.mdc` dosyasına aktar.
3. `TECHNICAL_ARCHITECTURE.md` içindeki klasör yapısını oluştur.
4. `SPRINT_01_TERRAIN.md` görevlerini sırayla uygula.
5. Her görevden sonra test sahnesinde doğrulama yap.

## Prototip Başarı Kriteri

Oyuncu seed tabanlı oluşturulan bir haritada kaynak katmanlarını inceleyebilmeli, enerji yapıları kurabilmeli, bunları elektrik ağına bağlayabilmeli ve köyün enerji talebini karşılayarak senaryoyu tamamlayabilmelidir.
