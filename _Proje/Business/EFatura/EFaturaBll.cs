using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Serialization;

namespace Business.EFatura
{
    public class EFaturaBll
    {
        public class EFaturaAyarlar
        {
            [Required]
            public string XsltDosyaYolu { get; set; }//xslt fatura tasarım dosyası

            [Required]
            public string XmlDosyaKlasor { get; set; }//üretilen efatura xml dosyasının kaydedileceği klasör

            [Required]
            public string EFaturaImzalayanTcKimlikNo { get; set; }//E Faturayı İmzalayan Kişinin Tc Kimlik Numarası
            public string EFaturaKesilenMusteriTcKimlikNo { get; set; }//E Fatura kesilen Müşterinin (Carinin) Tc Kimlik Numarası
            public string EFaturaKesilenMusteriAd { get; set; }//E Fatura kesilen Müşterinin (Carinin) Adı
            public string EFaturaKesilenMusteriSoyad { get; set; }//E Fatura kesilen Müşterinin (Carinin) Soyadı
            public string EFaturaKesilenMusteriEmail { get; set; }//E Fatura kesilen Müşterinin (Carinin) Emaili
            public string EFaturaKesilenMusteriTelefon { get; set; }//E Fatura kesilen Müşterinin (Carinin) Telefonu

            public string EFaturaImzalayanAdresIlAd { get; set; }//E Faturayı İmzalayan Kişinin İli
            public string EFaturaKesenKurumMersisNo { get; set; }//E Faturayı Kesen Kurumun 16 karakterli Mersis No
            public string EFaturaKesenKurumVergiDairesi { get; set; }//E Faturayı Kesen Kurumun Vergi Dairesi
        }
        public static void EFaturaOlustur(EFatura EFatura, EFaturaAyarlar EFaturaAyarlar)
        {
            //https://ebelge.gib.gov.tr/anasayfa.html
            //https://ebelge.gib.gov.tr/efaturamevzuat.html     >  KILAVUZLAR
            //https://ebelge.gib.gov.tr/dosyalar/kilavuzlar/UBLTR_1.2.1_Kilavuzlar.zip
            //Bkz:  BELGELER\UBL-TR Fatura - V 1.0.pdf      Bkz:  2.3 Fatura Elemanları-Detay (SAYFA-10)      
            //Bkz:  KOD LİSTELERİ\UBL-TR Kod Listeleri - V 1.23.pdf      
            //Bkz:  ORTAK ELEMANLAR\UBL-TR Ortak Elemanlar - V 0.7.pdf


            using (var transaction = new TransactionScope())
            {
                var invoiceType = new InvoiceType();

                //invoiceType.UBLExtensions //Mali Mühür Bilgileri
                invoiceType.UBLVersionID = new UBLVersionIDType() { Value = "2.1" };//indirdiğimiz UBLTR_1.2.1_Kilavuzlar.zip dosyasından oluşturduğumuz class versiyonu "UBL-2_1.cs"
                invoiceType.CustomizationID = new CustomizationIDType() { Value = "TR1.2" };//Bkz Kılavuz => 2.3.3
                invoiceType.ProfileID = new ProfileIDType() { Value = "EARSIVFATURA" };//Bkz Kılavuz => 2.3.4 Bkz:Kod Listeleri => 2.2 Senaryo (ProfileID)
                invoiceType.ID = new IDType() { Value = EFatura.FaturaNo };//Bkz Kılavuz => 2.3.5      13 haneli                                                    [EFatura Seri Sıra no]
                invoiceType.CopyIndicator = new CopyIndicatorType() { Value = false };//Bkz Kılavuz => 2.3.6 Asıl:false    Suret:true
                invoiceType.UUID = new UUIDType() { Value = Guid.NewGuid().ToString() };//Bkz Kılavuz => 2.3.7
                invoiceType.IssueDate = new IssueDateType() { Value = EFatura.TarihSaat };//Bkz Kılavuz => 2.3.8                                                    [Fatura Düzenlenme Tarihi]
                invoiceType.IssueTime = new IssueTimeType() { Value = EFatura.TarihSaat };//Bkz Kılavuz => 2.3.9                                                    [Fatura Düzenlenme Saati]
                invoiceType.InvoiceTypeCode = new InvoiceTypeCodeType() { Value = "SATIS" };//Bkz Kılavuz => 2.3.10  Bkz:Kod Listeleri => 1.4 InvoiceTypeCode       [Fatura Tipi]
                invoiceType.DocumentCurrencyCode = new DocumentCurrencyCodeType() { Value = "TRY" };//Bkz Kılavuz => 2.3.12                                         [Para Birimi]
                invoiceType.LineCountNumeric = new LineCountNumericType() { Value = EFatura.FaturaKalemSayisi };//Bkz Kılavuz => 2.3.18                             [FaturaKalemSayisi]


                #region TaxTotal
                invoiceType.TaxTotal = new TaxTotalType[]{
                    new TaxTotalType {
                        TaxAmount=new TaxAmountType { Value= EFatura.KdvToplam},
                        TaxSubtotal=new TaxSubtotalType[]
                        {
                            new TaxSubtotalType
                            {
                                TaxableAmount=new TaxableAmountType
                                {
                                    currencyID="TRY",
                                    Value=EFatura.NetToplam//Kdv Hariç Net Tutar
                                },
                                TaxAmount=new TaxAmountType
                                {
                                    currencyID="TRY",
                                    Value=EFatura.KdvToplam
                                },
                                CalculationSequenceNumeric=new CalculationSequenceNumericType{Value=1},
                                //Percent=new PercentType1{Value=18}//%18 Kdv için 18.0 girilmeli //TODO:Birden fazla KDV Oranına göre düzenle (1,8,18) gibi
                                TaxCategory=new TaxCategoryType
                                {
                                    TaxScheme=new TaxSchemeType
                                    {
                                        Name=new NameType1{Value="KDV"},
                                        TaxTypeCode=new TaxTypeCodeType{Value="0015"}//Bkz Kod Listesi=>1.9 TaxTypeCode VERGİ KODLARI LİSTESİ
                                    }
                                }
                            }

                        }
                    },

                }; //Bkz Ortak Elemanlar => 2.2.61 
                #endregion

                #region Signature
                invoiceType.Signature = new[]{
                    new SignatureType(){
                        ID = new IDType{
                            schemeID="VKN_TCKN",
                            Value=EFaturaAyarlar.EFaturaImzalayanTcKimlikNo
                        },
                        SignatoryParty=new PartyType{
                            PartyIdentification=new[]{
                                new PartyIdentificationType{
                                    ID=new IDType{
                                        schemeID="VKN",
                                        Value=EFaturaAyarlar.EFaturaImzalayanTcKimlikNo
                                    }
                                }
                            },
                            PostalAddress=new AddressType{//zorunlu değil
                                CityName=new CityNameType{Value= EFaturaAyarlar.EFaturaImzalayanAdresIlAd }
                                //TODO:İmzayan kişinin diğer adres bilgilerini getir
                            }
                        },
                        DigitalSignatureAttachment=new AttachmentType{
                            //EmbeddedDocumentBinaryObject      //TODO:İmzalama İşlemi olduğu zaman eklenecek
                            ExternalReference=new ExternalReferenceType{
                                URI=new URIType{
                                    Value="#Signature_"+EFatura.FaturaNo
                                }
                            }
                        }
                    }
                };//Bkz Kılavuz => 2.3.27                             [E Faturayı İmzayan Bilgisi] 
                #endregion

                #region AccountingSupplierParty
                invoiceType.AccountingSupplierParty = new SupplierPartyType
                {
                    Party = new PartyType
                    {
                        PartyIdentification = new PartyIdentificationType[] {
                            new PartyIdentificationType{
                                ID=new IDType{
                                    schemeID="VKN",
                                    Value=EFaturaAyarlar.EFaturaImzalayanTcKimlikNo
                                }
                            },
                            new PartyIdentificationType{
                                ID=new IDType{
                                    schemeID="MERSISNO",
                                    Value=EFaturaAyarlar.EFaturaKesenKurumMersisNo
                                }
                            }
                        },
                        PostalAddress = new AddressType
                        {//zorunlu değil
                            CityName = new CityNameType { Value = EFaturaAyarlar.EFaturaImzalayanAdresIlAd }
                            //TODO:İmzayan kişinin diğer adres bilgilerini getir
                        },
                        //Person //TODO:Faturayı Kesen Şahış ise burayı doldur
                        PartyTaxScheme = new PartyTaxSchemeType { TaxScheme = new TaxSchemeType { Name = new NameType1 { Value = EFaturaAyarlar.EFaturaKesenKurumVergiDairesi } } }
                    }
                };//Bkz Kılavuz => 2.3.28                             [FaturayıDüzenleyenTarafın Bilgileri] 
                #endregion

                #region AccountingCustomerParty
                invoiceType.AccountingCustomerParty = new CustomerPartyType
                {
                    Party = new PartyType
                    {
                        PartyIdentification = new PartyIdentificationType[] {
                            new PartyIdentificationType{
                                ID=new IDType{
                                    schemeID="TCKN",
                                    Value=EFaturaAyarlar.EFaturaKesilenMusteriTcKimlikNo
                                }
                            }
                        },
                        PartyName = new PartyNameType { Name = new NameType1 { Value = EFatura.CariAd } },
                        PostalAddress = new AddressType
                        {//zorunlu değil
                            //CityName = new CityNameType { Value = EFaturaAyarlar }
                            //TODO:Müşterinin diğer adres bilgilerini getir
                        },
                        Contact = new ContactType
                        {
                            ElectronicMail = new ElectronicMailType { Value = EFaturaAyarlar.EFaturaKesilenMusteriEmail },
                            Telephone = new TelephoneType { Value = EFaturaAyarlar.EFaturaKesilenMusteriTelefon }
                        },
                        Person = new PersonType
                        {
                            FirstName = new FirstNameType { Value = EFaturaAyarlar.EFaturaKesilenMusteriAd },
                            FamilyName = new FamilyNameType { Value = EFaturaAyarlar.EFaturaKesilenMusteriSoyad }

                        }
                    }

                };//Bkz Kılavuz => 2.3.29                             [Müşteri (Fatura Kesilen Cari) Bilgileri] 
                #endregion

                #region LegalMonetaryTotal-ALT TOPLAMLAR
                invoiceType.LegalMonetaryTotal = new MonetaryTotalType
                {
                    LineExtensionAmount = new LineExtensionAmountType { Value = EFatura.AraToplam },
                    TaxExclusiveAmount = new TaxExclusiveAmountType { Value = EFatura.NetToplam },
                    TaxInclusiveAmount = new TaxInclusiveAmountType { Value = EFatura.GenelToplam },
                    AllowanceTotalAmount = new AllowanceTotalAmountType { Value = EFatura.IskontoToplam },//opsiyonel
                    PayableAmount = new PayableAmountType { Value = EFatura.GenelToplam }
                };
                #endregion

                #region Opsiyonel Alanlar

                invoiceType.Note = new[] {
                        new NoteType { Value = Utility.SayiyiYaziyaCevir(EFatura.GenelToplam)},
                        new NoteType { Value = "İş bu fatura muhteviyatına 7 gün içerisinde itiraz edilmediği taktirde aynen kabul edilmiş sayılır." },
                        new NoteType { Value = $"Cari Kod:{EFatura.CariKod} Cari Ad:{EFatura.CariAd}"}
                };//Bkz Kılavuz => 2.3.11


                invoiceType.AdditionalDocumentReference = new[] {
                        new DocumentReferenceType
                        {
                            ID=new IDType{Value=Guid.NewGuid().ToString()},
                            IssueDate=new IssueDateType{Value=EFatura.TarihSaat},
                            DocumentType=new DocumentTypeType{Value="XSLT"},
                            Attachment=new AttachmentType
                            {
                                EmbeddedDocumentBinaryObject=new EmbeddedDocumentBinaryObjectType
                                {
                                    characterSetCode="UTF-8",
                                    encodingCode="Base64",
                                    filename="EFatura.xslt",
                                    mimeCode="application/xml",
                                    Value=Encoding.UTF8.GetBytes(new StreamReader(new FileStream(EFaturaAyarlar.XsltDosyaYolu,FileMode.Open,FileAccess.Read),Encoding.UTF8).ReadToEnd())
                                }
                            }
                        },
                        new DocumentReferenceType
                        {
                            ID=new IDType{Value=Guid.NewGuid().ToString()},
                            IssueDate=new IssueDateType{Value=EFatura.TarihSaat},
                            DocumentTypeCode=new DocumentTypeCodeType{Value="SendingType"},
                            DocumentType=new DocumentTypeType{Value="ELEKTRONIK"}
                        }

                };//Bkz Kılavuz => 2.3.26 [xslt fatura tasarım dosyası]

                #endregion


                #region InvoiceLine- FATURA KALEMLERİ
                List<InvoiceLineType> ListInvoiceLineType = new List<InvoiceLineType>();

                var iEFaturaDetay = 1;

                foreach (var EFaturaDetay in EFatura.EFaturaDetay)
                {
                    var invoiceLineType = new InvoiceLineType
                    {
                        ID = new IDType { Value = iEFaturaDetay.ToString() },
                        InvoicedQuantity = new InvoicedQuantityType { unitCode = EFaturaDetay.Birim, Value = EFaturaDetay.Miktar },
                        LineExtensionAmount = new LineExtensionAmountType { Value = EFaturaDetay.NetTutar },
                        Item = new ItemType { Name = new NameType1 { Value = EFaturaDetay.StokKod + " " + EFaturaDetay.StokAd } },
                        Price = new PriceType { PriceAmount = new PriceAmountType { Value = EFaturaDetay.Fiyat } },

                        #region İskonto
                        AllowanceCharge = new AllowanceChargeType[]
                        {
                            new AllowanceChargeType {
                                ChargeIndicator=new ChargeIndicatorType { Value=false},
                                MultiplierFactorNumeric=new MultiplierFactorNumericType{Value=EFaturaDetay.IskontoYuzde},
                                Amount=new AmountType2{Value=EFaturaDetay.IskontoTutar},
                                BaseAmount=new BaseAmountType{Value=EFaturaDetay.Tutar}
                            }
                        },//İskonto 
                        #endregion

                        #region Vergi
                        TaxTotal = new TaxTotalType
                        {
                            TaxAmount = new TaxAmountType { Value = EFaturaDetay.KdvTutar },
                            TaxSubtotal = new TaxSubtotalType[]
                            {
                                new TaxSubtotalType
                                {
                                    TaxableAmount=new TaxableAmountType
                                    {
                                        currencyID="TRY",
                                        Value=EFaturaDetay.NetTutar//Kdv Hariç Net Tutar
                                    },
                                    TaxAmount=new TaxAmountType
                                    {
                                        currencyID="TRY",
                                        Value=EFaturaDetay.KdvTutar
                                    },
                                    CalculationSequenceNumeric=new CalculationSequenceNumericType{Value=1},
                                    Percent=new PercentType1{Value=EFaturaDetay.KdvOran},//%18 Kdv için 18.00 gir                                
                                    TaxCategory=new TaxCategoryType
                                    {
                                        TaxScheme=new TaxSchemeType
                                        {
                                            Name=new NameType1{Value="KDV"},
                                            TaxTypeCode=new TaxTypeCodeType{Value="0015"}//Bkz Kod Listesi=>1.9 TaxTypeCode VERGİ KODLARI LİSTESİ
                                        }
                                    }
                                }
                            }
                        }
                        #endregion


                    };
                    ListInvoiceLineType.Add(invoiceLineType);
                    iEFaturaDetay++;
                }

                invoiceType.InvoiceLine = ListInvoiceLineType.ToArray();

                #endregion


                #region Xml Serialization

                #region XmlSerializerNamespaces
                var ns = new XmlSerializerNamespaces();
                ns.Add(prefix: "cac", ns: "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
                ns.Add(prefix: "xsi", ns: "http://www.w3.org/2001/XMLSchema-instance");

                ns.Add(prefix: "xades", ns: "http://uri.etsi.org/01903/v1.3.2#");
                ns.Add(prefix: "udt", ns: "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2");
                ns.Add(prefix: "ubltr", ns: "urn:oasis:names:specification:ubl:schema:xsd:TurkishCustomizationExtensionComponents");
                ns.Add(prefix: "qdt", ns: "urn:oasis:names:specification:ubl:schema:xsd:QualifiedDataTypes-2");
                ns.Add(prefix: "ext", ns: "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
                ns.Add(prefix: "cbc", ns: "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
                ns.Add(prefix: "ccts", ns: "urn:un:unece:uncefact:documentation:2");
                ns.Add(prefix: "ds", ns: "http://www.w3.org/2000/09/xmldsig#");
                #endregion

                var xmlWriterSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
                var ms = new MemoryStream();
                var writer = XmlWriter.Create(ms, xmlWriterSettings);
                var srl = new XmlSerializer(invoiceType.GetType());
                srl.Serialize(writer, invoiceType, ns);
                ms.Flush();
                ms.Seek(offset: 0, loc: SeekOrigin.Begin);
                var sr = new StreamReader(ms);
                var readXml = sr.ReadToEnd();
                if (!Directory.Exists(EFaturaAyarlar.XmlDosyaKlasor))
                    Directory.CreateDirectory(EFaturaAyarlar.XmlDosyaKlasor);


                var path = Path.Combine(EFaturaAyarlar.XmlDosyaKlasor, EFatura.FaturaNo + ".xml");//Her zaman üzerinde yazılacak

                using (var sWriter = new StreamWriter(path, append: false, encoding: Encoding.UTF8))//Fatura SeriSıraNo daha okunabilir, sıralanabilir.
                {
                    sWriter.Write(readXml);
                    sWriter.Close();

                }

                #endregion

            }//using transaction
        }


    }
}
