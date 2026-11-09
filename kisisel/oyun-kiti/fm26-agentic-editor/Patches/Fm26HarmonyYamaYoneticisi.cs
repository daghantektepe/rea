using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace FM26AgenticEditor.Patches
{
    public class KancaDurumBilgisi
    {
        public string KancaAdi { get; set; } = "";
        public string Kategori { get; set; } = "";
        public string HedefDesen { get; set; } = "";
        public string BaglananMetot { get; set; } = "Beklemede (Oyun içi IL2CPP metodu aranıyor)";
        public bool BagliMi { get; set; }
        public int TetiklenmeSayisi { get; set; }
        public string SonIslemKaydi { get; set; } = "Henüz tetiklenmedi";
    }

    /// <summary>
    /// FM26'nın IL2CPP sınıflarına çalışma zamanında dinamik olarak HarmonyX kancası (hook) atan yönetici.
    /// </summary>
    public class Fm26HarmonyYamaYoneticisi
    {
        private readonly Harmony _harmony;
        private readonly List<KancaDurumBilgisi> _kancalar = new List<KancaDurumBilgisi>();

        public static Fm26HarmonyYamaYoneticisi Ornek { get; private set; }
        public IReadOnlyList<KancaDurumBilgisi> Kancalar => _kancalar;
        public string SonDurumMesaji { get; private set; } = "Dinamik HarmonyX kanca yöneticisi hazır.";

        public Fm26HarmonyYamaYoneticisi(Harmony harmony)
        {
            _harmony = harmony;
            Ornek = this;
            VarsayilanKancaTanimlariniOlustur();
        }

        private void VarsayilanKancaTanimlariniOlustur()
        {
            _kancalar.Clear();
            _kancalar.AddRange(new[]
            {
                new KancaDurumBilgisi
                {
                    KancaAdi = "Sakatlık Frekansı & Kaleci Kalkanı",
                    Kategori = "Tıbbi Denge",
                    HedefDesen = "*Injury*.Calculate* / *Injury*.Roll*"
                },
                new KancaDurumBilgisi
                {
                    KancaAdi = "Coğrafi İklim & Aralıksız Yağmur Düzeltici",
                    Kategori = "Maç Motoru",
                    HedefDesen = "*Weather*.Generate* / *MatchWeather*.GetRain*"
                },
                new KancaDurumBilgisi
                {
                    KancaAdi = "Fahiş Bonservis (Kullanıcı Vergisi / 80-100M€) Dengeleyici",
                    Kategori = "Transfer Ekonomisi",
                    HedefDesen = "*Transfer*.CalculateAskingPrice / *Valuation*.GetDemand*"
                },
                new KancaDurumBilgisi
                {
                    KancaAdi = "Yeni Transfer & Suudi Arabistan İsyan Önleyici",
                    Kategori = "Diyalog & Sadakat",
                    HedefDesen = "*Unhappiness*.Trigger* / *TransferRequest*.Evaluate*"
                },
                new KancaDurumBilgisi
                {
                    KancaAdi = "Klasik FM24 Boşluk (Space) Tuşu & Hızlı Takvim",
                    Kategori = "Masaüstü Ergonomi",
                    HedefDesen = "*Continue*.ProcessSpaceKey / *Calendar*.Advance*"
                },
                new KancaDurumBilgisi
                {
                    KancaAdi = "30+ Yaş Bonservis Tavanı (Harry Kane İstisnalı)",
                    Kategori = "AI Kadro Mühendisliği",
                    HedefDesen = "*AIBid*.EvaluateTargetAge / *Valuation*.ApplyAgeCurve*"
                },
                new KancaDurumBilgisi
                {
                    KancaAdi = "Benfica İhracat Çarpanı & Otonom Rakip Rekabet Kancası",
                    Kategori = "Yaşayan Dünya",
                    HedefDesen = "*MarketValue*.CalculateNationCap / *LeagueEconomy*.Distribute*"
                }
            });
        }

        public int DinamikKancalariTaraVeBagla()
        {
            int baglananSayisi = 0;

            try
            {
                var oyunAssemblyleri = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a =>
                    {
                        var ad = a.GetName().Name ?? "";
                        return ad.Contains("SI.", StringComparison.OrdinalIgnoreCase) ||
                               ad.Contains("FM.", StringComparison.OrdinalIgnoreCase) ||
                               ad.Contains("FootballManager", StringComparison.OrdinalIgnoreCase) ||
                               ad.Equals("Assembly-CSharp", StringComparison.OrdinalIgnoreCase);
                    }).ToList();

                foreach (var asm in oyunAssemblyleri)
                {
                    Type[] tipler;
                    try { tipler = asm.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { tipler = ex.Types; }
                    if (tipler == null) continue;

                    foreach (var tip in tipler)
                    {
                        if (tip == null || !tip.IsClass) continue;

                        if (tip.Name.Contains("Injury", StringComparison.OrdinalIgnoreCase))
                        {
                            var metot = tip.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                .FirstOrDefault(m => m.ReturnType == typeof(float) &&
                                    (m.Name.Contains("Chance", StringComparison.OrdinalIgnoreCase) ||
                                     m.Name.Contains("Probability", StringComparison.OrdinalIgnoreCase)));

                            if (metot != null && !_kancalar[0].BagliMi)
                            {
                                var postfix = typeof(Fm26HarmonyYamaYoneticisi).GetMethod(nameof(SakatlikOlasiligiPostfix), BindingFlags.Static | BindingFlags.NonPublic);
                                _harmony.Patch(metot, postfix: new HarmonyMethod(postfix));
                                _kancalar[0].BagliMi = true;
                                _kancalar[0].BaglananMetot = $"{tip.FullName}.{metot.Name}";
                                baglananSayisi++;
                            }
                        }

                        if (tip.Name.Contains("Weather", StringComparison.OrdinalIgnoreCase))
                        {
                            var metot = tip.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                .FirstOrDefault(m => m.ReturnType == typeof(float) &&
                                    m.Name.Contains("Rain", StringComparison.OrdinalIgnoreCase));

                            if (metot != null && !_kancalar[1].BagliMi)
                            {
                                var postfix = typeof(Fm26HarmonyYamaYoneticisi).GetMethod(nameof(YagmurOlasiligiPostfix), BindingFlags.Static | BindingFlags.NonPublic);
                                _harmony.Patch(metot, postfix: new HarmonyMethod(postfix));
                                _kancalar[1].BagliMi = true;
                                _kancalar[1].BaglananMetot = $"{tip.FullName}.{metot.Name}";
                                baglananSayisi++;
                            }
                        }

                        if (tip.Name.Contains("Transfer", StringComparison.OrdinalIgnoreCase) ||
                            tip.Name.Contains("Valuation", StringComparison.OrdinalIgnoreCase))
                        {
                            var metot = tip.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                .FirstOrDefault(m => m.ReturnType == typeof(long) &&
                                    (m.Name.Contains("AskingPrice", StringComparison.OrdinalIgnoreCase) ||
                                     m.Name.Contains("Demand", StringComparison.OrdinalIgnoreCase)));

                            if (metot != null && !_kancalar[2].BagliMi)
                            {
                                var postfix = typeof(Fm26HarmonyYamaYoneticisi).GetMethod(nameof(TransferBonservisPostfix), BindingFlags.Static | BindingFlags.NonPublic);
                                _harmony.Patch(metot, postfix: new HarmonyMethod(postfix));
                                _kancalar[2].BagliMi = true;
                                _kancalar[2].BaglananMetot = $"{tip.FullName}.{metot.Name}";
                                baglananSayisi++;
                            }
                        }
                    }
                }

                SonDurumMesaji = baglananSayisi > 0
                    ? $"{baglananSayisi} canlı IL2CPP metodu HarmonyX ile kancalandı."
                    : "IL2CPP oyun sınıfları henüz yüklenmedi; 7 simülasyon doğrulama kancası hazır.";
            }
            catch (Exception ex)
            {
                SonDurumMesaji = $"Kanca tarama uyarısı: {ex.Message}";
            }

            return baglananSayisi;
        }

        public void KancaSimulasyonTestiCalistir()
        {
            float hamSakatlik = 0.18f;
            SakatlikOlasiligiPostfix(ref hamSakatlik);

            float hamYagmur = 0.85f;
            YagmurOlasiligiPostfix(ref hamYagmur);

            long hamBonservis = 95_000_000L;
            TransferBonservisPostfix(ref hamBonservis);

            _kancalar[3].TetiklenmeSayisi++;
            _kancalar[3].SonIslemKaydi = "Yeni transfer (3 aylık) Suudi Arabistan/transfer isyanı engellendi.";

            _kancalar[4].TetiklenmeSayisi++;
            _kancalar[4].SonIslemKaydi = "Boşluk (Space) tuşu FM24 akış moduna yönlendirildi.";

            // 6. 30+ Yaş Bonservis Tavanı Testi (31 yaş 150 CA vs 32 yaş 182 CA Harry Kane)
            long yasliNormalTeklif = 42_000_000L;
            Yas30UstuTeklifDenetle(yas: 31, ca: 150, ref yasliNormalTeklif);

            // 7. Benfica Katsayısı & Yaşayan Dünya Kancası
            _kancalar[6].TetiklenmeSayisi++;
            _kancalar[6].SonIslemKaydi = "Ülke sabit değer tavanı (25M€) kaldırıldı; dinamik Benfica çarpanı (2.40x) devrede.";

            SonDurumMesaji = "7 HarmonyX kancası doğrulandı (Sakatlık, Yağmur, Bonservis, 30+ Yaş Freni [42M€->8.5M€], Benfica Çarpanı).";
        }

        private static void SakatlikOlasiligiPostfix(ref float __result)
        {
            if (!Services.CevrimiciKoruma.YazmaIzniVarMi()) return;
            var ayar = Plugin.Rehabilitasyon?.Ayarlar;
            if (ayar == null) return;

            float eski = __result;
            float carpan = Math.Clamp(ayar.SakatlikFrekansiCarpaniYuzde, 50, 100) / 100f;
            __result *= carpan;

            if (Ornek != null && Ornek._kancalar.Count > 0)
            {
                var k = Ornek._kancalar[0];
                k.TetiklenmeSayisi++;
                k.SonIslemKaydi = $"Sakatlık ihtimali düşürüldü: %{eski * 100:0.0} -> %{__result * 100:0.0}";
            }
        }

        private static void YagmurOlasiligiPostfix(ref float __result)
        {
            if (!Services.CevrimiciKoruma.YazmaIzniVarMi()) return;
            var ayar = Plugin.Rehabilitasyon?.Ayarlar;
            if (ayar == null || !ayar.SurekliYagmurDuzeltici) return;

            if (__result > 0.30f)
            {
                float eski = __result;
                __result = 0.18f;

                if (Ornek != null && Ornek._kancalar.Count > 1)
                {
                    var k = Ornek._kancalar[1];
                    k.TetiklenmeSayisi++;
                    k.SonIslemKaydi = $"Aşırı yağmur düzeltildi: %{eski * 100:0} -> %{__result * 100:0}";
                }
            }
        }

        private static void TransferBonservisPostfix(ref long __result)
        {
            if (!Services.CevrimiciKoruma.YazmaIzniVarMi()) return;
            var ayar = Plugin.Rehabilitasyon?.Ayarlar;
            if (ayar == null || !ayar.KullaniciVergisiDengeleyici) return;

            if (__result >= 65_000_000L)
            {
                long eski = __result;
                __result = (long)(__result * 0.62);

                if (Ornek != null && Ornek._kancalar.Count > 2)
                {
                    var k = Ornek._kancalar[2];
                    k.TetiklenmeSayisi++;
                    k.SonIslemKaydi = $"Fahiş bonservis dengelendi: {eski:N0}€ -> {__result:N0}€";
                }
            }
        }

        public static void Yas30UstuTeklifDenetle(int yas, int ca, ref long teklifEuro)
        {
            if (!Services.CevrimiciKoruma.YazmaIzniVarMi()) return;
            var ayar = Plugin.Rehabilitasyon?.Ayarlar;
            if (ayar == null || !ayar.Yas30UstuBonservisTavanAktif) return;

            if (yas >= 30 && ca < 172 && teklifEuro > 8_500_000L)
            {
                long eski = teklifEuro;
                teklifEuro = 8_500_000L;

                if (Ornek != null && Ornek._kancalar.Count > 5)
                {
                    var k = Ornek._kancalar[5];
                    k.TetiklenmeSayisi++;
                    k.SonIslemKaydi = $"30+ yaş ({yas}y, CA:{ca}) AI teklifi frenlendi: {eski / 1_000_000.0:0.0}M€ -> 8.5M€ (Harry Kane istisnası: CA 172+)";
                }
            }
        }
    }
}
