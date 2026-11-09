// Bu dosya YALNIZCA oyun henüz kurulmamışken (BepInEx/interop/*.dll yokken)
// projenin bağımsız derlenip doğrulanabilmesi için devreye girer.
// OyunKlasoru altında BepInEx/interop/UnityEngine.CoreModule.dll bulunduğunda
// MSBuild bu dosyayı derleme dışı bırakır ve gerçek oyun interop DLL'lerini kullanır.

using System;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace Il2CppSystem
{
    public class Object : Il2CppObjectBase
    {
        public Object(IntPtr pointer) : base(pointer) { }
    }

    public class Delegate : Object
    {
        public Delegate(IntPtr pointer) : base(pointer) { }
    }
}

namespace UnityEngine
{
    [Flags]
    public enum HideFlags
    {
        None = 0,
        HideInHierarchy = 1,
        HideInInspector = 2,
        DontSaveInEditor = 4,
        NotEditable = 8,
        DontSaveInBuild = 16,
        DontUnloadUnusedAsset = 32,
        DontSave = 52,
        HideAndDontSave = 61
    }

    public enum KeyCode
    {
        None = 0,
        Escape = 27,
        F1 = 282,
        F2 = 283,
        F3 = 284,
        F4 = 285,
        F5 = 286,
        F6 = 287,
        F7 = 288,
        F8 = 289,
        F9 = 290,
        F10 = 291,
        F11 = 292,
        F12 = 293
    }

    public struct Rect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    public struct Vector2
    {
        public float x;
        public float y;

        public static Vector2 zero => new Vector2(0f, 0f);

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public static Color white => new Color(1f, 1f, 1f, 1f);
        public static Color green => new Color(0.2f, 0.9f, 0.4f, 1f);
        public static Color yellow => new Color(1f, 0.85f, 0.2f, 1f);
        public static Color red => new Color(1f, 0.35f, 0.35f, 1f);
        public static Color cyan => new Color(0.3f, 0.85f, 1f, 1f);

        public Color(float r, float g, float b, float a = 1f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }

    public class Object : Il2CppSystem.Object
    {
        public Object(IntPtr ptr) : base(ptr) { }

        public string name { get; set; } = "";
        public HideFlags hideFlags { get; set; }

        public static void DontDestroyOnLoad(Object target) { }
        public static Il2CppArrayBase<T> FindObjectsOfType<T>() where T : Object => null;
    }

    public class Component : Object
    {
        public Component(IntPtr ptr) : base(ptr) { }
        public GameObject gameObject => null;
    }

    public class Behaviour : Component
    {
        public Behaviour(IntPtr ptr) : base(ptr) { }
        public bool enabled { get; set; } = true;
    }

    public class MonoBehaviour : Behaviour
    {
        public MonoBehaviour(IntPtr ptr) : base(ptr) { }
    }

    public class GameObject : Object
    {
        public GameObject(IntPtr ptr) : base(ptr) { }
        public GameObject(string name) : base(IntPtr.Zero) { this.name = name; }
    }

    public static class Input
    {
        public static bool GetKeyDown(KeyCode key) => false;
    }

    public static class Mathf
    {
        public static int Clamp(int value, int min, int max) => value < min ? min : (value > max ? max : value);
        public static float Clamp(float value, float min, float max) => value < min ? min : (value > max ? max : value);
        public static int RoundToInt(float f) => (int)Math.Round(f);
    }

    public class GUILayoutOption : Il2CppSystem.Object
    {
        public GUILayoutOption(IntPtr ptr) : base(ptr) { }
    }

    public class GUIStyle : Il2CppSystem.Object
    {
        public GUIStyle(IntPtr ptr) : base(ptr) { }
    }

    public static class GUI
    {
        public class WindowFunction : Il2CppSystem.Delegate
        {
            public WindowFunction(IntPtr ptr) : base(ptr) { }
            public static implicit operator WindowFunction(Action<int> action) => null;
        }

        public static Color color { get; set; } = Color.white;
        public static Color backgroundColor { get; set; } = Color.white;
        public static bool enabled { get; set; } = true;

        public static void DragWindow() { }
        public static void DragWindow(Rect position) { }
    }

    public static class GUILayout
    {
        public static Rect Window(int id, Rect screenRect, GUI.WindowFunction func, string text, params GUILayoutOption[] options) => screenRect;
        public static Rect Window(int id, Rect screenRect, GUI.WindowFunction func, string text, Il2CppReferenceArray<GUILayoutOption> options) => screenRect;

        public static void Label(string text, params GUILayoutOption[] options) { }
        public static string TextField(string text, params GUILayoutOption[] options) => text;
        public static string TextArea(string text, params GUILayoutOption[] options) => text;
        public static bool Button(string text, params GUILayoutOption[] options) => false;
        public static bool Toggle(bool value, string text, params GUILayoutOption[] options) => value;
        public static float HorizontalSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options) => value;
        public static int Toolbar(int selected, string[] texts, params GUILayoutOption[] options) => selected;
        public static int Toolbar(int selected, Il2CppStringArray texts, params GUILayoutOption[] options) => selected;

        public static void BeginHorizontal(params GUILayoutOption[] options) { }
        public static void beginHorizontal_Box() { }
        public static void EndHorizontal() { }
        public static void BeginVertical(params GUILayoutOption[] options) { }
        public static void BeginVertical(string text, string style, params GUILayoutOption[] options) { }
        public static void EndVertical() { }
        public static Vector2 BeginScrollView(Vector2 scrollPosition, params GUILayoutOption[] options) => scrollPosition;
        public static void EndScrollView() { }
        public static void Space(float pixels) { }

        public static GUILayoutOption Width(float width) => null;
        public static GUILayoutOption Height(float height) => null;
        public static GUILayoutOption ExpandWidth(bool expand) => null;
        public static GUILayoutOption ExpandHeight(bool expand) => null;
    }
}

namespace UnityEngine.InputSystem.Controls
{
    public class KeyControl : Il2CppSystem.Object
    {
        public KeyControl(IntPtr ptr) : base(ptr) { }
        public bool wasPressedThisFrame => false;
    }
}

namespace UnityEngine.InputSystem
{
    using UnityEngine.InputSystem.Controls;

    public class Keyboard : Il2CppSystem.Object
    {
        public Keyboard(IntPtr ptr) : base(ptr) { }
        public static Keyboard current => null;
        public KeyControl f8Key => null;
        public KeyControl f9Key => null;
    }
}

