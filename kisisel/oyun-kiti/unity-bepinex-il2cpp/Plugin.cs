using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace OrnekIl2cppMod
{
    [BepInPlugin(Guid, Ad, Surum)]
    public class Plugin : BasePlugin
    {
        public const string Guid = "tr.kisisel.ornekil2cppmod";
        public const string Ad = "Örnek IL2CPP Mod";
        public const string Surum = "0.1.0";

        internal static ManualLogSource Gunluk;

        public override void Load()
        {
            Gunluk = Log;
            new Harmony(Guid).PatchAll();
            // MonoBehaviour'u IL2CPP tarafına kaydeder ve sahneye ekler.
            AddComponent<EditorPaneli>();
            Gunluk.LogInfo($"{Ad} {Surum} yüklendi. Panel: F8");
        }
    }

    // Oyun içi editör penceresi iskeleti (FMRTE benzeri bir aracın başlangıç noktası).
    public class EditorPaneli : MonoBehaviour
    {
        public EditorPaneli(IntPtr ptr) : base(ptr) { }

        private bool _acik;
        private string _arama = "";
        private Rect _pencere = new Rect(40, 40, 480, 360);

        private void Update()
        {
            // Not: Oyun yalnızca yeni Input System kullanıyorsa bu satır çalışmaz;
            // o durumda UnityEngine.InputSystem.Keyboard.current.f8Key.wasPressedThisFrame kullanın.
            if (Input.GetKeyDown(KeyCode.F8))
                _acik = !_acik;
        }

        private void OnGUI()
        {
            if (_acik)
                _pencere = GUILayout.Window(81232, _pencere, (GUI.WindowFunction)PaneliCiz, "Editör", Array.Empty<GUILayoutOption>());
        }

        private void PaneliCiz(int pencereId)
        {
            GUILayout.Label("Ara:", Array.Empty<GUILayoutOption>());
            _arama = GUILayout.TextField(_arama, Array.Empty<GUILayoutOption>());
            GUILayout.Label("Sonuçlar burada listelenecek.", Array.Empty<GUILayoutOption>());
            // /oyun-editoru ile REA'dan bulunan veri sınıflarına buradan erişin, örneğin:
            // foreach (var oyuncu in Object.FindObjectsOfType<OyuncuSinifi>()) { ... }
            GUI.DragWindow();
        }
    }
}
