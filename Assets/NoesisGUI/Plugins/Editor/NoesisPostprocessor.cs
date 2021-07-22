using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Video;

/// <summary>
/// Post-processor for XAMLs and Fonts
/// </summary>
public class NoesisPostprocessor : AssetPostprocessor
{
    public static void ImportAllAssets()
    {
        NoesisPostprocessor.ImportAllAssets((progress, asset) => EditorUtility.DisplayProgressBar("Reimport All XAMLs", asset, progress));
        EditorUtility.ClearProgressBar();
    }

    public delegate void UpdateProgress(float progress, string asset);

    public static void ImportAllAssets(UpdateProgress d)
    {
        var assets = AssetDatabase.FindAssets("")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(s => IsFont(s) || IsXaml(s))
            .Distinct().ToArray();

        NoesisPostprocessor.ImportAssets(assets, false, d);
    }

    private static void ImportAssets(string[] assets, bool reload, UpdateProgress d)
    {
        int numFonts = assets.Count(asset => asset.StartsWith("Assets/") && IsFont(asset));
        int numXamls = assets.Count(asset => asset.StartsWith("Assets/") && IsXaml(asset));
        int numAssets = numFonts + numXamls;

        if (numAssets > 0)
        {
            Log("→ Import assets (XAMLs: " + numXamls + " Fonts: " + numFonts + ")");

            float delta = 1.0f / numAssets;
            float progress = 0.0f;

            if (numXamls > 0)
            {
                NoesisUnity.Init();

                // Theme
                NoesisXaml theme = NoesisSettings.Get().applicationResources;
                if (theme != null)
                {
                    Log("Scanning for theme changes...");

                    bool changed;
                    ImportXaml(theme.source, false, false, out changed);

                    if (changed)
                    {
                        Log("↔ Reload ApplicationResources");
                        NoesisUnity.LoadApplicationResources();
                    }
                }
            }

            foreach (var asset in assets)
            {
                // Make sure read-only folders from Package Manager are not processed
                if (asset.StartsWith("Assets/"))
                {
                    try
                    {
                        if (IsFont(asset))
                        {
                            ImportFont(asset, true, reload);
                        }
                        else if (IsXaml(asset))
                        {
                            ImportXaml(asset, true, reload);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    if (d != null && (IsFont(asset) || IsXaml(asset)))
                    {
                        d(progress, asset);
                        progress += delta;
                    }
                }
            }

            Log("← Import assets");
        }
    }

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (NoesisSettings.IsNoesisEnabled())
        {
            EditorApplication.CallbackFunction d = null;

            // Delay the import process to have all texture assets ready
            d = () =>
            {
                EditorApplication.update -= d;

                string[] assets = importedAssets.Concat(movedAssets).ToArray();
                ImportAssets(assets,  NoesisSettings.Get().hotReloading, (progress, asset) =>
                    EditorUtility.DisplayProgressBar("Import XAMLs", asset, progress));
                EditorUtility.ClearProgressBar();
            };

            EditorApplication.update += d;
        }
    }

    private static bool HasExtension(string filename, string extension)
    {
        return filename.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsXaml(string filename)
    {
        return HasExtension(filename, ".xaml");
    }

    private static bool IsFont(string filename)
    {
        return HasExtension(filename, ".ttf") || HasExtension(filename, ".otf") || HasExtension(filename, ".ttc");
    }

    private static NoesisFont ImportFont(string filename, bool reimport, bool reload)
    {
        string path = Path.ChangeExtension(filename, ".asset");
        NoesisFont font = AssetDatabase.LoadAssetAtPath<NoesisFont>(path);

        if (font == null)
        {
            Log("↔ Create " + filename);
            font = (NoesisFont)ScriptableObject.CreateInstance(typeof(NoesisFont));
            AssetDatabase.CreateAsset(font, path);
        }

        byte[] content = File.ReadAllBytes(filename);

        if (reimport || font.content == null || !font.content.SequenceEqual(content) || font.source != filename)
        {
            Log("→ ImportFont " + filename);
            font.source = filename;
            font.content = content;
            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();
            Log("← ImportFont");
        }

        // Hot reloading of font
        if (NoesisUnity.Initialized && reimport && reload)
        {
            ReloadFont(font.source);
        }

        return font;
    }

    private static void ReloadFont(string uri)
    {
        Log("ReloadFont " + uri);
        NoesisFontProvider.instance.ReloadFont(uri);
    }

    private static void ScanFont(string uri, ref List<NoesisFont> fonts)
    {
        int index = uri.IndexOf('#');
        if (index != -1)
        {
            string folder = uri.Substring(0, index);
            if (Directory.Exists(folder))
            {
                string family = uri.Substring(index + 1);
                var files = Directory.GetFiles(folder).Where(s => IsFont(s));

                foreach (var font in files)
                {
                    bool hasFamily = false;

                    using (FileStream file = File.Open(font, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        hasFamily = NoesisUnity.HasFamily(file, family);
                    }

                    if (hasFamily)
                    {
                        fonts.Add(ImportFont(font, false, false));
                    }
                }
            }
        }
    }

    private static void ScanTexture(string uri, ref List<Texture> textures)
    {
        if (!String.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(uri)))
        {
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(uri);

            if (texture != null)
            {
                textures.Add(texture);
            }
        }
    }

    private static void ScanAudio(string uri, ref List<AudioClip> audios)
    {
        if (!String.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(uri)))
        {
            AudioClip audio = AssetDatabase.LoadAssetAtPath<AudioClip>(uri);

            if (audio != null)
            {
                audios.Add(audio);
            }
        }
    }

    private static void ScanVideo(string uri, ref List<VideoClip> videos)
    {
        if (!String.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(uri)))
        {
            VideoClip video = AssetDatabase.LoadAssetAtPath<VideoClip>(uri);

            if (video != null)
            {
                videos.Add(video);
            }
        }
    }

    private static void ScanXaml(string uri, ref List<NoesisXaml> xamls)
    {
        if (IsXaml(uri))
        {
            if (File.Exists(uri))
            {
                xamls.Add(ImportXaml(uri, false, false));
            }
        }
    }

    private static void ScanDependencies(string filename, out List<NoesisFont> fonts_, out List<Texture> textures_, out List<AudioClip> audios_, out List<VideoClip> videos_, out List<NoesisXaml> xamls_)
    {
        List<NoesisFont> fonts = new List<NoesisFont>();
        List<Texture> textures = new List<Texture>();
        List<AudioClip> audios = new List<AudioClip>();
        List<VideoClip> videos = new List<VideoClip>();
        List<NoesisXaml> xamls = new List<NoesisXaml>();

        try
        {
            using (FileStream file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                string directory = Path.GetDirectoryName(filename);
                Noesis.GUI.GetXamlDependencies(file, directory, (uri, type) =>
                {
                    try
                    {
                        if (type == Noesis.XamlDependencyType.Filename)
                        {
                            ScanXaml(uri, ref xamls);
                            ScanTexture(uri, ref textures);
                            ScanAudio(uri, ref audios);
                            ScanVideo(uri, ref videos);
                        }
                        else if (type == Noesis.XamlDependencyType.Font)
                        {
                            ScanFont(uri, ref fonts);
                        }
                        else if (type == Noesis.XamlDependencyType.UserControl)
                        {
                            string userControl = AssetDatabase.FindAssets("")
                                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                                .Where(s => String.Equals(Path.GetFileName(s), uri + ".xaml", StringComparison.OrdinalIgnoreCase))
                                .FirstOrDefault();

                            if (!String.IsNullOrEmpty(userControl) && userControl != filename)
                            {
                                ScanXaml(userControl, ref xamls);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                         Debug.LogException(e);
                    }
                });
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        fonts_ = fonts;
        textures_ = textures;
        audios_ = audios;
        videos_ = videos;
        xamls_ = xamls;
    }

    private static NoesisXaml ImportXaml(string filename, bool reimport, bool reload)
    {
        bool changed;
        return ImportXaml(filename, reimport, reload, out changed);
    }

    // A user control xaml can reference the same user control when used in a template, creating a circular
    // dependency (from the same xaml or from a merged dictionary) that we need to break. We are using
    // the following stack to detect when a xaml is being processed to stop scanning for its dependencies
    private static Stack<string> _xamlDependencyStack = new Stack<string>();

    private static NoesisXaml ImportXaml(string filename, bool reimport, bool reload, out bool changed)
    {
        changed = false;
        string path = Path.ChangeExtension(filename, ".asset");
        NoesisXaml xaml = AssetDatabase.LoadAssetAtPath<NoesisXaml>(path);

        if (xaml == null)
        {
            Log("↔ Create " + filename);
            xaml = (NoesisXaml)ScriptableObject.CreateInstance(typeof(NoesisXaml));
            AssetDatabase.CreateAsset(xaml, path);
        }

        // Unify CRLF,LF between Windows and macOS
        String text = File.ReadAllText(filename);
        text = text.Replace("\r\n", "\n");
        byte[] content = System.Text.Encoding.UTF8.GetBytes(text);

        // Keep track of the xaml being imported
        _xamlDependencyStack.Push(filename);

        if (reimport || xaml.content == null || !xaml.content.SequenceEqual(content) || xaml.source != filename)
        {
            Log("→ ImportXaml " + filename);
            changed = true;

            xaml.source = filename;
            xaml.content = content;

            List<NoesisFont> fonts;
            List<Texture> textures;
            List<AudioClip> audios;
            List<VideoClip> videos;
            List<NoesisXaml> xamls;
            xaml.UnregisterDependencies();
            ScanDependencies(filename, out fonts, out textures, out audios, out videos, out xamls);

            xaml.xamls = xamls.ToArray();
            xaml.textures = textures.Select(t => new NoesisXaml.Texture { uri = AssetDatabase.GetAssetPath(t), texture = t} ).ToArray();
            xaml.audios = audios.Select(t => new NoesisXaml.Audio { uri = AssetDatabase.GetAssetPath(t), audio = t} ).ToArray();
            xaml.videos = videos.Select(t => new NoesisXaml.Video { uri = AssetDatabase.GetAssetPath(t), video = t }).ToArray();
            xaml.fonts = fonts.ToArray();
            xaml.RegisterDependencies();

            EditorUtility.SetDirty(xaml);
            AssetDatabase.SaveAssets();
            Log("← ImportXaml");
        }
        else
        {
            // XAML didn't change, let's continue scanning its dependencies
            foreach (var dep in xaml.xamls)
            {
                if (dep && File.Exists(dep.source) && !_xamlDependencyStack.Contains(dep.source))
                {
                    bool changed_;
                    ImportXaml(dep.source, false, false, out changed_);
                    changed = changed || changed_;
                }
            }

            foreach (var dep in xaml.fonts)
            {
                if (dep && File.Exists(dep.source))
                {
                    ImportFont(dep.source, false, false);
                }
            }
        }

        // Dependencies scan finished, remove it from the stack
        _xamlDependencyStack.Pop();

        if (reimport)
        {
            // Show parsing errors in the console
            try
            {
                xaml.Load();

                if (reload)
                {
                    ReloadXaml(xaml.source);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e, xaml);
            }
        }

        return xaml;
    }

    private static void ReloadXaml(string uri)
    {
        Log("ReloadXaml" + uri);

        NoesisUnity.MuteLog();
        NoesisXamlProvider.instance.ReloadXaml(uri);
        NoesisUnity.UnmuteLog();
    }

    private void OnPreprocessTexture()
    {
        if (AssetDatabase.GetLabels(assetImporter).Contains("Noesis") || assetPath.StartsWith("Assets/NoesisGUI/Samples"))
        {
            // If the texture is going to be modified it is required to be readable
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.isReadable = true;
        }
    }

    private void OnPostprocessTexture(Texture2D texture)
    {
        // Although our samples use the label 'Noesis' it seems to be ignored by Unity when the package is being imported
        if (AssetDatabase.GetLabels(assetImporter).Contains("Noesis") || assetPath.StartsWith("Assets/NoesisGUI/Samples"))
        {
            Color[] c = texture.GetPixels(0);

            // NoesisGUI needs premultipled alpha
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                for (int i = 0; i < c.Length; i++)
                {
                    c[i].r = Mathf.LinearToGammaSpace(Mathf.GammaToLinearSpace(c[i].r) * c[i].a);
                    c[i].g = Mathf.LinearToGammaSpace(Mathf.GammaToLinearSpace(c[i].g) * c[i].a);
                    c[i].b = Mathf.LinearToGammaSpace(Mathf.GammaToLinearSpace(c[i].b) * c[i].a);
                }
            }
            else
            {
                for (int i = 0; i < c.Length; i++)
                {
                    c[i].r = c[i].r * c[i].a;
                    c[i].g = c[i].g * c[i].a;
                    c[i].b = c[i].b * c[i].a;
                }
            }

            // Set new content and make the texture unreadable at runtime
            texture.SetPixels(c, 0);
            texture.Apply(true, true);
        }

        if (NoesisUnity.Initialized)
        {
            // Reloading of texture
            UpdateTextures();
        }
    }

    private static Action<PlayModeStateChange> _updateTextures = null;

    private static void UpdateTextures()
    {
        // Texture native pointer is invalidated, update it before next frame is rendered. We cannot query
        // the new native pointer right now (during the import process) because it is not yet created
        NoesisTextureProvider.instance.dirtyTextures = true;

        // NOTE: When a texture is modified (while playing or not), next time Play is clicked it recreates
        // all live textures, so we need to set 'dirtyTextures' flag just after exiting Edit mode to
        // update texture native pointers before the first frame is rendered
        if (_updateTextures == null)
        {
            _updateTextures = (mode) =>
            {
                if (mode == PlayModeStateChange.ExitingEditMode)
                {
                    EditorApplication.playModeStateChanged -= _updateTextures;
                    _updateTextures = null;

                    NoesisTextureProvider.instance.dirtyTextures = true;
                }
            };

            EditorApplication.playModeStateChanged += _updateTextures;
        }
    }

    private static void Log(string message)
    {
        if (NoesisSettings.Get().debugImporter)
        {
            Debug.Log(message);
        }
    }
}
