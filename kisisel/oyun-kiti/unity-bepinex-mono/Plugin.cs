using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace OrnekMod
{
    [BepInPlugin(Guid, Ad, Surum)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "tr.kisisel.ornekmod";
        public const string Ad = "Örnek Mod";
        public const string Surum = "0.1.0";

        internal static ManualLogSource Gunluk;
        internal static ConfigEntry<float> Carpan;
        private ConfigEntry<KeyboardShortcut> _panelKisayolu;

        private bool _panelAcik;
        private Rect _pencere = new Rect(40, 40, 380, 180);

        private void Awake()
        {
            Gunluk = Logger;
            // Ayarlar: <Oyun>/BepInEx/config/tr.kisisel.ornekmod.cfg dosyasına yazılır.
            _panelKisayolu = Config.Bind("Genel", "PanelKisayolu", new KeyboardShortcut(KeyCode.F8),
                "Mod panelini açıp kapatan tuş.");
            Carpan = Config.Bind("Genel", "Carpan", 1.0f,
                new ConfigDescription("Örnek yamada kullanılan çarpan.", new AcceptableValueRange<float>(0.1f, 10f)));

            new Harmony(Guid).PatchAll();
            Gunluk.LogInfo($"{Ad} {Surum} yüklendi. Panel: {_panelKisayolu.Value}");
        }

        private void Update()
        {
            if (_panelKisayolu.Value.IsDown())
                _panelAcik = !_panelAcik;
        }

        private void OnGUI()
        {
            if (_panelAcik)
                _pencere = GUILayout.Window(81231, _pencere, PaneliCiz, Ad);
        }

        private void PaneliCiz(int pencereId)
        {
            GUILayout.Label($"Çarpan: {Carpan.Value:0.00}");
            Carpan.Value = GUILayout.HorizontalSlider(Carpan.Value, 0.1f, 10f);
            if (GUILayout.Button("Varsayılana dön"))
                Carpan.Value = 1.0f;
            GUI.DragWindow();
        }
    }

    // ÖRNEK HARMONY YAMASI
    // Hedef sınıf ve metodu /oyun-mod ile REA'dan bulun, sonra yorumu kaldırın.
    // Postfix, orijinal metot çalıştıktan sonra sonucu değiştirir.
    //
    // [HarmonyPatch(typeof(OyunSinifi), nameof(OyunSinifi.MetotAdi))]
    // internal static class OrnekYama
    // {
    //     private static void Postfix(ref int __result)
    //     {
    //         __result = Mathf.RoundToInt(__result * Plugin.Carpan.Value);
    //     }
    // }
}
