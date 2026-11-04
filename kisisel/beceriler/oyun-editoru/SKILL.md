---
name: oyun-editoru
description: Tek oyunculu bir oyun için kullanıcının kendi kaydını veya oyun içi verisini (oyuncu, takım, para, istatistik) görüntüleyip düzenleyen bir editör tasarla ve yaz; örneğin Football Manager için FMRTE benzeri bir gerçek zamanlı editör. REA ile veri yapılarını bul, en sağlam yöntemi seç. "Oyun editörü yap", "FMRTE alternatifi", "kayıt dosyası düzenleyici", "/oyun-editoru" denildiğinde kullan.
---

# Oyun editörü (tek oyunculu)

## Kapsam

Kullanıcının kendi kopyası ve kendi kayıtlarıyla, **çevrimdışı ve tek oyunculu** kullanım içindir.

Yapma:

- Çevrimiçi veya çok oyunculu modlarda kullanım (ör. FM'nin online kariyer ve draft modları).
- Anti-cheat veya DRM/anti-tamper atlatma.
- Oyun veritabanını veya kodunu yeniden dağıtma.

Editör, çevrimiçi bir oturum tespit ederse çalışmayı reddetmeli. Bunu tasarıma ekle.

## Yöntem seçimi (sağlamdan kırılgana)

Önce `oyun-mod` skill'indeki motor tespitini yap. Sonra ilk uygun yöntemi seç ve kullanıcıya nedenini söyle:

1. **Resmi araçlar.** Oyun bir editör sunuyorsa önce onu belirt. FM için: Pre-Game Editor (veritabanı `.fmf` dosyaları) ve In-Game Editor. Kullanıcının istediği bunlarla oluyorsa ek araç gerekmez.
2. **Dosya tabanlı.** Kayıt veya veri dosyası okunabilir bir biçimdeyse (JSON, XML, SQLite, sıkıştırılmış ama standart) doğrudan dosya editörü yaz. Biçimi `inspect_artifact`, `inspect_sqlite_database` ve `inspect_byte_entropy` ile incele. Yüksek entropi sıkıştırma veya şifreleme işaretidir. Şifreliyse bu yolu bırak.
3. **Oyun içi eklenti (Unity için en iyi yol).** BepInEx veya MelonLoader eklentisi oyunun kendi nesnelerine **adlarıyla** erişir. Bu yüzden oyun güncellemelerinde bellek adresi tabanlı araçlardan çok daha az bozulur.
   - IL2CPP'de `inspect_il2cpp_metadata` ile veri sınıflarını bul: ör. `Player`, `Person`, `Club`, `Contract`, `Attributes` benzeri türler, alanları ve getter/setter metotları.
   - Bir kısayolla açılan oyun içi pencere (IMGUI veya Unity UI) yaz: arama, liste, detay, düzenleme.
   - Şablon: `~/rea/kisisel/oyun-kiti/unity-bepinex-il2cpp/` (Mono oyunlar için `unity-bepinex-mono/`).
4. **Harici bellek editörü (FMRTE yaklaşımı).** Son çare; en kırılgan yöntem budur.
   - Prototipte `instrument_with_frida` ile çalışan süreçte yapıları oku. Yalnızca kullanıcının başlattığı yerel süreç olmalı ve anti-cheat bulunmamalı.
   - Kalıcı araçta adres yerine imza ve desen araması kullan (`search_bytes` ile doğrula). Sürüm tespiti ekle; bilinmeyen bir sürümde yazma işlemini kapat.

## Geliştirme sırası

1. **Salt okuma:** Bir oyuncunun adını ve birkaç özelliğini doğru göster. Kullanıcı oyunda doğrulasın.
2. **Tek alan yazma:** Örneğin moral veya kondisyon. Önce kaydın yedeğini aldır.
3. **Arayüz:** Arama, filtreleme, toplu düzenleme.
4. **Güvenlik:** Değer aralığı doğrulaması, geri al, değişiklik günlüğü.

Her adımda neyin _gözlendiğini_ (oyunda doğrulandı) ve neyin _tahmin_ olduğunu ayır.

## Teslim

Proje, derleme ve kurulum, kısayollar, desteklenen oyun sürümü ve güncellemede ne yapılacağı.
