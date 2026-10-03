# Program Kuralları (geliştiriciler için)

Bu belge Aşama 2'de referans projenin (`SteelFamewithCompositeColumn_ETABS`) `PROGRAM_KURALLARI.md` dosyasından uyarlanarak genişletilecek. Şimdilik geçerli olan kurallar aşağıda.

## 1. Çalışma düzeni
- Her iş önce bulgu ve öneri olarak sunulur, kullanıcı onaylayınca uygulanır.
- Kararı kullanıcı verir:
  - lisans;
  - dosya silme;
  - mimari değişiklik;
  - depoya girecek veri.
- Her değişiklik test edilir:
  - derleme;
  - birim veya matematik testi;
  - gerekiyorsa ETABS API ile **model kopyası** üzerinde test.
- Her aşama `DEGISIKLIKLER.md` dosyasına "Aşama N" başlığıyla ve test sonuçlarıyla yazılır. Ardından commit ve push yapılır (main).
- Ajan döngüsü `Ajan/README.md` dosyasında anlatılıyor. Görev kuyruğu `Ajan/Gorevler/KUYRUK.md`, kalıcı bilgi `Ajan/Hafiza/` altında.

## 2. Sınırlar
- Açık ETABS oturumuna bağlanılmaz. Orijinal modeller değiştirilmez; çalışma geçici kopya üzerinde yapılır.
- Kişisel bilgiler dışarı gönderilmez.
- Başka kaynaklardan (Fortran, eski VB kodu) kod aktarılırken hatalar taşınmaz. Bulunan hatalar raporlanır.

## 3. Depo
- Depoya girmeyenler (`.gitignore`):
  - derleme çıktıları;
  - ETABS ikili dosyaları ve DLL'leri;
  - CSI'ın `AISC14M.xml` dosyası; ETABS kurulumundan okunmalı;
  - ETABS model ve analiz dosyaları.
- Metin dosyaları UTF-8 olarak kaydedilir. PowerShell betikleri BOM'lu UTF-8 olmalı; Windows PowerShell 5.1 BOM'suz dosyada Türkçe karakterleri bozuyor.
