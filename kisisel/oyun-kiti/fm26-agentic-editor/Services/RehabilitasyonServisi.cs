using System;
using System.IO;
using System.Linq;
using System.Text;
using FM26AgenticEditor.Models;

namespace FM26AgenticEditor.Services
{
    /// <summary>
    /// Topluluk araştırmasında tespit edilen FM26 sistemik sorunlarını (Süper Lig düşük itibarı,
    /// kaleci ve mikro sakatlık kaosu, maç kondisyonu sıfırlanması, Suudi Arabistan/transfer isyanları,
    /// sürekli yağmur hatası ve 3D regen yüzlerinin AI yüz paketiyle değiştirilmesi) rehabilite eden servis.
    /// </summary>
    public class RehabilitasyonServisi
    {
        public SimulasyonRehabilitasyonAyarlari Ayarlar { get; } = new SimulasyonRehabilitasyonAyarlari();
        public string SonIslemOzeti { get; private set; } = "Rehabilitasyon modülleri hazır.";

        /// <summary>
        /// 1. Türkiye Süper Lig Gerçekçilik ve İtibar Dengeleme Paketi:
        /// - Süper Lig itibarını Avrupa İlk 8 seviyesine (7850) çıkarır.
        /// - GS, FB, BJK ve TS küresel itibarlarını kıtalararası yıldız transferine uygun seviyeye kalibre eder.
        /// - Yönetimlerin tesis/bütçe taleplerini sürekli reddetmesini önlemek için Yönetim Esnekliğini 18/20 yapar.
        /// - TFF güncel yabancı kuralı parametrelerini (14 kayıt / 11 ilk 11) eşitler.
        /// </summary>
        public void SuperLigGercekcilikPaketiUygula(FmVeriKoprusu kopru)
        {
            if (!CevrimiciKoruma.YazmaIzniVarMi())
            {
                SonIslemOzeti = CevrimiciKoruma.DurumMesaji;
                return;
            }

            int etkilenen = 0;
            foreach (var k in kopru.Kulupler.Where(x => x.LigAdi.Contains("Süper Lig", StringComparison.OrdinalIgnoreCase)))
            {
                int hedefItibar = k.KulupAdi switch
                {
                    var ad when ad.Contains("Galatasaray", StringComparison.OrdinalIgnoreCase) => 7950,
                    var ad when ad.Contains("Fenerbahçe", StringComparison.OrdinalIgnoreCase) => 7900,
                    var ad when ad.Contains("Beşiktaş", StringComparison.OrdinalIgnoreCase) => 7650,
                    var ad when ad.Contains("Trabzonspor", StringComparison.OrdinalIgnoreCase) => 7400,
                    _ => Math.Max(k.Itibar, 7000)
                };

                long ekSponsorluk = 15_000_000L;
                long eskiSponsor = k.SponsorlukGeliriYillikEuro;
                k.SponsorlukGeliriYillikEuro += ekSponsorluk;
                k.YabanciKadroSiniri = 14;
                k.YabanciIlk11Siniri = 11;

                kopru.KulupTesisVeItibarGuncelle(
                    k,
                    itibar: hedefItibar,
                    ligItibari: 7850, // Avrupa İlk 8 Lig Kalibrasyonu
                    antrenman: Math.Max(k.AntrenmanTesisleri, 17),
                    altyapi: Math.Max(k.AltyapiTesisleri, 16),
                    akademi: Math.Max(k.GenclikAkademisi, 17),
                    yonetimEsnekligi: 18 // Tesis ve bütçe taleplerini onaylama eğilimi
                );

                etkilenen++;
            }

            SonIslemOzeti = $"🇹🇷 Süper Lig Gerçekçilik Paketi uygulandı: {etkilenen} kulübün itibarı (Avrupa İlk 8 - 7850), sponsorluk geliri ve yönetim tesis esnekliği (18/20) kalibre edildi.";
            kopru.SonBilgiMesaji = SonIslemOzeti;
        }

        /// <summary>
        /// 2. Tıbbi Denge, Kaleci Koruması ve Mikro-Sakatlık Maç Kondisyonu (Sharpness) Kalkanı:
        /// - Kalecilerin temassız mikro sakatlıklarını (1-12 gün) anında temizler.
        /// - Kısa süreli (1-12 gün) mikro sakatlık yaşayan oyuncuların sıfırlanan Maç Kondisyonunu (Match Sharpness) %90'a korur.
        /// </summary>
        public void TibbiDengeVeMikroSakatlikKalkaniniCalistir(FmVeriKoprusu kopru)
        {
            if (!CevrimiciKoruma.YazmaIzniVarMi())
            {
                SonIslemOzeti = CevrimiciKoruma.DurumMesaji;
                return;
            }

            int iyilestirilenKaleci = 0;
            int korunanKondisyon = 0;

            foreach (var o in kopru.Oyuncular)
            {
                // Kaleci mikro sakatlık koruması
                if (Ayarlar.KaleciTemassizSakatlikKorumasi && o.KaleciMi && o.SakatMi && o.SakatlikGunSayisi <= 12)
                {
                    bool eSakat = o.SakatMi;
                    int eGun = o.SakatlikGunSayisi;
                    int eMacKond = o.MacKondisyonu;

                    o.SakatMi = false;
                    o.SakatlikGunSayisi = 0;
                    o.MacKondisyonu = Math.Max(o.MacKondisyonu, 92);

                    kopru.KayitEkle("Tıbbi Kalkan", o.AdSoyad, "Kaleci Mikro-Sakatlık İptali",
                        $"Sakat ({eGun} gün, MK:%{eMacKond})", "Sağlıklı (MK:%92)", () =>
                        {
                            o.SakatMi = eSakat;
                            o.SakatlikGunSayisi = eGun;
                            o.MacKondisyonu = eMacKond;
                        });
                    iyilestirilenKaleci++;
                }

                // 1-12 günlük mikro sakatlıklarda maç kondisyonunun (Match Sharpness) sıfırlanmasını engelle
                if (Ayarlar.MikroSakatlikKondisyonKoruyucu && o.SakatlikGunSayisi <= 12 && o.MacKondisyonu < 85)
                {
                    int eMacKond = o.MacKondisyonu;
                    o.MacKondisyonu = 90;
                    FmVeriKoprusu.Il2CppOzellikYaz(o.KaynakNesne, new[] { "MatchSharpness", "MatchFitness" }, 90);

                    kopru.KayitEkle("Tıbbi Kalkan", o.AdSoyad, "Maç Kondisyonu Koruması",
                        $"MK:%{eMacKond}", "MK:%90", () => o.MacKondisyonu = eMacKond);
                    korunanKondisyon++;
                }
            }

            SonIslemOzeti = $"🏥 Tıbbi Kalkan uygulandı: {iyilestirilenKaleci} kaleci mikro sakatlığı iptal edildi, {korunanKondisyon} oyuncunun maç kondisyonu (%90) korundu. Genel sakatlık çarpanı: %{Ayarlar.SakatlikFrekansiCarpaniYuzde}.";
            kopru.SonBilgiMesaji = SonIslemOzeti;
        }

        /// <summary>
        /// 3. İrrasyonel Transfer / Suudi Arabistan İsyan Filtresi:
        /// Kulübe yeni katılmış (<12 ay) veya genç oyuncuların gerçek dışı transfer talebi huzursuzluklarını temizler.
        /// </summary>
        public void IrrasyonelIsyanlariTemizle(FmVeriKoprusu kopru)
        {
            if (!CevrimiciKoruma.YazmaIzniVarMi())
            {
                SonIslemOzeti = CevrimiciKoruma.DurumMesaji;
                return;
            }

            int temizlenen = 0;
            foreach (var o in kopru.Oyuncular.Where(x => x.MutsuzlukVarMi && (x.TakimaKatilisAySayisi <= 12 || x.Yas <= 22)))
            {
                bool eMutsuz = o.MutsuzlukVarMi;
                int eMoral = o.Moral;

                o.MutsuzlukVarMi = false;
                o.Moral = Math.Max(o.Moral, 17);
                FmVeriKoprusu.Il2CppOzellikYaz(o.KaynakNesne, new[] { "IsUnhappy", "WantsTransfer" }, false);
                FmVeriKoprusu.Il2CppOzellikYaz(o.KaynakNesne, new[] { "Morale" }, o.Moral);

                kopru.KayitEkle("Diyalog Filtresi", o.AdSoyad, "İrrasyonel İsyan Temizliği",
                    $"Mutsuz (Moral:{eMoral})", $"Uyumlu (Moral:{o.Moral})", () =>
                    {
                        o.MutsuzlukVarMi = eMutsuz;
                        o.Moral = eMoral;
                    });
                temizlenen++;
            }

            SonIslemOzeti = $"🛡️ İrrasyonel İsyan Filtresi: {temizlenen} yeni/genç oyuncunun gerçek dışı transfer huzursuzluğu giderildi.";
            kopru.SonBilgiMesaji = SonIslemOzeti;
        }

        /// <summary>
        /// 4. Yapay Zeka Regen (Newgen) Yüz Paketi Eşleştirici (config.xml Üretici):
        /// Bozuk 3D regen yüzleri yerine oyundaki Newgen ID'lerini etnik bölgeye göre AI portreleriyle eşleştirir.
        /// </summary>
        public string AiNewgenYuzConfigOlustur(FmVeriKoprusu kopru, string hedefKlasor = "")
        {
            var newgenler = kopru.Oyuncular.Where(o => o.NewgenMi || o.Id >= 2000000000).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("<record>");
            sb.AppendLine("  <boolean id=\"preload\" value=\"false\"/>");
            sb.AppendLine("  <boolean id=\"amap\" value=\"false\"/>");
            sb.AppendLine("  <list id=\"maps\">");

            int sayac = 1;
            foreach (var n in newgenler)
            {
                string portreDosyasi = $"{n.EtnikBolgeKodu}/{n.EtnikBolgeKodu}_{sayac:0000}";
                sb.AppendLine($"    <!-- {n.AdSoyad} ({n.Uyruk}, {n.KulupAdi}) -->");
                sb.AppendLine($"    <record from=\"{portreDosyasi}\" to=\"graphics/pictures/person/r-{n.Id}/portrait\"/>");
                sb.AppendLine($"    <record from=\"{portreDosyasi}\" to=\"graphics/pictures/person/{n.Id}/portrait\"/>");
                sayac++;
            }

            sb.AppendLine("  </list>");
            sb.AppendLine("</record>");

            string xmlIcerik = sb.ToString();

            if (!string.IsNullOrWhiteSpace(hedefKlasor))
            {
                try
                {
                    Directory.CreateDirectory(hedefKlasor);
                    string dosyaYolu = Path.Combine(hedefKlasor, "config.xml");
                    File.WriteAllText(dosyaYolu, xmlIcerik, Encoding.UTF8);
                    SonIslemOzeti = $"🖼️ AI Regen Yüz Eşleştirici: {newgenler.Count} newgen oyuncu için '{dosyaYolu}' üretildi.";
                    kopru.SonBilgiMesaji = SonIslemOzeti;
                    return xmlIcerik;
                }
                catch (Exception ex)
                {
                    SonIslemOzeti = $"XML yazma uyarısı ({ex.Message}) — Önizleme bellekte üretildi.";
                }
            }
            else
            {
                SonIslemOzeti = $"🖼️ AI Regen Yüz Eşleştirici: {newgenler.Count} newgen için config.xml eşleştirmesi hazırlandı.";
                kopru.SonBilgiMesaji = SonIslemOzeti;
            }

            return xmlIcerik;
        }
    }
}

