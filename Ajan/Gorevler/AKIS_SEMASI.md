# Akış Şeması (Pipeline) — TASLAK

Durum: 2026-10-03. Kullanıcı onayı bekleniyor.

Bu taslakta "referans" şu proje demektir: `..\SteelFamewithCompositeColumn_ETABS`. Sürümü 2026.10.3; Aşama 1–17 tamamlanmış ve test edilmiş.

## 0. Temel tespit

Referans proje kolonları **zaten** istenen tipte çözüyor:
- kolon tipi: W (I/H) profil, etrafı beton, içinde boyuna donatı ve etriye (`EncasedRectangle`);
- ilgili dosyalar: `EncasedSections.xml`, `CompositeColumn.vb`;
- şartname: AISC 360-16/22 I2.1;
- ETABS 22 tarafı: `DesignCompositeColumn` ile doğrulama.

Bu nedenle yeni projede formüller yeniden yazılmaz; çerçeve, ETABS ve şartname katmanı **aynen alınır**.

Yeni olan iki şey var:
1. **Ajan mimarisi:** Yetenekler / Hafıza / Görevler klasörleri.
2. **Fortran'dan Sosyal Örümcek Algoritması (SSO).**

Bunların dışında referanstan farklı olacak noktaların (model, değişkenler, maliyet vb.) kullanıcıyla netleştirilmesi gerekiyor (bkz. Bölüm 4).

## 1. Referanstan çekilecek veriler

Her girdi bir yeteneğe bağlanır.

| # | Kaynak (referans) | İçerik | Hedef yetenek | Nasıl alınır |
|---|---|---|---|---|
| R1 | `PROGRAM_KURALLARI.md` | ortam, veri modeli, değerlendirme akışı, kompozit kurallar, derleme/test | tümü | kurallar uyarlanarak kopyalanır |
| R2 | `AISC360_22_…md`, `AISC360_16_…md` | gömülü kolon şartname özeti | Şartname | aynen |
| R3 | `CompositeColumn.vb`, `EncasedSections.xml` | gömülü kesit üretimi, Pno/EIeff, Mn (şekil değiştirme uyumu), etkileşim, detay kontrolleri | Şartname | aynen; regresyon testiyle |
| R4 | `ETABSClass.vb` | bağlantı, geçici kopya, grup okuma, `SetGeneral` / `DatabaseTables`, analiz, `DesignSteel` / `DesignCompositeColumn`, öteleme, yeniden başlatma | ETABS | aynen; ad alanı uyarlanır |
| R5 | `OptimizationClass.vb`, `OptimizationMethods.vb`, `Structures.vb` | 15 yöntem, `MethodCatalog`, ceza, önbellek, yedek | Optimizasyon | aynen; SSO eklenir |
| R6 | `MainForm*`, `ExcelExport.vb`, `App.config`, `ApplicationEvents.vb` | form, iş parçacığı, Excel, ayarlar | (arayüz) | aynen; başlık ve sürüm uyarlanır |
| R7 | `DEGISIKLIKLER.md` | referans değerler (525M, tohum 12345), kalibrasyon katsayısı 1,08–1,12, bilinen farklar | Hafıza | özetlenir |
| R8 | Claude hafızası: etabs-test-harness, etabs-api-quirks | vbc derlemesi, test programları, ETABS 22.6 tuzakları | ETABS / Hafıza | özetlenir |
| R9 | `KULLANIM_KILAVUZU.md` | kullanıcı kılavuzu yapısı | (kök) | uyarlanır |

Fortran kaynakları (`Algoritmalar\fortran\SocialSpider\`):

| # | Dosya | İçerik | Not |
|---|---|---|---|
| F1 | `SSO_Column\SSO.f90` (2015) | SSO ana döngüsü: dişi/erkek örümcekler, titreşim, çiftleşme, ceza | `subroutine SSO`; asıl referans |
| F2 | `SSO_Frame\SSO.f90` (2018) | çerçeve sürümü, bağımlı gruplar, `FrameWeightCalc` | alt programın adı **`Cuckoo`** (kopya artığı); F1 ile karşılaştırılacak |
| F3 | `SSO.m` (MATLAB) | algoritmanın özgün tanımı (Cuevas vd. 2013) | çapraz kontrol |
| F4 | `Composite_Design\SSO_Frame_Com\Source1.f90` | `encased_composite`, `filled_composite` | yalnızca R3 ile çapraz kontrol; R3 esas alınır |

## 2. Geliştirme aşamaları

```
Aşama 1  Ajan mimarisi + depo + kayıt dosyaları                    [bu aşama]
   │
Aşama 2  Referans kodun aktarılması (R1–R6)
   │       test: derleme (vbc + VS 2026), MathTest 15 yöntem = referans,
   │             525M kopyasında 2 değerlendirme = referans değerler (R7)
   │
Aşama 3  Fortran SSO incelemesi (F1–F3) → bulgu raporu
   │       (hatalar taşınmaz; ör. F2'deki ad, sabit iterationmax=500)
   │
Aşama 4  SSO'nun VB.NET'e aktarılması (OptimizationMethods + MethodCatalog)
   │       test: dişli treni / matematik problemleri, Fortran ile aynı tohumda davranış karşılaştırması
   │
Aşama 5  Projeye özgü farklar (Bölüm 4'teki kararlara göre)
   │       test: model kopyası üzerinde ETABS 22 API testi
   │
Aşama 6  Uçtan uca koşu: SSO + diğer yöntemler, final ETABS kompozit doğrulaması
   │       test: GUI uçtan uca testi, Excel doğrulaması
   │
Aşama 7  Kılavuz, README, dağıtım paketi
```

Her aşamada şu adımlar izlenir:
1. Öneri sunulur.
2. Onay beklenir.
3. Uygulanır.
4. Kendi test programlarımla doğrulanır.
5. `DEGISIKLIKLER.md` dosyasına "Aşama N" olarak yazılır.
6. Hafıza ve kuyruk güncellenir.
7. Commit ve push yapılır.

## 3. Yeteneklere göre kurallar (özet)

- **ETABS:**
  - yalnızca geçici kopya üzerinde çalışılır; açık ETABS oturumuna bağlanılmaz;
  - yeniden başlatma için EDB kullanılır;
  - kombinasyon seçimi her tasarımdan önce kontrol edilir;
  - gömülü kesit aramada `General` olarak tanımlanır, finalde `EncasedRectangle` olur.
- **Şartname:**
  - AISC 360-22 varsayılandır, 360-16 seçilebilir;
  - formül değişikliğinden sonra W10x45 regresyon testi yapılır (fark %1'in altında kalmalı).
- **Optimizasyon:**
  - her yöntem `MethodCatalog` üzerinden çağrılır;
  - tohum ve tekrar üretilebilirlik kayıt altına alınır;
  - ETABS çözücü gürültüsü yaklaşık 1e-13.

## 4. Kullanıcıyla netleştirilecekler

1. Referans proje zaten gömülü I/H kolonu çözüyor. Bu projeyi ondan ayıran ne?
   - Yalnızca SSO ve ajan mimarisi mi?
   - Yoksa beton boyutu, donatı veya profil yönü gibi değişkenler mi?
   - Başka bir model mi? 460Member veya yeni bir model olabilir.
   - Farklı bir maliyet tanımı mı?
2. GitHub deposunun adında "Tube" geçiyor, ama sistem tüp değil. Depo adı böyle kalsın mı?
3. SFCS'den gelen eski iskelet kod (`SSO_CF\CFCS.vb`, `Class1.vb`) Aşama 2'de kaldırılsın mı, yoksa `eski/` klasöründe arşiv olarak mı kalsın?
4. Lisans: MIT olsun mu? Test modelleri (EDB) depoya girsin mi?
