using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FM26AgenticEditor.Models;

namespace FM26AgenticEditor.Services
{
    /// <summary>
    /// "Agentic FM" katmanı:
    /// 1. Oyuncu verilerinden taktiksel rol, gelişim planı ve scout raporu üretir.
    /// 2. Kulüp finans ve tesis verilerinden transfer stratejisi çıkarır.
    /// 3. Basın toplantısı / soyunma odası konuşmaları için bağlama duyarlı diyalog üretir.
    /// Yerel LLM (Ollama: http://localhost:11434/v1/chat/completions) veya OpenAI/Claude uyumlu endpoint destekler;
    /// LLM bağlı değilse yerleşik kural tabanlı motorla anında yanıt verir.
    /// </summary>
    public class AgenticFmServisi
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

        public string ApiEndpoint { get; set; } = "http://localhost:11434/v1/chat/completions";
        public string ModelAdi { get; set; } = "qwen2.5:7b";
        public bool UzakLlmKullan { get; set; } = false;
        public bool CalisiyorMu { get; private set; }
        public string SonRapor { get; private set; } = "Henüz bir AI analizi çalıştırılmadı. Yukarıdan bir aksiyon seçin.";

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
CA/PA: {o.MevcutYetenekCA} / {o.PotansiyelYetenekPA} | Kondisyon: %{o.Kondisyon} | Moral: {o.Moral}/20
Özellikler (1-20): Bitiricilik:{o.Bitiricilik}, Pas:{o.Pas}, Teknik:{o.Teknik}, Karar Alma:{o.KararAlma}, Vizyon:{o.Vizyon}, Hız:{o.Hiz}, Dayanıklılık:{o.Dayaniklilik}
Haftalık Maaş: {o.HaftalikMaasEuro:N0}€ | Piyasa Değeri: {o.PiyasaDegeriEuro:N0}€

Kısa ve maddeler halinde:
1. En uygun FM26 oyuncu rolü ve görevini,
2. Bireysel antrenman odağını,
3. CA/PA farkına göre gelişim tavanını Türkçe olarak analiz et.";

            ArkaPlandaLlmSorgula(istem, () => YerelOyuncuRaporuUret(o));
        }

        public void KulupStratejiAnaliziYap(KulupVerisi k)
        {
            if (k == null)
            {
                SonRapor = "Lütfen önce Kulüp & Finans sekmesinden bir kulüp seçin.";
                return;
            }

            if (!UzakLlmKullan)
            {
                SonRapor = YerelKulupRaporuUret(k);
                return;
            }

            var istem = $@"Sen Football Manager 26 Futbol Direktörüsün (Director of Football).
Kulüp: {k.KulupAdi} ({k.LigAdi}) | İtibar: {k.Itibar}/10000
Bakiye: {k.BakiyeEuro:N0}€ | Transfer Bütçesi: {k.TransferButcesiEuro:N0}€ | Haftalık Maaş Bütçesi: {k.MaasButcesiHaftalikEuro:N0}€
Tesisler (1-20): Antrenman:{k.AntrenmanTesisleri}, Altyapı:{k.AltyapiTesisleri}, Gençlik Akademisi:{k.GenclikAkademisi}

3 maddede kısa Türkçe yönetim ve transfer stratejisi öner.";

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
                    $"1. [Sakin / Profesyonel (Moral +1)]: \"{k?.KulupAdi ?? "Kulübümüz"} olarak planımıza sadığız. {o?.AdSoyad ?? "Oyuncumuz"} antrenmanlarda harika bir özveri gösteriyor.\"\n" +
                    $"2. [Tutkulu / Motive Edici (Moral +2, Baskı +1)]: \"Bu formanın ağırlığını bilen herkes sahada %100'ünü verecek. Taraftarımız merak etmesin!\"\n" +
                    $"3. [Korumacı / Baskıyı Azaltan]: \"Medyanın beklentilerini anlıyorum ancak henüz çok genç ve gelişim sürecinde (CA:{o?.MevcutYetenekCA ?? 140} -> PA:{o?.PotansiyelYetenekPA ?? 170}).\"";
                return;
            }

            var istem = $"FM26 teknik direktörü olarak şu basın toplantısı sorusuna oyuncu moralini yükseltecek 3 farklı üslupta (Sakin, Tutkulu, Korumacı) kısa Türkçe yanıt üret: {konu}";
            ArkaPlandaLlmSorgula(istem, () => SonRapor);
        }

        private static string YerelOyuncuRaporuUret(OyuncuVerisi o)
        {
            int gelisimPayi = Math.Max(0, o.PotansiyelYetenekPA - o.MevcutYetenekCA);
            string onerilenRol = (o.Vizyon >= 16 && o.Pas >= 16)
                ? "Gelişmiş Oyun Kurucu (Hücum) / Serbest 10 Numara (Trequartista)"
                : (o.Hiz >= 16 && o.Teknik >= 15)
                    ? "İçe Kat Eden Kanat (Hücum) / Kanat Forvet"
                    : (o.Bitiricilik >= 15)
                        ? "Yaratıcı Forvet / Komple Forvet (Destek)"
                        : "İki Yönlü Orta Saha / Mezalla (Destek)";

            string antrenmanOdagi = o.Dayaniklilik < 15
                ? "Dayanıklılık & Çeviklik (Maç temposunu 90 dakikaya yaymak için)"
                : (o.Bitiricilik < 15 ? "Son Vuruş & Soğukkanlılık" : "Hücum Hareketliliği & Pas");

            string durumUyarisi = o.SakatMi
                ? "⚠️ Oyuncu şu an SAKAT. Antrenman yükünü düşürün veya FMRTE sekmesinden iyileştirin."
                : (o.Kondisyon < 85 ? "⚠️ Kondisyon kritik seviyede (%85 altı), rotasyon önerilir." : "✅ Fiziksel olarak ilk 11'e tamamen hazır.");

            return
                $"=== AGENTIC FM SCOUT & TAKTİK RAPORU: {o.AdSoyad.ToUpperInvariant()} ===\n" +
                $"• Mevcut / Potansiyel (CA/PA): {o.MevcutYetenekCA} / {o.PotansiyelYetenekPA} (Gelişim Açığı: +{gelisimPayi} puan)\n" +
                $"• Önerilen FM26 Rolü: {onerilenRol}\n" +
                $"• Bireysel Antrenman Odağı: {antrenmanOdagi}\n" +
                $"• Fiziksel & Moral Durumu: {durumUyarisi} (Moral: {o.Moral}/20)\n" +
                $"• Sözleşme & Değer Analizi: {o.SozlesmeBitisYili} yılına kadar haftalık {o.HaftalikMaasEuro:N0}€ kazanıyor. " +
                (gelisimPayi >= 20
                    ? "Wonderkid profilinde; serbest kalma maddesi olmadan sözleşmesini hemen uzatın!"
                    : "Kadro çekirdeği için ideal maliyet/performans dengesinde.");
        }

        private static string YerelKulupRaporuUret(KulupVerisi k)
        {
            string tesisNotu = k.AltyapiTesisleri < 18
                ? $"Altyapı ({k.AltyapiTesisleri}/20) ve Antrenman ({k.AntrenmanTesisleri}/20) tesislerini geliştirmek wonderkid üretimini doğrudan artırır."
                : "Tesisler dünya standartlarında; bütçeyi doğrudan A takım yıldızlarına yönlendirebilirsiniz.";

            return
                $"=== AGENTIC FM FUTBOL DİREKTÖRÜ RAPORU: {k.KulupAdi.ToUpperInvariant()} ===\n" +
                $"• Finansal Sağlık: Bakiye {k.BakiyeEuro:N0}€ | Transfer Bütçesi: {k.TransferButcesiEuro:N0}€\n" +
                $"• Maaş Tavanı Önerisi: Haftalık bütçenin ({k.MaasButcesiHaftalikEuro:N0}€) %12'sini (~{k.MaasButcesiHaftalikEuro * 12 / 100:N0}€) tek bir oyuncuya aşmayın.\n" +
                $"• Altyapı & Tesis Stratejisi: {tesisNotu}";
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

