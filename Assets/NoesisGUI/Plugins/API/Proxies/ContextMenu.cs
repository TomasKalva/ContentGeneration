//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.10
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


using System;
using System.Runtime.InteropServices;

namespace Noesis
{

public class ContextMenu : MenuBase {
  internal new static ContextMenu CreateProxy(IntPtr cPtr, bool cMemoryOwn) {
    return new ContextMenu(cPtr, cMemoryOwn);
  }

  internal ContextMenu(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) {
  }

  internal static HandleRef getCPtr(ContextMenu obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  #region Events
  public event RoutedEventHandler Closed {
    add {
      AddHandler(ClosedEvent, value);
    }
    remove {
      RemoveHandler(ClosedEvent, value);
    }
  }

  public event RoutedEventHandler Opened {
    add {
      AddHandler(OpenedEvent, value);
    }
    remove {
      RemoveHandler(OpenedEvent, value);
    }
  }

  #endregion

  public ContextMenu() {
  }

  protected override IntPtr CreateCPtr(Type type, out bool registerExtend) {
    if (type == typeof(ContextMenu)) {
      registerExtend = false;
      return NoesisGUI_PINVOKE.new_ContextMenu();
    }
    else {
      return base.CreateExtendCPtr(type, out registerExtend);
    }
  }

  public static DependencyProperty HasDropShadowProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ContextMenu_HasDropShadowProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty HorizontalOffsetProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ContextMenu_HorizontalOffsetProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty IsOpenProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ContextMenu_IsOpenProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty PlacementProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ContextMenu_PlacementProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty PlacementRectangleProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ContextMenu_PlacementRectangleProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty PlacementTargetProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ContextMenu_PlacementTargetProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty StaysOpenProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ContextMenu_StaysOpenProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static DependencyProperty VerticalOffsetProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ContextMenu_VerticalOffsetProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static RoutedEvent ClosedEvent {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ContextMenu_ClosedEvent_get();
      return (RoutedEvent)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public static RoutedEvent OpenedEvent {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ContextMenu_OpenedEvent_get();
      return (RoutedEvent)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public bool HasDropShadow {
    set {
      NoesisGUI_PINVOKE.ContextMenu_HasDropShadow_set(swigCPtr, value);
    } 
    get {
      bool ret = NoesisGUI_PINVOKE.ContextMenu_HasDropShadow_get(swigCPtr);
      return ret;
    } 
  }

  public float HorizontalOffset {
    set {
      NoesisGUI_PINVOKE.ContextMenu_HorizontalOffset_set(swigCPtr, value);
    } 
    get {
      float ret = NoesisGUI_PINVOKE.ContextMenu_HorizontalOffset_get(swigCPtr);
      return ret;
    } 
  }

  public bool IsOpen {
    set {
      NoesisGUI_PINVOKE.ContextMenu_IsOpen_set(swigCPtr, value);
    } 
    get {
      bool ret = NoesisGUI_PINVOKE.ContextMenu_IsOpen_get(swigCPtr);
      return ret;
    } 
  }

  public PlacementMode Placement {
    set {
      NoesisGUI_PINVOKE.ContextMenu_Placement_set(swigCPtr, (int)value);
    } 
    get {
      PlacementMode ret = (PlacementMode)NoesisGUI_PINVOKE.ContextMenu_Placement_get(swigCPtr);
      return ret;
    } 
  }

  public Rect PlacementRectangle {
    set {
      NoesisGUI_PINVOKE.ContextMenu_PlacementRectangle_set(swigCPtr, ref value);
    }

    get {
      IntPtr ret = NoesisGUI_PINVOKE.ContextMenu_PlacementRectangle_get(swigCPtr);
      if (ret != IntPtr.Zero) {
        return Marshal.PtrToStructure<Rect>(ret);
      }
      else {
        return new Rect();
      }
    }

  }

  public UIElement PlacementTarget {
    set {
      NoesisGUI_PINVOKE.ContextMenu_PlacementTarget_set(swigCPtr, UIElement.getCPtr(value));
    } 
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.ContextMenu_PlacementTarget_get(swigCPtr);
      return (UIElement)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public bool StaysOpen {
    set {
      NoesisGUI_PINVOKE.ContextMenu_StaysOpen_set(swigCPtr, value);
    } 
    get {
      bool ret = NoesisGUI_PINVOKE.ContextMenu_StaysOpen_get(swigCPtr);
      return ret;
    } 
  }

  public float VerticalOffset {
    set {
      NoesisGUI_PINVOKE.ContextMenu_VerticalOffset_set(swigCPtr, value);
    } 
    get {
      float ret = NoesisGUI_PINVOKE.ContextMenu_VerticalOffset_get(swigCPtr);
      return ret;
    } 
  }

  internal new static IntPtr Extend(string typeName) {
    return NoesisGUI_PINVOKE.Extend_ContextMenu(Marshal.StringToHGlobalAnsi(typeName));
  }
}

}

