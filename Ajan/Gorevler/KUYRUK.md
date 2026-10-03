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

## Aşama 5 — Dolgulu tüp kolon (CFT/CFP)
- [x] Kesit kaynağı: ETABS kütüphanesi (kullanıcı kararı).
- [x] Beton: modelde tanımlı malzeme (kullanıcı kararı).
- [x] Kütüphane incelemesi ve API testi (460Member kopyası) → `ASAMA5_ONERI.md`.
- [ ] (?) Öneriyi onayla: yapma kutu listesi, gömülü seçeneğinin kalması, borunun varsayılanı, süzgeç.
- [ ] İç çözücü (AISC 360-22 I2.2) ve ETABS ile karşılaştırma.

## Aşama 6 — Hibrit tasarım
- [x] Karar: hem kullanıcı hem optimizasyon değişkeni; "belirli kata kadar kompozit, üstü çelik" geçişinin optimizasyonu.
- [ ] Tasarımı (geçiş katı değişkeni, grup tipi Steel/Composite/Optimize) onaya sun.

## Aşama 7 — Uçtan uca koşu
## Aşama 8 — (isteğe bağlı) Kompozit döşeme
## Aşama 9 — Kılavuz, README, dağıtım
