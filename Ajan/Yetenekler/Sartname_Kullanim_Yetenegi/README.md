# Şartname Kullanım Yeteneği

Bu yetenek şu kuralları kapsar:

- **AISC 360-22 (ve 360-16):** gömülü kompozit kolonlar (Bölüm I: I1, I2.1, I5) ve çelik elemanlar (Bölüm D, E, F, G, H).
- **Kesit kuralları:** gömülü I/H profil + beton + boyuna donatı. Kontrol edilenler: en az çelik oranı (%1), en az donatı oranı (%0,4), boyuna ve enine donatı, pas payı.
- **Dayanımlar:**
  - eksenel basınç (Pno, Pe, EIeff);
  - eğilme (plastik gerilme dağılımı veya şekil değiştirme uyumu);
  - etkileşim (H1 / I5).
- **Servis kısıtları:** öteleme ve yerdeğiştirme sınırları, deprem ötelemesinin büyütülmesi.

Kaynaklar:

- referans projedeki `AISC360_22_Composite_Column_Rules.md`, `AISC360_16_Composite_Column_Rules.md` ve `CompositeColumn.vb`;
- Fortran: `Algoritmalar\fortran\SocialSpider\Composite_Design\SSO_Frame_Com\Source1.f90` (`encased_composite`).