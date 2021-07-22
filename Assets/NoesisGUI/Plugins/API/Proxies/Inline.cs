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

public class Inline : TextElement {
  internal new static Inline CreateProxy(IntPtr cPtr, bool cMemoryOwn) {
    return new Inline(cPtr, cMemoryOwn);
  }

  internal Inline(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) {
  }

  internal static HandleRef getCPtr(Inline obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  public Inline() {
  }

  protected override IntPtr CreateCPtr(Type type, out bool registerExtend) {
    registerExtend = false;
    return NoesisGUI_PINVOKE.new_Inline();
  }

  public static DependencyProperty TextDecorationsProperty {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.Inline_TextDecorationsProperty_get();
      return (DependencyProperty)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public InlineCollection SiblingInlines {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.Inline_SiblingInlines_get(swigCPtr);
      return (InlineCollection)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public Inline PreviousInline {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.Inline_PreviousInline_get(swigCPtr);
      return (Inline)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public Inline NextInline {
    get {
      IntPtr cPtr = NoesisGUI_PINVOKE.Inline_NextInline_get(swigCPtr);
      return (Inline)Noesis.Extend.GetProxy(cPtr, false);
    }
  }

  public TextDecorations TextDecorations {
    set {
      NoesisGUI_PINVOKE.Inline_TextDecorations_set(swigCPtr, (int)value);
    } 
    get {
      TextDecorations ret = (TextDecorations)NoesisGUI_PINVOKE.Inline_TextDecorations_get(swigCPtr);
      return ret;
    } 
  }

}

}

