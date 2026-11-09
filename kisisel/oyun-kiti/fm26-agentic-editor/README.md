# FM26 In-Game Editor, Topluluk Rehabilitasyon Paketi & Agentic FM (v0.3.0)

Football Manager 26 (Unity 6 / IL2CPP) için **BepInEx 6 IL2CPP** tabanlı oyun içi gerçek zamanlı veri düzenleyici, sistemik simülasyon rehabilitasyon paketi ve yapay zekâ (Agentic FM) eklentisi.

## Kısayollar

- **`F8`**: 5 Sekmeli Ana Oyun İçi Editör, Rehabilitasyon & Agentic FM penceresini açar/kapatır.
- **`F9`**: **Masaüstü Oyuncu Röntgeni (Mini-HUD Overlay)** şeridini açar/kapatır (Ana pencere kapalıyken bile seçili oyuncunun **Boy, Kilo, Ayak, Forma No, CA/PA, Kondisyon, Maç Kondisyonu, Sakatlık Eğilimi, Süreklilik, Serbest Kalma Bedeli ve Duran Top Hava Hakimiyeti Puanını** ekranın üst kısmında gösterir).

## Özellikler (5 Ana Sekme)

1. **`[1] Oyuncu & Röntgen` (FMRTE + Masaüstü Ergonomi):**
   - İsim, kulüp, pozisyon, uyruk ve **forma numarası** filtreleme.
   - FM26 kart arayüzünde alt katmanlara gizlenen **Boy, Kilo, Tercih Edilen Ayak, Serbest Kalma Bedeli, Sakatlık Eğilimi (`1–20`), Süreklilik (`1–20`), Önemli Maç Performansı (`1–20`) ve Maç Kondisyonu (Sharpness)** görünümü ve düzenleyicisi.
   - **CA / PA (`1–200`)** ve temel özellikler (`1–20`) canlı düzenleme.
   - Tek tıkla **⚡ Kondisyon & Maç Ritmi (%100) + Moral (20/20) + Sakatlık İyileştir**.

2. **`[2] Kulüp & Süper Lig` (Ekonomi & Yerel Gerçekçilik):**
   - **🇹🇷 Tek Tıkla Türkiye Süper Lig Gerçekçilik & İtibar Kalibrasyonu:**
     - Süper Lig itibarını **Avrupa İlk 8 (`7850`)** seviyesine çıkarır.
     - Galatasaray, Fenerbahçe, Beşiktaş ve Trabzonspor'un küresel itibarlarını, sponsorluk gelirlerini ve **Yönetim Tesis/Bütçe Onaylama Esnekliğini (`18/20`)** kalibre eder.
     - TFF yabancı kuralı (A Takım 14 / İlk 11) parametrelerini eşitler.
   - Kulüp Bakiyesi, Transfer Bütçesi, Maaş Bütçesi, İtibar (`1–10000`) ve Tesis (`1–20`) düzenleme.

3. **`[3] Rehabilitasyon & Maç Motoru` (Topluluk Eleştirileri Çözüm Merkezi):**
   - **🏥 Tıbbi Denge & Mikro-Sakatlık Kalkanı:**
     - Küresel Sakatlık Olasılık Çarpanı (`%50–%100`, varsayılan `%68` = `-%32` azaltma).
     - Kalecilerin temassız mikro sakatlıklarını (`1–12 gün`) otomatik iptal etme.
     - Kısa süreli mikro sakatlıklar sonrası **Maç Kondisyonunun (Match Sharpness) sıfırlanmasını önleme (`%90` koruma)**.
   - **🌧️ Maç Motoru & Çevre Yamaları:**
     - Aralıksız yağmur hatasını lig coğrafyasına göre dengeleme (`Weather Fix`).
     - Santra vuruşu sonrası savunma uyuşukluğu önleyici.
     - Klasik FM24 `Space` (Boşluk) tuşu davranışı ve oynanmayan arka plan/kadın liglerinin simülasyon yükünü azaltıcı (`Lite DB`).
   - **🛡️ Transfer & Diyalog Mantığı:**
     - AI kulüplerinin kullanıcıdan `80–100M €` istediği fahiş bonservis çarpanını (`User Tax`) dengeleme.
     - Yeni transferlerin (`<12 ay`) gerçek dışı Suudi Arabistan/transfer isyanlarını tek tuşla temizleme.
   - **🎯 Duran Top Forma No & Hava Hakimiyeti Rehberi:**
     - Duran top ekranında sadece forma numarası görünen oyuncular için **`Forma #No -> İsim + Boy + Zıplama + Kafa + Hava Hakimiyeti Puanı (0-100)`** tablosu.
   - **🖼️ AI Regen (Newgen) Yüz Paketi Eşleştirici (`config.xml`):**
     - Bozuk 3D regen yüzleri yerine oyundaki Newgen ID ve etnik bölgelerine göre `graphics/pictures/person/` altında çalışan `config.xml` dosyasını oyun içinden tek tıkla üretir.

4. **`[4] Agentic FM (AI)`:**
   - Seçili oyuncunun CA/PA ve özellik dağılımına göre **FM26 Rol & Bireysel Antrenman Analizi**.
   - Seçili kulübün bütçe ve tesislerine göre **Futbol Direktörü (DoF) Strateji Raporu**.
   - **Basın Toplantısı & Oyuncu Görüşmesi Diyalog Üretici** (Yerleşik motor veya `Ollama` / OpenAI uyumlu yerel LLM ile).

5. **`[5] Geri Al (Undo) & Canlı IL2CPP Keşif`:**
   - **Tam Geri Al (Undo) Yığını:** Yapılan her düzenleme (Süper Lig paketi ve Tıbbi Kalkan dahil) önceki değeriyle kaydedilir; tek tuşla geri alınır.
   - **Canlı IL2CPP Sınıf Tarayıcı:** Oyun içindeyken FM26 belleğindeki gerçek `Player`, `Club`, `Injury`, `Weather`, `PressConference` sınıflarını tarar ve `BepInEx/LogOutput.log` dosyasına yazar.

## Derleme

```powershell
cd E:\rea\kisisel\oyun-kiti\fm26-agentic-editor
dotnet build -c Release
```

FM26 ve BepInEx 6 IL2CPP kurulduktan sonra:

```powershell
dotnet build -c Release -p:OyunKlasoru="C:\Program Files (x86)\Steam\steamapps\common\Football Manager 26"
```
