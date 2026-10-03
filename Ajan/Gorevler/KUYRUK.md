# Görev Kuyruğu

`[ ]` bekliyor · `[~]` sürüyor · `[x]` bitti · `(?)` kullanıcı kararı gerekli

## Aşama 1 — Ajan mimarisi ve depo (2026-10-03)
- [x] SFCS, depo adıyla yeni klasöre kopyalandı. `bin`, `obj` ve `.vs` alınmadı. SFCS dokunulmadan duruyor.
- [x] `Ajan/kur_ajan_mimarisi.ps1` betiğiyle klasör mimarisi kuruldu. Betik iki kez çalıştırıldı; ikinci çalıştırmada üzerine yazma olmadı.
- [x] Akış şeması taslağı yazıldı (`AKIS_SEMASI.md`).
- [x] Kayıt dosyaları ve `.gitignore` eklendi; ilk commit ve push yapıldı.
- [ ] (?) Akış şeması onayı ve AKIS_SEMASI.md Bölüm 4'teki sorular.

## Aşama 2 — Referans kodun aktarılması
- [ ] R1–R6 kaynaklarını kopyala; ad alanı, başlık ve sürümü uyarla.
- [ ] (?) Eski iskelet kodun akıbeti.
- [ ] Derleme: vbc ve VS 2026.
- [ ] MathTest: 15 yöntem referansla aynı sonucu vermeli.
- [ ] 525M kopyası: 2 değerlendirme referans değerlerle aynı olmalı.

## Aşama 3 — Fortran SSO incelemesi
- [ ] F1, F2 ve F3'ü karşılaştır; bulgu raporunu hazırla.

## Aşama 4 — SSO'nun VB.NET'e aktarılması
- [ ] `OptimizationMethods.vb` dosyasına SSO'yu ekle ve `MethodCatalog`'a kaydet; parametreler formda görünsün.
- [ ] Matematik testleri ve Fortran ile davranış karşılaştırması.

## Aşama 5 — Projeye özgü farklar
- [ ] (?) Bölüm 4'teki kararlara göre tanımlanacak.

## Aşama 6 — Uçtan uca koşu ve doğrulama
## Aşama 7 — Kılavuz, README ve dağıtım
