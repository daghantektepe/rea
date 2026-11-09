# FM26 In-Game Editor, Yaşayan Futbol Dünyası & Agentic FM (v0.5.0)

Football Manager 26 (Unity 6 / IL2CPP) için **BepInEx 6 IL2CPP** tabanlı oyun içi gerçek zamanlı veri düzenleyici, **20 Yıllık Yaşayan Futbol Dünyası & AI Kadro Mühendisliği** motoru, sistemik simülasyon rehabilitasyon paketi ve yapay zekâ (Agentic FM) eklentisi.

## Kısayollar

- **`F8`**: 6 Sekmeli Ana Oyun İçi Editör, Yaşayan Dünya, Rehabilitasyon & Agentic FM penceresini açar/kapatır.
- **`F9`**: **Masaüstü Oyuncu Röntgeni (Mini-HUD Overlay)** şeridini açar/kapatır (Seçili oyuncunun **Boy, Kilo, Ayak, Forma No, CA/PA, Piyasa Değeri, Tutarlılık İndeksi ["1 Maç Messi / 1 Maç Halı Saha" Röntgeni], Sakatlık Eğilimi ve Duran Top Hava Hakimiyeti Puanını** ekranın üst kısmında gösterir).

## Özellikler (6 Ana Sekme)

1. **`[1] Oyuncu & Röntgen` (FMRTE + İstikrar Röntgeni):**
   - FM26 kart arayüzünde gizlenen **Boy, Kilo, Ayak, Serbest Kalma Bedeli, Sakatlık Eğilimi (`1–20`), Süreklilik (`1–20`), Önemli Maç Performansı (`1–20`), Maç Kondisyonu (`Sharpness`)** ve **Tutarlılık İndeksi (`0–100`)** görünümü ve düzenleyicisi.
   - **CA / PA (`1–200`)** ve temel özellikler (`1–20`) canlı düzenleme.

2. **`[2] Kulüp & Süper Lig` (Ekonomi & Yerel Gerçekçilik):**
   - **🇹🇷 Tek Tıkla Türkiye Süper Lig Gerçekçilik & İtibar Kalibrasyonu:** Süper Lig itibarını **Avrupa İlk 8 (`7850`)** seviyesine, GS/FB/BJK/TS itibarlarını ve yönetim tesis/bütçe esnekliğini (`18/20`) kalibre eder.

3. **`[3] 🌍 Yaşayan Dünya` (Dinamik Futbol Dünyası & Akıllı AI Direktör — v0.5.0):**
   - **🦅 Benfica İhracat Katsayısı (`1.0x -> 3.2x`):** Kulübünüz Avrupa'da başarılı oldukça ülke sabit değer tavanını (`25M €`) kırar; geliştirdiğiniz genç yeteneklerin piyasa değerini Benfica/Ajax/Elit Avrupa seviyesine (`75M–95M €`) ölçekler.
   - **🌊 Yükselen Dalga & Otonom Rakip Güçlendirici (`Anti-Add-Manager`):** Ligin itibarı arttıkça ligdeki diğer takımların da yayın/sponsorluk gelirlerini, tesislerini ve bütçelerini büyütür; ezeli rakiplerin batıp 2. lige düşmesini engeller.
   - **🧠 AI Kadro Mühendisliği 2.0 (30+ Yaş Bonservis Tavanı & Jenerasyon Yenileme):**
     - `30+ yaş` ve `CA < 172` olan oyunculara (Harry Kane sınıfı dünya yıldızları hariç) AI kulüplerinin `8.5M €` üstü bonservis gömmesini engeller.
     - Kadro yaş ortalaması `28.5` üstüne çıkan dev kulüpleri zorunlu olarak `18–23 yaş` wonderkid transferine yönlendirir.
   - **🌍 20 Yıllık Dünya Evrimi Simülatörü:** Her sezon yeni yükselen güç merkezleri (Göztepe, Bursaspor, Samsunspor, Parma, Deportivo vb.) ve gerçekçi mali döngüler üreterek 20 sezon boyunca değişen bir futbol dünyası kurar.
   - **🏆 Kulüp Mirası & Başarma Hissiyatı Ödülü:** Kazanılan Şampiyonlar Ligi / Avrupa zaferlerini kulübün kalıcı Miras Seviyesine (`1–10`), altyapı akademisine, sponsorluk sınıfına ve ihracat çarpanına işler.

4. **`[4] Rehabilitasyon & Paket` (Tıbbi Denge, Yağmur, Duran Top & Harici Paket Üretici):**
   - **Tıbbi Kalkan (`-%32` sakatlık indirimi, kaleci koruması, mikro sakatlık kondisyon koruyucu)**, **Aralıksız Yağmur Düzeltici**, **Fahiş Bonservis (`User Tax`) Dengeleyici**, **Suudi Arabistan İsyan Filtresi**, **Duran Top Forma No Rehberi** ve tek tıkla **Masaüstü Antrasit Skin / Editor Data XML / AI Regen `config.xml` / 0ms CSV Tablo** üretici.

5. **`[5] Agentic & Soyunma Odası` (Somut Maç Etkili Konuşmalar & AI Scout):**
   - **Somut Maç Etkili Soyunma Odası Konuşmaları:** "Hadi aslanım" yerine oyuncunun `Karar Alma (+2)`, `Süreklilik (+3)`, `Önemli Maç Performansı (+2)` ve `Moral` özelliklerine doğrudan etki eden 3 taktiksel/psikolojik müdahale.
   - **"1 Maç Messi / 1 Maç Halı Saha" Teşhis Motoru** ve LLM destekli basın toplantısı ajanı.

6. **`[6] Undo & Kanca`:**
   - **Tam Geri Al (Undo) Yığını**, **7 Dinamik HarmonyX Kancası** ve **Canlı IL2CPP Sınıf Tarayıcı**.

## Derleme

```powershell
cd E:\rea\kisisel\oyun-kiti\fm26-agentic-editor
dotnet build -c Release
```
