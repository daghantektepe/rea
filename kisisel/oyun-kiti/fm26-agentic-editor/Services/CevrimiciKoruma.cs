using System;
using System.Reflection;

namespace FM26AgenticEditor.Services
{
    /// <summary>
    /// /oyun-editoru kuralları gereği çevrimiçi (Online Kariyer, Fantasy Draft, Versus) oturumları tespit eder
    /// ve çok oyunculu modda veri değiştirmeyi otomatik olarak engeller.
    /// </summary>
    public static class CevrimiciKoruma
    {
        private static DateTime _sonKontrol = DateTime.MinValue;
        private static bool _cevrimiciOturumAktif;
        public static string DurumMesaji { get; private set; } = "Tek Oyunculu (Çevrimdışı Kariyer) - Düzenleme Açık";

        public static bool YazmaIzniVarMi()
        {
            if ((DateTime.UtcNow - _sonKontrol).TotalSeconds > 5)
            {
                _sonKontrol = DateTime.UtcNow;
                _cevrimiciOturumAktif = CevrimiciOturumTespitEt();
            }

            return !_cevrimiciOturumAktif;
        }

        private static bool CevrimiciOturumTespitEt()
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var ad = asm.GetName().Name ?? "";
                    if (!ad.Contains("SI", StringComparison.OrdinalIgnoreCase) &&
                        !ad.Contains("FM", StringComparison.OrdinalIgnoreCase) &&
                        !ad.Contains("Network", StringComparison.OrdinalIgnoreCase))
                        continue;

                    Type[] tipler;
                    try
                    {
                        tipler = asm.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        tipler = ex.Types;
                    }

                    if (tipler == null) continue;

                    foreach (var tip in tipler)
                    {
                        if (tip == null) continue;
                        if (!tip.Name.Contains("NetworkGame", StringComparison.OrdinalIgnoreCase) &&
                            !tip.Name.Contains("MultiplayerSession", StringComparison.OrdinalIgnoreCase) &&
                            !tip.Name.Contains("OnlineGame", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var prop = tip.GetProperty("IsConnected", BindingFlags.Public | BindingFlags.Static)
                                ?? tip.GetProperty("IsOnlineGame", BindingFlags.Public | BindingFlags.Static)
                                ?? tip.GetProperty("InNetworkGame", BindingFlags.Public | BindingFlags.Static);

                        if (prop != null && prop.PropertyType == typeof(bool))
                        {
                            var deger = (bool)prop.GetValue(null);
                            if (deger)
                            {
                                DurumMesaji = "UYARI: Çevrimiçi (Online) oturum tespit edildi! Editör salt-okunur moda kilitlendi.";
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Yansıma hatasında varsayılan tek oyunculu durumu koru.
            }

            DurumMesaji = "Tek Oyunculu (Çevrimdışı Kariyer) - Düzenleme Açık";
            return false;
        }
    }
}

