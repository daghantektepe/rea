---
name: turkce-yama
description: Bir oyunun veya uygulamanın metinlerini REA ile bulup çıkar, çeviri dosyası hazırla, Türkçe çevir ve geri yükleyecek yamayı veya modu yaz. "Türkçe yama yap", "oyunu Türkçeleştir", "metinleri çıkar", "/turkce-yama" denildiğinde kullan.
---

# Türkçe yama

## 1. Metinler nerede?

Motoru `oyun-mod` skill'indeki tabloyla tespit et. Sonra şu sırayla ara:

1. **Hazır yerelleştirme dosyaları:** `Localization`, `lang`, `i18n`, `strings`, `*.po`, `*.csv`, `*.json`, `*.xml`, StreamingAssets. `inspect_artifact` ile envanter çıkar.
2. **Unity:**
   - Mono: `inspect_managed_members` (`ldstr` sabit metinleri).
   - IL2CPP: `inspect_il2cpp_metadata` (string literal'lar).
   - Asset'lerin içindeki TextAsset ve tablolar.
3. **Godot:** `extract_artifact` ile PCK'yı aç. `.translation`, `.csv`, `.tres` dosyalarına bak.
4. **Electron/web:** `analyze_javascript_application`. i18n JSON paketleri.

Kullanıcıya bir özet göster: kaç dosya, yaklaşık kaç satır metin, hangi dillerin zaten var olduğu.

## 2. Çeviri dosyası

Tek bir `ceviri.csv` üret: `anahtar,kaynak_metin,turkce,not`.

- Değişkenleri ve biçim kodlarını koru: `{0}`, `%s`, `<color>`, `\n`.
- Uzunluk sınırı olan arayüz metinlerini `not` sütununda işaretle.
- Çeviriyi sen yap. Oyun terimleri için bir sözlük tut ve tutarlılığı koru.

## 3. Font ve karakter kontrolü

`ç ğ ı İ ö ş ü` karakterleri oyunun fontunda yoksa kutucuk görünür. Bunu erkenden test ettir. Gerekirse font değiştirme modunu plana ekle; ör. Unity TextMeshPro fallback font.

## 4. Geri yükleme

Yöntemi seç:

- Oyun dil eklemeyi destekliyorsa yeni dil dosyası olarak ekle.
- Unity: BepInEx eklentisi (çeviri tablosunu çalışma anında uygulayan bir Harmony yaması) veya XUnity.AutoTranslator'ın manuel çeviri dosyası biçimi.
- Godot: çeviri kaynaklarını değiştiren ek PCK.

Orijinal dosyaları yedekle. Kurulum ve kaldırma adımlarını yaz.

## Sınırlar

Oyunun orijinal metinlerini veya varlıklarını tam halde yeniden dağıtma. Yamayı yalnızca çeviri ve kurulum aracı olarak paylaşmayı öner. Oyun koruma altındaysa (DRM/anti-tamper) dosyalarına dokunmadan çalışan bir yöntem öner.
