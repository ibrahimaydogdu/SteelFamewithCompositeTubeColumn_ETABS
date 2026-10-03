# Akış Şeması (Pipeline)

- **Durum:** 2026-10-03. Kullanıcı Aşama 2'yi onayladı; Aşama 3 ve sonrası taslak halinde.
- **Referans:** `..\SteelFamewithCompositeColumn_ETABS`, sürüm 2026.10.3. Aşama 1–17 tamamlanmış ve test edilmiş.

## 0. Hedef (kullanıcı kararları, 2026-10-03)

| Konu | Referans proje | Bu proje |
|---|---|---|
| Kompozit kolon tipi | gömülü: W profil + beton + donatı (`EncasedRectangle`) | **dolgulu tüp:** çelik kutu (CFT, `FilledTube = 29`) veya boru (CFP, `FilledPipe = 30`) + beton |
| Kolon tipi seçimi | tüm kolonlar birlikte çelik **ya da** kompozit (`FormInfo.CompositeColumns`) | **hibrit:** her kolon grubu ayrı ayrı çelik veya kompozit |
| Döşeme | — | isteğe bağlı: kompozit döşeme tasarımı |
| Yöntemler | 15 metasezgisel | 15 + **Sosyal Örümcek Algoritması (SSO)**; Fortran'dan aktarılacak |
| Lisans / depo | MIT | MIT; ETABS modelleri (`.EDB`, `.$et`) depoya girer |

## 1. Referanstan çekilen ve çekilecek veriler

| # | Kaynak (referans) | Hedef yetenek | Durum |
|---|---|---|---|
| R1 | `PROGRAM_KURALLARI.md` | tümü | Aşama 2'de alındı (Bölüm B) |
| R2 | `AISC360_22_…md`, `AISC360_16_…md` | Şartname | Aşama 2'de alındı. Gömülü kesit içindir; CFT/CFP için I2.2 özeti Aşama 4'te eklenecek. |
| R3 | `CompositeColumn.vb`, `EncasedSections.xml` | Şartname | Aşama 2'de alındı. CFT/CFP hesabı aynı yapıyla eklenecek. |
| R4 | `ETABSClass.vb` | ETABS | Aşama 2'de alındı |
| R5 | `OptimizationClass.vb`, `OptimizationMethods.vb`, `Structures.vb` | Optimizasyon | Aşama 2'de alındı |
| R6 | `MainForm*`, `ExcelExport.vb`, `App.config`, `ApplicationEvents.vb` | arayüz | Aşama 2'de alındı |
| R7 | `DEGISIKLIKLER.md` | Hafıza | referans değerler ve bilinen farklar `Hafiza/` altına özetlenecek |
| R8 | Referans projenin derleme ve test yöntemi | ETABS / Hafıza | Aşama 2'de kullanıldı. MathTest ve ETest güncellendi. |

Fortran kaynakları (`Algoritmalar\fortran\SocialSpider\`):

| # | Dosya | İçerik | Not |
|---|---|---|---|
| F1 | `SSO_Column\SSO.f90` (2015) | SSO ana döngüsü: dişi/erkek örümcekler, titreşim, çiftleşme | `subroutine SSO`; asıl referans |
| F2 | `SSO_Frame\SSO.f90` (2018) | çerçeve sürümü, bağımlı gruplar | alt programın adı yanlışlıkla `Cuckoo`; F1 ile karşılaştırılacak |
| F3 | `SSO.m` (MATLAB) | algoritmanın özgün tanımı (Cuevas vd. 2013) | çapraz kontrol |
| F4 | `Composite_Design\SSO_Frame_Com\Source1.f90` | `filled_composite`, `encased_composite` | CFT formülleri için çapraz kontrol |

## 2. Geliştirme aşamaları

```
Aşama 1  Ajan mimarisi + depo + kayıt dosyaları                          [bitti]
Aşama 2  Referans kodun aktarılması, eski iskeletin kaldırılması          [bitti]
   │       test: derleme, MathTest = referans, 525M kopyası ETABS testi = referans
Aşama 3  Fortran SSO incelemesi (F1–F3) → bulgu raporu
Aşama 4  SSO → VB.NET (OptimizationMethods + MethodCatalog); matematik testleri
Aşama 5  Dolgulu tüp kolon (CFT/CFP):
   │       - kesit kataloğu: kutu/boru profil listesi + beton
   │       - ETABS'te kesit tanımı (OAPI'de Set metodu yok → DatabaseTables; API testiyle doğrulanacak)
   │       - iç çözücü: AISC 360-22 I2.2 (kompakt/narin sınıfı, Pno, EIeff, Mn, etkileşim)
   │       - ETABS DesignCompositeColumn ile karşılaştırma ve kalibrasyon
Aşama 6  Hibrit tasarım: kolon grubu başına çelik/kompozit
   │       - seçenekler: (a) grup başına kullanıcı seçimi; (b) tip değişkeni optimizasyona eklenir
   │       - maliyet: çelik + beton (+ kalıp yok, tüp kalıp işlevi görür)
Aşama 7  Uçtan uca koşu: SSO + diğer yöntemler, final ETABS doğrulaması, Excel
Aşama 8  (isteğe bağlı) Kompozit döşeme tasarımı
Aşama 9  Kılavuz, README, dağıtım paketi
```

Her aşamada izlenen adımlar:
1. Öneri sunulur ve onay beklenir.
2. Uygulanır.
3. Test programlarıyla doğrulanır.
4. `DEGISIKLIKLER.md` dosyasına "Aşama N" olarak yazılır.
5. Hafıza ve kuyruk güncellenir.
6. Commit ve push yapılır.

## 3. Yeteneklere göre kurallar (özet)

- **ETABS:**
  - yalnızca geçici kopya üzerinde çalışılır; açık ETABS oturumuna bağlanılmaz;
  - yeniden başlatma için EDB kullanılır;
  - kombinasyon seçimi her tasarımdan önce kontrol edilir.
- **Şartname:**
  - AISC 360-22 varsayılandır, 360-16 seçilebilir;
  - formül değişikliğinden sonra regresyon testi yapılır.
- **Optimizasyon:**
  - her yöntem `MethodCatalog` üzerinden çağrılır;
  - tohum kayıt altına alınır;
  - ETABS çözücü gürültüsü yaklaşık 1e-13.

## 4. Açık sorular (ilgili aşamada sorulacak)

1. **Aşama 5:**
   - Kutu ve boru kesit listesi nereden alınacak? Seçenekler:
     - AISC 16 HSS kütüphanesi (`AISC16M.xml`, ETABS kurulumunda);
     - `Modeller/525M/CompositeSections.txt` (300x300x12.7 … 1000x1000x31);
     - özel bir liste.
   - Beton sınıfı ne olacak?
2. **Aşama 6:** Hibrit seçim kullanıcı tarafından mı yapılacak, optimizasyon değişkeni olarak mı, yoksa ikisi birden mi?
3. **Aşama 8:** Kompozit döşeme kapsamı: ETABS kompozit kiriş tasarımı mı (`DesignCompositeBeam`), döşeme kalınlığı ve sac profil seçimi mi?
