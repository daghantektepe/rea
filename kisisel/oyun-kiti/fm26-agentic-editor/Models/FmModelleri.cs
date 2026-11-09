using System;
using System.Collections.Generic;

namespace FM26AgenticEditor.Models
{
    /// <summary>
    /// FM26 içindeki bir oyuncunun düzenlenebilir temel, fiziksel, tıbbi, gizli ve istikrar verilerini temsil eder.
    /// </summary>
    public class OyuncuVerisi
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } = "";
        public string KulupAdi { get; set; } = "";
        public string Pozisyon { get; set; } = "";
        public int FormaNumarasi { get; set; } = 10;
        public int Yas { get; set; }
        public string Uyruk { get; set; } = "";

        // Masaüstü Ergonomi: FM26 Kart Arayüzünde Gizlenen Fiziksel Veriler
        public int BoyCm { get; set; } = 180;
        public int KiloKg { get; set; } = 75;
        public string TercihEdilenAyak { get; set; } = "Sağ";

        // Yetenek (1 - 200)
        public int MevcutYetenekCA { get; set; }
        public int PotansiyelYetenekPA { get; set; }

        // Tıbbi Durum & Kondisyon
        public int Kondisyon { get; set; } = 100;      // 1 - 100 (Fiziksel Kondisyon)
        public int MacKondisyonu { get; set; } = 95;   // 1 - 100 (Match Sharpness - mikro sakatlıkta sıfırlanan değer)
        public int Moral { get; set; } = 15;           // 1 - 20
        public bool SakatMi { get; set; }
        public int SakatlikGunSayisi { get; set; }     // 1-3 veya 7-12 günlük mikro sakatlık tespiti için

        // Gizli Özellikler, İstikrar ("Messi vs Halı Saha") & Sadakat (1 - 20)
        public int SakatlikEgilimi { get; set; } = 8;        // Injury Proneness (1-20, düşük olması iyi)
        public int Sureklilik { get; set; } = 15;            // Consistency (1-20)
        public int OnemliMacPerformansi { get; set; } = 15;  // Important Matches (1-20)
        public int Profesyonellik { get; set; } = 15;        // Professionalism (1-20)
        public int BaskiyaDayaniklilik { get; set; } = 14;   // Pressure (1-20)
        public int Sadakat { get; set; } = 15;               // Loyalty (1-20)
        public int TakimaKatilisAySayisi { get; set; } = 14; // Yeni transferlerin erken isyanını engellemek için
        public bool MutsuzlukVarMi { get; set; }             // İrrasyonel Suudi Arabistan / transfer isyanı

        // Sözleşme & Değer
        public int HaftalikMaasEuro { get; set; }
        public int SozlesmeBitisYili { get; set; }
        public long PiyasaDegeriEuro { get; set; }
        public long SerbestKalmaBedeliEuro { get; set; }     // 0 ise serbest kalma maddesi yok

        // Temel Özellikler (1 - 20)
        public int Bitiricilik { get; set; }
        public int Pas { get; set; }
        public int Teknik { get; set; }
        public int KararAlma { get; set; }
        public int Vizyon { get; set; }
        public int Hiz { get; set; }
        public int Dayaniklilik { get; set; }

        // Duran Top & Hava Hakimiyeti Özellikleri (1 - 20)
        public int Ziplama { get; set; } = 12;     // Jumping Reach
        public int KafaVurusu { get; set; } = 12;  // Heading

        // Regen (Newgen) Görsel Eşleştirme Bilgisi
        public bool NewgenMi { get; set; }
        public string EtnikBolgeKodu { get; set; } = "MED"; // MED, EUR, SAM, AFR, ASI vb.

        /// <summary>
        /// Duran top ekranında forma numarasının yanında gösterilecek Hava Hakimiyeti (Aerial Threat) puanı (0-100).
        /// </summary>
        public int HavaHakimiyetiPuani =>
            Math.Clamp((int)Math.Round(((BoyCm - 160) * 0.8) + (Ziplama * 2.2) + (KafaVurusu * 1.5)), 1, 100);

        /// <summary>
        /// "1 Maç Messi, 1 Maç Halı Saha" sorununun kök nedenini ölçen Tutarlılık İndeksi (0-100).
        /// Süreklilik (%50), Önemli Maç Performansı (%30) ve Profesyonellik (%20) ağırlıklıdır.
        /// </summary>
        public int TutarlilikIndeksi =>
            Math.Clamp((int)Math.Round((Sureklilik * 2.5) + (OnemliMacPerformansi * 1.5) + (Profesyonellik * 1.0)), 1, 100);

        public string TutarlilikEtiketi =>
            TutarlilikIndeksi >= 80 ? "Elit İstikrar ✅"
            : (TutarlilikIndeksi >= 65 ? "Güvenilir"
            : "Dalgalı (1 Maç Messi / 1 Maç Halı Saha) ⚠️");

        public bool HarryKaneIstisnasiMi => Yas >= 30 && MevcutYetenekCA >= 172;

        public bool KaleciMi => Pozisyon.StartsWith("KL", StringComparison.OrdinalIgnoreCase) ||
                                Pozisyon.StartsWith("GK", StringComparison.OrdinalIgnoreCase);

        // IL2CPP Canlı Nesne Bağlantısı
        public IntPtr Il2CppNesneAdresi { get; set; }
        public object KaynakNesne { get; set; }
    }

    /// <summary>
    /// FM26 içindeki bir kulübün finans, tesis, yönetim esnekliği, Benfica ihracat katsayısı ve kadro yaş eğrisi verilerini temsil eder.
    /// </summary>
    public class KulupVerisi
    {
        public int Id { get; set; }
        public string KulupAdi { get; set; } = "";
        public string UlkeAdi { get; set; } = "Türkiye";
        public string LigAdi { get; set; } = "";
        public int LigItibari { get; set; } = 7100; // 1 - 10000
        public int Itibar { get; set; }             // 1 - 10000

        // Yaşayan Dünya & Dinamik İhracat (Benfica Katsayısı)
        public int AvrupaBasariPuani { get; set; } = 45;     // 0 - 200 (UEFA 5 yıllık katsayı / Avrupa mirası)
        public float IhracatDegerCarpani { get; set; } = 1.0f; // 1.0x (Sabit ülke tavanı) -> 3.2x (Benfica/Ajax ihracat tavanı)
        public float KadroYasOrtalamasi { get; set; } = 26.2f;
        public int KulupMirasSeviyesi { get; set; } = 1;     // 1 - 10 (Kazanılan kupalarla kalıcı sınıf atlayan miras)
        public string DunyaDurumEtiketi { get; set; } = "Dengeli";

        // Finans (Euro)
        public long BakiyeEuro { get; set; }
        public long TransferButcesiEuro { get; set; }
        public long MaasButcesiHaftalikEuro { get; set; }
        public long SponsorlukGeliriYillikEuro { get; set; }

        // Tesisler (1 - 20)
        public int AntrenmanTesisleri { get; set; }
        public int AltyapiTesisleri { get; set; }
        public int GenclikAkademisi { get; set; }

        // Yönetim (Board) & Lig Kuralları
        public int YonetimTesisVeButceEsnekligi { get; set; } = 12; // 1 - 20
        public int YabanciKadroSiniri { get; set; } = 14;
        public int YabanciIlk11Siniri { get; set; } = 11;

        // IL2CPP Canlı Nesne Bağlantısı
        public IntPtr Il2CppNesneAdresi { get; set; }
        public object KaynakNesne { get; set; }
    }

    /// <summary>
    /// Topluluk araştırmasındaki ve geri bildirimlerdeki sistemik FM26 sorunlarını düzelten canlı ayarlar.
    /// </summary>
    public class SimulasyonRehabilitasyonAyarlari
    {
        // 1. Ergonomi & Masaüstü Deneyimi
        public bool MiniHudRontgenAcik { get; set; } = true;
        public bool KlasikBoslukTusuYamasi { get; set; } = true;

        // 2. Tıbbi Denge & Mikro Sakatlık Kalkanı
        public int SakatlikFrekansiCarpaniYuzde { get; set; } = 68;
        public bool KaleciTemassizSakatlikKorumasi { get; set; } = true;
        public bool MikroSakatlikKondisyonKoruyucu { get; set; } = true;

        // 3. Maç Motoru & Çevre
        public bool SurekliYagmurDuzeltici { get; set; } = true;
        public bool SantraSavunmaUyarici { get; set; } = true;

        // 4. Ekonomi & Diyalog Mantığı
        public bool KullaniciVergisiDengeleyici { get; set; } = true;
        public bool IrrasyonelIsyanFiltresi { get; set; } = true;
        public bool PersonelItibarEsnetici { get; set; } = true;

        // 5. Performans & Regen Görsel
        public bool ArkaPlanSimulasyonHizlandirici { get; set; } = true;

        // 6. Yaşayan Futbol Dünyası & AI Kadro Mühendisliği (v0.5.0)
        public bool Yas30UstuBonservisTavanAktif { get; set; } = true;     // 30+ yaşa (CA<172) yüksek bonservis yasağı
        public bool AiJenerasyonYenilemeZorlayici { get; set; } = true;    // Yaşlanan dev kulüpleri genç transfere zorlar
        public bool DinamikIhracatKatsayisiAktif { get; set; } = true;     // Benfica Katsayısı: Başarıyla artan oyuncu değeri
        public bool OtonomRakipGuclendiriciAktif { get; set; } = true;     // Anti-Add-Manager: Ligdeki rakipleri rekabetçi tutar
        public bool SoyunmaOdasiGercekEtkiAktif { get; set; } = true;      // Soyunma odası konuşmalarının maça gerçek etkisi
    }

    public class DegisiklikKaydi
    {
        public DateTime Zaman { get; set; }
        public string HedefTur { get; set; } = "";
        public string HedefAd { get; set; } = "";
        public string AlanAdi { get; set; } = "";
        public string EskiDeger { get; set; } = "";
        public string YeniDeger { get; set; } = "";
        public Action GeriAlAksiyonu { get; set; }

        public override string ToString()
        {
            return $"[{Zaman:HH:mm:ss}] {HedefTur} ({HedefAd}) - {AlanAdi}: {EskiDeger} -> {YeniDeger}";
        }
    }

    public class Il2CppSinifBilgisi
    {
        public string AssemblyAdi { get; set; } = "";
        public string TamAd { get; set; } = "";
        public int AlanSayisi { get; set; }
        public int OzellikSayisi { get; set; }
        public int MetotSayisi { get; set; }
        public List<string> OrnekUyeler { get; set; } = new List<string>();
    }
}
