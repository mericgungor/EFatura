using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.EFatura
{
    //https://haacked.com/archive/2008/10/23/model-binding-to-a-list.aspx/

    public class EFatura
    {
        [Required]
        public string CariKod { get; set; }


        [Required]
        public string CariAd { get; set; }

        [Required]
        public DateTime TarihSaat { get; set; }

        [Required]
        public string FaturaNo { get; set; }

        [Required]
        public string Sube { get; set; }


        public List<EFaturaDetay> EFaturaDetay { get; set; }

        public int FaturaKalemSayisi
        {
            get
            {

                return EFaturaDetay.Count;

            }
        }

        public decimal AraToplam
        {
            get
            {
                decimal araToplam = 0;

                araToplam = EFaturaDetay.Select(o => o.Tutar).Sum();

                return Math.Round(araToplam, 2);

            }
        }
        public decimal IskontoToplam
        {
            get
            {
                decimal iskontoToplam = 0;

                iskontoToplam = EFaturaDetay.Select(o => o.IskontoTutar).Sum();

                return Math.Round(iskontoToplam, 2);

            }
        }
        public decimal NetToplam
        {
            get
            {
                decimal netToplam = 0;

                netToplam = EFaturaDetay.Select(o => o.NetTutar).Sum();

                return Math.Round(netToplam, 2);

            }
        }
        public decimal KdvToplam
        {
            get
            {
                decimal kdvToplam = 0;

                kdvToplam = EFaturaDetay.Select(o => o.KdvTutar).Sum();

                return Math.Round(kdvToplam, 2);

            }
        }
        public decimal GenelToplam
        {
            get
            {
                decimal genelToplam = 0;

                genelToplam = EFaturaDetay.Select(o => o.ToplamTutar).Sum();

                return Math.Round(genelToplam, 2);

            }
        }
    }
    public class EFaturaDetay
    {

        [Required]
        public string StokKod { get; set; }

        [Required]
        public string StokAd { get; set; }

        [Required]
        public int Miktar { get; set; }

        [Required]
        public string Birim { get; set; }

        [Required]
        public decimal Fiyat { get; set; }

        [Required]
        public decimal IskontoOran { get; set; }

        [Required]
        public decimal IskontoTutar { get; set; }

        [Required]
        public decimal KdvOran { get; set; }



        public decimal Tutar
        {
            get
            {
                return Math.Round(Miktar * Fiyat, 2);
            }
        }
        public decimal NetTutar
        {
            get
            {
                return Math.Round((Tutar - IskontoTutar), 2);
            }
        }
        public decimal KdvYuzde
        {
            get
            {
                return Math.Round(KdvOran / 100, 2);//   18=>0.08
            }
        }
        public decimal IskontoYuzde
        {
            get
            {
                return Math.Round(IskontoOran / 100, 2);//   18=>0.08
            }
        }
        public decimal KdvTutar
        {
            get
            {
                return Math.Round(NetTutar * KdvYuzde, 2);
            }
        }
        public decimal ToplamTutar
        {
            get
            {
                return Math.Round(NetTutar + KdvTutar, 2);
            }
        }


    }
}
