using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using FM26AgenticEditor.Services;
using FM26AgenticEditor.UI;
using HarmonyLib;
using UnityEngine;

namespace FM26AgenticEditor
{
    [BepInPlugin(Guid, Ad, Surum)]
    public class Plugin : BasePlugin
    {
        public const string Guid = "tr.kisisel.fm26agenticeditor";
        public const string Ad = "FM26 In-Game Editor, Rehabilitasyon & Agentic FM";
        public const string Surum = "0.3.0";

        internal static ManualLogSource Gunluk;
        internal static ConfigEntry<KeyCode> PanelKisayolu;
        internal static ConfigEntry<KeyCode> RontgenHudKisayolu;
        internal static ConfigEntry<string> LlmEndpoint;
        internal static ConfigEntry<string> LlmModel;
        internal static ConfigEntry<bool> UzakLlmAktif;
        internal static ConfigEntry<int> SakatlikCarpaniYuzde;

        internal static FmVeriKoprusu VeriKoprusu;
        internal static RehabilitasyonServisi Rehabilitasyon;
        internal static AgenticFmServisi AgentServisi;

        public override void Load()
        {
            Gunluk = Log;

            // BepInEx/config/tr.kisisel.fm26agenticeditor.cfg ayarları
            PanelKisayolu = Config.Bind("Genel", "PanelKisayolu", KeyCode.F8,
                "FM26 oyun içi editör ve Agentic FM panelini açıp kapatan kısayol tuşu.");
            RontgenHudKisayolu = Config.Bind("Genel", "RontgenHudKisayolu", KeyCode.F9,
                "Masaüstü Oyuncu Röntgeni (Boy/Kilo/CA-PA/Sakatlık Eğilimi) üst bilgi çubuğunu açıp kapatan tuş.");
            SakatlikCarpaniYuzde = Config.Bind("Rehabilitasyon", "SakatlikCarpaniYuzde", 68,
                "Küresel sakatlık olasılık çarpanı (%50-%100). %68 değeri sakatlıkları %32 azaltır.");

            UzakLlmAktif = Config.Bind("AgenticFM", "UzakLlmAktif", false,
                "True yapılırsa yerel/uzak LLM API (Ollama vb.) kullanılır; False ise yerleşik analiz motoru çalışır.");
            LlmEndpoint = Config.Bind("AgenticFM", "LlmEndpoint", "http://localhost:11434/v1/chat/completions",
                "OpenAI/Ollama uyumlu Chat Completions API adresi.");
            LlmModel = Config.Bind("AgenticFM", "LlmModel", "qwen2.5:7b",
                "Kullanılacak LLM model adı.");

            VeriKoprusu = new FmVeriKoprusu();
            Rehabilitasyon = new RehabilitasyonServisi();
            Rehabilitasyon.Ayarlar.SakatlikFrekansiCarpaniYuzde = Mathf.Clamp(SakatlikCarpaniYuzde.Value, 50, 100);

            AgentServisi = new AgenticFmServisi
            {
                UzakLlmKullan = UzakLlmAktif.Value,
                ApiEndpoint = LlmEndpoint.Value,
                ModelAdi = LlmModel.Value
            };

            new Harmony(Guid).PatchAll();

            // Paneli IL2CPP'ye kaydet ve sahne geçişlerinde (Ana Menü -> Kariyer -> Maç) silinmesini önle.
            var panel = AddComponent<FmEditorPaneli>();
            if (panel != null)
            {
                panel.hideFlags = HideFlags.HideAndDontSave;
                UnityEngine.Object.DontDestroyOnLoad(panel);
            }

            Gunluk.LogInfo($"{Ad} v{Surum} yüklendi! Editör: {PanelKisayolu.Value} | Masaüstü Röntgen HUD: {RontgenHudKisayolu.Value}");
        }
    }
}
