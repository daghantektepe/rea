using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FM26AgenticEditor.Models;
using UnityEngine;

namespace FM26AgenticEditor.Services
{
    /// <summary>
    /// FM26 IL2CPP nesneleri ile FMRTE tarzı oyun içi editör arayüzü arasındaki veri ve güvenlik köprüsü.
    /// </summary>
    public class FmVeriKoprusu
    {
        private readonly List<OyuncuVerisi> _oyuncular = new List<OyuncuVerisi>();
        private readonly List<KulupVerisi> _kulupler = new List<KulupVerisi>();
        private readonly Stack<DegisiklikKaydi> _geriAlYigini = new Stack<DegisiklikKaydi>();
        private readonly List<Il2CppSinifBilgisi> _bulunanSiniflar = new List<Il2CppSinifBilgisi>();

        public bool CanliIl2CppBagliMi { get; private set; }
        public string SonBilgiMesaji { get; set; } = "Hazır. Değişiklik yapmadan önce kaydınızın yedeğini alın.";

        public IReadOnlyList<OyuncuVerisi> Oyuncular => _oyuncular;
        public IReadOnlyList<KulupVerisi> Kulupler => _kulupler;
        public IReadOnlyCollection<DegisiklikKaydi> GeriAlGecmisi => _geriAlYigini;
        public IReadOnlyList<Il2CppSinifBilgisi> BulunanSiniflar => _bulunanSiniflar;

        public FmVeriKoprusu()
        {
            OrnekVerileriYukle();
        }

        public void VerileriYenile()
        {
            int bulunanCanli = CanliIl2CppNesneleriniBagla();
            if (bulunanCanli > 0)
            {
                CanliIl2CppBagliMi = true;
                SonBilgiMesaji = $"FM26 IL2CPP belleğinden {bulunanCanli} kayıt bağlandı.";
            }
            else
            {
                CanliIl2CppBagliMi = false;
                SonBilgiMesaji = "Simülasyon / Önizleme modu aktif (FM26 kariyer verisi bekleniyor).";
            }
        }

        public List<OyuncuVerisi> OyuncuAra(string sorgu)
        {
            if (string.IsNullOrWhiteSpace(sorgu))
                return _oyuncular.ToList();

            var s = sorgu.Trim();
            return _oyuncular.Where(o =>
                o.AdSoyad.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                o.KulupAdi.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                o.Pozisyon.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                o.Uyruk.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                o.FormaNumarasi.ToString() == s).ToList();
        }

        public List<KulupVerisi> KulupAra(string sorgu)
        {
            if (string.IsNullOrWhiteSpace(sorgu))
                return _kulupler.ToList();

            var s = sorgu.Trim();
            return _kulupler.Where(k =>
                k.KulupAdi.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                k.LigAdi.Contains(s, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // ==================== OYUNCU DÜZENLEME (GÜVENLİ & UNDO DESTEKLİ) ====================

        public void OyuncuCaPaGuncelle(OyuncuVerisi oyuncu, int yeniCa, int yeniPa)
        {
            if (!YazmaKontrol(oyuncu)) return;

            yeniCa = Mathf.Clamp(yeniCa, 1, 200);
            yeniPa = Mathf.Clamp(yeniPa, 1, 200);

            int eskiCa = oyuncu.MevcutYetenekCA;
            int eskiPa = oyuncu.PotansiyelYetenekPA;
            if (eskiCa == yeniCa && eskiPa == yeniPa) return;

            oyuncu.MevcutYetenekCA = yeniCa;
            oyuncu.PotansiyelYetenekPA = yeniPa;
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "CurrentAbility", "CA", "m_CurrentAbility" }, yeniCa);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "PotentialAbility", "PA", "m_PotentialAbility" }, yeniPa);

            KayitEkle("Oyuncu", oyuncu.AdSoyad, "CA/PA", $"{eskiCa}/{eskiPa}", $"{yeniCa}/{yeniPa}", () =>
            {
                oyuncu.MevcutYetenekCA = eskiCa;
                oyuncu.PotansiyelYetenekPA = eskiPa;
                Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "CurrentAbility", "CA", "m_CurrentAbility" }, eskiCa);
                Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "PotentialAbility", "PA", "m_PotentialAbility" }, eskiPa);
            });
        }

        public void OyuncuKondisyonMoralFulleyipIyilestir(OyuncuVerisi oyuncu)
        {
            if (!YazmaKontrol(oyuncu)) return;

            int eskiKondisyon = oyuncu.Kondisyon;
            int eskiMacKondisyonu = oyuncu.MacKondisyonu;
            int eskiMoral = oyuncu.Moral;
            bool eskiSakat = oyuncu.SakatMi;
            int eskiGun = oyuncu.SakatlikGunSayisi;
            bool eskiMutsuz = oyuncu.MutsuzlukVarMi;

            oyuncu.Kondisyon = 100;
            oyuncu.MacKondisyonu = 100;
            oyuncu.Moral = 20;
            oyuncu.SakatMi = false;
            oyuncu.SakatlikGunSayisi = 0;
            oyuncu.MutsuzlukVarMi = false;

            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "Condition", "Fitness" }, 100);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "MatchSharpness", "MatchFitness" }, 100);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "Morale", "PlayerMorale" }, 20);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "IsInjured", "Injured" }, false);

            KayitEkle("Oyuncu", oyuncu.AdSoyad, "Kondisyon/MaçRitmi/Moral/Sakatlık",
                $"K:%{eskiKondisyon} MK:%{eskiMacKondisyonu} M:{eskiMoral} Sakat:{eskiSakat}",
                "K:%100 MK:%100 M:20 Sağlıklı", () =>
                {
                    oyuncu.Kondisyon = eskiKondisyon;
                    oyuncu.MacKondisyonu = eskiMacKondisyonu;
                    oyuncu.Moral = eskiMoral;
                    oyuncu.SakatMi = eskiSakat;
                    oyuncu.SakatlikGunSayisi = eskiGun;
                    oyuncu.MutsuzlukVarMi = eskiMutsuz;
                });
        }

        public void OyuncuOzellikleriniGuncelle(
            OyuncuVerisi oyuncu,
            int bitiricilik, int pas, int teknik,
            int kararAlma, int vizyon, int hiz, int dayaniklilik,
            int sakatlikEgilimi, int sureklilik, int onemliMac)
        {
            if (!YazmaKontrol(oyuncu)) return;

            int eBit = oyuncu.Bitiricilik, ePas = oyuncu.Pas, eTek = oyuncu.Teknik;
            int eKar = oyuncu.KararAlma, eViz = oyuncu.Vizyon, eHiz = oyuncu.Hiz, eDay = oyuncu.Dayaniklilik;
            int eSak = oyuncu.SakatlikEgilimi, eSur = oyuncu.Sureklilik, eOne = oyuncu.OnemliMacPerformansi;

            oyuncu.Bitiricilik = Mathf.Clamp(bitiricilik, 1, 20);
            oyuncu.Pas = Mathf.Clamp(pas, 1, 20);
            oyuncu.Teknik = Mathf.Clamp(teknik, 1, 20);
            oyuncu.KararAlma = Mathf.Clamp(kararAlma, 1, 20);
            oyuncu.Vizyon = Mathf.Clamp(vizyon, 1, 20);
            oyuncu.Hiz = Mathf.Clamp(hiz, 1, 20);
            oyuncu.Dayaniklilik = Mathf.Clamp(dayaniklilik, 1, 20);
            oyuncu.SakatlikEgilimi = Mathf.Clamp(sakatlikEgilimi, 1, 20);
            oyuncu.Sureklilik = Mathf.Clamp(sureklilik, 1, 20);
            oyuncu.OnemliMacPerformansi = Mathf.Clamp(onemliMac, 1, 20);

            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "Finishing" }, oyuncu.Bitiricilik);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "Passing" }, oyuncu.Pas);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "Technique" }, oyuncu.Teknik);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "Decisions" }, oyuncu.KararAlma);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "Vision" }, oyuncu.Vizyon);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "Pace" }, oyuncu.Hiz);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "Stamina" }, oyuncu.Dayaniklilik);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "InjuryProneness" }, oyuncu.SakatlikEgilimi);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "Consistency" }, oyuncu.Sureklilik);
            Il2CppOzellikYaz(oyuncu.KaynakNesne, new[] { "ImportantMatches" }, oyuncu.OnemliMacPerformansi);

            KayitEkle("Oyuncu", oyuncu.AdSoyad, "Teknik & Gizli Özellikler (1-20)",
                $"Bit:{eBit} Pas:{ePas} SakatEğilim:{eSak} Sürek:{eSur}",
                $"Bit:{oyuncu.Bitiricilik} Pas:{oyuncu.Pas} SakatEğilim:{oyuncu.SakatlikEgilimi} Sürek:{oyuncu.Sureklilik}", () =>
                {
                    oyuncu.Bitiricilik = eBit; oyuncu.Pas = ePas; oyuncu.Teknik = eTek;
                    oyuncu.KararAlma = eKar; oyuncu.Vizyon = eViz; oyuncu.Hiz = eHiz; oyuncu.Dayaniklilik = eDay;
                    oyuncu.SakatlikEgilimi = eSak; oyuncu.Sureklilik = eSur; oyuncu.OnemliMacPerformansi = eOne;
                });
        }

        // ==================== KULÜP & FİNANS DÜZENLEME ====================

        public void KulupFinansGuncelle(KulupVerisi kulup, long yeniBakiye, long yeniTransferButcesi, long yeniMaasButcesi)
        {
            if (kulup == null || !CevrimiciKoruma.YazmaIzniVarMi())
            {
                SonBilgiMesaji = CevrimiciKoruma.DurumMesaji;
                return;
            }

            yeniBakiye = Math.Clamp(yeniBakiye, -500_000_000L, 2_000_000_000L);
            yeniTransferButcesi = Math.Clamp(yeniTransferButcesi, 0L, 1_500_000_000L);
            yeniMaasButcesi = Math.Clamp(yeniMaasButcesi, 0L, 50_000_000L);

            long eBakiye = kulup.BakiyeEuro;
            long eTransfer = kulup.TransferButcesiEuro;
            long eMaas = kulup.MaasButcesiHaftalikEuro;

            kulup.BakiyeEuro = yeniBakiye;
            kulup.TransferButcesiEuro = yeniTransferButcesi;
            kulup.MaasButcesiHaftalikEuro = yeniMaasButcesi;

            Il2CppOzellikYaz(kulup.KaynakNesne, new[] { "Balance", "CashBalance", "ClubBalance" }, yeniBakiye);
            Il2CppOzellikYaz(kulup.KaynakNesne, new[] { "TransferBudget", "RemainingTransferBudget" }, yeniTransferButcesi);
            Il2CppOzellikYaz(kulup.KaynakNesne, new[] { "WageBudget", "WeeklyWageBudget" }, yeniMaasButcesi);

            KayitEkle("Kulüp", kulup.KulupAdi, "Finans & Bütçe",
                $"Bakiye:{eBakiye:N0}€ / Transfer:{eTransfer:N0}€",
                $"Bakiye:{yeniBakiye:N0}€ / Transfer:{yeniTransferButcesi:N0}€", () =>
                {
                    kulup.BakiyeEuro = eBakiye;
                    kulup.TransferButcesiEuro = eTransfer;
                    kulup.MaasButcesiHaftalikEuro = eMaas;
                });
        }

        public void KulupTesisVeItibarGuncelle(KulupVerisi kulup, int itibar, int ligItibari, int antrenman, int altyapi, int akademi, int yonetimEsnekligi)
        {
            if (kulup == null || !CevrimiciKoruma.YazmaIzniVarMi())
            {
                SonBilgiMesaji = CevrimiciKoruma.DurumMesaji;
                return;
            }

            int eItibar = kulup.Itibar;
            int eLigItibar = kulup.LigItibari;
            int eAnt = kulup.AntrenmanTesisleri;
            int eAlt = kulup.AltyapiTesisleri;
            int eAka = kulup.GenclikAkademisi;
            int eYon = kulup.YonetimTesisVeButceEsnekligi;

            kulup.Itibar = Mathf.Clamp(itibar, 1, 10000);
            kulup.LigItibari = Mathf.Clamp(ligItibari, 1, 10000);
            kulup.AntrenmanTesisleri = Mathf.Clamp(antrenman, 1, 20);
            kulup.AltyapiTesisleri = Mathf.Clamp(altyapi, 1, 20);
            kulup.GenclikAkademisi = Mathf.Clamp(akademi, 1, 20);
            kulup.YonetimTesisVeButceEsnekligi = Mathf.Clamp(yonetimEsnekligi, 1, 20);

            Il2CppOzellikYaz(kulup.KaynakNesne, new[] { "Reputation" }, kulup.Itibar);
            Il2CppOzellikYaz(kulup.KaynakNesne, new[] { "TrainingFacilities" }, kulup.AntrenmanTesisleri);
            Il2CppOzellikYaz(kulup.KaynakNesne, new[] { "YouthFacilities" }, kulup.AltyapiTesisleri);
            Il2CppOzellikYaz(kulup.KaynakNesne, new[] { "YouthCoaching", "YouthAcademy" }, kulup.GenclikAkademisi);

            KayitEkle("Kulüp", kulup.KulupAdi, "Tesis, İtibar & Yönetim",
                $"İtibar:{eItibar} Lig:{eLigItibar} YönEsneklik:{eYon}",
                $"İtibar:{kulup.Itibar} Lig:{kulup.LigItibari} YönEsneklik:{kulup.YonetimTesisVeButceEsnekligi}", () =>
                {
                    kulup.Itibar = eItibar;
                    kulup.LigItibari = eLigItibar;
                    kulup.AntrenmanTesisleri = eAnt;
                    kulup.AltyapiTesisleri = eAlt;
                    kulup.GenclikAkademisi = eAka;
                    kulup.YonetimTesisVeButceEsnekligi = eYon;
                });
        }

        // ==================== GERİ AL (UNDO) YÖNETİMİ ====================

        public void KayitEkle(string hedefTur, string hedefAd, string alanAdi, string eskiDeger, string yeniDeger, Action geriAl)
        {
            var kayit = new DegisiklikKaydi
            {
                Zaman = DateTime.Now,
                HedefTur = hedefTur,
                HedefAd = hedefAd,
                AlanAdi = alanAdi,
                EskiDeger = eskiDeger,
                YeniDeger = yeniDeger,
                GeriAlAksiyonu = geriAl
            };
            _geriAlYigini.Push(kayit);
            SonBilgiMesaji = $"Kaydedildi: {kayit}";
            Plugin.Gunluk?.LogInfo($"[Edit] {kayit}");
        }

        public bool SonIslemiGeriAl()
        {
            if (_geriAlYigini.Count == 0)
            {
                SonBilgiMesaji = "Geri alınacak işlem yok.";
                return false;
            }

            var son = _geriAlYigini.Pop();
            son.GeriAlAksiyonu?.Invoke();
            SonBilgiMesaji = $"Geri alındı: {son.HedefAd} ({son.AlanAdi} -> {son.EskiDeger})";
            Plugin.Gunluk?.LogInfo($"[Undo] {SonBilgiMesaji}");
            return true;
        }

        public int TumunuGeriAl()
        {
            int sayac = 0;
            while (_geriAlYigini.Count > 0)
            {
                var kayit = _geriAlYigini.Pop();
                kayit.GeriAlAksiyonu?.Invoke();
                sayac++;
            }
            SonBilgiMesaji = $"{sayac} değişiklik başlangıç değerine geri döndürüldü.";
            Plugin.Gunluk?.LogInfo($"[UndoAll] {SonBilgiMesaji}");
            return sayac;
        }

        // ==================== CANLI IL2CPP METADATA & SINIF TARAYICISI ====================

        public List<Il2CppSinifBilgisi> CanliIl2CppSiniflariniTara(string filtre = "")
        {
            _bulunanSiniflar.Clear();
            string[] anahtarKelimeler = string.IsNullOrWhiteSpace(filtre)
                ? new[] { "Player", "Person", "Club", "Team", "Finance", "Contract", "Attribute", "Injury", "Weather", "PressConference", "Morale" }
                : new[] { filtre.Trim() };

            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var asmAdi = asm.GetName().Name ?? "";
                    if (asmAdi.StartsWith("System", StringComparison.OrdinalIgnoreCase) ||
                        asmAdi.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                        asmAdi.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase) ||
                        asmAdi.StartsWith("Harmony", StringComparison.OrdinalIgnoreCase) ||
                        asmAdi.StartsWith("BepInEx", StringComparison.OrdinalIgnoreCase))
                        continue;

                    Type[] tipler;
                    try
                    {
                        tipler = asm.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        tipler = ex.Types;
                    }

                    if (tipler == null) continue;

                    foreach (var tip in tipler)
                    {
                        if (tip == null || !tip.IsClass) continue;
                        bool eslesti = anahtarKelimeler.Any(k => tip.Name.Contains(k, StringComparison.OrdinalIgnoreCase));
                        if (!eslesti) continue;

                        var ozellikler = tip.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        var alanlar = tip.GetFields(BindingFlags.Public | BindingFlags.Instance);
                        var metotlar = tip.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                        var ornekUyeler = ozellikler.Select(p => $"{p.PropertyType.Name} {p.Name}")
                            .Concat(alanlar.Select(f => $"{f.FieldType.Name} {f.Name}"))
                            .Take(8)
                            .ToList();

                        _bulunanSiniflar.Add(new Il2CppSinifBilgisi
                        {
                            AssemblyAdi = asmAdi,
                            TamAd = tip.FullName ?? tip.Name,
                            AlanSayisi = alanlar.Length,
                            OzellikSayisi = ozellikler.Length,
                            MetotSayisi = metotlar.Length,
                            OrnekUyeler = ornekUyeler
                        });
                    }
                }

                Plugin.Gunluk?.LogInfo($"=== FM26 IL2CPP Sınıf Tarama Sonucu ({_bulunanSiniflar.Count} sınıf bulundu) ===");
                SonBilgiMesaji = $"IL2CPP taraması tamamlandı: {_bulunanSiniflar.Count} aday sınıf bulundu (BepInEx/LogOutput.log dosyasına yazıldı).";
            }
            catch (Exception ex)
            {
                SonBilgiMesaji = $"IL2CPP tarama hatası: {ex.Message}";
            }

            return _bulunanSiniflar;
        }

        private bool YazmaKontrol(OyuncuVerisi oyuncu)
        {
            if (oyuncu == null) return false;
            if (!CevrimiciKoruma.YazmaIzniVarMi())
            {
                SonBilgiMesaji = CevrimiciKoruma.DurumMesaji;
                return false;
            }
            return true;
        }

        private static int CanliIl2CppNesneleriniBagla() => 0;

        internal static bool Il2CppOzellikYaz(object hedef, string[] adayAdlar, object yeniDeger)
        {
            if (hedef == null) return false;
            var tip = hedef.GetType();
            foreach (var ad in adayAdlar)
            {
                var prop = tip.GetProperty(ad, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    try
                    {
                        var donusturulmus = Convert.ChangeType(yeniDeger, prop.PropertyType);
                        prop.SetValue(hedef, donusturulmus);
                        return true;
                    }
                    catch { }
                }

                var field = tip.GetField(ad, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    try
                    {
                        var donusturulmus = Convert.ChangeType(yeniDeger, field.FieldType);
                        field.SetValue(hedef, donusturulmus);
                        return true;
                    }
                    catch { }
                }
            }
            return false;
        }

        private void OrnekVerileriYukle()
        {
            _oyuncular.Clear();
            _oyuncular.AddRange(new[]
            {
                new OyuncuVerisi
                {
                    Id = 1001, AdSoyad = "Arda Güler", KulupAdi = "Real Madrid", Pozisyon = "OOS (OS, OÖ)", FormaNumarasi = 15,
                    Yas = 21, Uyruk = "Türkiye", BoyCm = 175, KiloKg = 70, TercihEdilenAyak = "Sol",
                    MevcutYetenekCA = 158, PotansiyelYetenekPA = 185,
                    Kondisyon = 94, MacKondisyonu = 92, Moral = 18, SakatMi = false, SakatlikGunSayisi = 0,
                    SakatlikEgilimi = 9, Sureklilik = 16, OnemliMacPerformansi = 17, Sadakat = 16, TakimaKatilisAySayisi = 28,
                    HaftalikMaasEuro = 110_000, SozlesmeBitisYili = 2029, PiyasaDegeriEuro = 75_000_000, SerbestKalmaBedeliEuro = 0,
                    Bitiricilik = 15, Pas = 17, Teknik = 18, KararAlma = 15, Vizyon = 18, Hiz = 14, Dayaniklilik = 14,
                    Ziplama = 8, KafaVurusu = 9, NewgenMi = false, EtnikBolgeKodu = "MED"
                },
                new OyuncuVerisi
                {
                    Id = 1002, AdSoyad = "Kenan Yıldız", KulupAdi = "Juventus", Pozisyon = "OÖ (SL), ST", FormaNumarasi = 10,
                    Yas = 21, Uyruk = "Türkiye", BoyCm = 187, KiloKg = 77, TercihEdilenAyak = "Her İkisi",
                    MevcutYetenekCA = 154, PotansiyelYetenekPA = 182,
                    Kondisyon = 91, MacKondisyonu = 95, Moral = 17, SakatMi = false, SakatlikGunSayisi = 0,
                    SakatlikEgilimi = 7, Sureklilik = 15, OnemliMacPerformansi = 16, Sadakat = 15, TakimaKatilisAySayisi = 30,
                    HaftalikMaasEuro = 85_000, SozlesmeBitisYili = 2029, PiyasaDegeriEuro = 65_000_000, SerbestKalmaBedeliEuro = 80_000_000,
                    Bitiricilik = 15, Pas = 15, Teknik = 17, KararAlma = 14, Vizyon = 15, Hiz = 16, Dayaniklilik = 15,
                    Ziplama = 13, KafaVurusu = 12, NewgenMi = false, EtnikBolgeKodu = "MED"
                },
                new OyuncuVerisi
                {
                    Id = 1003, AdSoyad = "Semih Kılıçsoy", KulupAdi = "Beşiktaş", Pozisyon = "ST, OÖ (SL)", FormaNumarasi = 9,
                    Yas = 21, Uyruk = "Türkiye", BoyCm = 178, KiloKg = 79, TercihEdilenAyak = "Sağ",
                    MevcutYetenekCA = 138, PotansiyelYetenekPA = 172,
                    Kondisyon = 82, MacKondisyonu = 45, Moral = 11, SakatMi = true, SakatlikGunSayisi = 4, // Mikro sakatlık + kondisyon düşüşü örneği!
                    SakatlikEgilimi = 10, Sureklilik = 14, OnemliMacPerformansi = 15, Sadakat = 17, TakimaKatilisAySayisi = 5, MutsuzlukVarMi = true,
                    HaftalikMaasEuro = 25_000, SozlesmeBitisYili = 2028, PiyasaDegeriEuro = 22_000_000, SerbestKalmaBedeliEuro = 35_000_000,
                    Bitiricilik = 15, Pas = 13, Teknik = 15, KararAlma = 13, Vizyon = 13, Hiz = 15, Dayaniklilik = 15,
                    Ziplama = 11, KafaVurusu = 13, NewgenMi = false, EtnikBolgeKodu = "MED"
                },
                new OyuncuVerisi
                {
                    Id = 1004, AdSoyad = "Barış Alper Yılmaz", KulupAdi = "Galatasaray", Pozisyon = "OÖ (SĞ, SL), ST", FormaNumarasi = 53,
                    Yas = 26, Uyruk = "Türkiye", BoyCm = 186, KiloKg = 82, TercihEdilenAyak = "Sağ",
                    MevcutYetenekCA = 146, PotansiyelYetenekPA = 155,
                    Kondisyon = 98, MacKondisyonu = 98, Moral = 19, SakatMi = false, SakatlikGunSayisi = 0,
                    SakatlikEgilimi = 4, Sureklilik = 17, OnemliMacPerformansi = 16, Sadakat = 16, TakimaKatilisAySayisi = 42,
                    HaftalikMaasEuro = 48_000, SozlesmeBitisYili = 2028, PiyasaDegeriEuro = 30_000_000, SerbestKalmaBedeliEuro = 0,
                    Bitiricilik = 13, Pas = 12, Teknik = 14, KararAlma = 13, Vizyon = 12, Hiz = 17, Dayaniklilik = 18,
                    Ziplama = 16, KafaVurusu = 15, NewgenMi = false, EtnikBolgeKodu = "MED"
                },
                new OyuncuVerisi
                {
                    Id = 1005, AdSoyad = "Uğurcan Çakır", KulupAdi = "Trabzonspor", Pozisyon = "KL", FormaNumarasi = 1,
                    Yas = 30, Uyruk = "Türkiye", BoyCm = 191, KiloKg = 78, TercihEdilenAyak = "Sağ",
                    MevcutYetenekCA = 145, PotansiyelYetenekPA = 152,
                    Kondisyon = 88, MacKondisyonu = 50, Moral = 15, SakatMi = true, SakatlikGunSayisi = 3, // Kaleci mikro sakatlık örneği!
                    SakatlikEgilimi = 6, Sureklilik = 16, OnemliMacPerformansi = 17, Sadakat = 19, TakimaKatilisAySayisi = 96,
                    HaftalikMaasEuro = 38_000, SozlesmeBitisYili = 2028, PiyasaDegeriEuro = 11_000_000, SerbestKalmaBedeliEuro = 0,
                    Bitiricilik = 3, Pas = 13, Teknik = 12, KararAlma = 15, Vizyon = 13, Hiz = 11, Dayaniklilik = 14,
                    Ziplama = 17, KafaVurusu = 6, NewgenMi = false, EtnikBolgeKodu = "MED"
                },
                new OyuncuVerisi
                {
                    Id = 2002109401, AdSoyad = "Emre Demirci (Newgen)", KulupAdi = "Fenerbahçe", Pozisyon = "STP (M)", FormaNumarasi = 4,
                    Yas = 18, Uyruk = "Türkiye", BoyCm = 193, KiloKg = 84, TercihEdilenAyak = "Sol",
                    MevcutYetenekCA = 124, PotansiyelYetenekPA = 179,
                    Kondisyon = 96, MacKondisyonu = 90, Moral = 13, SakatMi = false, SakatlikGunSayisi = 0,
                    SakatlikEgilimi = 6, Sureklilik = 15, OnemliMacPerformansi = 14, Sadakat = 16, TakimaKatilisAySayisi = 3, MutsuzlukVarMi = true,
                    HaftalikMaasEuro = 8_500, SozlesmeBitisYili = 2029, PiyasaDegeriEuro = 9_500_000, SerbestKalmaBedeliEuro = 25_000_000,
                    Bitiricilik = 6, Pas = 14, Teknik = 13, KararAlma = 15, Vizyon = 12, Hiz = 15, Dayaniklilik = 16,
                    Ziplama = 18, KafaVurusu = 17, NewgenMi = true, EtnikBolgeKodu = "MED"
                }
            });

            _kulupler.Clear();
            _kulupler.AddRange(new[]
            {
                new KulupVerisi
                {
                    Id = 201, KulupAdi = "Galatasaray", UlkeAdi = "Türkiye", LigAdi = "Süper Lig", LigItibari = 7100, Itibar = 7350,
                    BakiyeEuro = 42_000_000, TransferButcesiEuro = 28_000_000, MaasButcesiHaftalikEuro = 1_650_000, SponsorlukGeliriYillikEuro = 45_000_000,
                    AntrenmanTesisleri = 16, AltyapiTesisleri = 15, GenclikAkademisi = 16, YonetimTesisVeButceEsnekligi = 11
                },
                new KulupVerisi
                {
                    Id = 202, KulupAdi = "Fenerbahçe", UlkeAdi = "Türkiye", LigAdi = "Süper Lig", LigItibari = 7100, Itibar = 7300,
                    BakiyeEuro = 38_000_000, TransferButcesiEuro = 32_000_000, MaasButcesiHaftalikEuro = 1_700_000, SponsorlukGeliriYillikEuro = 44_000_000,
                    AntrenmanTesisleri = 16, AltyapiTesisleri = 15, GenclikAkademisi = 15, YonetimTesisVeButceEsnekligi = 11
                },
                new KulupVerisi
                {
                    Id = 203, KulupAdi = "Beşiktaş", UlkeAdi = "Türkiye", LigAdi = "Süper Lig", LigItibari = 7100, Itibar = 7050,
                    BakiyeEuro = 18_000_000, TransferButcesiEuro = 15_000_000, MaasButcesiHaftalikEuro = 1_150_000, SponsorlukGeliriYillikEuro = 30_000_000,
                    AntrenmanTesisleri = 15, AltyapiTesisleri = 16, GenclikAkademisi = 16, YonetimTesisVeButceEsnekligi = 10
                },
                new KulupVerisi
                {
                    Id = 204, KulupAdi = "Trabzonspor", UlkeAdi = "Türkiye", LigAdi = "Süper Lig", LigItibari = 7100, Itibar = 6800,
                    BakiyeEuro = 12_000_000, TransferButcesiEuro = 9_500_000, MaasButcesiHaftalikEuro = 750_000, SponsorlukGeliriYillikEuro = 20_000_000,
                    AntrenmanTesisleri = 15, AltyapiTesisleri = 15, GenclikAkademisi = 16, YonetimTesisVeButceEsnekligi = 10
                }
            });
        }
    }
}
