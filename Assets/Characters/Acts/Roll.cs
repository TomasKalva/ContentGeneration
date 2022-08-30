using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Roll : AnimatedAct
{
    [SerializeField, Curve(0f, 0f, 1f, 30f, true)]
    AnimationCurve speedF;

    Vector2 direction;
    public Vector2 Direction
    {
        get => direction;
        set => direction = value.normalized;
    }

    public override void OnStart(Agent agent)
    {
        PlayAnimation(agent);

        agent.movement.VelocityUpdater = new CurveVelocityUpdater(speedF, duration, () => Direction.X0Z());

        /*
        movementContraints = new List<MovementConstraint>()
        {
            new VelocityInDirection(() => Direction.X0Z()),
            new TurnToDirection(() => Direction),
        };*/

        SetupMovementConstraints(agent,
            new VelocityInDirection(() => Direction.X0Z()),
            new TurnToDirection(() => Direction)
            );
        /*agent.movement.AddMovementConstraints(new MovementConstraint[]
        {
            new VelocityInDirection(() => Direction.X0Z()),
            new TurnToDirection(() => Direction),
        });*/
        //movementContraints.ForEach(con => agent.movement.Constraints.Add(con));
    }

    public override void EndAct(Agent agent)
    {
        MovementContraints.ForEach(con => con.Finished = true);
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

#if UNITY_EDITOR
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
#endif
