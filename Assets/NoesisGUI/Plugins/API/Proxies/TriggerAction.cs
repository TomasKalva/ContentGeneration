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

public class TriggerAction : DependencyObject {
  internal new static TriggerAction CreateProxy(IntPtr cPtr, bool cMemoryOwn) {
    return new TriggerAction(cPtr, cMemoryOwn);
  }

  internal TriggerAction(IntPtr cPtr, bool cMemoryOwn) : base(cPtr, cMemoryOwn) {
  }

  internal static HandleRef getCPtr(TriggerAction obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  protected TriggerAction() {
  }

}

}

