using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Roll : AnimatedAct
{
    [SerializeField, Curve(0f, 0f, 1f, 15f, true)]
    AnimationCurve speedF;

    Vector2 direction;
    public Vector2 Direction
    {
        get => direction;
        set => direction = value;
    }

    public override IEnumerator Perform(Agent agent)
    {
        agent.animator.CrossFade(animationName, 0.05f);
        var t = 0f;
        while ((t += Time.deltaTime) <= duration)
        {
            var speed = speedF.Evaluate(t / duration);
            agent.movement.InputImpulse(direction, speed);
            agent.movement.Turn(direction);
            Debug.Log($"t: {t}, speed: {speed}");
            //agent.movement.InputImpulse(direction, 15f);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield return null;
    }
}
public class CurveAttribute : PropertyAttribute
{
    public float PosX, PosY;
    public float RangeX, RangeY;
    public bool b;
    public int x;

    public CurveAttribute(float PosX, float PosY, float RangeX, float RangeY, bool b)
    {
        this.PosX = PosX;
        this.PosY = PosY;
        this.RangeX = RangeX;
        this.RangeY = RangeY;
        this.b = b;
    }
}


[CustomPropertyDrawer(typeof(CurveAttribute))]
public class CurveDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CurveAttribute curve = attribute as CurveAttribute;
        if (property.propertyType == SerializedPropertyType.AnimationCurve)
        {
            if (curve.b) EditorGUI.CurveField(position, property, Color.cyan, new Rect(curve.PosX, curve.PosY, curve.RangeX, curve.RangeY));
        }
    }
}
