---
name: oyun-mod
description: Kullanıcının bilgisayarındaki tek oyunculu bir oyunun motorunu REA ile tespit et, oyun kodunu ve verisini incele, uygun mod altyapısını (BepInEx, MelonLoader, Godot mod yükleyici vb.) seç ve ilk modu birlikte yaz. "Şu oyuna mod yapmak istiyorum", "oyunda şu değeri değiştirmek istiyorum", "/oyun-mod" denildiğinde kullan.
---

# Oyun modu

## Kapsam (önce kontrol et)

Destekle: tek oyunculu veya çevrimdışı modlar, kullanıcının kendi kaydı ve verisi; arayüz, denge, içerik, erişilebilirlik ve çeviri modları; kullanıcının sahip olduğu kopya.

Yardım etme:

- Çok oyunculu veya çevrimiçi modlarda avantaj sağlama.
- Anti-cheat'i atlatma.
- DRM, lisans kontrolü veya anti-tamper korumasını atlatma (Denuvo, Steam DRM yaması vb.).
- Oyunun kodunu veya varlıklarını yeniden dağıtma.

Oyunda böyle bir koruma tespit edersen modu ona dokunmadan tasarla. Mümkün değilse bunu açıkça söyle.

## 1. Motoru tespit et

`inspect_desktop_application` ile oyun klasörüne bak. İpuçları:

| Görülen                                                                | Motor          | REA aracı                                             |
| ---------------------------------------------------------------------- | -------------- | ----------------------------------------------------- |
| `*_Data/Managed/Assembly-CSharp.dll`                                   | Unity (Mono)   | `inspect_managed_artifact`, `inspect_managed_members` |
| `GameAssembly.dll` + `*_Data/il2cpp_data/Metadata/global-metadata.dat` | Unity (IL2CPP) | `inspect_il2cpp_metadata`; derin kod için Ghidra      |
| `.pck` dosyası veya exe'ye gömülü PCK                                  | Godot          | `inspect_artifact`, `extract_artifact`                |
| `*.pak` / `Engine/` klasörü                                            | Unreal         | `inspect_artifact`; kod için native analiz            |
| Başka bir native exe                                                   | Özel motor     | `open_binary` + Ghidra                                |

Unity sürümünü (`UnityPlayer.dll` sürüm bilgisi veya `globalgamemanagers`) ve mimariyi not et.

## 2. Değiştirilecek yeri bul

Kullanıcının isteğini somut bir **iz**e çevir: ekranda görünen metin, ayar adı, sınıf adı tahmini (`Player`, `Inventory`, `Transfer`, `Finance`...).

- Mono: `inspect_managed_members` ile tür ve metotları ara, `ldstr` metinleriyle UI metnine bağla.
- IL2CPP: `inspect_il2cpp_metadata` ile tür ve metot adlarını çıkar. Gövde için Ghidra gerekir.
- Bulduğun 1–3 aday metodu kanıtlarıyla göster ve hangisini yamalayacağını kullanıcıyla netleştir.

## 3. Altyapıyı seç ve modu yaz

- Unity Mono → BepInEx 5 + HarmonyX. Şablon: `~/rea/kisisel/oyun-kiti/unity-bepinex-mono/`.
- Unity IL2CPP → BepInEx 6 (IL2CPP) veya MelonLoader. Şablon: `~/rea/kisisel/oyun-kiti/unity-bepinex-il2cpp/`.
- Godot → Oyunun mod desteği varsa onu kullan. Yoksa çıkarılan scriptleri inceleyip bir yama paketi hazırla.
- Veri dosyası tabanlı oyunlar (JSON, XML, SQLite, ini) → Doğrudan veri düzenleme aracı yaz; kod yamasından her zaman daha basittir.

Modu yazarken:

- Kurulumdan önce oyun klasörünün ve kayıtların yedeğini aldır.
- Önce en küçük doğrulanabilir değişikliği yap (ör. log satırı veya tek bir değer). Sonra büyüt.
- Ayarları mod konfigürasyonuna koy (BepInEx `Config.Bind`).
- Oyun güncellemesinde nelerin bozulabileceğini README'ye yaz.

## 4. Teslim

Mod projesi, derleme komutu, kurulum adımları, kaldırma adımları ve bilinen sınırlar.
