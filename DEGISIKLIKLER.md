# Değişiklik Kaydı

Her iş "Aşama N" başlığıyla ve test sonuçlarıyla birlikte buraya yazılır. En yeni aşama en üstte durur.

---

## 2026-10-03 — Aşama 2: Referans kodun aktarılması, eski iskeletin kaldırılması

**Kullanıcı kararları (Aşama 1 sorularına cevap)**
- **Kolon tipi:** kompozit kolonlar **dolgulu tüp** olacak; çelik kutu (CFT) veya boru (CFP) içine beton doldurulur. Aşama 1'de raporlara eklenen "gömülü kesit" düzeltmesi bu nedenle geri alındı.
- **Hibrit tasarım:** her kolon grubu ayrı ayrı çelik veya kompozit seçilebilecek.
- **Kompozit döşeme:** isteğe bağlı.
- **Depo adı:** adındaki "Tube" kolonların tüp olmasını anlatıyor; ad kalıyor.
- **Eski iskelet kod:** kaldırılacak.
- **Lisans:** ücretsiz → MIT.
- **ETABS modelleri:** depoya girebilir.

**Yapılanlar**
- Eski iskelet kaldırıldı: `SSO_CF/` (`CFCS.vb`, `Class1.vb`, form, proje dosyaları, `AISC14M.xml`) ve `CFCS.sln`.
- Test modelleri `Modeller/525M` ve `Modeller/CivilComp3` altına taşındı. Modeller `.EDB` ve `.$et` olarak depoya girdi; analiz çıktıları ve `.ebk` girmedi (`.gitignore`).
- Referans projeden (SteelFamewithCompositeColumn_ETABS 2026.10.3) şu dosyalar **bayt düzeyinde aynı** kopyalandı (`cmp` ile kontrol edildi):
  - kaynak kod: `ApplicationEvents.vb`, `CompositeColumn.vb`, `ETABSClass.vb`, `ExcelExport.vb`, `MainForm.*`, `OptimizationClass.vb`, `OptimizationMethods.vb`, `Structures.vb`, `My Project/*`;
  - ayar ve proje dosyaları: `App.config`, `EncasedSections.xml`, `packages.config`, `.vbproj`, `.sln`;
  - belgeler ve araçlar: `AISC360_16/22_Composite_Column_Rules.md`, `LICENSE` (MIT), `tools/md2html.py`.
- Uyarlamalar (davranışı değiştirmez):
  - proje ve çözüm `SteelFrameWithCompositeTubeColumns(.vbproj / ETABS.sln)` olarak yeniden adlandırıldı; proje, çözüm ve assembly GUID'leri yenilendi;
  - `AssemblyInfo`: ürün adı `SteelFrameWithCompositeTubeColumns`, telif "© 2026 Ibrahim Aydogdu", sürüm **0.2.0.0**;
  - pencere başlığı "Steel Frame Optimization with Composite Tube Columns (ETABS)" oldu.
  - Kök ad alanı ve exe adı `FrameSap2000` olarak korundu, böylece ayar ve yedek dosyalarıyla uyum sürüyor.
- Belgeler:
  - `PROGRAM_KURALLARI.md`: A bölümü projeye özgü kurallar, hedef, durum ve depo düzenini anlatıyor; B bölümü referansın kuralları.
  - `KULLANIM_KILAVUZU.md` ve `README.md` referanstan alındı; başlarına geliştirme durumu notu eklendi.
  - `KOD_INCELEME_RAPORU.md` notu güncellendi.
  - `Ajan/` altındaki akış şeması, görev kuyruğu, hafıza ve yetenek açıklamaları CFT/CFP ve hibrit hedefe göre yenilendi. Mimari betiği de aynı içeriği üretecek şekilde güncellendi.

**Testler**

| Test | Sonuç |
|---|---|
| vbc derlemesi (uygulama, MathTest, ETest; yeni ve referans kaynaklar) | 6/6 başarılı, hata yok |
| MSBuild (Visual Studio 2026), `Rebuild`, Debug | 0 hata, 0 uyarı. Exe'de ürün adı ve sürüm 0.2.0.0 doğru. |
| Form açılış testi | form "…Composite Tube Columns (ETABS)" başlığıyla açıldı ve yanıt veriyor |
| MathTest: dişli treni, 15 yöntem × Levy açık/kapalı × 3 tohum, 3000 değerlendirme | yeni ve referans derlemenin çıktısı **birebir aynı** (30 satır, `cmp`); hata yok |
| ETABS 22.6: `525Member` kopyası, kompozit mod, AISC 360-22, tohum 12345, 2 değerlendirme ve önbellekten tekrar | yeni ve referans derleme **birebir aynı** (aşağıda) |
| Mimari betiği (geçici klasörde yeniden çalıştırma) | 7 README üretildi, hepsi depodakilerle aynı |
| Model dosyaları | testlerden sonra `Modeller/525M/525Member.EDB` SFCS'deki orijinalle aynı. Testler kopya üzerinde çalıştı. |

ETABS testinin ayrıntıları:

| | Referans derleme | Yeni derleme |
|---|---|---|
| Değerlendirme 1: maliyet / ceza | 7121,40 / 2,0706 | 7121,40 / 2,0706 |
| Değerlendirme 2: maliyet / ceza | 7092,09 / 1,8054 | 7092,09 / 1,8054 |
| Kompozit oranları (2) | 0,828 0,805 0,955 0,915 0,915 0,915 0,700 0,788 0,637 0,963 | aynı |
| Önbellekten tekrar | 7121,40 / 2,0706 (0 s) | aynı |
| Süre: başlangıç / toplam | 517 s / 770 s | 480 s / 715 s |
| Çalışma klasörünün silinmesi, ETABS'in kapanması | evet | evet |

Notlar:
- Bu değerler referansın Aşama 3–4 kayıtlarındaki 7906,97 / 1,3548 değerlerinden farklı. Fark beklenen bir durum: referans kod o ölçümden sonra P-Delta, servis ötelemesi, kombinasyon ve onarım değişiklikleri aldı.
- Eski oturumdan kalan ETest programı, referansta sonradan kaldırılan `FrameInfo.DispLimit` alanını kullanıyordu. Testin kopyası bu satır olmadan derlendi.

---

## 2026-10-03 — Aşama 1: Proje klasörü, ajan mimarisi, kayıt düzeni

**Yapılanlar**
- Çalışma dizini olarak **SFCS** seçildi. Gerekçeler:
  - ETABS 22 ile aynı API ailesi (`ETABSv1`);
  - test modelleri (`525M`, `CivilComp3`);
  - kesit havuzu dosyaları.

  SSO_CF ise ETABS 2016 API'sine bağlı ve kompozit kolon tasarım arayüzü yok.
- SFCS, depo adıyla aynı olan `SteelFamewithCompositeTubeColumn_ETABS` klasörüne kopyalandı. `bin`, `obj`, `.vs` ve `*.suo` dosyaları alınmadı. Orijinal SFCS ve SSO_CF klasörlerine dokunulmadı.
- `Ajan/kur_ajan_mimarisi.ps1` betiği yazıldı ve çalıştırıldı. Betik şu klasörleri kuruyor ve her birine amaç açıklaması (`README.md`) koyuyor:
  - `Ajan/`;
  - `Ajan/Yetenekler/Optimizasyon_Yetenegi`, `Ajan/Yetenekler/ETABS_Kullanim_Yetenegi`, `Ajan/Yetenekler/Sartname_Kullanim_Yetenegi`;
  - `Ajan/Hafiza`;
  - `Ajan/Gorevler`.
- Yazılan dosyalar:
  - `Ajan/Gorevler/AKIS_SEMASI.md`: referanstan çekilecek veriler ve aşamalar (taslak);
  - `Ajan/Gorevler/KUYRUK.md`;
  - `Ajan/Hafiza/PROJE_DURUMU.md`.
- Kök kayıt dosyaları oluşturuldu: `DEGISIKLIKLER.md`, `PROGRAM_KURALLARI.md`, `KULLANIM_KILAVUZU.md`. Eski iskeletin inceleme raporu `KOD_INCELEME_RAPORU.md` aktif klasörden buraya taşındı.
- `.gitignore` eklendi. Depoya girmeyenler:
  - derleme çıktıları;
  - ETABS ikili dosyaları;
  - CSI'ın `AISC14M.xml` dosyası;
  - ETABS model ve analiz dosyaları.
- İlk commit ve push yapıldı (main).

**Tespit**
- Referans proje (`SteelFamewithCompositeColumn_ETABS`) kolonları zaten istenen tipte çözüyor: W profil + beton + donatı, yani `EncasedRectangle`.
- `KOD_INCELEME_RAPORU.md` raporundaki CFT (dolgulu kutu) varsayımı bu nedenle geçersiz. Raporun başına düzeltme notu eklendi.

**Testler**
- Mimari betiği temiz klasörde çalıştırıldı: 7 README oluşturuldu.
- Betik ikinci kez çalıştırıldı: 7 dosyanın hepsi için "zaten var" çıktı, üzerine yazma olmadı.
- Türkçe karakterler Windows PowerShell 5.1'de doğru göründü. Betik BOM'lu UTF-8 olarak kaydedildi.
- Kod değişmediği için derleme testi yapılmadı. Derleme Aşama 2'de, referans kod aktarıldıktan sonra yapılacak.
