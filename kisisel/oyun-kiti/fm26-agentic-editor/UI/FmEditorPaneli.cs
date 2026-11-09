using System;
using System.Linq;
using FM26AgenticEditor.Models;
using FM26AgenticEditor.Services;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FM26AgenticEditor.UI
{
    /// <summary>
    /// F8 ile açılan 6 sekmeli FM26 Oyun İçi Editör, Yaşayan Futbol Dünyası, Rehabilitasyon & Agentic FM Arayüzü
    /// + F9 ile açılıp kapanan "Masaüstü Oyuncu Röntgeni (Mini-HUD Overlay)" çubuğu.
    /// </summary>
    public class FmEditorPaneli : MonoBehaviour
    {
        public FmEditorPaneli(IntPtr ptr) : base(ptr) { }

        private bool _acik;
        private int _aktifSekme;
        private Rect _pencere = new Rect(25, 55, 940, 625);
        private Rect _hudPencere = new Rect(25, 8, 940, 42);
        private GUI.WindowFunction _pencereFonksiyonu;
        private GUI.WindowFunction _hudFonksiyonu;

        // Arama ve Seçim Durumu
        private string _oyuncuArama = "";
        private string _kulupArama = "";
        private string _il2cppFiltre = "Player";
        private string _basinSorusu = "Genç yıldızımızın gelişimi ve gelecek maçtaki taktik planımız";
        private string _paketOnizleme = "";
        private OyuncuVerisi _seciliOyuncu;
        private KulupVerisi _seciliKulup;

        // Kaydırma Çubukları
        private Vector2 _oyuncuListeScroll = Vector2.zero;
        private Vector2 _kulupListeScroll = Vector2.zero;
        private Vector2 _duranTopScroll = Vector2.zero;
        private Vector2 _undoScroll = Vector2.zero;
        private Vector2 _il2cppScroll = Vector2.zero;

        // Düzenleme Alanı Tamponları (Oyuncu)
        private int _duzenCa;
        private int _duzenPa;
        private int _duzenBitiricilik;
        private int _duzenPas;
        private int _duzenTeknik;
        private int _duzenKararAlma;
        private int _duzenVizyon;
        private int _duzenHiz;
        private int _duzenDayaniklilik;
        private int _duzenSakatlikEgilimi;
        private int _duzenSureklilik;
        private int _duzenOnemliMac;

        // Düzenleme Alanı Tamponları (Kulüp)
        private string _duzenBakiyeStr = "";
        private string _duzenTransferStr = "";
        private string _duzenMaasStr = "";
        private int _duzenItibar;
        private int _duzenLigItibari;
        private int _duzenAntrenman;
        private int _duzenAltyapi;
        private int _duzenAkademi;
        private int _duzenYonetimEsnekligi;

        private void Awake()
        {
            _pencereFonksiyonu = DelegateSupport.ConvertDelegate<GUI.WindowFunction>((Action<int>)PencereCiz);
            _hudFonksiyonu = DelegateSupport.ConvertDelegate<GUI.WindowFunction>((Action<int>)HudCiz);

            if (Plugin.VeriKoprusu != null)
            {
                var ilkOyuncu = Plugin.VeriKoprusu.Oyuncular.FirstOrDefault();
                if (ilkOyuncu != null) OyuncuSec(ilkOyuncu);

                var ilkKulup = Plugin.VeriKoprusu.Kulupler.FirstOrDefault();
                if (ilkKulup != null) KulupSec(ilkKulup);
            }
        }

        private void Update()
        {
            if (AnaPanelKisayoluBasildiMi())
                _acik = !_acik;

            if (HudKisayoluBasildiMi() && Plugin.Rehabilitasyon != null)
                Plugin.Rehabilitasyon.Ayarlar.MiniHudRontgenAcik = !Plugin.Rehabilitasyon.Ayarlar.MiniHudRontgenAcik;
        }

        private static bool AnaPanelKisayoluBasildiMi()
        {
            try
            {
                if (Keyboard.current != null && Keyboard.current.f8Key != null && Keyboard.current.f8Key.wasPressedThisFrame)
                    return true;
            }
            catch { }

            try
            {
                var tus = Plugin.PanelKisayolu != null ? Plugin.PanelKisayolu.Value : KeyCode.F8;
                if (Input.GetKeyDown(tus))
                    return true;
            }
            catch { }

            return false;
        }

        private static bool HudKisayoluBasildiMi()
        {
            try
            {
                if (Keyboard.current != null && Keyboard.current.f9Key != null && Keyboard.current.f9Key.wasPressedThisFrame)
                    return true;
            }
            catch { }

            try
            {
                var tus = Plugin.RontgenHudKisayolu != null ? Plugin.RontgenHudKisayolu.Value : KeyCode.F9;
                if (Input.GetKeyDown(tus))
                    return true;
            }
            catch { }

            return false;
        }

        private void OnGUI()
        {
            if (Plugin.Rehabilitasyon != null && Plugin.Rehabilitasyon.Ayarlar.MiniHudRontgenAcik && _seciliOyuncu != null)
            {
                if (_hudFonksiyonu == null)
                    _hudFonksiyonu = DelegateSupport.ConvertDelegate<GUI.WindowFunction>((Action<int>)HudCiz);

                _hudPencere = GUILayout.Window(82601, _hudPencere, _hudFonksiyonu, "Masaüstü Oyuncu Röntgeni (F9: Gizle/Göster | F8: Tam Editör)");
            }

            if (!_acik)
                return;

            if (_pencereFonksiyonu == null)
                _pencereFonksiyonu = DelegateSupport.ConvertDelegate<GUI.WindowFunction>((Action<int>)PencereCiz);

            _pencere = GUILayout.Window(82600, _pencere, _pencereFonksiyonu, "FM26 In-Game Editor, Yaşayan Dünya & Agentic FM v0.5.0 (F8)");
        }

        private void HudCiz(int id)
        {
            var o = _seciliOyuncu;
            if (o != null)
            {
                string serbestKalma = o.SerbestKalmaBedeliEuro > 0 ? $"{o.SerbestKalmaBedeliEuro / 1_000_000.0:0.#}M€" : "Yok";
                GUILayout.Label(
                    $"#{o.FormaNumarasi} {o.AdSoyad} ({o.KulupAdi}) | Boy:{o.BoyCm}cm Kilo:{o.KiloKg}kg Ayak:{o.TercihEdilenAyak} | " +
                    $"CA/PA:{o.MevcutYetenekCA}/{o.PotansiyelYetenekPA} | Değer:{o.PiyasaDegeriEuro / 1_000_000.0:0.#}M€ | " +
                    $"Tutarlılık:{o.TutarlilikIndeksi}/100 ({o.TutarlilikEtiketi}) | SakatlıkEğilimi:{o.SakatlikEgilimi}/20 | Hava:{o.HavaHakimiyetiPuani}/100");
            }
            GUI.DragWindow();
        }

        private void PencereCiz(int id)
        {
            // Üst Durum Çubuğu
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Durum: {CevrimiciKoruma.DurumMesaji} | Aktif Sezon: {Plugin.YasayanDunya.AktifEvrimYili}");
            if (GUILayout.Button("HUD Röntgen (F9)", GUILayout.Width(125)))
            {
                Plugin.Rehabilitasyon.Ayarlar.MiniHudRontgenAcik = !Plugin.Rehabilitasyon.Ayarlar.MiniHudRontgenAcik;
            }
            if (GUILayout.Button("↩ Son İşlemi Geri Al", GUILayout.Width(145)))
            {
                Plugin.VeriKoprusu.SonIslemiGeriAl();
                if (_seciliOyuncu != null) OyuncuSec(_seciliOyuncu);
                if (_seciliKulup != null) KulupSec(_seciliKulup);
            }
            GUILayout.EndHorizontal();

            // 6 Sekmeli Navigasyon
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_aktifSekme == 0 ? "● [1] Oyuncu & Röntgen" : "[1] Oyuncu & Röntgen")) _aktifSekme = 0;
            if (GUILayout.Button(_aktifSekme == 1 ? "● [2] Kulüp & Süper Lig" : "[2] Kulüp & Süper Lig")) _aktifSekme = 1;
            if (GUILayout.Button(_aktifSekme == 2 ? "● [3] 🌍 Yaşayan Dünya" : "[3] 🌍 Yaşayan Dünya")) _aktifSekme = 2;
            if (GUILayout.Button(_aktifSekme == 3 ? "● [4] Rehabilitasyon & Paket" : "[4] Rehabilitasyon & Paket")) _aktifSekme = 3;
            if (GUILayout.Button(_aktifSekme == 4 ? "● [5] Agentic & Soyunma Odası" : "[5] Agentic & Soyunma Odası")) _aktifSekme = 4;
            if (GUILayout.Button(_aktifSekme == 5 ? $"● [6] Undo ({Plugin.VeriKoprusu.GeriAlGecmisi.Count}) & Kanca" : $"[6] Undo ({Plugin.VeriKoprusu.GeriAlGecmisi.Count}) & Kanca")) _aktifSekme = 5;
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            switch (_aktifSekme)
            {
                case 0:
                    OyuncuSekmesiCiz();
                    break;
                case 1:
                    KulupVeSuperLigSekmesiCiz();
                    break;
                case 2:
                    YasayanDunyaSekmesiCiz();
                    break;
                case 3:
                    RehabilitasyonSekmesiCiz();
                    break;
                case 4:
                    AgenticSekmesiCiz();
                    break;
                case 5:
                    UndoVeIl2CppSekmesiCiz();
                    break;
            }

            GUILayout.Space(4);
            GUILayout.Label($"Bilgi: {Plugin.VeriKoprusu.SonBilgiMesaji}");
            GUI.DragWindow();
        }

        // ==================== SEKME 0: OYUNCU & MASAÜSTÜ RÖNTGEN ====================

        private void OyuncuSekmesiCiz()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(265));
            GUILayout.Label("İsim / Kulüp / Pozisyon / Forma No Ara:");
            _oyuncuArama = GUILayout.TextField(_oyuncuArama ?? "");

            _oyuncuListeScroll = GUILayout.BeginScrollView(_oyuncuListeScroll, GUILayout.Width(260), GUILayout.Height(425));
            foreach (var o in Plugin.VeriKoprusu.OyuncuAra(_oyuncuArama))
            {
                string uyari = o.SakatMi ? "🏥" : (o.MutsuzlukVarMi ? "⚠️" : (o.HarryKaneIstisnasiMi ? "🌟" : ""));
                string etiket = _seciliOyuncu?.Id == o.Id
                    ? $"▶ #{o.FormaNumarasi} {o.AdSoyad} ({o.MevcutYetenekCA}/{o.PotansiyelYetenekPA}) {uyari}"
                    : $"#{o.FormaNumarasi} {o.AdSoyad} - {o.KulupAdi} ({o.MevcutYetenekCA}) {uyari}";

                if (GUILayout.Button(etiket))
                    OyuncuSec(o);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUILayout.BeginVertical();
            if (_seciliOyuncu == null)
            {
                GUILayout.Label("Soldaki listeden düzenlemek istediğiniz oyuncuyu seçin.");
            }
            else
            {
                string serbestKalma = _seciliOyuncu.SerbestKalmaBedeliEuro > 0
                    ? $"{_seciliOyuncu.SerbestKalmaBedeliEuro:N0}€"
                    : "Yok";

                GUILayout.Label($"#{_seciliOyuncu.FormaNumarasi} {_seciliOyuncu.AdSoyad} ({_seciliOyuncu.Yas} yaş, {_seciliOyuncu.Uyruk}) | Boy: {_seciliOyuncu.BoyCm} cm | Kilo: {_seciliOyuncu.KiloKg} kg | Ayak: {_seciliOyuncu.TercihEdilenAyak}");
                GUILayout.Label($"Kulüp: {_seciliOyuncu.KulupAdi} | Poz: {_seciliOyuncu.Pozisyon} | Değer: {_seciliOyuncu.PiyasaDegeriEuro:N0}€ | Serbest Kalma: {serbestKalma}");
                GUILayout.Label($"Tutarlılık ('Messi vs Halı Saha' Röntgeni): {_seciliOyuncu.TutarlilikIndeksi}/100 ({_seciliOyuncu.TutarlilikEtiketi}) | Hava Hakimiyeti: {_seciliOyuncu.HavaHakimiyetiPuani}/100");
                GUILayout.Label($"Fiziksel: Kondisyon %{_seciliOyuncu.Kondisyon} | Maç Kondisyonu: %{_seciliOyuncu.MacKondisyonu} | Moral: {_seciliOyuncu.Moral}/20 | {(_seciliOyuncu.SakatMi ? $"SAKAT ({_seciliOyuncu.SakatlikGunSayisi} gün) 🏥" : "Sağlıklı ✅")}");

                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("⚡ Kondisyon/MaçRitmi Full + İyileştir"))
                {
                    Plugin.VeriKoprusu.OyuncuKondisyonMoralFulleyipIyilestir(_seciliOyuncu);
                }
                if (GUILayout.Button("🌟 +10 CA Geliştir"))
                {
                    _duzenCa = Mathf.Clamp(_seciliOyuncu.MevcutYetenekCA + 10, 1, 200);
                    Plugin.VeriKoprusu.OyuncuCaPaGuncelle(_seciliOyuncu, _duzenCa, _duzenPa);
                }
                if (GUILayout.Button("🧠 AI Scout & İstikrar Raporu"))
                {
                    Plugin.AgentServisi.OyuncuScoutAnaliziYap(_seciliOyuncu);
                    _aktifSekme = 4;
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(4);
                GUILayout.Label($"Mevcut Yetenek (CA: {_duzenCa} / 200) & Potansiyel Yetenek (PA: {_duzenPa} / 200):");
                _duzenCa = Mathf.RoundToInt(GUILayout.HorizontalSlider(_duzenCa, 1f, 200f));
                _duzenPa = Mathf.RoundToInt(GUILayout.HorizontalSlider(_duzenPa, 1f, 200f));

                if (GUILayout.Button("CA / PA Değişikliğini Uygula"))
                {
                    Plugin.VeriKoprusu.OyuncuCaPaGuncelle(_seciliOyuncu, _duzenCa, _duzenPa);
                }

                GUILayout.Space(4);
                GUILayout.Label("--- Teknik & Gizli Özellikler (1 - 20) ---");
                _duzenBitiricilik = OzellikSliderCiz("Bitiricilik", _duzenBitiricilik);
                _duzenPas = OzellikSliderCiz("Pas", _duzenPas);
                _duzenTeknik = OzellikSliderCiz("Teknik", _duzenTeknik);
                _duzenKararAlma = OzellikSliderCiz("Karar Alma", _duzenKararAlma);
                _duzenVizyon = OzellikSliderCiz("Vizyon", _duzenVizyon);
                _duzenHiz = OzellikSliderCiz("Hız", _duzenHiz);
                _duzenDayaniklilik = OzellikSliderCiz("Dayanıklılık", _duzenDayaniklilik);
                _duzenSakatlikEgilimi = OzellikSliderCiz("Sakatlık Eğilimi (Düşük=İyi)", _duzenSakatlikEgilimi);
                _duzenSureklilik = OzellikSliderCiz("Süreklilik (Gizli - İstikrar)", _duzenSureklilik);
                _duzenOnemliMac = OzellikSliderCiz("Önemli Maçlar (Gizli)", _duzenOnemliMac);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Özellikleri Kaydet"))
                {
                    Plugin.VeriKoprusu.OyuncuOzellikleriniGuncelle(
                        _seciliOyuncu,
                        _duzenBitiricilik, _duzenPas, _duzenTeknik,
                        _duzenKararAlma, _duzenVizyon, _duzenHiz, _duzenDayaniklilik,
                        _duzenSakatlikEgilimi, _duzenSureklilik, _duzenOnemliMac);
                }
                if (GUILayout.Button("Elit & İstikrarlı Yap (Sakatlık=2, Süreklilik=20)"))
                {
                    _duzenBitiricilik = _duzenPas = _duzenTeknik = _duzenKararAlma = _duzenVizyon = _duzenHiz = _duzenDayaniklilik = 20;
                    _duzenSakatlikEgilimi = 2;
                    _duzenSureklilik = _duzenOnemliMac = 20;
                    Plugin.VeriKoprusu.OyuncuOzellikleriniGuncelle(
                        _seciliOyuncu, 20, 20, 20, 20, 20, 20, 20, 2, 20, 20);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private static int OzellikSliderCiz(string ad, int deger)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{ad}: {deger}", GUILayout.Width(185));
            int yeni = Mathf.RoundToInt(GUILayout.HorizontalSlider(deger, 1f, 20f));
            GUILayout.EndHorizontal();
            return yeni;
        }

        // ==================== SEKME 1: KULÜP & SÜPER LİG PAKETİ ====================

        private void KulupVeSuperLigSekmesiCiz()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🇹🇷 Tek Tıkla Türkiye Süper Lig Gerçekçilik & İtibar Kalibrasyonu Uygula (Avrupa İlk 8 + Yönetim Esnekliği)"))
            {
                Plugin.Rehabilitasyon.SuperLigGercekcilikPaketiUygula(Plugin.VeriKoprusu);
                if (_seciliKulup != null) KulupSec(_seciliKulup);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(260));
            GUILayout.Label("Kulüp / Lig Ara:");
            _kulupArama = GUILayout.TextField(_kulupArama ?? "");

            _kulupListeScroll = GUILayout.BeginScrollView(_kulupListeScroll, GUILayout.Width(255), GUILayout.Height(395));
            foreach (var k in Plugin.VeriKoprusu.KulupAra(_kulupArama))
            {
                string etiket = _seciliKulup?.Id == k.Id
                    ? $"▶ {k.KulupAdi} ({k.Itibar} | {k.IhracatDegerCarpani:0.00}x)"
                    : $"{k.KulupAdi} ({k.Itibar} | {k.IhracatDegerCarpani:0.00}x)";

                if (GUILayout.Button(etiket))
                    KulupSec(k);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUILayout.BeginVertical();
            if (_seciliKulup == null)
            {
                GUILayout.Label("Soldaki listeden bir kulüp seçin.");
            }
            else
            {
                GUILayout.Label($"Seçili Kulüp: {_seciliKulup.KulupAdi} ({_seciliKulup.LigAdi} - Lig İtibarı: {_seciliKulup.LigItibari}/10000)");
                GUILayout.Label($"Miras Seviyesi: {_seciliKulup.KulupMirasSeviyesi}/10 | Benfica İhracat Çarpanı: {_seciliKulup.IhracatDegerCarpani:0.00}x | Durum: {_seciliKulup.DunyaDurumEtiketi}");
                GUILayout.Label($"Bakiye: {_seciliKulup.BakiyeEuro:N0}€ | Transfer Bütçesi: {_seciliKulup.TransferButcesiEuro:N0}€ | Sponsorluk: {_seciliKulup.SponsorlukGeliriYillikEuro:N0}€/yıl");

                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("💰 +25M € Transfer Bütçesi Ekle"))
                {
                    Plugin.VeriKoprusu.KulupFinansGuncelle(
                        _seciliKulup,
                        _seciliKulup.BakiyeEuro + 25_000_000L,
                        _seciliKulup.TransferButcesiEuro + 25_000_000L,
                        _seciliKulup.MaasButcesiHaftalikEuro);
                    KulupSec(_seciliKulup);
                }
                if (GUILayout.Button("🏗️ Tesisleri & Yönetim Esnekliğini 20 Yap"))
                {
                    Plugin.VeriKoprusu.KulupTesisVeItibarGuncelle(
                        _seciliKulup, _seciliKulup.Itibar, _seciliKulup.LigItibari, 20, 20, 20, 20);
                    KulupSec(_seciliKulup);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(4);
                GUILayout.Label("Bakiye (€):");
                _duzenBakiyeStr = GUILayout.TextField(_duzenBakiyeStr ?? "");
                GUILayout.Label("Transfer Bütçesi (€):");
                _duzenTransferStr = GUILayout.TextField(_duzenTransferStr ?? "");
                GUILayout.Label("Haftalık Maaş Bütçesi (€):");
                _duzenMaasStr = GUILayout.TextField(_duzenMaasStr ?? "");

                if (GUILayout.Button("Finans Değerlerini Uygula"))
                {
                    long.TryParse(_duzenBakiyeStr, out long yeniBakiye);
                    long.TryParse(_duzenTransferStr, out long yeniTransfer);
                    long.TryParse(_duzenMaasStr, out long yeniMaas);
                    Plugin.VeriKoprusu.KulupFinansGuncelle(_seciliKulup, yeniBakiye, yeniTransfer, yeniMaas);
                }

                GUILayout.Space(6);
                GUILayout.Label($"Kulüp İtibarı (1-10000): {_duzenItibar} | Lig İtibarı (1-10000): {_duzenLigItibari}");
                _duzenItibar = Mathf.RoundToInt(GUILayout.HorizontalSlider(_duzenItibar, 1f, 10000f));
                _duzenLigItibari = Mathf.RoundToInt(GUILayout.HorizontalSlider(_duzenLigItibari, 1f, 10000f));
                _duzenAntrenman = OzellikSliderCiz("Antrenman Tesisleri", _duzenAntrenman);
                _duzenAltyapi = OzellikSliderCiz("Altyapı Tesisleri", _duzenAltyapi);
                _duzenAkademi = OzellikSliderCiz("Gençlik Akademisi", _duzenAkademi);
                _duzenYonetimEsnekligi = OzellikSliderCiz("Yönetim Talepleri Onay Esnekliği", _duzenYonetimEsnekligi);

                if (GUILayout.Button("Tesis, İtibar & Yönetim Değerlerini Uygula"))
                {
                    Plugin.VeriKoprusu.KulupTesisVeItibarGuncelle(
                        _seciliKulup, _duzenItibar, _duzenLigItibari, _duzenAntrenman, _duzenAltyapi, _duzenAkademi, _duzenYonetimEsnekligi);
                }
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        // ==================== SEKME 2: 🌍 YAŞAYAN FUTBOL DÜNYASI & AI KADRO MÜHENDİSLİĞİ ====================

        private void YasayanDunyaSekmesiCiz()
        {
            var yd = Plugin.YasayanDunya;
            var a = Plugin.Rehabilitasyon.Ayarlar;

            GUILayout.Label($"=== 🌍 Yaşayan Futbol Dünyası, Benfica Katsayısı & Akıllı AI Direktör (Aktif Sezon: {yd.AktifEvrimYili}) ===");

            GUILayout.BeginHorizontal();
            a.DinamikIhracatKatsayisiAktif = GUILayout.Toggle(a.DinamikIhracatKatsayisiAktif, "Benfica İhracat Çarpanı (Ülke 25M€ tavanını kır)");
            a.OtonomRakipGuclendiriciAktif = GUILayout.Toggle(a.OtonomRakipGuclendiriciAktif, "Otonom Rakip Güçlendirici (Add Manager'a son)");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            a.Yas30UstuBonservisTavanAktif = GUILayout.Toggle(a.Yas30UstuBonservisTavanAktif, "30+ Yaş Bonservis Tavanı (Harry Kane CA 172+ hariç)");
            a.AiJenerasyonYenilemeZorlayici = GUILayout.Toggle(a.AiJenerasyonYenilemeZorlayici, "AI Dev Kulüpler İçin Jenerasyon Yenileme (Wonderkid Odaklı)");
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("--- 1. Ekonomik Evrim, Rakip Rekabeti & AI Kadro Planlaması Aksiyonları ---");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"🦅 1. Benfica İhracat Katsayısı Uygula ({_seciliKulup?.KulupAdi ?? "Kulüp"})"))
            {
                yd.BenficaIhracatKatsayisiUygula(Plugin.VeriKoprusu, _seciliKulup);
            }
            if (GUILayout.Button("🌊 2. Yükselen Dalga: Ligdeki Rakipleri Güçlendir (Anti-Add-Manager)"))
            {
                yd.YukselenDalgaRakipGuclendiriciCalistir(Plugin.VeriKoprusu);
                if (_seciliKulup != null) KulupSec(_seciliKulup);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🧠 3. AI 30+ Yaş Tavanı & Jenerasyon Yenileme Çalıştır"))
            {
                yd.AiKadroMuhendisligiVeYasTavaniniCalistir(Plugin.VeriKoprusu);
            }
            if (GUILayout.Button($"🌍 4. Futbol Dünyasını +1 Sezon Evrimleştir ({yd.AktifEvrimYili} -> {yd.AktifEvrimYili + 1})"))
            {
                yd.YirmiYillikDunyaEvrimiIlerlet(Plugin.VeriKoprusu);
                if (_seciliKulup != null) KulupSec(_seciliKulup);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label($"--- 2. Kulüp Mirası & Başarma Hissiyatı Ödülü ({_seciliKulup?.KulupAdi ?? "Seçili Kulüp"}) ---");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🏆 Şampiyonlar Ligi Zaferi Mirası İşle"))
            {
                yd.KulupMirasiZaferiIslet(Plugin.VeriKoprusu, _seciliKulup, "UEFA Şampiyonlar Ligi Şampiyonluğu");
                if (_seciliKulup != null) KulupSec(_seciliKulup);
            }
            if (GUILayout.Button("🥇 Avrupa Kupası / Yarı Final Mirası İşle"))
            {
                yd.KulupMirasiZaferiIslet(Plugin.VeriKoprusu, _seciliKulup, "Avrupa Kupası & Elit Tur Başarısı");
                if (_seciliKulup != null) KulupSec(_seciliKulup);
            }
            if (GUILayout.Button("⭐ Alt Ligden Zirveye Mucize Mirası İşle"))
            {
                yd.KulupMirasiZaferiIslet(Plugin.VeriKoprusu, _seciliKulup, "Alt Ligden Şampiyonlar Ligi'ne Yükseliş Mirası");
                if (_seciliKulup != null) KulupSec(_seciliKulup);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Yaşayan Dünya & AI Evrim Bülteni:");
            GUILayout.TextArea(yd.SonEvrimBulteni ?? "", GUILayout.Height(230));
        }

        // ==================== SEKME 3: REHABİLİTASYON, MAÇ MOTORU & PAKET ÜRETİCİ ====================

        private void RehabilitasyonSekmesiCiz()
        {
            var r = Plugin.Rehabilitasyon;
            var a = r.Ayarlar;

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(430));
            GUILayout.Label("=== 1. Tıbbi Denge & Mikro-Sakatlık Kalkanı ===");
            GUILayout.Label($"Küresel Sakatlık Olasılık Çarpanı: %{a.SakatlikFrekansiCarpaniYuzde} (Önerilen: %65-%75)");
            a.SakatlikFrekansiCarpaniYuzde = Mathf.RoundToInt(GUILayout.HorizontalSlider(a.SakatlikFrekansiCarpaniYuzde, 50f, 100f));
            a.KaleciTemassizSakatlikKorumasi = GUILayout.Toggle(a.KaleciTemassizSakatlikKorumasi, "Kalecilerin temassız mikro sakatlıklarını (1-12 gün) iptal et");
            a.MikroSakatlikKondisyonKoruyucu = GUILayout.Toggle(a.MikroSakatlikKondisyonKoruyucu, "Mikro sakatlık sonrası Maç Kondisyonu (Sharpness) sıfırlanmasını önle");

            if (GUILayout.Button("🏥 Tıbbi Kalkanı Şimdi Çalıştır (Kaleci & Maç Kondisyonu Koru)"))
            {
                r.TibbiDengeVeMikroSakatlikKalkaniniCalistir(Plugin.VeriKoprusu);
            }

            GUILayout.Space(6);
            GUILayout.Label("=== 2. Maç Motoru, Çevre & Ergonomi Yamaları ===");
            a.SurekliYagmurDuzeltici = GUILayout.Toggle(a.SurekliYagmurDuzeltici, "Aralıksız yağmur hatasını lig coğrafyasına göre dengele (Weather Fix)");
            a.SantraSavunmaUyarici = GUILayout.Toggle(a.SantraSavunmaUyarici, "Santra vuruşu sonrası savunma uyuşukluğunu engelle");
            a.KlasikBoslukTusuYamasi = GUILayout.Toggle(a.KlasikBoslukTusuYamasi, "Klasik FM24 Boşluk (Space) tuşu ilerleme davranışını kullan");
            a.ArkaPlanSimulasyonHizlandirici = GUILayout.Toggle(a.ArkaPlanSimulasyonHizlandirici, "Oynanmayan arka plan/kadın liglerinin simülasyon yükünü azalt (Lite DB)");

            GUILayout.Space(6);
            GUILayout.Label("=== 3. Transfer & Diyalog Mantığı ===");
            a.KullaniciVergisiDengeleyici = GUILayout.Toggle(a.KullaniciVergisiDengeleyici, "AI kulüplerinin 80-100M€ fahiş bonservis çarpanını (User Tax) dengele");
            a.IrrasyonelIsyanFiltresi = GUILayout.Toggle(a.IrrasyonelIsyanFiltresi, "Yeni transferlerin (<12 ay) Suudi Arabistan/transfer isyanını engelle");
            a.PersonelItibarEsnetici = GUILayout.Toggle(a.PersonelItibarEsnetici, "Alt liglerden antrenör/gözlemci transferindeki katı itibar engelini esnet");

            if (GUILayout.Button("🛡️ Yeni Transferlerin İrrasyonel İsyanlarını Şimdi Temizle"))
            {
                r.IrrasyonelIsyanlariTemizle(Plugin.VeriKoprusu);
            }
            GUILayout.EndVertical();

            GUILayout.Space(12);

            GUILayout.BeginVertical();
            GUILayout.Label("=== 4. Duran Top Forma No & Hava Hakimiyeti Tablosu ===");
            _duranTopScroll = GUILayout.BeginScrollView(_duranTopScroll, GUILayout.Height(145));
            foreach (var o in Plugin.VeriKoprusu.Oyuncular.OrderByDescending(x => x.HavaHakimiyetiPuani))
            {
                GUILayout.Label($"Forma #{o.FormaNumarasi} -> {o.AdSoyad} ({o.Pozisyon}) | Boy:{o.BoyCm}cm Zıp:{o.Ziplama} Kafa:{o.KafaVurusu} | Hava: {o.HavaHakimiyetiPuani}/100");
            }
            GUILayout.EndScrollView();

            GUILayout.Space(6);
            GUILayout.Label("=== 5. Harici Mod & Tema Paketi Üretici (Skin / XML / Regen / CSV) ===");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🎨 Antrasit Skin Üret"))
            {
                _paketOnizleme = Plugin.PaketUretici.MasaustuAntrasitSkinPaketiUret();
                Plugin.VeriKoprusu.SonBilgiMesaji = Plugin.PaketUretici.SonUretimRaporu;
            }
            if (GUILayout.Button("📦 SüperLig+LiteDB XML"))
            {
                _paketOnizleme = Plugin.PaketUretici.SuperLigVePerformansEditorDataUret(Plugin.VeriKoprusu);
                Plugin.VeriKoprusu.SonBilgiMesaji = Plugin.PaketUretici.SonUretimRaporu;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🖼️ AI Regen config.xml"))
            {
                _paketOnizleme = r.AiNewgenYuzConfigOlustur(Plugin.VeriKoprusu);
            }
            if (GUILayout.Button("📊 0ms Kadro CSV Tablo"))
            {
                _paketOnizleme = Plugin.PaketUretici.HizliKadroKiyaslamaTablosuUret(Plugin.VeriKoprusu);
                Plugin.VeriKoprusu.SonBilgiMesaji = Plugin.PaketUretici.SonUretimRaporu;
            }
            GUILayout.EndHorizontal();

            GUILayout.TextArea(string.IsNullOrEmpty(_paketOnizleme)
                ? "Yukarıdaki butonlarla: (1) FM24 Antrasit/Lacivert Masaüstü Skini, (2) Süper Lig + TFF + LiteDB Editor Data XML, (3) AI Regen Yüz config.xml ve (4) Masaüstü Kadro Röntgen CSV tablosunu Belgeler/Sports Interactive klasörüne tek tıkla üretebilirsiniz."
                : _paketOnizleme, GUILayout.Height(150));
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        // ==================== SEKME 4: AGENTIC FM & GERÇEK ETKİLİ SOYUNMA ODASI ====================

        private void AgenticSekmesiCiz()
        {
            var ajan = Plugin.AgentServisi;
            GUILayout.Label("=== Agentic FM 2.0: Scout, 'Messi vs Halı Saha' İstikrar Röntgeni & Gerçek Etkili Soyunma Odası ===");

            GUILayout.BeginHorizontal();
            ajan.UzakLlmKullan = GUILayout.Toggle(ajan.UzakLlmKullan, "Yerel/Uzak LLM API Kullan (Ollama / OpenAI uyumlu)");
            GUILayout.EndHorizontal();

            if (ajan.UzakLlmKullan)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Endpoint:", GUILayout.Width(70));
                ajan.ApiEndpoint = GUILayout.TextField(ajan.ApiEndpoint ?? "");
                GUILayout.Label("Model:", GUILayout.Width(50));
                ajan.ModelAdi = GUILayout.TextField(ajan.ModelAdi ?? "", GUILayout.Width(140));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(4);
            GUILayout.Label($"--- 1. Somut Maç Etkili Soyunma Odası Konuşmaları (Hedef: {_seciliOyuncu?.AdSoyad ?? "Seçilmedi"}) ---");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🎯 Taktik Odak (Karar+2, Vizyon+1)"))
            {
                ajan.SoyunmaOdasiGercekEtkiUygula(Plugin.VeriKoprusu, _seciliOyuncu, 0);
                if (_seciliOyuncu != null) OyuncuSec(_seciliOyuncu);
            }
            if (GUILayout.Button("🛡️ İstikrar Aşıla ('Halı Saha' Kır: Sürek+3)"))
            {
                ajan.SoyunmaOdasiGercekEtkiUygula(Plugin.VeriKoprusu, _seciliOyuncu, 1);
                if (_seciliOyuncu != null) OyuncuSec(_seciliOyuncu);
            }
            if (GUILayout.Button("🔥 Ültimatom Ver (Rehaveti Kır: ÖnMaç+2)"))
            {
                ajan.SoyunmaOdasiGercekEtkiUygula(Plugin.VeriKoprusu, _seciliOyuncu, 2);
                if (_seciliOyuncu != null) OyuncuSec(_seciliOyuncu);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label("--- 2. AI Scout, Futbol Direktörü & Basın Toplantısı Analizi ---");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"🧠 Oyuncu & İstikrar Raporu ({_seciliOyuncu?.AdSoyad ?? "Seçilmedi"})"))
            {
                ajan.OyuncuScoutAnaliziYap(_seciliOyuncu);
            }
            if (GUILayout.Button($"📊 Kulüp & Yaş Eğrisi Stratejisi ({_seciliKulup?.KulupAdi ?? "Seçilmedi"})"))
            {
                ajan.KulupStratejiAnaliziYap(_seciliKulup);
            }
            if (GUILayout.Button("🎙️ Basın Toplantısı Yanıtı Üret"))
            {
                ajan.BasinToplantisiYanitiUret(_seciliOyuncu, _seciliKulup, _basinSorusu);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label("Basın Toplantısı / Oyuncu Görüşmesi Gündemi:");
            _basinSorusu = GUILayout.TextField(_basinSorusu ?? "");

            GUILayout.Space(4);
            GUILayout.Label("AI Ajan & Soyunma Odası Çıktısı:");
            GUILayout.TextArea(ajan.SonRapor ?? "", GUILayout.Height(225));
        }

        // ==================== SEKME 5: GERİ AL (UNDO), HARMONYX KANCA & CANLI IL2CPP ====================

        private void UndoVeIl2CppSekmesiCiz()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(450));
            GUILayout.Label("=== Dinamik HarmonyX Maç, Ekonomi & Yaşayan Dünya Kancaları ===");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔌 IL2CPP Kancalarını Bağla"))
            {
                Plugin.YamaYoneticisi.DinamikKancalariTaraVeBagla();
                Plugin.VeriKoprusu.SonBilgiMesaji = Plugin.YamaYoneticisi.SonDurumMesaji;
            }
            if (GUILayout.Button("🧪 7 Kanca Simülasyon Testi"))
            {
                Plugin.YamaYoneticisi.KancaSimulasyonTestiCalistir();
                Plugin.VeriKoprusu.SonBilgiMesaji = Plugin.YamaYoneticisi.SonDurumMesaji;
            }
            GUILayout.EndHorizontal();

            if (Plugin.YamaYoneticisi != null)
            {
                foreach (var k in Plugin.YamaYoneticisi.Kancalar)
                {
                    GUILayout.Label($"• [{k.Kategori}] {k.KancaAdi} (Tetik:{k.TetiklenmeSayisi}) -> {k.SonIslemKaydi}");
                }
            }

            GUILayout.Space(6);
            GUILayout.Label($"=== Değişiklik Geçmişi ({Plugin.VeriKoprusu.GeriAlGecmisi.Count} kayıt) ===");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("↩ Son İşlemi Geri Al"))
            {
                Plugin.VeriKoprusu.SonIslemiGeriAl();
                if (_seciliOyuncu != null) OyuncuSec(_seciliOyuncu);
                if (_seciliKulup != null) KulupSec(_seciliKulup);
            }
            if (GUILayout.Button("🔄 Tümünü Geri Al"))
            {
                Plugin.VeriKoprusu.TumunuGeriAl();
                if (_seciliOyuncu != null) OyuncuSec(_seciliOyuncu);
                if (_seciliKulup != null) KulupSec(_seciliKulup);
            }
            GUILayout.EndHorizontal();

            _undoScroll = GUILayout.BeginScrollView(_undoScroll, GUILayout.Width(445), GUILayout.Height(165));
            foreach (var kayit in Plugin.VeriKoprusu.GeriAlGecmisi)
            {
                GUILayout.Label(kayit.ToString());
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUILayout.BeginVertical();
            GUILayout.Label("=== Canlı FM26 IL2CPP Sınıf Tarayıcı ===");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filtre:", GUILayout.Width(45));
            _il2cppFiltre = GUILayout.TextField(_il2cppFiltre ?? "");
            if (GUILayout.Button("🔍 Sınıfları Tara", GUILayout.Width(130)))
            {
                Plugin.VeriKoprusu.CanliIl2CppSiniflariniTara(_il2cppFiltre);
            }
            GUILayout.EndHorizontal();

            _il2cppScroll = GUILayout.BeginScrollView(_il2cppScroll, GUILayout.Height(390));
            foreach (var s in Plugin.VeriKoprusu.BulunanSiniflar.Take(60))
            {
                GUILayout.Label($"[{s.AssemblyAdi}] {s.TamAd} (Öz:{s.OzellikSayisi}, M:{s.MetotSayisi})");
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void OyuncuSec(OyuncuVerisi o)
        {
            _seciliOyuncu = o;
            if (o == null) return;
            _duzenCa = o.MevcutYetenekCA;
            _duzenPa = o.PotansiyelYetenekPA;
            _duzenBitiricilik = o.Bitiricilik;
            _duzenPas = o.Pas;
            _duzenTeknik = o.Teknik;
            _duzenKararAlma = o.KararAlma;
            _duzenVizyon = o.Vizyon;
            _duzenHiz = o.Hiz;
            _duzenDayaniklilik = o.Dayaniklilik;
            _duzenSakatlikEgilimi = o.SakatlikEgilimi;
            _duzenSureklilik = o.Sureklilik;
            _duzenOnemliMac = o.OnemliMacPerformansi;
        }

        private void KulupSec(KulupVerisi k)
        {
            _seciliKulup = k;
            if (k == null) return;
            _duzenBakiyeStr = k.BakiyeEuro.ToString();
            _duzenTransferStr = k.TransferButcesiEuro.ToString();
            _duzenMaasStr = k.MaasButcesiHaftalikEuro.ToString();
            _duzenItibar = k.Itibar;
            _duzenLigItibari = k.LigItibari;
            _duzenAntrenman = k.AntrenmanTesisleri;
            _duzenAltyapi = k.AltyapiTesisleri;
            _duzenAkademi = k.GenclikAkademisi;
            _duzenYonetimEsnekligi = k.YonetimTesisVeButceEsnekligi;
        }
    }
}
