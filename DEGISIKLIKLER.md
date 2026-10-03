# Değişiklik Kaydı

Her iş "Aşama N" başlığıyla ve test sonuçlarıyla birlikte buraya yazılır. En yeni aşama en üstte durur.

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
