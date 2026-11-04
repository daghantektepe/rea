---
name: ozellik-coz
description: Bir masaüstü, Electron veya web uygulamasındaki belirli bir özelliğin nasıl çalıştığını REA ile kaynak kodu olmadan çöz, kanıtlarıyla Türkçe açıkla ve istenirse kullanıcının projesine benzerini yaz. "Bu uygulamada X nasıl çalışıyor", "şu özelliğin aynısını bana yap", "/ozellik-coz" denildiğinde kullan.
---

# Özellik çözümleme

Kullanıcıyla Türkçe konuş. Teknik terimleri ilk geçtiği yerde bir cümleyle açıkla.

## 1. Hedefi netleştir

Eksikse yalnızca şunları sor (tek mesajda):

- Uygulama nerede? (kurulum klasörü, `.app`, `.asar`, web adresi)
- Hangi özellik? Tek bir davranış olsun: "arama", "/ komut menüsü", "panoya kopyalama".
- Sonuçta ne istiyor: sadece açıklama mı, yoksa kendi projesine uyarlama mı? Uyarlama ise proje klasörü ve teknolojisi.

## 2. Doğru REA aracını seç

`reverse-engineer-anything` skill'inin yönlendirme kurallarına uy. Kısa özet:

| Hedef                                         | İlk araç                                                                                                          |
| --------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| Kurulu masaüstü uygulaması (türü belli değil) | `inspect_desktop_application`                                                                                     |
| Electron: `app.asar` veya `resources/app`     | `analyze_javascript_application`                                                                                  |
| Web sayfası                                   | Kullanıcının açık Chrome'u için `list_browser_targets`; kayıt varsa HAR ile `inspect_web_network_capture`         |
| .NET uygulaması                               | `inspect_managed_artifact`, ardından `inspect_managed_members`                                                    |
| Native (C/C++/Swift)                          | `open_binary`, ardından `search_strings` / `trace_feature` / `procedure_pseudo_code` (Hopper veya Ghidra gerekir) |

Özelliği bir **iz** ile bul: arayüzdeki metin, kısayol, menü adı, hata mesajı veya ağ isteği yolu. `trace_application_feature` (JavaScript) veya `search_strings` + `trace_feature` (native) ile o izden koda ilerle. Büyük çıktılarda `detail: "summary"` ve view'ları kullan.

## 3. Raporla

Şu başlıklarla, kısa tut:

1. **Kısaca nasıl çalışıyor:** 3–5 cümle.
2. **Akış:** Kullanıcı eylemi → bileşenler → sonuç. Electron'da renderer → preload → IPC → main zincirini göster.
3. **Kanıtlar:** Dosya, modül, fonksiyon ve satır aralığı ya da adres. Her iddianın yanına türünü yaz: _gözlendi_, _türetildi_, _tahmin_.
4. **Bilinmeyenler:** REA'nın kapsamadığı kısımlar (sunucu tarafı, şifreli yapılandırma vb.).

## 4. Uyarla (istenirse)

- Davranışı kullanıcının projesinin kendi teknolojisiyle **sıfırdan** yaz. Hedef uygulamanın kodunu, varlıklarını veya metinlerini kopyalama; amaç fikri ve mimariyi anlamak.
- Küçük bir test ekle ve çalıştır.

## Sınırlar

DRM, lisans kontrolü veya erişim kontrolü aşmaya yardım etme. Kullanıcıya ait olmayan sistemlere giriş yapma. REA yerelde çalışır; sonuçları sen görürsün.
