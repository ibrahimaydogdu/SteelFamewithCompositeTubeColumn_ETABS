# Proje Durumu

**Son güncelleme:** 2026-10-03, Aşama 1.

## Kimlik

- **Konu:** Çelik çerçevelerin optimizasyonu. Kolonlar **gömülü kompozit**: W (I/H) profil, etrafı betonla kaplı, boyuna donatı ve etriyeli. Kirişler W profili.
- **Ortam:** ETABS 22.6 (`ETABSv1.dll`), VB.NET, .NET Framework 4.7.2 veya üstü, Visual Studio 2026.
- **Klasör:** `...\MVS2010\ETABS\SteelFamewithCompositeTubeColumn_ETABS`. Başlangıç içeriği SFCS'den kopyalandı.
- **Depo:** github.com/ibrahimaydogdu/SteelFamewithCompositeTubeColumn_ETABS (main).

## Verilen kararlar

- **2026-10-03 (kullanıcı):**
  - Ajan mimarisi kurulacak: Yetenekler, Hafıza, Görevler.
  - Referans projenin kuralları, şartnameleri, akışı ve yöntemleri aktarılacak.
  - Fortran SSO entegre edilecek.
- **2026-10-03 (kullanıcı):** Proje klasörünün adı depo adıyla aynı olacak.
- **2026-10-03:** Çalışma dizini olarak SFCS seçildi. SSO_CF yerine seçilme nedenleri:
  - ETABSv1 API'si;
  - .NET ve ETABS 22 uyumu;
  - test modelleri ve kesit havuzları.
  - Orijinal SFCS ve SSO_CF klasörleri yedek olarak olduğu gibi duruyor.
- **2026-10-03:** Depoya girmeyecekler (referans projenin kuralıyla aynı):
  - `AISC14M.xml` (CSI'ın dosyası; ETABS kurulumunda var);
  - ETABS ikili dosyaları;
  - model ve analiz dosyaları.
  - Lisans ve EDB konusu kullanıcıya soruldu.

## Bekleyen sorular

Bkz. `../Gorevler/AKIS_SEMASI.md`, Bölüm 4.

## Önemli bilgiler

- ETABS 22 `eFramePropType` değerleri:
  - I = 1;
  - BuiltupUHybrid = 27; eski kod kompozit kesitleri yanlışlıkla bu değerle arıyordu;
  - FilledTube = 29;
  - EncasedRectangle = 31.
- Referans proje gömülü kesit ve donatıyı `DatabaseTables` ile yazıyor. Bunun nedeni OAPI'de bu iş için setter olmaması.
- Referans projede kompozit tasarım sonuçları `Composite Column Summary` tablosundan okunuyor; `GetSummaryResults` ETABS 22.6'da kaymış veri döndürüyor.
- Eski iskelet kodun inceleme bulguları kökteki `KOD_INCELEME_RAPORU.md` dosyasında.
