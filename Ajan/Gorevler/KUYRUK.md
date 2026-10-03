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

## Aşama 3 — Fortran SSO incelemesi
- [ ] F1, F2 ve F3'ü karşılaştır; bulgu raporunu hazırla.

## Aşama 4 — SSO'nun VB.NET'e aktarılması
- [ ] `OptimizationMethods.vb` dosyasına SSO'yu ekle ve `MethodCatalog`'a kaydet; parametreler formda görünsün.
- [ ] Matematik testleri ve Fortran ile davranış karşılaştırması.

## Aşama 5 — Dolgulu tüp kolon (CFT/CFP)
- [ ] (?) Kesit listesi ve beton sınıfı.
- [ ] ETABS'te dolgulu kesit tanımı (DatabaseTables) için API testi.
- [ ] İç çözücü (AISC 360-22 I2.2) ve ETABS ile karşılaştırma.

## Aşama 6 — Hibrit tasarım
- [ ] (?) Seçimi kim yapacak: kullanıcı mı, optimizasyon değişkeni mi?

## Aşama 7 — Uçtan uca koşu
## Aşama 8 — (isteğe bağlı) Kompozit döşeme
## Aşama 9 — Kılavuz, README, dağıtım
