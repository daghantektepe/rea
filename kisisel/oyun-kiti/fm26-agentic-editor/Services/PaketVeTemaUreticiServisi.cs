using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FM26AgenticEditor.Services
{
    /// <summary>
    /// Araştırma raporundaki dosya tabanlı 3 stratejik paketi doğrudan üreten servis:
    /// 1. Masaüstü Ergonomisi Odaklı Antrasit/Lacivert Skin & Oyuncu Röntgen Paneli (skins/FM26_Masaustu_Antrasit)
    /// 2. Türkiye Süper Lig Gerçekçilik, TFF Yabancı Kuralı, Sakatlık/Yağmur & Kadın Futbolu Hafifletilmiş (Lite DB) Editor Data XML
    /// 3. Sıfır Gecikmeli Kadro & Transfer Karşılaştırma Tablosu (CSV / Markdown)
    /// </summary>
    public class PaketVeTemaUreticiServisi
    {
        public string SonUretimRaporu { get; private set; } = "Henüz harici mod paketi üretilmedi.";
        public string SonUretilenIcerikOnizleme { get; private set; } = "";

        public static string VarsayilanCiktiKlasoru =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Sports Interactive", "Football Manager 2026", "FM26_Rehabilitasyon_Paketleri");

        /// <summary>
        /// 1. Masaüstü Ergonomisi Odaklı Arayüz Skini (FM24 Antrasit/Lacivert Renk Paleti + 1080p/1440p Font + Boy/Kilo/CA-PA Paneli)
        /// </summary>
        public string MasaustuAntrasitSkinPaketiUret(string ciktiKlasoru = "")
        {
            string hedef = string.IsNullOrWhiteSpace(ciktiKlasoru)
                ? Path.Combine(VarsayilanCiktiKlasoru, "skins", "FM26_Masaustu_Antrasit")
                : ciktiKlasoru;

            string skinConfigXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!-- FM26 Masaüstü Ergonomisi ve Antrasit/Lacivert Renk Paleti Skini -->
<record>
  <string id=""name"" value=""FM26 Masaüstü Ergonomi (Antrasit &amp; Röntgen)""/>
  <string id=""author"" value=""FM26AgenticEditor""/>
  <translation id=""description"" value=""Konsol tipi kartlar yerine masaüstü bilgi yoğunluğunu (Boy, Kilo, Sözleşme, Sakatlık Riski) ana ekrana taşır; göz yormayan FM24 antrasit/lacivert paletini uygular.""/>
  <string id=""version"" value=""1.0""/>
  <flags id=""parent"" value=""fm""/>
  <integer id=""required_version"" value=""2600""/>
</record>";

            string settingsXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<properties>
  <!-- FM24 Standardı Yumuşatılmış Antrasit ve Koyu Lacivert Palet (Göz Yorgunluğu Önleyici) -->
  <colour name=""bg_primary"" red=""27"" green=""34"" blue=""44"" alpha=""255""/>          <!-- #1B222C Antrasit Arka Plan -->
  <colour name=""bg_card_surface"" red=""35"" green=""44"" blue=""56"" alpha=""255""/>     <!-- #232C38 Yumuşak Panel -->
  <colour name=""bg_header_navy"" red=""20"" green=""30"" blue=""48"" alpha=""255""/>      <!-- #141E30 Koyu Lacivert Başlık -->
  <colour name=""text_primary"" red=""230"" green=""236"" blue=""242"" alpha=""255""/>     <!-- #E6ECF2 Kırık Beyaz Metin -->
  <colour name=""accent_continue"" red=""46"" green=""160"" blue=""67"" alpha=""255""/>    <!-- Sağ Üst Görünür İlerleme Butonu -->

  <!-- 1080p ve 1440p Masaüstü Monitörler İçin Net Font Ölçeklemesi -->
  <real name=""font_scale_1080p"" value=""1.08""/>
  <real name=""font_scale_1440p"" value=""1.18""/>
  <integer name=""window_open_transition_ms"" value=""0""/> <!-- Pencere açılış/kapanış gecikmesini sıfırla -->

  <!-- Oyuncu Profilinde Alt Katmana Gizlenen Verileri Ana Ekranda Zorunlu Göster -->
  <boolean name=""show_player_height_weight_on_header"" value=""true""/>
  <boolean name=""show_release_clause_on_overview"" value=""true""/>
  <boolean name=""show_injury_proneness_badge"" value=""true""/>
  <boolean name=""set_piece_show_player_names_and_jumping"" value=""true""/>
</properties>";

            try
            {
                Directory.CreateDirectory(Path.Combine(hedef, "settings"));
                File.WriteAllText(Path.Combine(hedef, "skin_config.xml"), skinConfigXml, Encoding.UTF8);
                File.WriteAllText(Path.Combine(hedef, "settings", "fm26_masaustu_settings.xml"), settingsXml, Encoding.UTF8);
                SonUretimRaporu = $"🎨 Masaüstü Antrasit Skin paketi üretildi: {hedef}";
            }
            catch (Exception ex)
            {
                SonUretimRaporu = $"Skin önizlemesi bellekte üretildi ({ex.Message})";
            }

            SonUretilenIcerikOnizleme = skinConfigXml + "\n\n" + settingsXml;
            return SonUretilenIcerikOnizleme;
        }

        /// <summary>
        /// 2. Türkiye Süper Lig Gerçekçilik, TFF Yabancı Kuralı, Sakatlık/Yağmur Dengeleme & Kadın Futbolu Performans (Lite DB) XML Paketi
        /// </summary>
        public string SuperLigVePerformansEditorDataUret(FmVeriKoprusu kopru, string ciktiKlasoru = "")
        {
            string hedef = string.IsNullOrWhiteSpace(ciktiKlasoru)
                ? Path.Combine(VarsayilanCiktiKlasoru, "editor data")
                : ciktiKlasoru;

            var sb = new StringBuilder();
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<!-- FM26 Türkiye Süper Lig Gerçekçilik, Tıbbi/Meteorolojik Denge ve Performans (Lite DB) Paketi -->");
            sb.AppendLine(@"<record>");
            sb.AppendLine(@"  <string id=""name"" value=""FM26 Türkiye Gerçekçilik + Tıbbi Denge + Lite DB Paketi""/>");
            sb.AppendLine(@"  <list id=""db_changes"">");
            sb.AppendLine(@"    <!-- 1. Süper Lig Uluslararası İtibar Kalibrasyonu (Avrupa İlk 8 Seviyesi) -->");
            sb.AppendLine(@"    <record db_type=""competition"" name=""Trendyol Süper Lig"" property=""reputation"" new_value=""7850""/>");
            sb.AppendLine(@"    <!-- 2. TFF Güncel Yabancı Oyuncu Kuralı (A Takım 14 / İlk 11: 11 + Mavi Kart/Türki Cumhuriyet muafiyeti) -->");
            sb.AppendLine(@"    <record db_type=""fixture_rules"" nation=""Turkey"" max_foreign_players_in_squad=""14"" max_foreign_players_in_eleven=""11"" blue_card_exempt=""true""/>");

            foreach (var k in kopru.Kulupler)
            {
                sb.AppendLine($@"    <record db_type=""club"" id=""{k.Id}"" name=""{k.KulupAdi}"" reputation=""{Math.Max(k.Itibar, 7400)}"" board_flexibility=""18"" sponsorship_euro=""{k.SponsorlukGeliriYillikEuro}""/>");
            }

            sb.AppendLine(@"    <!-- 3. Küresel Sakatlık Frekansı (-%32) ve Kaleci Temassız Sakatlık İndirimi -->");
            sb.AppendLine(@"    <record db_type=""simatch_physics"" global_injury_multiplier=""0.68"" goalkeeper_non_contact_injury_multiplier=""0.15"" micro_injury_sharpness_retention=""0.90""/>");
            sb.AppendLine(@"    <!-- 4. Meteorolojik Yağmur Anomalisi Düzeltmesi -->");
            sb.AppendLine(@"    <record db_type=""weather_matrix"" continuous_rain_cap=""0.18"" waterlogged_pitch_threshold=""0.85""/>");
            sb.AppendLine(@"    <!-- 5. Performans Odaklı İnce Veritabanı (Oynanmayan Kadın Alt Ligleri ve Pasif Kıtalar Arka Plan Simülasyon Yükü Kapatma) -->");
            sb.AppendLine(@"    <record db_type=""simulation_performance"" disable_inactive_womens_lower_leagues=""true"" background_competition_detail_level=""minimum""/>");
            sb.AppendLine(@"  </list>");
            sb.AppendLine(@"</record>");

            string xml = sb.ToString();
            try
            {
                Directory.CreateDirectory(hedef);
                string dosya = Path.Combine(hedef, "FM26_SuperLig_TibbiDenge_LiteDB.xml");
                File.WriteAllText(dosya, xml, Encoding.UTF8);
                SonUretimRaporu = $"📦 Editor Data (.xml) paketi üretildi: {dosya}";
            }
            catch (Exception ex)
            {
                SonUretimRaporu = $"Editor Data önizlemesi bellekte üretildi ({ex.Message})";
            }

            SonUretilenIcerikOnizleme = xml;
            return xml;
        }

        /// <summary>
        /// 3. Sıfır Gecikmeli Masaüstü Kadro & Transfer Kıyaslama Tablosu (CSV + Markdown)
        /// </summary>
        public string HizliKadroKiyaslamaTablosuUret(FmVeriKoprusu kopru, string ciktiKlasoru = "")
        {
            string hedef = string.IsNullOrWhiteSpace(ciktiKlasoru)
                ? VarsayilanCiktiKlasoru
                : ciktiKlasoru;

            var sb = new StringBuilder();
            sb.AppendLine("FormaNo;AdSoyad;Kulup;Pozisyon;Yas;BoyCm;KiloKg;Ayak;CA;PA;GelisimFarki;Kondisyon;MacKondisyonu;SakatlikEgilimi;Sureklilik;OnemliMac;HavaHakimiyeti;MaasHaftalikEuro;SerbestKalmaEuro;DegerEuro");

            foreach (var o in kopru.Oyuncular.OrderByDescending(x => x.PotansiyelYetenekPA))
            {
                sb.AppendLine(
                    $"{o.FormaNumarasi};{o.AdSoyad};{o.KulupAdi};{o.Pozisyon};{o.Yas};{o.BoyCm};{o.KiloKg};{o.TercihEdilenAyak};" +
                    $"{o.MevcutYetenekCA};{o.PotansiyelYetenekPA};{o.PotansiyelYetenekPA - o.MevcutYetenekCA};" +
                    $"{o.Kondisyon};{o.MacKondisyonu};{o.SakatlikEgilimi};{o.Sureklilik};{o.OnemliMacPerformansi};" +
                    $"{o.HavaHakimiyetiPuani};{o.HaftalikMaasEuro};{o.SerbestKalmaBedeliEuro};{o.PiyasaDegeriEuro}");
            }

            string csv = sb.ToString();
            try
            {
                Directory.CreateDirectory(hedef);
                string dosya = Path.Combine(hedef, "FM26_Masaustu_Kadro_Rontgen_Tablosu.csv");
                File.WriteAllText(dosya, csv, Encoding.UTF8);
                SonUretimRaporu = $"📊 Masaüstü Kadro & Transfer Röntgen Tablosu (CSV) dışa aktarıldı: {dosya}";
            }
            catch (Exception ex)
            {
                SonUretimRaporu = $"Tablo önizlemesi bellekte üretildi ({ex.Message})";
            }

            SonUretilenIcerikOnizleme = csv;
            return csv;
        }
    }
}
