using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OrnekIl2cppMod
{
    [BepInPlugin(Guid, Ad, Surum)]
    public class Plugin : BasePlugin
    {
        public const string Guid = "tr.kisisel.ornekil2cppmod";
        public const string Ad = "Örnek IL2CPP Mod";
        public const string Surum = "0.1.0";

        internal static ManualLogSource Gunluk;
        internal static ConfigEntry<KeyCode> PanelTusu;

        public override void Load()
        {
            Gunluk = Log;
            PanelTusu = Config.Bind("Genel", "PanelTusu", KeyCode.F8, "Oyun içi editör panelini açıp kapatan tuş.");

            new Harmony(Guid).PatchAll();

            // MonoBehaviour'u IL2CPP tarafına kaydeder, sahneye ekler ve sahne geçişlerinde silinmesini önler.
            var panel = AddComponent<EditorPaneli>();
            if (panel != null)
            {
                panel.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(panel);
            }

            Gunluk.LogInfo($"{Ad} {Surum} yüklendi. Panel kısayolu: {PanelTusu.Value}");
        }
    }

    // Oyun içi editör penceresi iskeleti (FM26 / Unity IL2CPP uyumlu).
    public class EditorPaneli : MonoBehaviour
    {
        public EditorPaneli(IntPtr ptr) : base(ptr) { }

        private bool _acik;
        private string _arama = "";
        private Rect _pencere = new Rect(40, 40, 480, 360);
        private GUI.WindowFunction _pencereFonksiyonu;

        private void Awake()
        {
            // IL2CPP'de C# metodunu Il2CppSystem.Delegate türevi olan GUI.WindowFunction'a dönüştürür.
            _pencereFonksiyonu = DelegateSupport.ConvertDelegate<GUI.WindowFunction>((Action<int>)PaneliCiz);
        }

        private void Update()
        {
            if (KisayolBasildiMi())
                _acik = !_acik;
        }

        private static bool KisayolBasildiMi()
        {
            // 1. Yeni Unity Input System (FM26 / Unity 6 varsayılanı)
            try
            {
                if (Keyboard.current != null && Keyboard.current.f8Key != null && Keyboard.current.f8Key.wasPressedThisFrame)
                    return true;
            }
            catch
            {
                // Oyun InputSystem modülünü yüklemediyse eski Input sistemine düş.
            }

            // 2. Eski Unity Input System (Legacy InputManager)
            try
            {
                var tus = Plugin.PanelTusu != null ? Plugin.PanelTusu.Value : KeyCode.F8;
                if (Input.GetKeyDown(tus))
                    return true;
            }
            catch
            {
                // Legacy Input devre dışıysa yoksay.
            }

            return false;
        }

        private void OnGUI()
        {
            if (!_acik)
                return;

            if (_pencereFonksiyonu == null)
                _pencereFonksiyonu = DelegateSupport.ConvertDelegate<GUI.WindowFunction>((Action<int>)PaneliCiz);

            _pencere = GUILayout.Window(81232, _pencere, _pencereFonksiyonu, "Oyun İçi Editör (F8)");
        }

        private void PaneliCiz(int pencereId)
        {
            GUILayout.Label("Ara:");
            _arama = GUILayout.TextField(_arama ?? "");
            GUILayout.Label("Sonuçlar burada listelenecek.");
            // /oyun-editoru ile REA'dan bulunan veri sınıflarına buradan erişin, örneğin:
            // foreach (var oyuncu in UnityEngine.Object.FindObjectsOfType<OyuncuSinifi>()) { ... }
            GUI.DragWindow();
        }
    }
}
