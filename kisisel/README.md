# Kişisel REA kurulumu (daghantektepe/rea)

Bu klasör, [morluto/rea](https://github.com/morluto/rea) fork'unu npm paketi
yerine **doğrudan kaynak koddan** kullanmak içindir. Avantajları:

- Upstream'deki yeni özellikler npm'e çıkmadan önce sizde olur.
- MCP kaydı bu klasördeki derlemeyi gösterir; güncellemek için yeniden derlemek yeterlidir.
- Kendi değişikliklerinizi (yeni skill notları, yamalar) bu dalda tutabilirsiniz.

Upstream dosyalarına dokunulmadı; tüm kişisel eklemeler `kisisel/` altındadır.
Bu sayede upstream birleştirmeleri çakışmasız ilerler.

## 1. Kurulum (SSD'ye)

Gerekenler: `git`, Node.js **24.22.1+** (önerilen; 22.23.4+ veya 26.12.1+ da olur),
npm ve [Claude Code](https://code.claude.com). Linux'ta ayrıca `ps` (procps) gerekir.
Kurulum yaklaşık 1,5 GB yer kaplar (`node_modules` dahil).

**SSD notu:** Harici bir SSD kullanıyorsanız diski APFS (macOS) veya NTFS
(Windows) olarak biçimlendirin. exFAT'ta sembolik bağlantılar ve çalıştırma
izinleri sorun çıkarır. MCP kaydı bu klasörü gösterdiği için SSD takılı değilken
REA araçları Claude Code'da görünmez; SSD'yi takıp Claude Code'u yeniden başlatmanız yeterli.

### macOS / Linux

Klasörü kendi SSD'nize göre değiştirin (dahili disk için `~/rea` yeterli):

```bash
git clone -b kisisel https://github.com/daghantektepe/rea.git "/Volumes/SSD/rea"
```

```bash
bash "/Volumes/SSD/rea/kisisel/kur.sh" kur --client claude_code
```

### Windows (PowerShell)

```powershell
git clone -b kisisel https://github.com/daghantektepe/rea.git D:\rea
```

```powershell
powershell -ExecutionPolicy Bypass -File D:\rea\kisisel\kur.ps1 kur --client claude_code
```

### Betik ne yapar?

1. Node, npm ve git sürümlerini kontrol eder (hiçbir şey kurmaz).
2. `npm ci` ve derleme yapar.
3. `rea setup` çalıştırır. Setup Claude Code'a REA MCP sunucusunu ekler, planı
   gösterir ve **onayınızı ister**. Mevcut yapılandırmanın yedeği alınır.
4. Türkçe skill'leri `~/.claude/skills/` altına bağlar (macOS/Linux'ta sembolik
   bağlantı, Windows'ta kopya). Aynı adda bir klasör varsa önce yedekler.

Bittiğinde Claude Code'u kapatıp açın ve kontrol edin:

```bash
claude mcp list
```

Listede `rea` görünmeli. Claude Code içinde `/` yazınca Türkçe skill'ler de görünür.

## 2. Güncel tutma

```bash
bash "/Volumes/SSD/rea/kisisel/kur.sh" guncelle
```

Bu komut `morluto/rea` deposunun `main` dalını yerel dalınıza birleştirir,
yeniden derler ve kurulumu yeniler. Fork'u GitHub'da da güncellemek için
`--push` ekleyin. Windows'ta `kur.ps1 guncelle` kullanın.

Kontrol için `kur.sh dogrula` (Windows'ta `kur.ps1 dogrula`).

## 3. Claude Code'da kullanım

Bir terminalde çalışmak istediğiniz klasöre gidip (ör. oyun veya proje klasörü)
`claude` yazın. Sonra ya doğrudan Türkçe sorun ya da hazır iş akışlarından birini
çağırın:

| Komut               | Ne yapar                                                                      |
| ------------------- | ----------------------------------------------------------------------------- |
| `/ozellik-coz`      | Bir uygulamadaki özelliğin nasıl çalıştığını çözer, istenirse benzerini yazar |
| `/dosya-guvenlik`   | İndirilen dosyayı çalıştırmadan inceler, 🟢/🟡/🔴 Türkçe rapor verir          |
| `/web-api-haritasi` | HAR kaydından veya açık sayfadan API haritası çıkarır, test ortamı kurar      |
| `/oyun-mod`         | Oyunun motorunu tespit eder, mod altyapısı seçer, ilk modu yazar              |
| `/oyun-editoru`     | Tek oyunculu oyun için FMRTE benzeri veri/kayıt editörü tasarlar ve yazar     |
| `/turkce-yama`      | Oyun/uygulama metinlerini çıkarır, çevirir, Türkçe yama hazırlar              |

Örnekler:

```text
/dosya-guvenlik ~/Downloads/kurulum.exe
/oyun-mod "C:\Program Files (x86)\Steam\steamapps\common\OyunAdi" — hangi motor, mod için nereden başlamalıyım?
/oyun-editoru Football Manager için oyuncu arayıp moral ve kondisyon düzenleyebileceğim bir panel istiyorum
```

Skill dosyaları `kisisel/beceriler/` altında düz metindir; istediğiniz gibi
düzenleyebilirsiniz. Oyun mod şablonları `kisisel/oyun-kiti/` altındadır.

## 4. Terminalden kullanım (isteğe bağlı)

Her komutu `node <kurulum klasörü>/scripts/rea.mjs …` ile çalıştırabilirsiniz. Kısaltma için
kabuk yapılandırmanıza ekleyin:

```bash
alias rea="node /Volumes/SSD/rea/scripts/rea.mjs"   # kendi kurulum klasörünüz
```

Sonra `rea --help`, `rea doctor` ve aşağıdaki örnekler çalışır.

## 5. Analiz motorları (isteğe bağlı)

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

## 6. Neler için kullanabilirsiniz?

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

### Oyunlar (tek oyunculu)

- Mod: motor tespiti, değiştirilecek sınıf ve metodun bulunması, BepInEx/MelonLoader
  eklentisi (`/oyun-mod`).
- Kayıt ve veri editörü: Football Manager için FMRTE benzeri oyun içi panel
  (`/oyun-editoru`).
- Türkçe yama: metinleri çıkarma, çeviri tablosu, font kontrolü (`/turkce-yama`).

### Öğrenme ve eğlence

- Eski oyunlar ve DOS/PC-98 programları (Ghidra ile 16-bit analiz) — bir
  hesaplamanın nasıl yapıldığını çıkarıp C'ye çevirmek.
- CTF görevleri: ELF düzeni, GDB/Rizin oturumları, çökme dökümü analizi.

### CI / otomasyon

[examples/github-actions/rea-analysis.yml](../examples/github-actions/rea-analysis.yml)
örneğiyle kendi projelerinizin derleme çıktılarını her sürümde otomatik
inceleyebilirsiniz (ör. bundle'a beklenmeyen bir modül girdi mi?).

## 7. Sınırlar

REA yalnızca yerelde çalışır; dosyalarınızı yüklemez (ajanınızın model
sağlayıcısı sonuçları görür). Hukuka uygun kullanım sizin sorumluluğunuzdadır:
DRM, lisans kontrolü, anti-cheat veya erişim kontrollerini aşmak, başkasının
korunan kodunu kopyalamak desteklenmez. Uygulamaların lisans sözleşmelerine ve
kullanım koşullarına dikkat edin. Ayrıntılar: [DISCLAIMER.md](../DISCLAIMER.md).

## 8. Sorun giderme

- `rea doctor` neyin eksik olduğunu ve nasıl düzeltileceğini söyler.
- Ajan REA araçlarını görmüyorsa ajanı tamamen yeniden başlatın.
- Kurulum ayrıntıları: [docs/installation.md](../docs/installation.md),
  CLI rehberi: [docs/cli.md](../docs/cli.md).
