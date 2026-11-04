# Kişisel REA kurulumu (daghantektepe/rea)

Bu klasör, [morluto/rea](https://github.com/morluto/rea) fork'unu npm paketi
yerine **doğrudan kaynak koddan** kullanmak içindir. Avantajları:

- Upstream'deki yeni özellikler npm'e çıkmadan önce sizde olur.
- MCP kaydı bu klasördeki derlemeyi gösterir; güncellemek için yeniden derlemek yeterlidir.
- Kendi değişikliklerinizi (yeni skill notları, yamalar) bu dalda tutabilirsiniz.

Upstream dosyalarına dokunulmadı; tüm kişisel eklemeler `kisisel/` altındadır.
Bu sayede upstream birleştirmeleri çakışmasız ilerler.

## 1. Kurulum

Gerekenler: `git`, Node.js **24.22.1+** (önerilen; 22.23.4+ veya 26.12.1+ da olur) ve npm.
Linux'ta ayrıca `ps` komutu (procps) gerekir.

macOS / Linux / WSL:

```bash
git clone -b kisisel https://github.com/daghantektepe/rea.git ~/rea
```

```bash
~/rea/kisisel/kur.sh
```

Betik `npm ci` ve derlemeyi yapar, sonra `rea setup` çalıştırır. Setup hangi
ajanları (Claude Code, Claude Desktop, Cursor, Codex, Gemini CLI…) yapılandıracağını
sorar, planı gösterir ve onay ister. Mevcut yapılandırmaların yedeği alınır.
Ajanı doğrudan seçmek için:

```bash
~/rea/kisisel/kur.sh kur --client claude_code --client claude_desktop
```

Kurulumdan sonra ajanınızı yeniden başlatın. Claude Desktop'ı tamamen kapatıp
(pencereyi kapatmak yetmez) öyle onaylayın.

Windows (PowerShell, betik yerine elle):

```powershell
git clone -b kisisel https://github.com/daghantektepe/rea.git $HOME\rea
cd $HOME\rea; npm ci; npm run build:cached; node scripts/rea.mjs setup
```

## 2. Güncel tutma

```bash
~/rea/kisisel/kur.sh guncelle
```

Bu komut `morluto/rea` deposunun `main` dalını yerel dalınıza birleştirir,
yeniden derler ve kurulumu (skill dosyaları dahil) yeniler. Fork'u GitHub'da da
güncellemek için `--push` ekleyin.

Kontrol için:

```bash
~/rea/kisisel/kur.sh dogrula
```

## 3. Terminalden kullanım

Her komutu `node ~/rea/scripts/rea.mjs …` ile çalıştırabilirsiniz. Kısaltma için
kabuk yapılandırmanıza ekleyin:

```bash
alias rea="node $HOME/rea/scripts/rea.mjs"
```

Sonra `rea --help`, `rea doctor` ve aşağıdaki örnekler çalışır.

## 4. Analiz motorları (isteğe bağlı)

JavaScript/Electron, .NET, web, SQLite, APK imzası, Go/Rust bağımlılıkları,
PE/ELF başlıkları gibi analizler **ek araç gerektirmez**. Derin native analiz
(pseudocode, assembly, çağrı grafikleri) için birini seçin:

| Motor  | Ne zaman                                | Ayar                                                       |
| ------ | --------------------------------------- | ---------------------------------------------------------- |
| Hopper | macOS'ta en kolay yol; setup kurabilir  | Setup sırasında önerilir (onaylarsanız kurulur)            |
| Ghidra | Ücretsiz, her platform; JDK 21 gerekir  | `GHIDRA_INSTALL_DIR=/yol/ghidra_12.x` ve setup'ı yenileyin |
| IDA    | Lisansınız varsa                        | [docs/ida-provider.md](../docs/ida-provider.md)            |
| JADX   | Android APK'larda sınıf/metot decompile | `REA_JADX_MCP_JAR` ([rehber](../docs/android-analysis.md)) |

Ortam değişkenini ayarladıktan sonra `kisisel/kur.sh kur` komutunu tekrar
çalıştırın; setup değeri MCP kaydına ekler.

## 5. Neler için kullanabilirsiniz?

Aşağıdaki istemler REA kurulu bir ajana (ör. Claude Code) doğrudan yazılabilir.
Uygulama adlarını ve yolları kendinizinkilerle değiştirin.

### Ürün ve özellik araştırması

En güçlü kullanım alanı budur: beğendiğiniz bir özelliğin nasıl çalıştığını
kaynak kod olmadan anlayıp kendi ürününüze benzerini yazmak.

```text
Bilgisayarımdaki Notion masaüstü uygulamasında "/" komut menüsünün nasıl
çalıştığını REA ile incele. Renderer, preload ve main process arasındaki
akışı kanıtlarıyla göster, sonra benim React projeme benzer bir menü yaz.
```

- Electron uygulamaları (Slack, Notion, Figma, Discord, VS Code, Obsidian):
  IPC kanalları, kısayollar, yerel depolama, eklenti mimarisi.
- Web siteleri: sayfanın hangi API'leri çağırdığı, hangi scriptlerin yüklendiği,
  bir etkileşimin hangi koda bağlı olduğu.
- İki sürüm arasında ne değişti: `compare-application-versions` ile bir
  uygulamanın güncellemesinde eklenen/kalkan modülleri görün.

### Kendi verinizi ve cihazlarınızı anlamak

```text
~/Library/Application Support/<uygulama> altındaki SQLite veritabanını REA ile
incele. Hangi tablolarda ne tutulduğunu ve notlarımı dışa aktarmak için hangi
sorgunun gerektiğini söyle.
```

- Uygulamaların yerel SQLite veritabanları (notlar, mesajlar, geçmiş) —
  kendi verinizi dışa aktarmak için.
- Telefonunuzdaki bir Android uygulamasının izinleri, manifest beyanları,
  imza sertifikası (`inspect-android-package`, `inspect-apk-signature`).
- Akıllı ev/IoT cihazınızın firmware dosyası (Linux'ta binwalk/unblob ile).
- Bir HAR dosyası (tarayıcı DevTools → Network → Save all as HAR) ile bir web
  uygulamasının API'sini dokümante etmek.

### Güvenlik ve güven kontrolü

```text
İndirdiğim ~/Downloads/kurulum.exe dosyasını çalıştırmadan REA ile incele:
imzalayan kim, hangi DLL'leri ve fonksiyonları içe aktarıyor, şüpheli bir
şey var mı? Sonuçları kanıtlarıyla özetle.
```

- PE Authenticode imzaları, import tabloları, entropi (paketlenmiş/şifreli
  bölüm tespiti) — dosyayı çalıştırmadan.
- Go/Rust programlarının gömülü bağımlılık listesi: bilinen açığı olan bir
  kütüphane sürümü kullanıyor mu?
- npm paketi veya Electron uygulamasında beklenmedik ağ çağrıları ya da native
  eklentiler.
- Elektron uygulamalarının güvenlik sigortaları (`inspect-electron-fuses`).

### Yapay zekâ ve veri işleri

- GGUF / safetensors model dosyalarının başlıkları: parametre sayısı,
  kuantizasyon, mimari — ağırlıkları yüklemeden.
- **RL / ajan test ortamı:** Kaydedilmiş bir web oturumunu (HAR) ajanların
  pratik yapabileceği sahte bir API ortamına çevirin
  (`rea environment-build ./oturum.har ./ortamim`). Kendi ajanlarınızı gerçek
  sisteme dokunmadan test etmek için kullanışlıdır.

### Öğrenme ve eğlence

- Eski oyunlar ve DOS/PC-98 programları (Ghidra ile 16-bit analiz) — bir
  hesaplamanın nasıl yapıldığını çıkarıp C'ye çevirmek.
- CTF görevleri: ELF düzeni, GDB/Rizin oturumları, çökme dökümü analizi.
- Unity (IL2CPP) ve Godot (PCK) oyun dosyalarının yapısı.

### CI / otomasyon

[examples/github-actions/rea-analysis.yml](../examples/github-actions/rea-analysis.yml)
örneğiyle kendi projelerinizin derleme çıktılarını her sürümde otomatik
inceleyebilirsiniz (ör. bundle'a beklenmeyen bir modül girdi mi?).

## 6. Sınırlar

REA yalnızca yerelde çalışır; dosyalarınızı yüklemez (ajanınızın model
sağlayıcısı sonuçları görür). Hukuka uygun kullanım sizin sorumluluğunuzdadır:
DRM, lisans kontrolü, anti-cheat veya erişim kontrollerini aşmak, başkasının
korunan kodunu kopyalamak desteklenmez. Uygulamaların lisans sözleşmelerine ve
kullanım koşullarına dikkat edin. Ayrıntılar: [DISCLAIMER.md](../DISCLAIMER.md).

## 7. Sorun giderme

- `rea doctor` neyin eksik olduğunu ve nasıl düzeltileceğini söyler.
- Ajan REA araçlarını görmüyorsa ajanı tamamen yeniden başlatın.
- Kurulum ayrıntıları: [docs/installation.md](../docs/installation.md),
  CLI rehberi: [docs/cli.md](../docs/cli.md).
