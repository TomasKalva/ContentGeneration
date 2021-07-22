using UnityEngine;
using System.IO;
using System;

/// <summary>
/// Noesis global settings
/// </summary>
public class NoesisSettings: ScriptableObject
{
#if UNITY_EDITOR
    private static bool CanLoadNoesis()
    {
        try
        {
            Noesis.GUI.GetBuildVersion();
            return true;
        }
        catch (DllNotFoundException)
        {
            return false;
        }
    }
#endif

    public static bool IsNoesisEnabled()
    {
#if UNITY_EDITOR
        // There are three scenarios where Unity is not able to load our Noesis library.
        // One is when the Unity package is being installed, we detect this by having a flag file inside Editor/ folder
        // Second one happens when the /Library folder is being recreated, this is detected by having a file inside that folder
        // Third is when doing a Unity "Reimport All" operation
        string installingFile = Path.Combine(Application.dataPath, "NoesisGUI/Plugins/Editor/installing");
        string libraryFile = Path.Combine(Application.dataPath, "../Library/noesis");
        return _noesisEnabled && File.Exists(libraryFile) && !File.Exists(installingFile) && !UnityEditorInternal.InternalEditorUtility.inBatchMode && CanLoadNoesis();
#else
        return _noesisEnabled;
#endif
    }

    private static bool _noesisEnabled = true;
    private static NoesisSettings _settings;

    public static NoesisSettings Get()
    {
        if (_settings == null)
        {
            // Theme loading tries to load Noesis library and this is a bad point to allow that
            _noesisEnabled = false;

            _settings = Resources.Load<NoesisSettings>("NoesisSettings");

#if UNITY_EDITOR
            if (_settings == null)
            {
                if (Directory.GetFiles(Application.dataPath, "NoesisSettings.asset", SearchOption.AllDirectories).Length != 0)
                {
                    // In rare situations (for example when upgrading the project to a new version of Unity),
                    // NoesisSettings exists but Unity doesn't load it because it is not registered yet.
                    // In this case, we return a default instance without caching it
                    return (NoesisSettings)ScriptableObject.CreateInstance(typeof(NoesisSettings));
                }

                _settings = (NoesisSettings)ScriptableObject.CreateInstance(typeof(NoesisSettings));
                _settings.applicationResources = UnityEditor.AssetDatabase.LoadAssetAtPath<NoesisXaml>("Assets/NoesisGUI/Theme/NoesisTheme.DarkBlue.asset");
                _settings.defaultFont = UnityEditor.AssetDatabase.LoadAssetAtPath<NoesisFont>("Assets/NoesisGUI/Theme/Fonts/PT Root UI_Regular.asset");

                Directory.CreateDirectory(Application.dataPath + "/Resources");
                UnityEditor.AssetDatabase.CreateAsset(_settings, "Assets/Resources/NoesisSettings.asset");
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.Log("A new settings file was created in 'Assets/Resources/NoesisSettings.assets'. Please move it to a different 'Resources' folder if needed");
            }
            else
            {
                NoesisUnity.SetLicense(_settings.licenseName, _settings.licenseKey);
            }
#endif

            _noesisEnabled = true;
        }

        return _settings;
    }

    public void OnSave()
    {
        NoesisUnity.SetLicense(licenseName, licenseKey);
    }

    [Header("License")]
    [Tooltip("Fill with the Name value your were given when purchasing your Noesis license")]
    public string licenseName = "";

    [Tooltip("Fill with the Key value your were given when purchasing your Noesis license")]
    public string licenseKey = "";

    [Header("XAML")]
    [Tooltip("Sets a collection of application-scope resources, such as styles and brushes. " +
        "Provides a simple way to support a consistent theme across your application")]
    public NoesisXaml applicationResources;

    [Tooltip("Default value for FontFamily when it is not specified in a control or text element.")]
    public NoesisFont defaultFont;

    [Tooltip("Default value for FontSize when it is not specified in a control or text element")]
    public float defaultFontSize = 15.0f;

    [Tooltip("Default value for FontWeight when it is not specified in a control or text element")]
    public Noesis.FontWeight defaultFontWeight = Noesis.FontWeight.Normal;

    [Tooltip("Default value for FontStretch when it is not specified in a control or text element")]
    public Noesis.FontStretch defaultFontStretch = Noesis.FontStretch.Normal;

    [Tooltip("Default value for FontStyle when it is not specified in a control or text element")]
    public Noesis.FontStyle defaultFontStyle = Noesis.FontStyle.Normal;

    public enum TextureSize
    {
        _256x256,
        _512x512,
        _1024x1024,
        _2048x2048,
        _4096x4096
    }

    public enum OffscreenSampleCount
    {
        _SameAsUnity,
        _1x,
        _2x,
        _4x,
        _8x
    }

    public enum LinearRendering
    {
        _SamesAsUnity,
        _Enabled,
        _Disabled
    }

    [Header("Rendering (*)")]
    [Tooltip("Dimensions of texture used to cache glyphs")]
    public TextureSize glyphTextureSize = TextureSize._1024x1024;

    [Tooltip("Multisampling of offscreen textures")]
    public OffscreenSampleCount offscreenSampleCount = OffscreenSampleCount._1x;

    [Tooltip("Number of offscreen textures created at startup")]
    public uint offscreenInitSurfaces = 0;

    [Tooltip("Max number of offscreen textures (0 = unlimited)")]
    public uint offscreenMaxSurfaces = 0;

    [Tooltip("Enables linear color space")]
    public LinearRendering linearRendering = LinearRendering._SamesAsUnity;

    [Header("Editor Settings")]
    [Tooltip("Enables generation of thumbnails and previews")]
    public bool previewEnabled = true;

    [Tooltip("When enabled XAML AssetPostProcessor logs debug information. Useful to report issues to developers")]
    public bool debugImporter = false;

    [Tooltip("Enables live reloading of resources when game is in Play Mode")]
    public bool hotReloading = false;

    public enum LogVerbosity
    {
        Quiet,
        Normal,
        Bindings
    }

    [Tooltip("Level of log verbosity")]
    public LogVerbosity logVerbosity = LogVerbosity.Quiet;

    [System.Serializable]
    public struct Cursor
    {
        public Texture2D Texture;
        public Vector2 HotSpot;
    }

    [Tooltip("The cursor that appears when an application is starting")]
    public Cursor AppStarting;
    [Tooltip("The Arrow cursor")]
    public Cursor Arrow;
    [Tooltip("The arrow with a compact disk cursor")]
    public Cursor ArrowCD;
    [Tooltip("The crosshair cursor")]
    public Cursor Cross;
    [Tooltip("A hand cursor")]
    public Cursor Hand;
    [Tooltip("A help cursor which is a combination of an arrow and a question mark")]
    public Cursor Help;
    [Tooltip("An I-beam cursor, which is used to show where the text cursor appears when the mouse is clicked")]
    public Cursor IBeam;
    [Tooltip("A cursor with which indicates that a particular region is invalid for a given operation")]
    public Cursor No;
    [Tooltip("A special cursor that is invisible")]
    public Cursor None;
    [Tooltip("A pen cursor")]
    public Cursor Pen;
    [Tooltip("The scroll all cursor")]
    public Cursor ScrollAll;
    [Tooltip("The scroll east cursor")]
    public Cursor ScrollE;
    [Tooltip("The scroll north cursor")]
    public Cursor ScrollN;
    [Tooltip("The scroll northeast cursor")]
    public Cursor ScrollNE;
    [Tooltip("The scroll north/south cursor")]
    public Cursor ScrollNS;
    [Tooltip("A scroll northwest cursor")]
    public Cursor ScrollNW;
    [Tooltip("The scroll south cursor")]
    public Cursor ScrollS;
    [Tooltip("A south/east scrolling cursor")]
    public Cursor ScrollSE;
    [Tooltip("The scroll southwest cursor")]
    public Cursor ScrollSW;
    [Tooltip("The scroll west cursor")]
    public Cursor ScrollW;
    [Tooltip("A west/east scrolling cursor")]
    public Cursor ScrollWE;
    [Tooltip("A four-headed sizing cursor, which consists of four joined arrows that point north, south, east, and west")]
    public Cursor SizeAll;
    [Tooltip("A two-headed northeast/southwest sizing cursor")]
    public Cursor SizeNESW;
    [Tooltip("A two-headed north/south sizing cursor")]
    public Cursor SizeNS;
    [Tooltip("A two-headed northwest/southeast sizing ursor")]
    public Cursor SizeNWSE;
    [Tooltip("A two-headed west/east sizing cursor")]
    public Cursor SizeWE;
    [Tooltip("An up arrow cursor, which is typically used to identify an insertion point")]
    public Cursor UpArrow;
    [Tooltip("Specifies a wait (or hourglass) cursor")]
    public Cursor Wait;
}
