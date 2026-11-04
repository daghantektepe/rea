---
name: web-api-haritasi
description: Bir web uygulamasının kullandığı API'yi REA ile çıkarıp okunabilir bir Türkçe API haritası (uç noktalar, parametreler, örnek yanıtlar) hazırla. İsteğe bağlı olarak kayıttan ajanların deneyebileceği sahte bir test ortamı (AgentEnv) kur. "Bu sitenin API'si ne", "HAR dosyasını incele", "/web-api-haritasi" denildiğinde kullan.
---

# Web API haritası

## Girdi seç (en güvenilirden başla)

1. **HAR dosyası (önerilen):** Kullanıcı Chrome'da F12 → Network → "Preserve log" → işlemleri yapar → sağ tık → "Save all as HAR with content". Büyük ve modern sitelerde en verimli yol budur.
2. **Açık tarayıcı:** Kullanıcı Chrome'u `--remote-debugging-port=9222` ile başlatır. `list_browser_targets`, sonra `inspect_web_page`; scriptler için `analyze_web_bundle`.
   - Çok büyük (MB'larca) bundle'larda statik analiz süre veya bellek sınırına takılabilir. Önce HAR'a geç, ya da `analysis_timeout_ms`, `total_analysis_timeout_ms` ve `max_heap_mb` değerlerini birlikte artır. Per-script değeri toplam değerden büyük olamaz.
   - SPA'larda API genelde başka bir origin'dedir. `allowed_origins` listesine onları da ekle.

HAR dosyaları oturum çerezleri ve token içerebilir. Kullanıcıyı uyar: paylaşmadan önce temizlesin. Rapora gizli değer yazma.

## Analiz

- `inspect_web_network_capture`: İstekler, yanıtlar ve gövde şekilleri.
- Gerekirse `trace_web_source_location` / `export_web_scripts`: Bir isteğin hangi koddan çıktığını bul.

## Çıktı: `api-haritasi.md`

Kullanıcının seçtiği klasöre yaz:

```
# <Site> API haritası (kayıt tarihi, kaynak HAR)
## Kimlik doğrulama: nasıl görünüyor (değerleri gizle)
## Uç noktalar
| Metot | Yol | Amaç (tahmin/gözlem) | Önemli parametreler | Yanıt şekli |
## Örnek akışlar: "liste → detay → güncelle"
## Bilinmeyenler
```

Yol parametrelerini şablonla (`/items/{id}`). Her uç noktanın yanına "gözlendi" (kayıtta var) veya "tahmin" (sadece kodda geçiyor) yaz.

## Test ortamı (istenirse)

CLI: `rea environment-build kayit.har ./ortam --name ortam` (MCP'de ise `capture_browser_scenario` + `export_environment_bundle`; önce REA skill'indeki environments rehberini oku). Hangi araçların durumdan yanıtlandığını, hangilerinin tekrar oynatıldığını ve hangi yazma işlemlerinin neden engellendiğini kullanıcıya özetle. Çalıştırmak için AgentEnv ve Docker gerekir.

## Sınırlar

Yalnızca kullanıcının erişim yetkisi olan hesap ve sistemler. Bot korumasını, hız sınırlarını veya kimlik doğrulamayı aşmaya çalışma. Sitenin kullanım koşullarını hatırlat.
