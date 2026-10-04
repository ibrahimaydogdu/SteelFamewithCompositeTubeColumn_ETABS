# Değişiklik Kaydı

Her iş "Aşama N" başlığıyla ve test sonuçlarıyla birlikte buraya yazılır. En yeni aşama en üstte durur.

---

## 2026-10-04 — Aşama 5.1: D/C oranı sınırı (ETABS ile tutarlılık) ve deprem şartnamesi süneklik süzgeci

**1. D/C oranı sınırı (kullanıcı: "1. notu incele, ciddi sorun")**
- **Bulgu:** Aşama 5 testinde grup 6 için ETABS "Combined D/C ratio exceeded" mesajı verdi, oysa PMM oranı 0,969'du.
  - ETABS'in AISC 360-22 kompozit kolon ve çelik çerçeve el kitaplarına göre ETABS oranı 1,0 ile değil, tasarım tercihlerindeki **D/C ratio limit** (varsayılan 0,95) ile karşılaştırıyor.
  - Grup 6'nın beş kolonunda yalnızca 0,969 işaretlendi; 0,858 işaretlenmedi.
  - Sınır kopyada 0,45 yapıldığında 0,455–0,52'lik çelik elemanlar için `DesignSteel.GetSummaryResults` hata ve uyarı alanlarını **boş** döndürdü. Program sınır aşımını göremiyordu.
  - Modellerde `SRLIMIT 0.95`.
  - Sonuç: program (referanstan beri) oranı 0,95–1,0 arasındaki çelik ve kompozit elemanları uygun sayıyordu; ETABS'te bu elemanlar yetersiz görünüyordu.
- **Düzeltme** (kullanıcı onayı: sınır modelden):
  - `InitializeRatioLimits` / `RatioLimit`, sınırları `Steel Frame Design Preferences - <kod>` ve `Composite Column Design Preferences - <kod>` tablolarının `DCLimit` alanından okuyor. Alan AISC 360-22, 360-16 ve 360-10'da aynı.
  - App.config `DesignRatioLimit`: boş = modeldeki sınır; sayı = çalışma kopyasına yazılır. ETABS yeniden başlatılınca tekrar yazılır.
  - Çelik grubunun oranı = ETABS oranı / çelik sınırı.
  - Kompozit grubun oranı = max(detay oranı, dayanım / kompozit sınırı). `CompositeStrength` ham kalır; kalibrasyonda kullanılıyor.
  - ETABS doğrulama oranları (`ETABSRatioByVar`, koruma döngüsü, uyarılar) = oran / sınır.
  - Günlük: `Info: D/C ratio limits (...)` satırı ve ETABS kontrol satırlarında "(D/C limit 0.95)".
- **API bilgileri:**
  - `DesignSteel.AISC360_22.GetPreference(37)` ve `DesignCompositeColumn.AISC360_22.GetPreference(18)` da aynı değeri veriyor. Kod tablolardan okuduğu için çelik koduna bağlı değil.
  - Tablo yazımı tercihlerde yalnızca `DCLimit` değerini değiştiriyor (yazmadan önce ve sonra alan alan karşılaştırıldı).

**2. Deprem şartnamesi süneklik süzgeci (kullanıcı isteği)**
- **Kaynak:** kullanıcının paylaştığı TBDY 2018 Tablo 9.3, "Kompozit Elemanlar" satırları (AISC 341-10 Tablo D1.1 ile aynı):

  | Düzey | Kutu cidarı b/t | Boru cidarı D/t |
  |---|---|---|
  | Yüksek (λhd) | 1,4 √(E/Fy) | 0,076 E/Fy |
  | Sınırlı (λmd) | 2,26 √(E/Fy) | 0,15 E/Fy |

  AISC 341-16 bağlantısının (Scribd) içeriği okunamadı.
- **Kod:**
  - `TubeSettings_.SeismicDuctility` (None / Moderate / High; kullanıcı kararıyla varsayılan **High**);
  - `SeismicLimit`, `SeismicSlenderness` (b: kütüphanedeki HSS'te B − 3t, yapma kutuda B − 2t);
  - katalog süzgeci ve günlük satırı;
  - `TubeSections.xml` ayarı.
- **Sonuç** (Fy = 345 MPa):
  - katalog yüksek sünekliğe göre 100 kesit (27 HSS + 73 yapma), sınırlı sünekliğe göre 132, süzgeç yokken 149;
  - örnek: yüksek süneklikte 600 mm yapma kutuda levha en az 20 mm, 1000 mm'de en az 30 mm.

Sürüm 0.4.1. Belgeler:
- kılavuz: 2.4 (`DesignRatioLimit`), 7.5 deprem tablosu, yeni 7.6;
- kurallar: A6;
- `README.md`.

**Testler**

| Test | Sonuç |
|---|---|
| vbc ve MSBuild `Rebuild` | 0 hata, 0 uyarı; exe 0.4.1.0 |
| MathTest | Aşama 3–4 ile birebir aynı |
| TubeTest (31 kontrol) | hepsi geçti. Yüksek: 100 kesit, hepsi b/t ≤ 33,7; BU1000X1000X25 (b/t = 38) atıldı, BU1000X1000X30 (31,3) kaldı. Sınırlı: 132. Süzgeçsiz: 149. Yüksek düzeyde borular D/t ≤ 44,1 (57 boru). XML varsayılanı High. |
| **525M, gömülü mod, sınır modelden** (ETABS 22.6, tohum 12345) | sınırlar 0,95 / 0,95 okundu. **Yeni regresyon değerleri:** 7564,91 / 1,4580 ve 7068,84 / 1,8032; önbellek tekrarı aynı. Oranlar 0,95'e göre ölçekleniyor (ör. 0,805 / 0,95 = 0,847). |
| **525M, tüp modu uçtan uca** (SSO, 4 örümcek, 12 analiz, final) | ayrıntılar aşağıda |
| `DesignRatioLimit = 1.0` | sınır her iki tercih tablosuna yazıldı ve geri okundu (1 / 1). Sonuç: 7348,43 / 1,8238 ve 6886,35 / 2,4916. Eski referanstan (7121,40 / 2,0706) farkının nedeni aşağıda. |

Tüp modu uçtan uca testinin ayrıntıları:
- katalog 100 kesit, sınırlar 0,95;
- 10 kolon grubunda ETABS / iç hesap PMM oranları:

  | Grup | ETABS | İç hesap |
  |---|---|---|
  | 5 | 0,788 | 0,790 |
  | 6 | 0,777 | 0,780 |
  | 9 | 0,678 | 0,684 |
  | 14 | 0,171 | 0,172 |

  Fark %1'in altında ve iç hesap her grupta biraz daha güvenli tarafta; kalibrasyon uyarısı yok.
- Hiçbir kolonda "Combined D/C ratio exceeded" mesajı yok.
- Kısa koşuda final cezası 0,5565; bu ceza öteleme kısıtından geliyor, bu bir optimizasyon sonucu değil.

`DesignRatioLimit = 1.0` testindeki farkın nedeni:
- Başlangıç sınırları (Ub/Lb) ETABS'in otomatik kesit seçimiyle bulunuyor ve ETABS bu seçimde de D/C sınırını kullanıyor.
- Sınır 1,0 olunca ETABS daha hafif kesitler seçiyor (ör. grup 5: W760X314 → W360X314; grup 8: W760X284 → W690X265); üst sınırlar değişiyor (ör. grup 7: 240 → 231).
- Bu, ayrı bir çalıştırmayla doğrulandı.
- Eski referans "ETABS 0,95, program 1,0" karışımından geliyordu; tek ve tutarlı bir ayarla yeniden üretilemez. Bundan sonra regresyon değeri olarak 7564,91 / 1,4580 kullanılacak.

---

## 2026-10-03 — Aşama 5: Dolgulu tüp (CFT/CFP) kompozit kolonlar

**Kullanıcı kararları** (inceleme ve öneri: `Ajan/Gorevler/ASAMA5_ONERI.md`)
- Kesitler ETABS kütüphanesinden alınacak. Program, yapma (levhadan kaynaklı) kutuları kendisi üretecek.
- Dolgu betonu modelde tanımlı beton malzemesi olacak.
- Boru varsayılan olarak kapalı, ayarla açılabilir. Moment çerçevesinde boruya kiriş bağlantısı zor.
- Süzgeç: kare kutu.
- En küçük boyut 300 mm. Kullanıcının sorusu üzerine araştırıldı:
  - AISC 360-22 dolgulu kolon için alt sınır koymuyor;
  - TBDY 2018 7.3.1.1 betonarme kolon için 300 mm istiyor (kompozit kolonlar Bölüm 9'da);
  - beton dökümü ve kiriş bağlantısı da 200 mm'lik kolonu zorlaştırıyor.
- Gömülü kesit seçeneği **kalıyor**. Tek program, formda kompozit tip seçimi.

**Kod**
- Yeni `TubeColumn.vb`:
  - `CompositeType_` (Encased / FilledTube), `TubeShape_`, `TubeSection_`;
  - `TubeSettings_` ve kesit kataloğu: kütüphanedeki STEEL_BOX / STEEL_PIPE (tasarım kalınlığı) + yapma kutular `BU<B>X<B>X<t>`, `MinDimension` ve Tablo I1.1a/b λmax süzgeci, çelik alanına göre sıralama.
- Yeni `TubeSections.xml` ayar dosyası (exe yanına kopyalanır).
- Dayanım hesabı için referanstaki `FilledBox` / `FilledPipe` sınıfları kullanıldı (I2.2, I3.4, I4, I5). Formüller değişmedi.
- `ETABSClass.vb`:
  - tüp modda kompozit grubun değişkeni `Tubes` listesinin indisi;
  - W listesine bağlı yerler katalog yardımcılarıyla genelleştirildi: `IsTubeVar`, `CatalogCount`, `SecArea`, `SecDepth`, `SecName`, `FindSection`, `ColumnGap`. Etkilenen yerler:
    - geometri onarımı ve geometri cezası;
    - `NearestFeasible`, onarım adımları (`RepairSteps`, F2, F4, G2);
    - final koruması (`StepUpETABSFailures`, `GeometryFits`);
    - kesit ataması, maliyet ve maliyet dökümü, değişken açıklaması (`HSS… [CFT 559x559x23.6]`).
  - `CompositeSec`: tüp ya da gömülü kesit.
  - Ub/Lb: tüp grubunda Lb = 0. Ub, ilk tasarımdaki W kesitinin Fy·A değerine en az eşit Pno'lu ilk tüpten başlar; referanstaki kayma payı eklenir.
  - Dolgu betonu `ResolveTubeConcrete` ile bulunur: ayardaki ad modelde varsa o, yoksa modelin ilk beton malzemesi. Tüp modda donatı malzemesi okunmaz.
  - Final: `CreateTubeSections`, General kesitleri aynı adla `Filled Steel Tube/Pipe` tablolarına yazar (açık boyutlar; `FromFile` ETABS 22.6'da çalışmıyor). Ardından ETABS kompozit kolon tasarımı yapılır.
- `Structures.vb`: `FormInfo.CompositeType`. Eski yedeklerde değer 0 olduğu için gömülü kabul edilir.
- `MainForm`:
  - "Composite columns" kutusunun yanına tip seçimi eklendi; varsayılan *Filled tube (box / pipe)*;
  - tüp seçiliyken *Rebar* / *Formwork* maliyet kutuları pasif;
  - sonuç dosyasından tip tanıma (`[CFT ` / `[CFP `);
  - Check Structure'da kesitler `FindSection` ile okunur.
- `OptimizationMethods.vb`: ACO'nun "hafif kesit" tercihi değişkenin kataloğunu kullanıyor (`AcoHeuristic(d, s)`). W değişkenlerinde sonuç aynı.
- Sürüm 0.4.0. Belgeler:
  - kılavuzda 5.2 ve yeni 7.5;
  - `PROGRAM_KURALLARI.md` A5;
  - `README.md`.

**Testler**

| Test | Sonuç |
|---|---|
| vbc (uygulama, MathTest, ETest, TubeTest, form testi, uçtan uca test) | hata yok |
| MathTest (16 yöntem × Levy açık/kapalı × 3 tohum) | Aşama 3–4 çıktısıyla **birebir aynı** |
| TubeTest (ETABS'siz, 23 kontrol) | hepsi geçti. Varsayılan katalog 149 kesit: kütüphaneden 45 kare HSS (≥ 300 mm) + 104 yapma kutu. Elle hesapla Pno: HSS 559×23,6 için 23.564 kN, Ø711×25,4 boru için 27.829 kN (C2 = 0,95). λmax süzgeci: BU1000X1000X6 atıldı, BU400X400X6 kaldı. Boru ve dikdörtgen kutu seçenekleri çalışıyor. XML ayarları okunuyor. |
| Form testi | "Composite columns" + *Filled tube (box / pipe)* görünüyor; *Rebar* / *Formwork* kutuları pasif |
| **Gömülü mod regresyonu** (ETABS 22.6, 525Member kopyası, tohum 12345, 2 değerlendirme) | 7121,40 / 2,0706 ve 7092,09 / 1,8054; kompozit oranlar ve önbellek tekrarı Aşama 2 referansıyla **birebir aynı** |
| **Tüp modu uçtan uca** (525Member kopyası, AISC 360-22, SSO, 4 örümcek, 12 analiz, `Opt_Finalize` dahil) | Ayrıntılar aşağıda. |

Tüp modu uçtan uca testinin ayrıntıları:
- **Başlangıç:**
  - katalog 149 kesit, dolgu betonu 4000Psi (modelden);
  - sınırlar: kiriş W[12–163]; tüp grupları [0–34] … [0–71] (149 kesit içinde).
- **Arama sırasında:** General kesitler (`CFT_…`); 6 kez kesit oluşturma, ortalama 4,9 s.
- **Final:**
  - 7 kesit `Filled Steel Tube` tablosuyla oluşturuldu, tasarım prosedürü 13;
  - `m_best.$et` dosyasında örnek kesit: `SHAPE "Filled Steel Tube" D 550 B 550 TF 20 TW 20 FILLMATERIAL "4000Psi"`.
- **İç hesap / ETABS karşılaştırması** (10 kolon grubu, PMM):

  | Grup | ETABS | İç hesap |
  |---|---|---|
  | 5 | 0,862 | 0,868 |
  | 6 | 0,969 | 0,975 |
  | 7 | 0,654 | 0,658 |
  | 8 | 0,839 | 0,844 |
  | 9 | 0,475 | 0,456 |
  | 10 | 0,636 | 0,611 |
  | 11 | 0,291 | 0,296 |
  | 12 | 0,486 | 0,491 |
  | 13 | 0,180 | 0,188 |
  | 14 | 0,342 | 0,349 |

  En büyük ETABS / iç hesap oranı 1,041; program `CompositeStrengthFactor = 1,04` önerdi. Gömülü kesitte bu oran yaklaşık 1,04–1,12 idi; dolgulu kutuda uyum daha iyi.
- **Süreler:**
  - analiz ortalaması 21,9 s;
  - ETABS kompozit kolon tasarımı (yalnızca finalde, 225 kolon) **849 s**;
  - toplam 2022 s.
- **Çıktılar:** sonuç XML'i, Excel, `_best.EDB` ve yedek yazıldı; çalışma klasörü silindi, ETABS kapandı.

Notlar:
- 12 analizlik bütçeyle uygun (cezasız) tasarım bulunamadı; final cezası 0,8365. Test, final yolunu en iyi bellek üyesiyle çalıştırdı. Bu kısa test bir optimizasyon sonucu değildir.
- Grup 6 için ETABS PMM oranı 0,969 iken "Combined D/C ratio exceeded" mesajı verdi. Mesajın hangi istasyon veya kontrolden geldiği incelenmeli.
- **Sismik süneklik:** AISC 341 (TBDY Bölüm 9) kompozit moment çerçevesinde dolgulu kolonlar için daha sıkı genişlik/kalınlık sınırları (yüksek / orta süneklik) koyar. Katalog şu an yalnızca AISC 360 λmax süzgecini uyguluyor. İstenirse ayar olarak eklenebilir.

---

## 2026-10-03 — Aşama 3–4: Sosyal Örümcek Algoritması (SSO) — Fortran'dan çeviri, 16. yöntem

**İstek:** "Fortran kodunu bulduysan çevir. Referans ETABS programında 15 adet optimizasyon programı var. 16. olarak SSO ekle." Plandaki Aşama 3 (inceleme) ile Aşama 4 (çeviri) birlikte yapıldı.

**Kaynaklar** (`fortran\SocialSpider\`):
- `SSO_Column\SSO.f90` (2015);
- `SSO_Frame\SSO.f90` (2018);
- `SSO.m`;
- Cuevas vd. (2013).

Karşılaştırma, çeviri kararları ve taşınmayan 8 hata `Ajan/Yetenekler/Optimizasyon_Yetenegi/SSO_CEVIRI_NOTLARI.md` dosyasında. Taşınmayan hataların başlıcaları:
- sınır kırpmasında önceki döngüden kalan `k` indisi;
- yuvarlamanın yeni tasarım yerine eski konuma yazılması;
- en iyi örümcek ile en iyi dişinin karışması ve dizi dışına taşma;
- toplamı 1'i aşan rulet olasılıkları.

**Kod**
- Yeni dosya `SocialSpider.vb` (`Partial Class OptimizationClass`):
  - `Main_SocialSpider`: dişi ve erkek hareketleri;
  - `SpiderMating`: baskın erkeklerin çiftleşmesi;
  - isteğe bağlı `SpiderJump` (SSO_Column eki);
  - yardımcılar: ağırlık, titreşim uzaklığı, medyan, cinsiyet dengesi.
- `Structures.vb`:
  - `OptMethod_.SocialSpider = 15`;
  - `Member_.IsMale`: örümceğin cinsiyeti; bellek her döngüde sıralandığı için üyeyle birlikte taşınıyor;
  - `AlgorithmState_.SpiderFemales`: dişi sayısı, yedeğe girer.
  - Alanlar yalnızca eklendi; eski yedekler okunabilir.
- `OptimizationClass.Main`: SSO çağrısı eklendi.
- `OptimizationMethods.vb`:
  - `InitMethodState` SSO durumunu kuruyor;
  - katalog girdisi eklendi: PF 0,7; çiftleşme yarıçapı 0,5 × aralık; kabul *If better* / *Always*; *Spider jump* hayır.
- `.vbproj`: `SocialSpider.vb` eklendi.
- Sürüm 0.3.0.
- Belgeler:
  - `KULLANIM_KILAVUZU.md`: 6.1 yöntem tablosu, 6.2 SSO önerileri, durum notu;
  - `README.md`: 16 yöntem;
  - `PROGRAM_KURALLARI.md`: A3, B6;
  - `Ajan/`: akış şeması, kuyruk, hafıza.

**Kullanıcı kararları (bu aşamada alındı, sonraki aşamalara işlendi)**
- **Aşama 5:** tüp kesitler ETABS kütüphanesinden alınacak.
- **Aşama 6:** kolon tipi hem kullanıcı tarafından belirlenebilecek hem de optimizasyon değişkeni olacak. Amaç, "belirli kata kadar kompozit, üstü çelik" geçişinin optimizasyonu. Taslak tasarım `Ajan/Gorevler/AKIS_SEMASI.md` Bölüm 4'te.

**Testler**

| Test | Sonuç |
|---|---|
| vbc derlemesi (uygulama, MathTest, SSO testi, form testi, uçtan uca test) | hata yok |
| MSBuild (VS 2026) `Rebuild` | 0 hata, 0 uyarı |
| MathTest (dişli treni, 3000 analiz, 20 üye, 3 tohum) | ilk 30 satır (15 yöntem) Aşama 2 referans çıktısıyla **birebir aynı**. SSO: 2,31E-11 / 1,18E-09 / 2,70E-12, Levy ile aynı (SSO Levy kullanmıyor); hata yok. |
| SSO testi: 7 parametre bileşimi × N = 20/5/2 × 3 tohum | hata, sınır dışı değişken veya durma yok; dişi sayısı her döngüde korundu (`genderErr = 0`); yedek XML gidiş-dönüşünde cinsiyetler ve dişi sayısı aynı. Döngü başına analiz ≈ 1,0–1,15·N, sıçramayla ≈ 1,35–1,5·N. |
| Form testi (ekran görüntüsü) | yöntem listesinde 16 öğe, sonuncusu "Social Spider (SSO)"; parametre kutusu katalogdan kuruluyor. Kabul seçeneğinin metni kutuya sığmıyordu; kısaltıldı. |
| ETABS 22.6 uçtan uca: `525Member` kopyası, kompozit mod, AISC 360-22, tohum 12345, 4 örümcek, 12 analiz, `Opt_Finalize` dahil | başlangıç 490 s; başlangıç belleği 8 analiz, en iyi 8670,7; 1. döngü sonunda 15 analiz, uygun tasarım 8670,71. Final ETABS kompozit doğrulaması çalıştı; sonuç XML'i, Excel, `_best.EDB` ve yedek yazıldı; çalışma klasörü silindi, ETABS kapandı. Toplam 1458 s. Yedekte `SocialSpider`, `SpiderFemales = 2` ve 2 dişi / 2 erkek. |

Notlar:
- Uçtan uca testteki tek uyarı, referansta da bilinen kalibrasyon önerisi: ETABS / iç kompozit oranı en fazla 1,040, öneri `CompositeStrengthFactor = 1,04`. Referans kurallarında önerilen aralık 1,08–1,12.
- Bu makinede Fortran derleyicisi (gfortran, Intel ifort/ifx, flang) olmadığı için Fortran ile adım adım sayısal karşılaştırma yapılamadı. Davranış, makale ve üç kaynak sürümle formül düzeyinde karşılaştırıldı.

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
