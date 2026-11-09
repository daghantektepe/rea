using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FM26AgenticEditor.Models;
using UnityEngine;

namespace FM26AgenticEditor.Services
{
    /// <summary>
    /// "Agentic FM 2.0" katmanı:
    /// 1. Oyuncu verilerinden taktiksel rol, gelişim planı ve "1 Maç Messi / 1 Maç Halı Saha" Tutarlılık Teşhisi üretir.
    /// 2. Kulüp finans, tesis ve Benfica İhracat Çarpanı verilerinden uzun vadeli strateji çıkarır.
    /// 3. Soyunma odası konuşmalarını "hiçbir işe yaramayan oyuncak" olmaktan çıkarıp, oyuncuların Karar Alma,
    ///    Süreklilik, Önemli Maç Performansı ve Moral değerlerine somut maç içi etki olarak uygular.
    /// </summary>
    public class AgenticFmServisi
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

        public string ApiEndpoint { get; set; } = "http://localhost:11434/v1/chat/completions";
        public string ModelAdi { get; set; } = "qwen2.5:7b";
        public bool UzakLlmKullan { get; set; } = false;
        public bool CalisiyorMu { get; private set; }
        public string SonRapor { get; private set; } = "Henüz bir AI analizi veya soyunma odası müdahalesi çalıştırılmadı.";

        /// <summary>
        /// Soyunma odası konuşmasını sadece kuru metin olmaktan çıkarıp oyuncunun gerçek maç içi zihinsel
        /// ve istikrar parametrelerine uygular (Undo destekli).
        /// </summary>
        public void SoyunmaOdasiGercekEtkiUygula(FmVeriKoprusu kopru, OyuncuVerisi o, int stratejiTipi)
        {
            if (o == null || !CevrimiciKoruma.YazmaIzniVarMi())
            {
                SonRapor = o == null ? "Lütfen önce bir oyuncu seçin." : CevrimiciKoruma.DurumMesaji;
                return;
            }

            int eKarar = o.KararAlma;
            int eVizyon = o.Vizyon;
            int eSureklilik = o.Sureklilik;
            int eOnemli = o.OnemliMacPerformansi;
            int eMoral = o.Moral;

            string baslik;
            string aciklama;

            switch (stratejiTipi)
            {
                case 0: // Taktiksel Odak & Ön Alan Presi
                    o.KararAlma = Mathf.Clamp(o.KararAlma + 2, 1, 20);
                    o.Vizyon = Mathf.Clamp(o.Vizyon + 1, 1, 20);
                    o.Moral = Mathf.Clamp(o.Moral + 1, 1, 20);
                    baslik = "🎯 Taktiksel Odak & Ön Alan Pres Talimatı";
                    aciklama = $"\"Rakibin oyun kurmasını birinci bölgede bozacağız; topu kazandığın an dikine ara pası ara!\"\n" +
                               $"Somut Maç Etkisi: Karar Alma {eKarar}->{o.KararAlma} (+2), Vizyon {eVizyon}->{o.Vizyon} (+1), Moral {eMoral}->{o.Moral}.";
                    break;

                case 1: // "1 Maç Messi 1 Maç Halı Saha" İstikrarsızlık Tedavisi (Mentorluk & Baskı Azaltma)
                    o.Sureklilik = Mathf.Clamp(o.Sureklilik + 3, 1, 20);
                    o.OnemliMacPerformansi = Mathf.Clamp(o.OnemliMacPerformansi + 2, 1, 20);
                    o.Moral = Mathf.Clamp(o.Moral + 2, 1, 20);
                    baslik = "🛡️ Özgüven & İstikrar Aşılaması (Halı Saha Dalgalanmasını Kır)";
                    aciklama = $"\"Hata yapmaktan korkma; sorumluluk bende. Sahada basit başla ve ritmini bul!\"\n" +
                               $"Somut Maç Etkisi: Süreklilik {eSureklilik}->{o.Sureklilik} (+3), Önemli Maçlar {eOnemli}->{o.OnemliMacPerformansi} (+2), Tutarlılık İndeksi: {o.TutarlilikIndeksi}/100 ({o.TutarlilikEtiketi}).";
                    break;

                default: // Sert Uyarı / Rehaveti Kır
                    o.KararAlma = Mathf.Clamp(o.KararAlma + 2, 1, 20);
                    o.OnemliMacPerformansi = Mathf.Clamp(o.OnemliMacPerformansi + 2, 1, 20);
                    o.Sureklilik = Mathf.Clamp(o.Sureklilik + 1, 1, 20);
                    baslik = "🔥 Soyunma Odası Ültimatomu (Rehaveti Kır)";
                    aciklama = $"\"Bu formayı giyiyorsan sahada yürüyemezsin! İkinci yarıda her ikili mücadelede seni göreceğim!\"\n" +
                               $"Somut Maç Etkisi: Karar Alma {eKarar}->{o.KararAlma} (+2), Önemli Maçlar {eOnemli}->{o.OnemliMacPerformansi} (+2), Süreklilik {eSureklilik}->{o.Sureklilik} (+1).";
                    break;
            }

            kopru.KayitEkle("Soyunma Odası", o.AdSoyad, baslik,
                $"Karar:{eKarar} Sürek:{eSureklilik} ÖnMaç:{eOnemli}",
                $"Karar:{o.KararAlma} Sürek:{o.Sureklilik} ÖnMaç:{o.OnemliMacPerformansi}", () =>
                {
                    o.KararAlma = eKarar;
                    o.Vizyon = eVizyon;
                    o.Sureklilik = eSureklilik;
                    o.OnemliMacPerformansi = eOnemli;
                    o.Moral = eMoral;
                });

            SonRapor = $"=== {baslik} ({o.AdSoyad}) ===\n{aciklama}";
        }

        public void OyuncuScoutAnaliziYap(OyuncuVerisi o)
        {
            if (o == null)
            {
                SonRapor = "Lütfen önce Oyuncular sekmesinden bir oyuncu seçin.";
                return;
            }

            if (!UzakLlmKullan)
            {
                SonRapor = YerelOyuncuRaporuUret(o);
                return;
            }

            var istem = $@"Sen Football Manager 26 baş gözlemcisi (Chief Scout) ve taktik analistisin.
Oyuncu: {o.AdSoyad} ({o.Yas} yaş, {o.Uyruk}, Kulüp: {o.KulupAdi}, Pozisyon: {o.Pozisyon})
CA/PA: {o.MevcutYetenekCA} / {o.PotansiyelYetenekPA} | Tutarlılık İndeksi: {o.TutarlilikIndeksi}/100 ({o.TutarlilikEtiketi})
Gizli Özellikler: Süreklilik:{o.Sureklilik}/20, Önemli Maçlar:{o.OnemliMacPerformansi}/20, Sakatlık Eğilimi:{o.SakatlikEgilimi}/20
Özellikler: Bitiricilik:{o.Bitiricilik}, Pas:{o.Pas}, Teknik:{o.Teknik}, Karar Alma:{o.KararAlma}, Vizyon:{o.Vizyon}, Hız:{o.Hiz}, Dayanıklılık:{o.Dayaniklilik}

Kısa ve maddeler halinde:
1. En uygun FM26 oyuncu rolü ve görevini,
2. '1 maç Messi 1 maç halı saha' dalgalanması olup olmadığını ve çözümünü,
3. Avrupa ihracat (Benfica standardı) bonservis potansiyelini Türkçe analiz et.";

            ArkaPlandaLlmSorgula(istem, () => YerelOyuncuRaporuUret(o));
        }

        public void KulupStratejiAnaliziYap(KulupVerisi k)
        {
            if (k == null)
            {
                SonRapor = "Lütfen önce Kulüp & Süper Lig sekmesinden bir kulüp seçin.";
                return;
            }

            if (!UzakLlmKullan)
            {
                SonRapor = YerelKulupRaporuUret(k);
                return;
            }

            var istem = $@"Sen Football Manager 26 Futbol Direktörüsün.
Kulüp: {k.KulupAdi} ({k.LigAdi}) | İtibar: {k.Itibar}/10000 | Benfica İhracat Çarpanı: {k.IhracatDegerCarpani:0.00}x | Kadro Yaş Ort: {k.KadroYasOrtalamasi:0.0}
Bakiye: {k.BakiyeEuro:N0}€ | Transfer Bütçesi: {k.TransferButcesiEuro:N0}€
3 maddede kısa Türkçe kadro mühendisliği ve ihracat stratejisi öner.";

            ArkaPlandaLlmSorgula(istem, () => YerelKulupRaporuUret(k));
        }

        public void BasinToplantisiYanitiUret(OyuncuVerisi o, KulupVerisi k, string soruBaglami)
        {
            string konu = string.IsNullOrWhiteSpace(soruBaglami)
                ? $"{o?.AdSoyad ?? "Takım"} performansı ve transfer söylentileri"
                : soruBaglami;

            if (!UzakLlmKullan)
            {
                SonRapor =
                    $"[Agentic FM - Basın Toplantısı & Diyalog Önerisi]\n" +
                    $"Gündem: \"{konu}\"\n\n" +
                    $"1. [Sakin / Profesyonel (Takım Konsantrasyonu +1)]: \"{k?.KulupAdi ?? "Kulübümüz"} artık Avrupa'nın elit bir ihracat ve yarışma kulübüdür ({k?.IhracatDegerCarpani ?? 1.5f:0.00}x). {o?.AdSoyad ?? "Oyuncumuz"} sistemimizin merkezinde.\"\n" +
                    $"2. [Tutkulu / Meydan Okuyan (Moral +2)]: \"10 yıl önceki statükoyu yıktık; artık oyuncularımızı ucuza kapatan değil, Avrupa piyasasını belirleyen kulübüz!\"\n" +
                    $"3. [Korumacı / İstikrar Odaklı (Baskıyı Azaltır)]: \"{o?.AdSoyad ?? "Genç oyuncumuz"} henüz gelişim evresinde (Tutarlılık: {o?.TutarlilikIndeksi ?? 75}/100). Üzerindeki medya baskısını ben üstleniyorum.\"";
                return;
            }

            var istem = $"FM26 teknik direktörü olarak şu basın toplantısı sorusuna 3 farklı üslupta kısa Türkçe yanıt üret: {konu}";
            ArkaPlandaLlmSorgula(istem, () => SonRapor);
        }

        private static string YerelOyuncuRaporuUret(OyuncuVerisi o)
        {
            int gelisimPayi = Math.Max(0, o.PotansiyelYetenekPA - o.MevcutYetenekCA);
            string onerilenRol = (o.Vizyon >= 16 && o.Pas >= 16)
                ? "Gelişmiş Oyun Kurucu (Hücum) / Serbest 10 Numara"
                : (o.Hiz >= 16 && o.Teknik >= 15)
                    ? "İçe Kat Eden Kanat (Hücum) / Kanat Forvet"
                    : (o.Bitiricilik >= 15)
                        ? "Yaratıcı Forvet / Komple Forvet (Destek)"
                        : "İki Yönlü Orta Saha / Mezalla (Destek)";

            string tutarlilikTeshisi = o.TutarlilikIndeksi < 65
                ? $"⚠️ DİKKAT ('1 Maç Messi / 1 Maç Halı Saha' Teşhisi): Oyuncunun Süreklilik ({o.Sureklilik}/20) ve Önemli Maç ({o.OnemliMacPerformansi}/20) değerleri düşük! Çözüm: Yukarıdaki '🛡️ Özgüven & İstikrar Aşılaması' soyunma odası konuşmasını uygulayın."
                : $"✅ İstikrar Röntgeni ({o.TutarlilikIndeksi}/100 - {o.TutarlilikEtiketi}): Süreklilik ({o.Sureklilik}/20) ve Önemli Maç ({o.OnemliMacPerformansi}/20) değerleri üst düzey; her hafta standart performans verir.";

            string yasUyarisi = o.Yas >= 30
                ? (o.HarryKaneIstisnasiMi
                    ? "🌟 30+ Yaş Elit İstisna (Harry Kane Sınıfı): Yaşına rağmen CA (172+) elit olduğu için değerini koruyor."
                    : "📉 30+ Yaş Amortisman Uyarısı: Avrupa kulüpleri bu yaşta yüksek bonservis ödemez; alternatif genç (18-22 yaş) ikamesini hazırlayın.")
                : $"🦅 Benfica İhracat Potansiyeli: Avrupa'da başarılı bir kulüp çarpanıyla (2.4x) bu oyuncunun gerçek piyasa tavanı ~{Math.Clamp((o.PotansiyelYetenekPA - 110) * 1_250_000L, 25_000_000L, 115_000_000L):N0}€ seviyesidir.";

            return
                $"=== AGENTIC FM SCOUT & TUTARLILIK RÖNTGENİ: {o.AdSoyad.ToUpperInvariant()} ===\n" +
                $"• Mevcut / Potansiyel (CA/PA): {o.MevcutYetenekCA} / {o.PotansiyelYetenekPA} (Gelişim Açığı: +{gelisimPayi} puan)\n" +
                $"• Önerilen FM26 Rolü: {onerilenRol}\n" +
                $"• {tutarlilikTeshisi}\n" +
                $"• {yasUyarisi}";
        }

        private static string YerelKulupRaporuUret(KulupVerisi k)
        {
            string yasAnalizi = k.KadroYasOrtalamasi >= 28.5f
                ? $"⚠️ Kadro Yaşlanması ({k.KadroYasOrtalamasi:0.0} yaş): Takım 3 sezon içinde çöküş yaşayabilir! 'Yaşayan Dünya' sekmesinden AI Jenerasyon Yenilemeyi çalıştırın."
                : $"✅ Kadro Yaş Eğrisi ({k.KadroYasOrtalamasi:0.0} yaş): Dinamik ve uzun vadeli rekabete uygun.";

            return
                $"=== AGENTIC FM FUTBOL DİREKTÖRÜ RAPORU: {k.KulupAdi.ToUpperInvariant()} ===\n" +
                $"• Kulüp Mirası & Durum: Seviye {k.KulupMirasSeviyesi}/10 ({k.DunyaDurumEtiketi})\n" +
                $"• Benfica İhracat Çarpanı: {k.IhracatDegerCarpani:0.00}x | Avrupa Başarı Puanı: {k.AvrupaBasariPuani}/200\n" +
                $"• {yasAnalizi}\n" +
                $"• Finansal Güç: Bakiye {k.BakiyeEuro:N0}€ | Transfer Bütçesi: {k.TransferButcesiEuro:N0}€ | Sponsorluk: {k.SponsorlukGeliriYillikEuro:N0}€/yıl";
        }

        private void ArkaPlandaLlmSorgula(string istem, Func<string> yedekYanitUretici)
        {
            if (CalisiyorMu) return;
            CalisiyorMu = true;
            SonRapor = $"LLM ajanına bağlanılıyor ({ApiEndpoint} - {ModelAdi})...";

            Task.Run(async () =>
            {
                try
                {
                    var govde = JsonSerializer.Serialize(new
                    {
                        model = ModelAdi,
                        messages = new[]
                        {
                            new { role = "system", content = "Sen Football Manager 26 içine gömülü Türkçe konuşan uzman bir taktik ve veri ajanısın." },
                            new { role = "user", content = istem }
                        },
                        temperature = 0.4
                    });

                    using var icerik = new StringContent(govde, Encoding.UTF8, "application/json");
                    using var yanit = await _http.PostAsync(ApiEndpoint, icerik);
                    yanit.EnsureSuccessStatusCode();

                    var json = await yanit.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var metin = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    SonRapor = string.IsNullOrWhiteSpace(metin) ? yedekYanitUretici() : metin.Trim();
                }
                catch (Exception ex)
                {
                    SonRapor = $"[Uzak LLM erişilemedi ({ex.Message}) — Yerel Analiz Motoru Devrede]\n\n" + yedekYanitUretici();
                }
                finally
                {
                    CalisiyorMu = false;
                }
            });
        }
    }
}
