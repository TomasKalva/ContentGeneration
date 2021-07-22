using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public class NoesisUpdater
{
    static NoesisUpdater()
    {
        EditorApplication.update += CheckVersion;
    }

    private static void CheckVersion()
    {
        EditorApplication.update -= CheckVersion;

        if (!UnityEditorInternal.InternalEditorUtility.inBatchMode)
        {
            string localVersion = NoesisVersion.GetCached();
            string version = NoesisVersion.Get();

            // Remove the file that indicates Noesis package is being installed
            AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/Editor/installing");

            // Detect if /Library is being recreated
            string noesisFile = Path.Combine(Application.dataPath, "../Library/noesis");
            bool libraryFolderRecreated = !File.Exists(noesisFile);
            if (libraryFolderRecreated)
            {
                File.Create(noesisFile).Dispose();
            }

            if (localVersion != version && version != "0.0.0")
            {
                if (NoesisVersion.RestartNeeded())
                {
                    Debug.LogWarning("Please restart Unity to reload NoesisGUI native plugin! " + 
                        "If error persists remove 'Assets/NoesisGUI/Plugins' and reimport again.");
                    return;
                }

                string title;

                if (localVersion != "")
                {
                    title = "Upgrading NoesisGUI " + localVersion + " -> " + version;
                }
                else
                {
                    title = "Installing NoesisGUI " +  version;
                }

                EditorUtility.DisplayProgressBar(title, "", 0.0f);
                GoogleAnalyticsHelper.LogEvent("Install", version, 0);

                EditorUtility.DisplayProgressBar(title, "Upgrading project", 0.10f);
                Upgrade(localVersion);

                EditorUtility.DisplayProgressBar(title, "Updating version", 0.20f);
                NoesisVersion.SetCached(version);

                EditorUtility.DisplayProgressBar(title, "Creating default settings", 0.35f);
                NoesisSettings.Get();

                EditorUtility.DisplayProgressBar(title, "Extracting documentation...", 0.40f);
                ExtractTar("NoesisGUI/Doc.tar", "/../NoesisDoc", "/../NoesisDoc");

                EditorUtility.DisplayProgressBar(title, "Extracting blend samples...", 0.55f);
                ExtractTar("NoesisGUI/Samples/Samples-blend.tar", "/..", "/../Blend");

                NoesisPostprocessor.ImportAllAssets((progress, asset) =>
                {
                    EditorUtility.DisplayProgressBar(title, asset, 0.60f + progress * 0.40f);
                });

                EditorApplication.update += ShowWelcomeWindow;
                EditorUtility.ClearProgressBar();

                Debug.Log("NoesisGUI v" + version + " successfully installed");
            }
            else if (libraryFolderRecreated)
            {
                NoesisPostprocessor.ImportAllAssets();
            }
        }
    }

    private static void ShowWelcomeWindow()
    {
        EditorApplication.update -= ShowWelcomeWindow;
        NoesisWelcome.Open();
    }

    private static string NormalizeVersion(string version)
    {
        string pattern = @"^(\d+).(\d+).(\d+)((a|b|rc|f)(\d*))?$";
        var match = Regex.Match(version.ToLower(), pattern);

        string normalized = "";

        if (match.Success)
        {
            normalized += match.Groups[1].Value.PadLeft(2, '0');
            normalized += ".";
            normalized += match.Groups[2].Value.PadLeft(2, '0');
            normalized += ".";
            normalized += match.Groups[3].Value.PadLeft(2, '0');

            if (match.Groups[4].Length > 0)
            {
                if (match.Groups[5].Value == "a")
                {
                    normalized += ".0.";
                }
                else if (match.Groups[5].Value == "b")
                {
                    normalized += ".1.";
                }
                else if (match.Groups[5].Value == "rc")
                {
                    normalized += ".2.";
                }
                else if (match.Groups[5].Value == "f")
                {
                    normalized += ".3.";
                }

                normalized += match.Groups[6].Value.PadLeft(2, '0');
            }
            else
            {
                normalized += ".3.00";
            }
        }
        else
        {
            Debug.LogError("Unexpected version format " + version);
        }

        return normalized;
    }

    private static bool PatchNeeded(string from, string to)
    {
        if (from.Length == 0)
        {
            return false;
        }
        else
        {
            return String.Compare(NormalizeVersion(from), NormalizeVersion(to)) < 0;
        }
    }

    private static void Upgrade(string version)
    {
        if (PatchNeeded(version, "1.3.0a1"))
        {
            Debug.LogError("Upgrading from '" + version + "' not supported. Please install in a clean project");
        }

        if (PatchNeeded(version, "2.1.0b10"))
        {
            Upgrade_2_1_0_b10();
        }

        if (PatchNeeded(version, "2.1.0rc4"))
        {
            Upgrade_2_1_0_rc4();
        }
        
        if (PatchNeeded(version, "2.2.0b6"))
        {
            Upgrade_2_2_0_b6();
        }

        if (PatchNeeded(version, "2.2.3"))
        {
            Upgrade_2_2_3();
        }

        if (PatchNeeded(version, "3.0.0rc7"))
        {
            Upgrade_3_0_0_rc7();
        }

        if (PatchNeeded(version, "3.0.0"))
        {
            Upgrade_3_0_0();
        }

        if (PatchNeeded(version, "3.0.7"))
        {
            Upgrade_3_0_7();
        }

        RemoveEmptyScripts();
    }

    private static void Upgrade_2_1_0_b10()
    {
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Common");
    }

    private static void Upgrade_2_1_0_rc4()
    {
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/TicTacToe");
    }

    private static void Upgrade_2_2_0_b6()
    {
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Buttons/ControlResources.xaml");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Buttons/ControlResources.asset");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Samples/Buttons/LogoResources.xaml", "Assets/NoesisGUI/Samples/Buttons/Resources.xaml");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Samples/Buttons/LogoResources.asset", "Assets/NoesisGUI/Samples/Buttons/Resources.asset");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Buttons/ElementExtensions.cs");

        AssetDatabase.MoveAsset("Assets/NoesisGUI/Samples/ControlGallery/Resources", "Assets/NoesisGUI/Samples/ControlGallery/Data");

        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Localization/ControlResources.xaml");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Localization/ControlResources.asset");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Samples/Localization/LogoResources.xaml", "Assets/NoesisGUI/Samples/Localization/Resources.xaml");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Samples/Localization/LogoResources.asset", "Assets/NoesisGUI/Samples/Localization/Resources.asset");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Localization/rounded-mgenplus-1c-regular.ttf");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Localization/rounded-mgenplus-1c-regular.asset");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Localization/XamlDependencies.cs");
        
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Login/ControlResources.xaml");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Login/ControlResources.asset");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Login/ElementExtensions.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Samples/Login/LogoResources.xaml", "Assets/NoesisGUI/Samples/Login/Resources.xaml");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Samples/Login/LogoResources.asset", "Assets/NoesisGUI/Samples/Login/Resources.asset");

        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/QuestLog/ElementExtensions.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Samples/QuestLog/LogoResources.xaml", "Assets/NoesisGUI/Samples/QuestLog/Resources.xaml");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Samples/QuestLog/LogoResources.asset", "Assets/NoesisGUI/Samples/QuestLog/Resources.asset");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/QuestLog/Images/QuestImages.xaml");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/QuestLog/Images/QuestImages.asset");

        AssetDatabase.MoveAsset("Assets/NoesisGUI/Samples/Scoreboard/Game.cs", "Assets/NoesisGUI/Samples/Scoreboard/ViewModel.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/Scoreboard/Player.cs");
    }

    private static void Upgrade_2_2_3()
    {
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisCallbackAttribute.cs", "Assets/NoesisGUI/Plugins/API/Core/CallbackAttribute.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisDragDrop.cs", "Assets/NoesisGUI/Plugins/API/Core/DragDrop.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisError.cs", "Assets/NoesisGUI/Plugins/API/Core/Error.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisEvents.cs", "Assets/NoesisGUI/Plugins/API/Core/Events.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisExtend.cs", "Assets/NoesisGUI/Plugins/API/Core/Extend.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisExtendBoxing.cs", "Assets/NoesisGUI/Plugins/API/Core/ExtendBoxing.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisExtendImports.cs", "Assets/NoesisGUI/Plugins/API/Core/ExtendImports.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisExtendProps.cs", "Assets/NoesisGUI/Plugins/API/Core/ExtendProps.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisHitTest.cs", "Assets/NoesisGUI/Plugins/API/Core/HitTest.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisLog.cs", "Assets/NoesisGUI/Plugins/API/Core/Log.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisRenderDevice.cs", "Assets/NoesisGUI/Plugins/API/Core/RenderDevice.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisRenderer.cs", "Assets/NoesisGUI/Plugins/API/Core/RendererNoesis.cs");
        AssetDatabase.MoveAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisUriHelper.cs", "Assets/NoesisGUI/Plugins/API/Core/UriHelper.cs");
    }

    private static void Upgrade_3_0_0_rc7()
    {
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/Libraries/MacOS/Noesis.bundle");
    }

    private static void Upgrade_3_0_0()
    {
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/ControlGallery");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Samples/ControlGallery.unity");
    }

    private static void Upgrade_3_0_7()
    {
        if (File.Exists(Application.dataPath + "/NoesisGUI/Settings/Resources/NoesisSettings.asset"))
        { 
            Directory.CreateDirectory(Application.dataPath + "/Resources");
            AssetDatabase.MoveAsset("Assets/NoesisGUI/Settings/Resources/NoesisSettings.asset", "Assets/Resources/NoesisSettings.asset");
            Directory.Delete(Application.dataPath + "/NoesisGUI/Settings", true);
            Debug.Log("'NoesisSettings.assets' has been moved to 'Assets/Resources'. Please move it to a different 'Resources' folder if needed");
        } 
    }

    private static void RemoveEmptyScripts()
    {
        // From time to time we need to rename scripts (for example Unity doesn't like having a script called Grid.cs)
        // As there is no way to do the rename when instaling the unity package we need to do this trick: in the 
        // unity package both the renamed script and the original one (empty) are included. That way, the compilation
        // phase will succeed. After that, just at this point we remove the empty scripts
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/Collection.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/FrameworkOptions.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/FreezableCollection.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/Grid.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/KeyStates.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/ManipulationModes.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/Matrix2.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/Matrix3.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/Matrix3DProjection.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/MouseState.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/PlaneProjection.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/Pointi.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/Projection.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/Recti.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/ResourceKeyType.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/Sizei.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/TimelineEventArgs.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/Transform2.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/Transform3.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Proxies/Vector3.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisSoftwareKeyboard.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/API/Core/NoesisView.cs");
        AssetDatabase.DeleteAsset("Assets/NoesisGUI/Plugins/NoesisSoftwareKeyboard.cs");
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parentFolder = System.IO.Path.GetDirectoryName(path);
            string newFolder = System.IO.Path.GetFileName(path);

            AssetDatabase.CreateFolder(parentFolder, newFolder);
        }
    }

    private static void DeleteFolder(string folder)
    {
        try
        {
            string path = Application.dataPath + folder;
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch (Exception) { }
    }

    private static void ExtractTar(string tarFile, string outputPath, string deletePath)
    {
        string tarPath = Path.Combine(Application.dataPath, tarFile);

        if (File.Exists(tarPath))
        {
            DeleteFolder(deletePath);

            string destPath = Application.dataPath + outputPath;
            byte[] buffer = new byte[512];

            using (var tar = File.OpenRead(tarPath))
            {
                while (tar.Read(buffer, 0, 512) > 0)
                {
                    string filename = Encoding.ASCII.GetString(buffer, 0, 100).Trim((char)0);

                    if (!String.IsNullOrEmpty(filename))
                    {
                        long size = Convert.ToInt64(Encoding.ASCII.GetString(buffer, 124, 11).Trim(), 8);

                        if (size > 0)
                        {
                            string path = destPath + "/" + filename;
                            Directory.CreateDirectory(Path.GetDirectoryName(path));

                            using (var file = File.Create(path))
                            {
                                long blocks = (size + 511) / 512;
                                for (int i = 0; i < blocks; i++)
                                {
                                    tar.Read(buffer, 0, 512);
                                    file.Write(buffer, 0, (Int32)Math.Min(size, 512));
                                    size -= 512;
                                }
                            }
                        }
                    }
                }
            }

            AssetDatabase.DeleteAsset(Path.Combine("Assets", tarFile));
        }
    }
}
