using System;
using System.Collections.Generic;
using System.Text;

namespace Business.EFatura
{
    public class BirimBll
    {
        public static List<Birim> GetirBirimListesi()
        {
            //https://ebelge.gib.gov.tr/anasayfa.html
            //https://ebelge.gib.gov.tr/efaturamevzuat.html     >  KILAVUZLAR
            //https://ebelge.gib.gov.tr/dosyalar/kilavuzlar/UBLTR_1.2.1_Kilavuzlar.zip
            //Bkz:  KOD LİSTELERİ\UBL-TR Kod Listeleri - V 1.23.pdf
            //Bkz:  ÖLÇÜ BİRİM KODLARI LİSTESİ (SAYFA-7)

            List<Birim> ListBirim = new List<Birim>();

            ListBirim.Add(new Birim() { BirimKod = "C62", BirimAd = "ADET" });
            ListBirim.Add(new Birim() { BirimKod = "LTR", BirimAd = "LİTRE" });

            return ListBirim;
        }

        public class Birim
        {
            public string BirimKod { get; set; }
            public string BirimAd { get; set; }
        }

    }

}
