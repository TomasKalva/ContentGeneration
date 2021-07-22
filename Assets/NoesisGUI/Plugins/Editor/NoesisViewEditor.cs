using System;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NoesisView))]
public class NoesisViewEditor : Editor
{
    enum RenderMode
    {
        None,
        Wireframe,
        Batches,
        Overdraw
    };

    private RenderMode ToRenderMode(Noesis.RenderFlags renderFlags)
    {
        if ((renderFlags & Noesis.RenderFlags.Wireframe) > 0)
        {
            return RenderMode.Wireframe;
        }
        else if ((renderFlags & Noesis.RenderFlags.ColorBatches) > 0)
        {
            return RenderMode.Batches;
        }
        else if ((renderFlags & Noesis.RenderFlags.Overdraw) > 0)
        {
            return RenderMode.Overdraw;
        }
        else
        {
            return RenderMode.None;
        }
    }

    private Noesis.RenderFlags ToRenderFlags(RenderMode renderMode)
    {
        if (renderMode == RenderMode.Wireframe)
        {
            return Noesis.RenderFlags.Wireframe;
        }
        else if (renderMode == RenderMode.Batches)
        {
            return Noesis.RenderFlags.ColorBatches;
        }
        else if (renderMode == RenderMode.Overdraw)
        {
            return Noesis.RenderFlags.Overdraw;
        }

        return 0;
    }

    public override void OnInspectorGUI()
    {
        NoesisView view = target as NoesisView;

        // Register changes in the component so scene can be saved, and Undo is also enabled
        Undo.RecordObject(view, "Noesis View");

        EditorGUILayout.LabelField(new GUIContent("Render Mode", "Views attached to camera objects work in 'Camera Overlay' mode. 'Render Texture' mode is enabled in all other cases"),
            new GUIContent(view.IsRenderToTexture() ? "Render Texture" : "Camera Overlay"), EditorStyles.popup);

        if (view.IsRenderToTexture())
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Target Texture", "The texture to render this View into"));
            view.Texture = (RenderTexture)EditorGUILayout.ObjectField(view.Texture, typeof(RenderTexture), false);
            EditorGUILayout.EndHorizontal();
            view.ContinuousRendering = EditorGUILayout.Toggle(new GUIContent("Continuous Rendering", "When continuous rendering is disabled, rendering only happens when UI changes." +
                " For performance purposes and to save battery this is the default mode when rendering to texture.\n\nThis flag is not available in 'Camera Overlay' mode and instead the property " +
                "NoesisView.NeedsRendering must be used with a manually repainted camera."), view.ContinuousRendering);
        }

        EditorGUILayout.Space();
        view.Xaml = (NoesisXaml)EditorGUILayout.ObjectField(new GUIContent("XAML", "User interface definition XAML"), view.Xaml, typeof(NoesisXaml), false);

        EditorGUILayout.BeginHorizontal();
        GUIContent[] options = { new GUIContent("Low Quality"), new GUIContent("Medium Quality"), new GUIContent("High Quality"), new GUIContent("Custom Quality") };
        int[] values = { 0, 1, 2, 3};
        float inMaxError = view.TessellationMaxPixelError;
        int value = inMaxError == 0.7f ? 0 : inMaxError == 0.4f ? 1 : inMaxError == 0.2f ? 2 : 3;
        value = EditorGUILayout.IntPopup(new GUIContent("Tessellation Pixel Error", "Tessellation curve tolerance in screen space. " + 
            "'Medium Quality' is usually fine for PPAA (non-multisampled) while 'High Quality' is the recommended pixel error if you are rendering to a 8x multisampled surface"),
            value, options, values);
        float outMaxError = value == 0 ? 0.7f : value == 1 ? 0.4f : value == 2 ? 0.2f: inMaxError;
        view.TessellationMaxPixelError = Math.Max(0.01f, EditorGUILayout.FloatField(outMaxError, GUILayout.Width(64)));
        EditorGUILayout.EndHorizontal();

        Noesis.RenderFlags renderFlags = view.RenderFlags;

        RenderMode renderMode = (RenderMode)EditorGUILayout.EnumPopup(new GUIContent("Debug Render Flags",
            "Enables debugging render flags." + 
            "\n\n- Wireframe: toggles wireframe mode when rendering triangles" + 
            "\n\n- ColorBatches: each batch submitted to the GPU is given a unique solid color" + 
            "\n\n- Overdraw: displays pixel overdraw using blending layers. Different colors are used for each type of triangles." +
                "'Green' for normal ones, 'Red' for opacities and 'Blue' for clipping masks"
            ), ToRenderMode(renderFlags));
        renderFlags = (renderFlags & ~(Noesis.RenderFlags.Wireframe | Noesis.RenderFlags.ColorBatches | Noesis.RenderFlags.Overdraw))
            | ToRenderFlags(renderMode);

        bool ppaa = EditorGUILayout.Toggle(new GUIContent("Enable PPAA", 
            "Per-Primitive Antialiasing extrudes the contours of the geometry and smooths them. " +
            "It is a 'cheap' antialiasing algorithm useful when GPU MSAA is not enabled"),
            (renderFlags & Noesis.RenderFlags.PPAA) > 0);
        renderFlags = (renderFlags & ~Noesis.RenderFlags.PPAA) | (ppaa ? Noesis.RenderFlags.PPAA : 0);

        bool lcd = EditorGUILayout.Toggle(new GUIContent("Subpixel Rendering", 
            "Enables subpixel rendering compatible with LCD displays"),
            (renderFlags & Noesis.RenderFlags.LCD) > 0);
        renderFlags = (renderFlags & ~Noesis.RenderFlags.LCD) | (lcd ? Noesis.RenderFlags.LCD : 0);

        view.RenderFlags = renderFlags;

        EditorGUILayout.Space();

        view.EnableExternalUpdate = EditorGUILayout.Toggle(new GUIContent("External Update",
            "When enabled, the view must be explicitly updated by calling 'ExternalUpdate()'. " +
            "By default, the view is automatically updated during LateUpdate"
            ), view.EnableExternalUpdate);

        view.EnableKeyboard = EditorGUILayout.Toggle(new GUIContent("Enable Keyboard",
            "Indicates if keyboard input events are processed by this view"), view.EnableKeyboard);

        view.EnableMouse = EditorGUILayout.Toggle(new GUIContent("Enable Mouse",
            "Indicates if mouse input events are processed by this view"), view.EnableMouse);

        view.EnableTouch = EditorGUILayout.Toggle(new GUIContent("Enable Touch",
            "Indicates if touch input events are processed by this view"), view.EnableTouch);

        view.EnableGamepad = EditorGUILayout.Toggle(new GUIContent("Enable Gamepad",
            "Indicates if gamepad input events are processed by this view"), view.EnableGamepad);

        view.EmulateTouch = EditorGUILayout.Toggle(new GUIContent("Emulate Touch",
            "Indicates if touch input events are emulated by the Mouse"), view.EmulateTouch);

        view.UseRealTimeClock = EditorGUILayout.Toggle(new GUIContent("Real Time Clock",
            "Indicates if 'Time.realtimeSinceStartup' is used instead of 'Time.time' for animations"), view.UseRealTimeClock);
    }

    public override bool HasPreviewGUI()
    {
        return Application.isPlaying;
    }

    public override bool RequiresConstantRepaint()
    {
        return Application.isPlaying;
    }

    private GUIStyle _previewHeaderStyle;
    private GUIStyle _previewTextStyle;

    public override void OnPreviewGUI(Rect rect_, GUIStyle background)
    {
        NoesisView view = target as NoesisView;
        Noesis.ViewStats stats = view.GetStats();

        if (_previewHeaderStyle == null)
        {
#if UNITY_2020_1_OR_NEWER
            _previewHeaderStyle = new GUIStyle(EditorStyles.largeLabel)
#else
            _previewHeaderStyle = new GUIStyle(EditorStyles.whiteLargeLabel)
#endif
            {
                alignment = TextAnchor.UpperLeft,
                fontStyle = FontStyle.Bold
            };
        }
        if (_previewTextStyle == null)
        {
#if UNITY_2020_1_OR_NEWER
            _previewTextStyle = new GUIStyle(EditorStyles.label)
#else
            _previewTextStyle = new GUIStyle(EditorStyles.whiteLabel)
#endif
            {
                richText = true,
                fontStyle = FontStyle.Normal
            };
        }

        GUI.Label(new Rect(rect_.x + 5, rect_.y + 5, rect_.width, rect_.height), view.Xaml.source, _previewHeaderStyle);

        StringBuilder left = new StringBuilder();
        left.AppendLine("\n\nFrame Time (ms)");
        left.AppendLine("Update Time (ms)");
        left.AppendLine("Render Time (ms)");
        left.AppendLine();
        left.AppendLine("Triangles");
        left.AppendLine("Draws");
        left.AppendLine("Batches");
        left.AppendLine("Tessellations");
        left.AppendLine("Geometry Size (kB)");
        left.AppendLine("Flushes");
        left.AppendLine();
        left.AppendLine("Stencil Masks");
        left.AppendLine("Opacity Groups");
        left.AppendLine("RT Switches");
        left.AppendLine();
        left.AppendLine("Ramps Uploaded");
        left.AppendLine("Rasterized Glyphs");
        left.AppendLine("Discarded Glyph Tiles");

        _previewTextStyle.alignment = TextAnchor.UpperLeft;
        GUI.Label(new Rect(rect_.x + 15, rect_.y + 5, 220, 500), left.ToString(), _previewTextStyle);

        var format = new System.Globalization.NumberFormatInfo { NumberDecimalSeparator = "." };

        StringBuilder right = new StringBuilder();
        right.AppendLine("\n\n<b>" + stats.FrameTime.ToString("#,##0.00", format) + "</b>");
        right.AppendLine("<b>" + stats.UpdateTime.ToString("#,##0.00", format) + "</b>");
        right.AppendLine("<b>" + stats.RenderTime.ToString("#,##0.00", format) + "</b>");
        right.AppendLine();
        right.AppendLine("<b>" + stats.Triangles + "</b>");
        right.AppendLine("<b>" + stats.Draws + "</b>");
        right.AppendLine("<b>" + stats.Batches + "</b>");
        right.AppendLine("<b>" + stats.Tessellations + "</b>");
        right.AppendLine("<b>" + String.Format("{0:F0}", stats.GeometrySize / 1024) + "</b>");
        right.AppendLine("<b>" + stats.Flushes + "</b>");
        right.AppendLine();
        right.AppendLine("<b>" + stats.Masks + "</b>");
        right.AppendLine("<b>" + stats.Opacities + "</b>");
        right.AppendLine("<b>" + stats.RenderTargetSwitches + "</b>");
        right.AppendLine();
        right.AppendLine("<b>" + stats.UploadedRamps + "</b>");
        right.AppendLine("<b>" + stats.RasterizedGlyphs + "</b>");
        right.AppendLine("<b>" + stats.DiscardedGlyphTiles + "</b>");

        _previewTextStyle.alignment = TextAnchor.UpperRight;
        GUI.Label(new Rect(rect_.x + 15, rect_.y + 5, 220, 500), right.ToString(), _previewTextStyle);
    }
}
