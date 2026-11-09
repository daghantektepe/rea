using System;
using System.Collections.Generic;

namespace FM26AgenticEditor.Models
{
    /// <summary>
    /// FM26 içindeki bir oyuncunun düzenlenebilir temel, fiziksel, tıbbi ve gizli verilerini temsil eder.
    /// Topluluk raporunda belirtilen "alt menülere gizlenmiş masaüstü verilerini" (Boy, Kilo, Sakatlık Eğilimi,
    /// Maç Kondisyonu, Serbest Kalma, Duran Top Hava Hakimiyeti) tek bakışta sunar.
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

        // Gizli Özellikler & Sadakat (1 - 20)
        public int SakatlikEgilimi { get; set; } = 8;        // Injury Proneness (1-20, düşük olması iyi)
        public int Sureklilik { get; set; } = 15;            // Consistency (1-20)
        public int OnemliMacPerformansi { get; set; } = 15;  // Important Matches (1-20)
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

        public bool KaleciMi => Pozisyon.StartsWith("KL", StringComparison.OrdinalIgnoreCase) ||
                                Pozisyon.StartsWith("GK", StringComparison.OrdinalIgnoreCase);

        // IL2CPP Canlı Nesne Bağlantısı
        public IntPtr Il2CppNesneAdresi { get; set; }
        public object KaynakNesne { get; set; }
    }

    /// <summary>
    /// FM26 içindeki bir kulübün finans, tesis, yönetim esnekliği ve lig itibarı verilerini temsil eder.
    /// </summary>
    public class KulupVerisi
    {
        public int Id { get; set; }
        public string KulupAdi { get; set; } = "";
        public string UlkeAdi { get; set; } = "Türkiye";
        public string LigAdi { get; set; } = "";
        public int LigItibari { get; set; } = 7100; // 1 - 10000
        public int Itibar { get; set; }             // 1 - 10000

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
        public int YonetimTesisVeButceEsnekligi { get; set; } = 12; // 1 - 20 (Talepleri reddetme eğilimini çözer)
        public int YabanciKadroSiniri { get; set; } = 14;           // TFF A Takım yabancı sınırı
        public int YabanciIlk11Siniri { get; set; } = 11;           // TFF İlk 11 yabancı sınırı

        // IL2CPP Canlı Nesne Bağlantısı
        public IntPtr Il2CppNesneAdresi { get; set; }
        public object KaynakNesne { get; set; }
    }

    /// <summary>
    /// Topluluk araştırmasındaki sistemik FM26 sorunlarını düzelten canlı rehabilitasyon ayarları.
    /// </summary>
    public class SimulasyonRehabilitasyonAyarlari
    {
        // 1. Ergonomi & Masaüstü Deneyimi
        public bool MiniHudRontgenAcik { get; set; } = true;
        public bool KlasikBoslukTusuYamasi { get; set; } = true;

        // 2. Tıbbi Denge & Mikro Sakatlık Kalkanı
        public int SakatlikFrekansiCarpaniYuzde { get; set; } = 68; // %68 = -%32 sakatlık azaltma (Rapor önerisi: -%25..-%35)
        public bool KaleciTemassizSakatlikKorumasi { get; set; } = true;
        public bool MikroSakatlikKondisyonKoruyucu { get; set; } = true;

        // 3. Maç Motoru & Çevre
        public bool SurekliYagmurDuzeltici { get; set; } = true;
        public bool SantraSavunmaUyarici { get; set; } = true;

        // 4. Ekonomi & Diyalog Mantığı
        public bool KullaniciVergisiDengeleyici { get; set; } = true; // 80-100M€ fahiş bonservis frenleyici
        public bool IrrasyonelIsyanFiltresi { get; set; } = true;     // Yeni transferlerin Suudi Arabistan isyanını engeller
        public bool PersonelItibarEsnetici { get; set; } = true;      // Alt liglerden antrenör/gözlemci getirme kolaylığı

        // 5. Performans & Regen Görsel
        public bool ArkaPlanSimulasyonHizlandirici { get; set; } = true; // Kadın futbolu / pasif lig detay azaltıcı
    }

    /// <summary>
    /// FMRTE tarzı güvenli Geri Al (Undo) yığını için her bir düzenleme adımını kaydeder.
    /// </summary>
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

    /// <summary>
    /// Oyun içinde IL2CPP metadata/assembly taraması yapıldığında bulunan aday sınıfları raporlar.
    /// </summary>
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
