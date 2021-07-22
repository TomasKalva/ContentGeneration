using Noesis;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

/// <summary>
/// Xaml provider
/// </summary>
public class NoesisXamlProvider: XamlProvider
{
    public static NoesisXamlProvider instance = new NoesisXamlProvider();

    NoesisXamlProvider()
    {
        _xamls = new Dictionary<string, Value>();
    }

    public void Register(NoesisXaml xaml)
    {
        string uri = xaml.source;
        Value v;

        if (_xamls.TryGetValue(uri, out v))
        {
            v.refs++;
            v.xaml = xaml;
            _xamls[uri] = v;
        }
        else
        {
            _xamls[uri] = new Value() { refs = 1, xaml = xaml };
        }
    }

    public void Unregister(NoesisXaml xaml)
    {
        string uri = xaml.source;
        Value v;

        if (_xamls.TryGetValue(uri, out v))
        {
            if (v.refs == 1)
            {
                _xamls.Remove(xaml.source);
            }
            else
            {
                v.refs--;
                _xamls[uri] = v;
            }
        }
    }

    public override Stream LoadXaml(string uri)
    {
        Value v;
        if (_xamls.TryGetValue(uri, out v))
        {
            return new MemoryStream(v.xaml.content);
        }

        return null;
    }

    public void ReloadXaml(string uri)
    {
        Value v;
        if (_xamls.TryGetValue(uri, out v))
        {
            RaiseXamlChanged(uri);
        }
    }

    public struct Value
    {
        public int refs;
        public NoesisXaml xaml;
    }

    private Dictionary<string, Value> _xamls;
}

/// <summary>
/// Audio provider
/// </summary>
public class AudioProvider
{
    public static AudioProvider instance = new AudioProvider();

    AudioProvider()
    {
        _audios = new Dictionary<string, Value>();
    }

    public void Register(string uri, UnityEngine.AudioClip audio)
    {
        Value v;
        if (_audios.TryGetValue(uri, out v))
        {
            v.refs++;
            v.audio = audio;
            _audios[uri] = v;
        }
        else
        {
            _audios[uri] = new Value() { refs = 1, audio = audio };
        }
    }

    public void Unregister(string uri)
    {
        Value v;
        if (_audios.TryGetValue(uri, out v))
        {
            if (v.refs == 1)
            {
                _audios.Remove(uri);
            }
            else
            {
                v.refs--;
                _audios[uri] = v;
            }
        }
    }

    public void PlayAudio(string uri, float volume)
    {
        Value v;
        if (_audios.TryGetValue(uri, out v) && v.audio != null)
        {
            UnityEngine.AudioSource.PlayClipAtPoint(v.audio, UnityEngine.Vector3.zero, volume);
        }
        else
        {
            UnityEngine.Debug.LogError("AudioClip not found '" + uri + "'");
        }
    }

    public struct Value
    {
        public int refs;
        public UnityEngine.AudioClip audio;
    }

    private Dictionary<string, Value> _audios;
}

/// <summary>
/// Video provider
/// </summary>
public class VideoProvider
{
    public static VideoProvider instance = new VideoProvider();

    VideoProvider()
    {
        _videos = new Dictionary<string, Value>();
    }

    public void Register(string uri, UnityEngine.Video.VideoClip video)
    {
        Value v;
        if (_videos.TryGetValue(uri, out v))
        {
            v.refs++;
            v.video = video;
            _videos[uri] = v;
        }
        else
        {
            _videos[uri] = new Value() { refs = 1, video = video };
        }
    }

    public void Unregister(string uri)
    {
        Value v;
        if (_videos.TryGetValue(uri, out v))
        {
            if (v.refs == 1)
            {
                _videos.Remove(uri);
            }
            else
            {
                v.refs--;
                _videos[uri] = v;
            }
        }
    }

    public UnityEngine.Video.VideoClip GetVideoClip(string uri)
    {
        Value v;
        if (_videos.TryGetValue(uri, out v) && v.video != null)
        {
            return v.video;
        }
        else
        {
            return null;
        }
    }

    public struct Value
    {
        public int refs;
        public UnityEngine.Video.VideoClip video;
    }

    private Dictionary<string, Value> _videos;
}

/// <summary>
/// Texture provider
/// </summary>
public class NoesisTextureProvider: TextureProvider
{
    public static NoesisTextureProvider instance = new NoesisTextureProvider();

    NoesisTextureProvider()
    {
        Texture.RegisterCallbacks();
        _textures = new Dictionary<string, Value>();
    }

    public void Register(string uri, UnityEngine.Texture texture)
    {
        Value v;
        if (_textures.TryGetValue(uri, out v))
        {
            v.refs++;
            v.texture = texture;
            _textures[uri] = v;
        }
        else
        {
            _textures[uri] = new Value() { refs = 1, texture = texture };
        }
    }

    public void Unregister(string uri)
    {
        Value v;
        if (_textures.TryGetValue(uri, out v))
        {
            if (v.refs == 1)
            {
                _textures.Remove(uri);
            }
            else
            {
                v.refs--;
                _textures[uri] = v;
            }
        }
    }

    public override void GetTextureInfo(string uri, out uint width_, out uint height_)
    {
        int width = 0;
        int height = 0;
        int numLevels = 0;
        System.IntPtr nativePtr = IntPtr.Zero;

        Value v;
        if (_textures.TryGetValue(uri, out v))
        {
            if (v.texture != null)
            {
                width = v.texture.width;
                height = v.texture.height;
                numLevels = v.texture is UnityEngine.Texture2D ? ((UnityEngine.Texture2D)v.texture).mipmapCount : 1;
                nativePtr = v.texture.GetNativeTexturePtr();

#if UNITY_EDITOR
                v.nativePtr = nativePtr;
                _textures[uri] = v;
#endif
            }
        }

        // Send to C++
        Noesis_TextureProviderStoreTextureInfo(swigCPtr.Handle, uri, width, height, numLevels, nativePtr);

        width_ = (uint)width;
        height_ = (uint)height;
    }

#if UNITY_EDITOR
    public bool dirtyTextures = false;

    public void UpdateTextures()
    {
        if (!dirtyTextures) return;
        dirtyTextures = false;

        List<KeyValuePair<string, Value>> textures = new List<KeyValuePair<string, Value>>();

        // Look for textures that have changed their native pointer
        foreach (var kv in _textures)
        {
            if (kv.Value.texture != null)
            {
                string uri = kv.Key;
                Value v = kv.Value;
                v.nativePtr = v.texture.GetNativeTexturePtr();

                if (v.nativePtr != kv.Value.nativePtr)
                {
                    textures.Add(new KeyValuePair<string, Value>(uri, v));
                }
            }
        }

        // Update native pointer in C++ texture provider cache
        foreach (var kv in textures)
        {
            String uri = kv.Key;
            Value v = kv.Value;

            int width = v.texture.width;
            int height = v.texture.height;
            int numLevels = v.texture is UnityEngine.Texture2D ? ((UnityEngine.Texture2D)v.texture).mipmapCount : 1;

            // Send to C++
            Noesis_TextureProviderStoreTextureInfo(swigCPtr.Handle, uri, width, height, numLevels, v.nativePtr);

            _textures[uri] = v;

            // Notify of texture changes to use the new native pointer
            RaiseTextureChanged(uri);
        }
    }
#endif

    public struct Value
    {
        public int refs;
        public UnityEngine.Texture texture;
#if UNITY_EDITOR
        public IntPtr nativePtr;
#endif
    }

    private Dictionary<string, Value> _textures;

    internal new static IntPtr Extend(string typeName)
    {
        return Noesis_TextureProviderExtend(System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(typeName));
    }

    #region Imports
    [DllImport(Library.Name)]
    static extern IntPtr Noesis_TextureProviderExtend(IntPtr typeName);

    [DllImport(Library.Name)]
    static extern void Noesis_TextureProviderStoreTextureInfo(IntPtr cPtr,
        [MarshalAs(UnmanagedType.LPStr)] string filename, int width, int height, int numLevels,
        IntPtr nativePtr);
    #endregion
}

/// <summary>
/// Font provider
/// </summary>
public class NoesisFontProvider: FontProvider
{
    private static LockFontCallback _lockFont = LockFont;
    private static UnlockFontCallback _unlockFont = UnlockFont;
    public static NoesisFontProvider instance = new NoesisFontProvider();

    NoesisFontProvider()
    {
        Noesis_FontProviderSetLockUnlockCallbacks(_lockFont, _unlockFont);
        _fonts = new Dictionary<string, Value>();
    }

    public void Register(NoesisFont font)
    {
        bool register = false;

        string uri = font.source;
        Value v;

        if (_fonts.TryGetValue(uri, out v))
        {
            register = v.font != font;

            v.refs++;
            v.font = font;
            _fonts[uri] = v;
        }
        else
        {
            register = true;
            _fonts[uri] = new Value() { refs = 1, font = font };
        }

        if (register)
        {
            string folder = System.IO.Path.GetDirectoryName(uri);
            string filename = System.IO.Path.GetFileName(uri);
            RegisterFont(folder, filename);
        }
    }

    public void Unregister(NoesisFont font)
    {
        string uri = font.source;
        Value v;

        if (_fonts.TryGetValue(uri, out v))
        {
            if (v.refs == 1)
            {
                _fonts.Remove(uri);
            }
            else
            {
                v.refs--;
                _fonts[uri] = v;
            }
        }
    }

    public void ReloadFont(string uri)
    {
        Value v;
        if (_fonts.TryGetValue(uri, out v))
        {
            // TODO: Review this API, it should be enough to notify with the uri
            List<Face> faces = new List<Face>();

            using (MemoryStream stream = new MemoryStream(v.font.content))
            {
                GUI.EnumFontFaces(stream, (index_, family_, weight_, style_, stretch_) =>
                {
                    faces.Add(new Face()
                    {
                        index = index_, family = family_,
                        weight = weight_, style = style_, stretch = stretch_
                    });
                });
            }

            string folder = System.IO.Path.GetDirectoryName(uri).Replace('\\', '/') + "/";
            foreach (Face face in faces)
            {
                RaiseFontChanged(folder, face.family, face.weight, face.stretch, face.style);
            }
        }
    }

    struct Face
    {
        public int index;
        public string family;
        public FontWeight weight;
        public FontStyle style;
        public FontStretch stretch;
    }

    private delegate void LockFontCallback(string folder, string filename, out IntPtr handle, out IntPtr addr, out int length);
    [MonoPInvokeCallback(typeof(LockFontCallback))]
    private static void LockFont(string folder, string filename, out IntPtr handle, out IntPtr addr, out int length)
    {
        try
        {
            NoesisFontProvider provider = NoesisFontProvider.instance;

            Value v;
            provider._fonts.TryGetValue(folder + "/" + filename, out v);

            if (v.font != null && v.font.content != null)
            {
                GCHandle h = GCHandle.Alloc(v.font.content, GCHandleType.Pinned);
                handle = GCHandle.ToIntPtr(h);
                addr = h.AddrOfPinnedObject();
                length = v.font.content.Length;
                return;
            }
        }
        catch (Exception exception)
        {
            Error.UnhandledException(exception);
        }

        handle = IntPtr.Zero;
        addr = IntPtr.Zero;
        length = 0;
    }

    private delegate void UnlockFontCallback(IntPtr handle);
    [MonoPInvokeCallback(typeof(UnlockFontCallback))]
    private static void UnlockFont(IntPtr handle)
    {
        // In rare cases, the passed handle belongs to a domain already unloaded. That memory has
        // been already deallocated so we can safely ignore the exception
        try
        {
            GCHandle h = GCHandle.FromIntPtr(handle);
            h.Free();
        }
        catch (Exception) {}
    }

    public struct Value
    {
        public int refs;
        public NoesisFont font;
    }

    private Dictionary<string, Value> _fonts;

    internal new static IntPtr Extend(string typeName)
    {
        return Noesis_FontProviderExtend(System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(typeName));
    }

    #region Imports
    [DllImport(Library.Name)]
    static extern IntPtr Noesis_FontProviderExtend(IntPtr typeName);

    [DllImport(Library.Name)]
    static extern void Noesis_FontProviderSetLockUnlockCallbacks(LockFontCallback lockFont, UnlockFontCallback unlockFont);
    #endregion
}