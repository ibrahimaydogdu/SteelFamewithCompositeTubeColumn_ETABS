# Görev Kuyruğu

`[ ]` bekliyor · `[~]` sürüyor · `[x]` bitti · `(?)` kullanıcı kararı gerekli

## Aşama 1 — Ajan mimarisi ve depo (2026-10-03) ✔
- [x] SFCS, depo adıyla yeni klasöre kopyalandı.
- [x] `Ajan/kur_ajan_mimarisi.ps1` ile klasör mimarisi kuruldu.
- [x] Akış şeması taslağı, kayıt dosyaları, `.gitignore`. İlk commit ve push yapıldı.

## Aşama 2 — Referans kodun aktarılması (2026-10-03) ✔
- [x] Kullanıcı kararları işlendi: CFT/CFP, hibrit tasarım, kompozit döşeme isteğe bağlı, iskelet kod kaldırılacak, MIT, EDB depoya girecek.
- [x] Eski iskelet kaldırıldı. Modeller `Modeller/` klasörüne taşındı.
- [x] R1–R6 kopyalandı; ad, sürüm (0.2.0) ve GUID'ler uyarlandı.
- [x] Derleme: vbc ve MSBuild (VS 2026).
- [x] MathTest: referansla birebir aynı.
- [x] 525M kopyası üzerinde ETABS testi: referans derlemeyle karşılaştırıldı.

## Aşama 3–4 — Fortran SSO incelemesi ve çevirisi (2026-10-03) ✔
- [x] F1, F2 ve F3 karşılaştırıldı; 8 hata bulundu ve taşınmadı (`SSO_CEVIRI_NOTLARI.md`).
- [x] `SocialSpider.vb`; `OptMethod_.SocialSpider = 15`, katalog girdisi, `Member_.IsMale`, `AlgorithmState_.SpiderFemales`.
- [x] Testler: MathTest (diğer 15 yöntem değişmedi), SSO testi, form görüntüsü, ETABS uçtan uca testi.

## Aşama 5 — Dolgulu tüp kolon (CFT/CFP) (2026-10-03) ✔
- [x] Kararlar:
  - ETABS kütüphanesi + yapma kutular;
  - beton modelden;
  - boru kapalı;
  - kare kutu, en az 300 mm;
  - gömülü seçenek kalıyor (tek program).
- [x] `TubeColumn.vb`, `TubeSections.xml`, ETABSClass katalog yardımcıları, form tip seçimi.
- [x] Testler:
  - TubeTest 23/23;
  - MathTest ve gömülü mod regresyonu birebir aynı;
  - tüp modu uçtan uca: ETABS / iç hesap ≤ 1,041.
- [x] Grup 6 mesajı incelendi (2026-10-04). Neden: ETABS çelik ve kompozit tasarımında D/C oranını 1,0 ile değil **"D/C ratio limit"** ile karşılaştırıyor (modellerde 0,95). Program ise 1,0 kullanıyor; 0,95–1,0 arasındaki elemanları uygun sayıyor.
  - Kanıt: grup 6'nın beş kolonunda yalnızca PMM 0,969 olan işaretlendi, 0,858 işaretlenmedi.
  - API sınır aşımını göstermiyor: sınır 0,45 yapıldığında 0,52'lik elemanların hata ve uyarı alanları boş geldi.
  - Tercih numaraları: `DesignSteel.AISC360_22.GetPreference(37)` ve `DesignCompositeColumn.AISC360_22.GetPreference(18)`, ikisi de 0,95.
  - Sorun referanstan devralındı ve çelik elemanları da etkiliyor.
- [x] Aşama 5.1 (2026-10-04, 0.4.1):
  - D/C sınırı modelden okunuyor (`DCLimit`) ve oranlar sınıra bölünüyor;
  - TBDY 2018 Tablo 9.3 süneklik süzgeci eklendi (varsayılan yüksek).
  - Yeni 525M regresyon değerleri: 7564,91 / 1,4580 ve 7068,84 / 1,8032.

## Aşama 6 — Hibrit tasarım (2026-10-05, 0.5.0) ✔
- [x] Karar: hem kullanıcı hem optimizasyon değişkeni; "belirli kata kadar kompozit, üstü çelik" geçişinin optimizasyonu.
- [x] Öneri yazıldı: `ASAMA6_ONERI.md` (2026-10-05). İçeriği:
  - Steel / Composite / Hybrid modları;
  - yığın başına geçiş değişkeni;
  - Optimize gruplarda iki kesit değişkeni;
  - geçiş geometrisi kuralı;
  - grup çakışması denetimi.
- [x] Kararlar alındı (yığın + grup bazında geçiş, kullanıcı gruplaması, ters düzende uyarı, 460 kalıyor, form tablosu) ve uygulandı.
- [x] Testler:
  - regresyon birebir aynı;
  - yığın bazında ve grup bazında ETABS testlerinde tip uyumsuzluğu 0;
  - uçtan uca test final doğrulamasıyla tamamlandı.

## Aşama 7 — Uçtan uca koşu
## Aşama 8 — (isteğe bağlı) Kompozit döşeme
## Aşama 9 — Kılavuz, README, dağıtım
