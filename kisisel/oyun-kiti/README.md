# Oyun mod kiti

Tek oyunculu Unity oyunları için iki başlangıç şablonu. Bunları Claude Code'daki
`/oyun-mod` ve `/oyun-editoru` iş akışları kullanır.

| Klasör                  | Ne zaman                                                             | Altyapı              |
| ----------------------- | -------------------------------------------------------------------- | -------------------- |
| `unity-bepinex-mono/`   | Oyun klasöründe `*_Data/Managed/Assembly-CSharp.dll` var             | BepInEx 5 + HarmonyX |
| `unity-bepinex-il2cpp/` | `GameAssembly.dll` ve `il2cpp_data/Metadata/global-metadata.dat` var | BepInEx 6 IL2CPP     |

Hangisi olduğunu bilmiyorsanız Claude Code'a şunu yazın:
`/oyun-mod "<oyun klasörü>" hangi motor, mod için nereden başlamalıyım?`

## Gerekenler

- [.NET SDK 8](https://dotnet.microsoft.com/download) (`dotnet --version`)
- Oyuna kurulmuş BepInEx: Mono için 5.x, IL2CPP için 6.x bleeding edge.
  Resmi kaynak: https://github.com/BepInEx/BepInEx ve https://builds.bepinex.dev.
  Kurduktan sonra oyunu bir kez açıp kapatın; BepInEx klasörlerini oluşturur.
- IL2CPP oyunlarda BepInEx sizin Unity sürümünüzü desteklemiyorsa alternatif
  [MelonLoader](https://github.com/LavaGang/MelonLoader). `/oyun-mod` şablonu
  ona uyarlayabilir.

## Derleme ve kurulum

```bash
cd kisisel/oyun-kiti/unity-bepinex-mono
dotnet build -c Release -p:OyunKlasoru="/oyun/klasoru" -p:OyunVeriKlasoru="OyunAdi_Data"
```

Çıkan DLL'i `<Oyun>/BepInEx/plugins/` altına kopyalayın ve oyunu açın.
Günlük: `<Oyun>/BepInEx/LogOutput.log`. Panel kısayolu **F8**.

Kaldırmak için DLL'i silin. BepInEx'i tamamen kaldırmak için `BepInEx/`,
`winhttp.dll` ve `doorstop_config.ini` dosyalarını silin.

## Önce yedek

Mod denemeden önce **oyun kayıtlarınızın** yedeğini alın. Football Manager'da
kayıtlar Belgeler / Sports Interactive / Football Manager 20xx / games altındadır.

## Football Manager notu (FMRTE alternatifi)

- Önce resmi araçları değerlendirin: **Pre-Game Editor** (ücretsiz; veritabanı
  düzenleme, `.fmf`) ve **In-Game Editor** (ücretli DLC).
- Kendi gerçek zamanlı editörünüz için en sağlam yol **oyun içi eklenti**dir
  (`unity-bepinex-il2cpp/` içindeki `EditorPaneli`). Oyunun nesnelerine adlarıyla
  eriştiği için yamalarda FMRTE'nin bellek adresi yaklaşımından daha az bozulur.
  Bu yalnızca oyun Unity tabanlıysa geçerlidir; FM'nin hangi sürümünün Unity
  kullandığını `/oyun-mod` ile kendi kurulumunuzda doğrulayın.
- Oyun Unity değilse veya eklenti yüklenemiyorsa `/oyun-editoru` harici bellek
  yaklaşımını (Frida ile prototip) anlatır.
- Yalnızca çevrimdışı kariyer kayıtlarında kullanın; online modlarda kullanmayın.
  DRM/anti-tamper korumasına dokunulmaz.

## Durum

Bu şablonlar BepInEx'in resmi proje yapısına göre yazıldı ama **henüz
derlenip bir oyunda denenmedi**. İlk derlemede paket sürümü veya interop
referanslarıyla ilgili küçük düzeltmeler gerekebilir; `/oyun-mod` bunları
sizin oyununuza göre yapar.
