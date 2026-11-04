---
name: dosya-guvenlik
description: İndirilen bir dosyayı (exe, msi, dmg, app, apk, zip, npm paketi, Electron uygulaması) çalıştırmadan REA ile inceleyip Türkçe ve sade bir güven raporu çıkar. "Bu dosya güvenli mi", "şunu açmadan kontrol et", "/dosya-guvenlik" denildiğinde kullan.
---

# Dosya güven kontrolü

Dosyayı **asla çalıştırma.** Yalnızca statik REA araçlarını kullan.

## Adımlar

1. **Kimlik:** `inspect_artifact`. Biçim, SHA-256 ve içerik envanteri. Arşivse üyelerini özetle.
2. **İmza:**
   - Windows PE: `inspect_pe_certificates`. İmzalayan kim? Bu araç imzayı doğrulamaz, yalnızca kimlik alanlarını okur. Bunu kullanıcıya açıkça söyle.
   - Android: `inspect_apk_signature`.
   - macOS: `inspect_signature`.
3. **Ne yapabiliyor:**
   - PE için `inspect_pe_imports`. Riskli API gruplarını işaretle: süreç enjeksiyonu (`WriteProcessMemory`, `CreateRemoteThread`), tuş kaydı (`SetWindowsHookEx`), kayıt defteri ile başlangıca eklenme, ağ (`WinHttp`, `InternetOpen`), şifreleme.
   - `inspect_pe_load_config`. TLS callback var mı?
   - `inspect_byte_entropy`. Yüksek entropili bölümler paketlenmiş veya şifrelenmiş kod olabilir.
4. **Uygulama türüne göre ek kontrol:**
   - Electron: `inspect_electron_fuses` ve `analyze_javascript_application`. Uzak sunucu adresleri, `child_process` kullanımı, native eklentiler.
   - Go/Rust: `inspect_go_binary` / `inspect_rust_binary`. Bağımlılık sürümleri; bilinen açıklar varsa belirt ve kaynağını göster.
   - APK: `inspect_android_package`. İzinler, dışa açık bileşenler.
   - .NET: `inspect_managed_native_boundaries`.

## Rapor biçimi

```
Sonuç: 🟢 Belirgin risk yok | 🟡 Dikkat | 🔴 Şüpheli
Dosya: ad, boyut, SHA-256
İmzalayan: ... (doğrulanmadı / doğrulandı)
Öne çıkanlar: en fazla 5 madde, her biri kanıtıyla
Önerim: tek cümle (ör. "SHA-256'yı VirusTotal'da aratın", "resmi siteden indirin")
```

Abartma. Riskli bir API'nin varlığı kötü niyet kanıtı değildir; bağlamını söyle. "Güvenli" diye kesin hüküm verme. Statik analiz her şeyi göremez.
