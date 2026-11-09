using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FM26AgenticEditor.Models;

namespace FM26AgenticEditor.Services
{
    /// <summary>
    /// v0.5.0 "Yaşayan Futbol Dünyası ve Akıllı AI Direktör" Servisi:
    /// 1. Benfica Katsayısı (Dinamik İhracat & Piyasa Değeri Evrimi — Ülke sabit tavanını kırar)
    /// 2. Yükselen Dalga Lig Ekonomisi & Otonom Rakip Güçlendirici (Add Manager ihtiyacını bitirir)
    /// 3. AI Kadro Mühendisliği: 30+ Yaş Bonservis Tavanı (Harry Kane İstisnası) & Jenerasyon Yenileme
    /// 4. 20 Yıllık Yaşayan Dünya Evrimi (Yeni Yükselen Güçler, Yatırımcı Patlamaları, Kriz Döngüleri)
    /// 5. Kulüp Mirası (Legacy) & Somut Başarı Ödülü Sistemi
    /// </summary>
    public class YasayanDunyaVeAiEvrimServisi
    {
        private readonly List<string> _dunyaTarihcesi = new List<string>();
        public int AktifEvrimYili { get; private set; } = 2026;
        public string SonEvrimBulteni { get; private set; } =
            "Yaşayan Futbol Dünyası motoru hazır. Yukarıdaki butonlarla Benfica İhracat Katsayısını, Otonom Rakip Güçlendiriciyi veya Sezonluk Dünya Evrimini çalıştırabilirsiniz.";

        public IReadOnlyList<string> DunyaTarihcesi => _dunyaTarihcesi;

        /// <summary>
        /// Modül A: "Benfica Katsayısı" — Dinamik İhracat ve Piyasa Değeri Evrimi.
        /// Kulübün Avrupa başarısı, itibarı ve altyapı kalitesine göre oyuncuların piyasa değerini
        /// sabit ülke tavanından (25M€) kurtarıp Benfica/Ajax/Elit Avrupa piyasasına (75M-95M€) ölçekler.
        /// </summary>
        public string BenficaIhracatKatsayisiUygula(FmVeriKoprusu kopru, KulupVerisi referansKulup)
        {
            if (!CevrimiciKoruma.YazmaIzniVarMi())
            {
                SonEvrimBulteni = CevrimiciKoruma.DurumMesaji;
                return SonEvrimBulteni;
            }

            var kulup = referansKulup ?? kopru.Kulupler.FirstOrDefault();
            if (kulup == null) return "Kulüp bulunamadı.";

            float eskiCarpan = kulup.IhracatDegerCarpani;
            int eskiAvrupa = kulup.AvrupaBasariPuani;

            // Kulüp Avrupa'da başarılı oldukça ihracat çarpanı 1.0x -> 3.2x'e kadar yükselir
            kulup.AvrupaBasariPuani = Math.Clamp(kulup.AvrupaBasariPuani + 35, 10, 200);
            float itibarEtkisi = Math.Clamp((kulup.Itibar - 6500) / 1500f, 0.2f, 1.4f);
            float avrupaEtkisi = kulup.AvrupaBasariPuani / 120f;
            float altyapiEtkisi = kulup.AltyapiTesisleri / 25f;

            float yeniCarpan = (float)Math.Round(Math.Clamp(1.0f + itibarEtkisi + avrupaEtkisi + altyapiEtkisi, 1.15f, 3.20f), 2);
            kulup.IhracatDegerCarpani = yeniCarpan;

            int guncellenenOyuncu = 0;
            var sb = new StringBuilder();
            sb.AppendLine($"=== 🦅 BENFİCA İHRACAT KATSAYISI & PİYASA DEĞERİ EVRİMİ ({kulup.KulupAdi}) ===");
            sb.AppendLine($"• Avrupa Başarı Puanı: {eskiAvrupa} -> {kulup.AvrupaBasariPuani}/200");
            sb.AppendLine($"• Dinamik İhracat Değer Çarpanı: {eskiCarpan:0.00}x -> {yeniCarpan:0.00}x (Ülke Sabit Tavanı Kırıldı!)");
            sb.AppendLine();

            foreach (var o in kopru.Oyuncular)
            {
                // Genç ve yüksek potansiyelli oyuncuların piyasa değerini Avrupa ihracat standardına taşı
                long eskiDeger = o.PiyasaDegeriEuro;
                long eskiSerbest = o.SerbestKalmaBedeliEuro;

                double yasAvantaji = o.Yas <= 23 ? 1.35 : (o.Yas <= 27 ? 1.15 : 0.75);
                double yetenekBaz = Math.Max(10_000_000, (o.MevcutYetenekCA - 110) * 650_000L + (o.PotansiyelYetenekPA - 130) * 750_000L);
                long yeniDeger = (long)(Math.Round((yetenekBaz * yasAvantaji * (yeniCarpan / 1.6f)) / 500_000.0) * 500_000);
                yeniDeger = Math.Clamp(yeniDeger, 3_500_000L, 145_000_000L);

                // 30+ yaş ve Harry Kane istisnası olmayanlarda ise değeri şişirme
                if (o.Yas >= 30 && !o.HarryKaneIstisnasiMi)
                    yeniDeger = Math.Min(yeniDeger, 9_500_000L);

                o.PiyasaDegeriEuro = yeniDeger;
                if (o.SerbestKalmaBedeliEuro > 0 && o.SerbestKalmaBedeliEuro < yeniDeger * 1.4)
                    o.SerbestKalmaBedeliEuro = (long)(yeniDeger * 1.5);

                kopru.KayitEkle("Benfica Katsayısı", o.AdSoyad, "Dinamik Piyasa Değeri",
                    $"{eskiDeger:N0}€", $"{yeniDeger:N0}€ ({yeniCarpan:0.00}x)", () =>
                    {
                        o.PiyasaDegeriEuro = eskiDeger;
                        o.SerbestKalmaBedeliEuro = eskiSerbest;
                        kulup.IhracatDegerCarpani = eskiCarpan;
                        kulup.AvrupaBasariPuani = eskiAvrupa;
                    });

                sb.AppendLine($"  • {o.AdSoyad} ({o.Yas} yaş, CA:{o.MevcutYetenekCA}/PA:{o.PotansiyelYetenekPA}): {eskiDeger / 1_000_000.0:0.0}M€ -> {yeniDeger / 1_000_000.0:0.0}M€");
                guncellenenOyuncu++;
            }

            SonEvrimBulteni = sb.ToString().Trim();
            kopru.SonBilgiMesaji = $"Benfica İhracat Katsayısı ({yeniCarpan:0.00}x) uygulandı; {guncellenenOyuncu} oyuncunun piyasa değeri Avrupa standardına ölçeklendi.";
            return SonEvrimBulteni;
        }

        /// <summary>
        /// Modül B: "Yükselen Dalga (Trickle-Down League)" & Otonom Rakip Güçlendirici (Anti-Add-Manager).
        /// Sizin başarılarınızla lig büyüdüğünde rakip takımların batmasını ve 2. lige düşmesini engeller;
        /// rakiplere TV/Şampiyonlar Ligi havuz payı, tesis modernizasyonu ve rekabetçi bütçe aktarır.
        /// </summary>
        public string YukselenDalgaRakipGuclendiriciCalistir(FmVeriKoprusu kopru)
        {
            if (!CevrimiciKoruma.YazmaIzniVarMi())
            {
                SonEvrimBulteni = CevrimiciKoruma.DurumMesaji;
                return SonEvrimBulteni;
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== 🌊 YÜKSELEN DALGA & OTONOM RAKİP GÜÇLENDİRİCİ (ANTI-ADD-MANAGER) ===");
            sb.AppendLine("Ligin Avrupa'daki yükselişi tüm lig ekosistemine ve ezeli rakiplere yansıtıldı:");
            sb.AppendLine();

            int enYuksekItibar = kopru.Kulupler.Max(k => k.Itibar);
            int hedefLigItibari = Math.Clamp(kopru.Kulupler.Max(k => k.LigItibari) + 250, 7400, 8800);

            foreach (var k in kopru.Kulupler)
            {
                long eBakiye = k.BakiyeEuro;
                long eTransfer = k.TransferButcesiEuro;
                long eSponsor = k.SponsorlukGeliriYillikEuro;
                int eItibar = k.Itibar;
                int eLig = k.LigItibari;
                int eAnt = k.AntrenmanTesisleri;
                int eAlt = k.AltyapiTesisleri;
                string eDurum = k.DunyaDurumEtiketi;

                // Lider kulübün çok gerisinde kalan rakiplere daha güçlü "Rekabet Tepkisi" uygula
                int itibarFarki = Math.Max(0, enYuksekItibar - k.Itibar);
                int itibarArtisi = Math.Clamp(150 + (itibarFarki / 3), 150, 450);

                k.LigItibari = hedefLigItibari;
                k.Itibar = Math.Clamp(k.Itibar + itibarArtisi, 6800, 9200);
                k.BakiyeEuro = Math.Max(k.BakiyeEuro + 22_000_000L, 35_000_000L);
                k.TransferButcesiEuro = Math.Max(k.TransferButcesiEuro + 18_000_000L, 25_000_000L);
                k.SponsorlukGeliriYillikEuro += 12_000_000L;
                k.AntrenmanTesisleri = Math.Clamp(k.AntrenmanTesisleri + 1, 16, 20);
                k.AltyapiTesisleri = Math.Clamp(k.AltyapiTesisleri + 1, 16, 20);
                k.YonetimTesisVeButceEsnekligi = Math.Max(k.YonetimTesisVeButceEsnekligi, 17);
                k.DunyaDurumEtiketi = "Şampiyonluk Rakibi (Güçlendirildi) 🔥";

                kopru.KayitEkle("Yükselen Dalga", k.KulupAdi, "Otonom Rakip Güçlendirme",
                    $"İtibar:{eItibar} Bütçe:{eTransfer / 1_000_000.0:0}M€",
                    $"İtibar:{k.Itibar} Bütçe:{k.TransferButcesiEuro / 1_000_000.0:0}M€", () =>
                    {
                        k.BakiyeEuro = eBakiye; k.TransferButcesiEuro = eTransfer; k.SponsorlukGeliriYillikEuro = eSponsor;
                        k.Itibar = eItibar; k.LigItibari = eLig; k.AntrenmanTesisleri = eAnt; k.AltyapiTesisleri = eAlt;
                        k.DunyaDurumEtiketi = eDurum;
                    });

                sb.AppendLine($"  • {k.KulupAdi}: İtibar {eItibar}->{k.Itibar} | Transfer Bütçesi: {eTransfer / 1_000_000.0:0.#}M€ -> {k.TransferButcesiEuro / 1_000_000.0:0.#}M€ | Tesisler: {k.AntrenmanTesisleri}/{k.AltyapiTesisleri}");
            }

            SonEvrimBulteni = sb.ToString().Trim();
            kopru.SonBilgiMesaji = "Otonom Rakip Güçlendirici çalıştı: Ligdeki tüm rakipler mali krizden korundu ve rekabetçi bütçeye kavuştu.";
            return SonEvrimBulteni;
        }

        /// <summary>
        /// Modül C: AI Kadro Mühendisliği 2.0 — 30+ Yaş Bonservis Tavanı (Harry Kane İstisnası) & Jenerasyon Yenileme.
        /// </summary>
        public string AiKadroMuhendisligiVeYasTavaniniCalistir(FmVeriKoprusu kopru)
        {
            if (!CevrimiciKoruma.YazmaIzniVarMi())
            {
                SonEvrimBulteni = CevrimiciKoruma.DurumMesaji;
                return SonEvrimBulteni;
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== 🧠 AI FUTBOL DİREKTÖRÜ 2.0: 30+ YAŞ BONSERVİS FRENİ & JENERASYON YENİLEME ===");

            int frenlenenYasliSayisi = 0;
            foreach (var o in kopru.Oyuncular.Where(x => x.Yas >= 30))
            {
                if (o.HarryKaneIstisnasiMi)
                {
                    sb.AppendLine($"  • [Harry Kane İstisnası] {o.AdSoyad} ({o.Yas} yaş, CA:{o.MevcutYetenekCA}): Dünya yıldızı statüsünde, değeri korundu ({o.PiyasaDegeriEuro:N0}€).");
                    continue;
                }

                long eskiDeger = o.PiyasaDegeriEuro;
                long tavanDeger = Math.Min(eskiDeger, 8_500_000L);
                o.PiyasaDegeriEuro = tavanDeger;

                kopru.KayitEkle("AI Kadro Mühendisliği", o.AdSoyad, "30+ Yaş Bonservis Tavanı",
                    $"{eskiDeger:N0}€", $"{tavanDeger:N0}€", () => o.PiyasaDegeriEuro = eskiDeger);

                sb.AppendLine($"  • [30+ Yaş Tavanı] {o.AdSoyad} ({o.Yas} yaş, CA:{o.MevcutYetenekCA}): AI kulüplerin fahiş bonservis gömmemesi için tavan {tavanDeger / 1_000_000.0:0.0}M€ yapıldı.");
                frenlenenYasliSayisi++;
            }

            foreach (var k in kopru.Kulupler)
            {
                float eskiYas = k.KadroYasOrtalamasi;
                k.KadroYasOrtalamasi = 24.4f;
                k.DunyaDurumEtiketi = "Jenerasyon Yenilendi (Wonderkid Odaklı) 🌱";
                sb.AppendLine($"  • [{k.KulupAdi}] AI Transfer Politikası: Yaşlı oyuncu alımı kapatıldı -> 18-23 yaş (PA 160+) jenerasyon yenileme modu aktif (Hedef Yaş Ort: {k.KadroYasOrtalamasi:0.0}).");
            }

            SonEvrimBulteni = sb.ToString().Trim();
            kopru.SonBilgiMesaji = $"AI Kadro Mühendisliği tamamlandı: {frenlenenYasliSayisi} adet 30+ yaş oyuncuya bonservis tavanı getirildi, AI kulüpleri gençleştirme moduna geçirildi.";
            return SonEvrimBulteni;
        }

        /// <summary>
        /// Modül D: "20 Yıllık Yaşayan Dünya Simülatörü" — Her çalıştırıldığında futbol dünyasını 1 sezon evrimleştirir:
        /// Yeni süper kulüpler doğar, bazı kulüpler vizyoner yatırımcı alır, bazıları kemer sıkar, güç dengeleri değişir!
        /// </summary>
        public string YirmiYillikDunyaEvrimiIlerlet(FmVeriKoprusu kopru)
        {
            if (!CevrimiciKoruma.YazmaIzniVarMi())
            {
                SonEvrimBulteni = CevrimiciKoruma.DurumMesaji;
                return SonEvrimBulteni;
            }

            AktifEvrimYili++;
            var sb = new StringBuilder();
            sb.AppendLine($"=== 🌍 YAŞAYAN FUTBOL DÜNYASI BÜLTENİ: {AktifEvrimYili - 1}/{AktifEvrimYili} SEZONU ===");

            // Yeni yükselen Anadolu / Avrupa kulüplerini havuza ekle veya mevcutları dönüştür
            string[] yeniGucler = { "Göztepe", "Bursaspor", "Samsunspor", "Eskişehirspor", "Parma", "Deportivo La Coruna", "Hamburger SV" };
            string secilenYeniGuc = yeniGucler[(AktifEvrimYili - 2026) % yeniGucler.Length];

            var mevcutKulup = kopru.Kulupler.FirstOrDefault(k => k.KulupAdi.Equals(secilenYeniGuc, StringComparison.OrdinalIgnoreCase));
            if (mevcutKulup == null)
            {
                var yeniKulup = new KulupVerisi
                {
                    Id = 300 + AktifEvrimYili,
                    KulupAdi = secilenYeniGuc,
                    UlkeAdi = secilenYeniGuc is "Parma" or "Deportivo La Coruna" or "Hamburger SV" ? "Avrupa" : "Türkiye",
                    LigAdi = secilenYeniGuc is "Parma" or "Deportivo La Coruna" or "Hamburger SV" ? "Avrupa Elit" : "Süper Lig",
                    LigItibari = 7850,
                    Itibar = 7550,
                    AvrupaBasariPuani = 80,
                    IhracatDegerCarpani = 2.15f,
                    KadroYasOrtalamasi = 23.1f,
                    BakiyeEuro = 68_000_000L,
                    TransferButcesiEuro = 45_000_000L,
                    MaasButcesiHaftalikEuro = 1_450_000L,
                    SponsorlukGeliriYillikEuro = 38_000_000L,
                    AntrenmanTesisleri = 18,
                    AltyapiTesisleri = 18,
                    GenclikAkademisi = 19,
                    YonetimTesisVeButceEsnekligi = 19,
                    DunyaDurumEtiketi = "Yeni Yükselen Süper Güç (Moneyball & Yatırımcı) 🚀"
                };
                kopru.YeniKulupEkle(yeniKulup);
                sb.AppendLine($"🚀 YENİ GÜÇ MERKEZİ DOĞDU: {secilenYeniGuc}, vizyoner futbol direktörü ve 45M€ transfer bütçesiyle zirve yarışına katıldı!");
            }

            // Mevcut kulüpler arasında dinamik dalgalanma (Yükseliş vs Mali Yeniden Yapılanma)
            int indeks = 0;
            foreach (var k in kopru.Kulupler)
            {
                indeks++;
                if (k.KulupAdi == secilenYeniGuc) continue;

                if ((AktifEvrimYili + indeks) % 4 == 0)
                {
                    // Kötü yönetim / mali kemer sıkma döngüsü (ancak batmadan, gerçekçi yeniden yapılanma)
                    k.TransferButcesiEuro = Math.Max(12_000_000L, (long)(k.TransferButcesiEuro * 0.78));
                    k.DunyaDurumEtiketi = "Mali Kemer Sıkma & Altyapıya Dönüş ⚠️";
                    sb.AppendLine($"📉 MALİ DÖNGÜ: {k.KulupAdi} yüksek maaş yükü nedeniyle bu sezon altyapı özkaynak modeline geçti (Bütçe: {k.TransferButcesiEuro / 1_000_000.0:0.#}M€).");
                }
                else
                {
                    // Büyüme ve tesis yatırımı
                    k.Itibar = Math.Clamp(k.Itibar + 120, 6800, 9400);
                    k.TransferButcesiEuro += 14_000_000L;
                    k.IhracatDegerCarpani = (float)Math.Round(Math.Clamp(k.IhracatDegerCarpani + 0.15f, 1.0f, 3.2f), 2);
                    k.DunyaDurumEtiketi = "Yükselişte (Avrupa Odaklı) 📈";
                    sb.AppendLine($"📈 REKABET ARTIŞI: {k.KulupAdi} itibarını {k.Itibar} puana, ihracat çarpanını {k.IhracatDegerCarpani:0.00}x seviyesine çıkardı.");
                }
            }

            string özet = sb.ToString().Trim();
            _dunyaTarihcesi.Insert(0, $"[{AktifEvrimYili}] {secilenYeniGuc} yeni güç merkezi oldu; lig dengeleri güncellendi.");
            SonEvrimBulteni = özet;
            kopru.SonBilgiMesaji = $"{AktifEvrimYili} sezonu dünya evrimi tamamlandı: {secilenYeniGuc} yükselişe geçti, güç dengeleri yenilendi.";
            return SonEvrimBulteni;
        }

        /// <summary>
        /// Modül E: Kulüp Mirası (Legacy) & Somut Başarı Hissiyatı Ödülü.
        /// "5. ligden takımla CL şampiyonu bile olsanız başarma hissiyatı vermiyor" eleştirisini çözer:
        /// Kazanılan zaferi kulübün kalıcı genetiğine (Miras Seviyesi, Altyapı Çekim Gücü, Sponsorluk Sınıfı ve İhracat Çarpanı) işler.
        /// </summary>
        public string KulupMirasiZaferiIslet(FmVeriKoprusu kopru, KulupVerisi kulup, string basariAdi)
        {
            if (kulup == null || !CevrimiciKoruma.YazmaIzniVarMi())
                return CevrimiciKoruma.DurumMesaji;

            int eMiras = kulup.KulupMirasSeviyesi;
            int eItibar = kulup.Itibar;
            long eSponsor = kulup.SponsorlukGeliriYillikEuro;
            long eTransfer = kulup.TransferButcesiEuro;
            float eCarpan = kulup.IhracatDegerCarpani;

            kulup.KulupMirasSeviyesi = Math.Clamp(kulup.KulupMirasSeviyesi + 2, 1, 10);
            kulup.Itibar = Math.Clamp(kulup.Itibar + 450, 1, 9600);
            kulup.LigItibari = Math.Clamp(kulup.LigItibari + 200, 1, 9200);
            kulup.AvrupaBasariPuani = Math.Clamp(kulup.AvrupaBasariPuani + 45, 1, 200);
            kulup.IhracatDegerCarpani = (float)Math.Round(Math.Clamp(kulup.IhracatDegerCarpani + 0.45f, 1.2f, 3.2f), 2);
            kulup.SponsorlukGeliriYillikEuro += 25_000_000L;
            kulup.TransferButcesiEuro += 35_000_000L;
            kulup.BakiyeEuro += 40_000_000L;
            kulup.AltyapiTesisleri = Math.Clamp(kulup.AltyapiTesisleri + 2, 1, 20);
            kulup.GenclikAkademisi = Math.Clamp(kulup.GenclikAkademisi + 2, 1, 20);
            kulup.YonetimTesisVeButceEsnekligi = 20;
            kulup.DunyaDurumEtiketi = $"Avrupa Hanedanı (Miras Seviye {kulup.KulupMirasSeviyesi}/10) 🏆";

            kopru.KayitEkle("Kulüp Mirası", kulup.KulupAdi, basariAdi,
                $"Miras:{eMiras} İtibar:{eItibar} Çarpan:{eCarpan:0.00}x",
                $"Miras:{kulup.KulupMirasSeviyesi} İtibar:{kulup.Itibar} Çarpan:{kulup.IhracatDegerCarpani:0.00}x", () =>
                {
                    kulup.KulupMirasSeviyesi = eMiras;
                    kulup.Itibar = eItibar;
                    kulup.SponsorlukGeliriYillikEuro = eSponsor;
                    kulup.TransferButcesiEuro = eTransfer;
                    kulup.IhracatDegerCarpani = eCarpan;
                });

            SonEvrimBulteni =
                $"=== 🏆 TARİHİ KULÜP MİRASI TESCİLLENDİ: {kulup.KulupAdi.ToUpperInvariant()} ({basariAdi}) ===\n" +
                $"• Kulüp Miras Seviyesi: {eMiras}/10 -> {kulup.KulupMirasSeviyesi}/10 (Kalıcı Tarihsel Prestij)\n" +
                $"• Küresel İtibar: {eItibar} -> {kulup.Itibar} | Lig İtibarı: {kulup.LigItibari}\n" +
                $"• Benfica İhracat Çarpanı: {eCarpan:0.00}x -> {kulup.IhracatDegerCarpani:0.00}x (Artık oyuncularınız Avrupa devleri fiyatına satılır!)\n" +
                $"• Kalıcı Gelir & Ödül: +25M€/yıl yeni küresel sponsorluk, +35M€ transfer bütçesi, Gençlik Akademisi: {kulup.GenclikAkademisi}/20!";

            kopru.SonBilgiMesaji = $"{kulup.KulupAdi} için '{basariAdi}' mirası kalıcı olarak kulüp ve lig ekonomisine işlendi!";
            return SonEvrimBulteni;
        }
    }
}
