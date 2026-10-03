# ETABS Kullanım Yeteneği

Bu yetenek şunları kapsar:

- ETABS 22.6 OAPI (`ETABSv1.dll`) ile bağlantı;
- **yalnızca model kopyası** üzerinde çalışma: geçici klasöre kopyalanır, orijinal model ve açık ETABS oturumuna dokunulmaz;
- grup tabanlı kesit atama, analiz, çelik ve kompozit kolon tasarımı (`DesignSteel`, `DesignCompositeColumn`);
- öteleme ve yerdeğiştirme okuma;
- yeniden başlatma ve bellek yönetimi.

Kaynak: referans projedeki `ETABSClass.vb` dosyası ve onun PROGRAM_KURALLARI.md Bölüm 4 ve 9.

Bilinen tuzaklar (referans projede ölçülmüş):

- E2K gidiş-dönüşü modeli kayıplı değiştiriyor; bu yüzden yalnızca EDB kullanılmalı.
- Yeniden açılan modelde kombinasyon seçimi siliniyor; her tasarımdan önce kontrol edilmeli.
- `eFramePropType` değerleri: I = 1, FilledTube = 29, **EncasedRectangle = 31**, EncasedCircle = 32.