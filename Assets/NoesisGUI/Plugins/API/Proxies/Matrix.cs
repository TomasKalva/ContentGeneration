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

[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Matrix {

  [MarshalAs(UnmanagedType.R4)]
  private float _m11;

  [MarshalAs(UnmanagedType.R4)]
  private float _m12;

  [MarshalAs(UnmanagedType.R4)]
  private float _m21;

  [MarshalAs(UnmanagedType.R4)]
  private float _m22;

  [MarshalAs(UnmanagedType.R4)]
  private float _offsetX;

  [MarshalAs(UnmanagedType.R4)]
  private float _offsetY;

  public float M11 {
    get { return _m11; }
    set { _m11 = value; }
  }

  public float M12 {
    get { return _m12; }
    set { _m12 = value; }
  }

  public float M21 {
    get { return _m21; }
    set { _m21 = value; }
  }

  public float M22 {
    get { return _m22; }
    set { _m22 = value; }
  }

  public float OffsetX {
    get { return _offsetX; }
    set { _offsetX = value; }
  }

  public float OffsetY {
    get { return _offsetY; }
    set { _offsetY = value; }
  }

  public Matrix(float m11, float m12, float m21, float m22, float offsetX, float offsetY) {
    _m11 = m11; _m12 = m12;
    _m21 = m21; _m22 = m22;
    _offsetX = offsetX; _offsetY = offsetY;
  }

  public static Matrix Identity {
    get { return _identity; }
  }

  public bool IsIdentity {
    get {
      return _m11 == 1.0f && _m12 == 0.0f &&
             _m21 == 0.0f && _m22 == 1.0f &&
             _offsetX == 0.0f && _offsetY == 0.0f;
    }
  }

  public void SetIdentity() {
    this = _identity;
  }

  public static Matrix operator *(Matrix m0, Matrix m1) {
    return new Matrix(
      m0._m11 * m1._m11 + m0._m12 * m1._m21,
      m0._m11 * m1._m12 + m0._m12 * m1._m22,
      m0._m21 * m1._m11 + m0._m22 * m1._m21,
      m0._m21 * m1._m12 + m0._m22 * m1._m22,
      m0._offsetX * m1._m11 + m0._offsetY * m1._m21 + m1._offsetX,
      m0._offsetX * m1._m12 + m0._offsetY * m1._m22 + m1._offsetY);
  }

  public static Matrix Multiply(Matrix m0, Matrix m1) {
    return m0 * m1;
  }

  public void Append(Matrix matrix) {
    this *= matrix;
  }

  public void Prepend(Matrix matrix) {
    this = matrix * this;
  }

  public void Rotate(float angle) {
    angle %= 360.0f; // Doing the modulo before converting to radians reduces total error
    this *= CreateRotationRadians(angle * (float)(Math.PI / 180.0f));
  }

  public void RotatePrepend(float angle) {
    angle %= 360.0f; // Doing the modulo before converting to radians reduces total error
    this = CreateRotationRadians(angle * (float)(Math.PI / 180.0)) * this;
  }

  public void RotateAt(float angle, float centerX, float centerY) {
    angle %= 360.0f; // Doing the modulo before converting to radians reduces total error
    this *= CreateRotationRadians(angle * (float)(Math.PI / 180.0), centerX, centerY);
  }

  public void RotateAtPrepend(float angle, float centerX, float centerY) {
    angle %= 360.0f; // Doing the modulo before converting to radians reduces total error
    this = CreateRotationRadians(angle * (float)(Math.PI / 180.0), centerX, centerY) * this;
  }

  public void Scale(float scaleX, float scaleY) {
    this *= CreateScaling(scaleX, scaleY);
  }

  public void ScalePrepend(float scaleX, float scaleY) {
    this = CreateScaling(scaleX, scaleY) * this;
  }

  public void ScaleAt(float scaleX, float scaleY, float centerX, float centerY) {
    this *= CreateScaling(scaleX, scaleY, centerX, centerY);
  }

  public void ScaleAtPrepend(float scaleX, float scaleY, float centerX, float centerY) {
    this = CreateScaling(scaleX, scaleY, centerX, centerY) * this;
  }

  public void Skew(float skewX, float skewY) {
    skewX %= 360.0f;
    skewY %= 360.0f;
    this *= CreateSkewRadians(
      skewX * (float)(Math.PI / 180.0),
      skewY * (float)(Math.PI / 180.0));
  }

  public void SkewPrepend(float skewX, float skewY) {
    skewX %= 360.0f;
    skewY %= 360.0f;
    this = CreateSkewRadians(
      skewX * (float)(Math.PI / 180.0),
      skewY * (float)(Math.PI / 180.0)) * this;
  }

  public void Translate(float offsetX, float offsetY) {
    _offsetX += offsetX;
    _offsetY += offsetY;
  }

  public void TranslatePrepend(float offsetX, float offsetY) {
    this = CreateTranslation(offsetX, offsetY) * this;
  }

  public Point Transform(Point point) {
    float x = point.X;
    float y = point.Y;
    MultiplyPoint(ref x, ref y);
    return new Point(x, y);
  }

  public void Transform(Point[] points) {
    if (points != null) {
      for (int i = 0; i < points.Length; ++i) {
        float x = points[i].X;
        float y = points[i].Y;
        MultiplyPoint(ref x, ref y);
        points[i] = new Point(x, y);
      }
    }
  }

  public Vector Transform(Vector vector) {
    float x = vector.X;
    float y = vector.Y;
    MultiplyVector(ref x, ref y);
    return new Vector(x, y);
  }

  public void Transform(Vector[] vectors) {
    if (vectors != null) {
      for (int i = 0; i < vectors.Length; ++i) {
        float x = vectors[i].X;
        float y = vectors[i].Y;
        MultiplyVector(ref x, ref y);
        vectors[i] = new Vector(x, y);
      }
    }
  }

  public float Determinant {
    get { return (_m11  * _m22) - (_m12 * _m21); }
  }

  public bool HasInverse {
    get { return Math.Abs(Determinant) < 0.0001f; }
  }

  public void Invert() {
    float determinant = Determinant;
    if (Math.Abs(determinant) < 0.0001f) {
      throw new InvalidOperationException("Matrix is not Invertible");
    }
    float invdet = 1.0f / determinant;
    this = new Matrix(
      _m22 * invdet, -_m12 * invdet,
      -_m21 * invdet, _m11 * invdet,
      (_m21 * _offsetY - _offsetX * _m22) * invdet, (_offsetX * _m12 - _m11 * _offsetY) * invdet);
  }

  #region Transform helpers
  private static Matrix CreateRotationRadians(float angle) {
    return CreateRotationRadians(angle, 0.0f, 0.0f);
  }

  private static Matrix CreateRotationRadians(float angle, float centerX, float centerY) {
    float sin = (float)Math.Sin(angle);
    float cos = (float)Math.Cos(angle);
    float dx = (centerX * (1.0f - cos)) + (centerY * sin);
    float dy = (centerY * (1.0f - cos)) - (centerX * sin);
    return new Matrix(cos, sin, -sin, cos, dx, dy);
  }

  private static Matrix CreateScaling(float scaleX, float scaleY) {
    return CreateScaling(scaleX, scaleY, 0.0f, 0.0f);
  }

  private static Matrix CreateScaling(float scaleX, float scaleY, float centerX, float centerY) {
    return new Matrix(scaleX, 0, 0, scaleY, centerX - scaleX * centerX, centerY - scaleY * centerY);
  }

  private static Matrix CreateSkewRadians(float skewX, float skewY) {
    return new Matrix(
      1.0f, (float)Math.Tan(skewY),
      (float)Math.Tan(skewX), 1.0f,
      0.0f, 0.0f);
  }

  private static Matrix CreateTranslation(float offsetX, float offsetY) {
    return new Matrix(1.0f, 0.0f, 0.0f, 1.0f, offsetX, offsetY);
  }

  private void MultiplyPoint(ref float x, ref float y) {
    float xadd = y * _m21 + _offsetX;
    float yadd = x * _m12 + _offsetY;
    x *= _m11; x += xadd;
    y *= _m22; y += yadd;
  }

  private void MultiplyVector(ref float x, ref float y) {
    float xadd = y * _m21;
    float yadd = x * _m12;
    x *= _m11; x += xadd;
    y *= _m22; y += yadd;
  }
  #endregion

  #region Identity matrix
  private static readonly Matrix _identity = new Matrix(1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
  #endregion

}

}

