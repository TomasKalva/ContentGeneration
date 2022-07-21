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

        movementContraints = new List<MovementConstraint>()
        {
            new VelocityInDirection(() => Direction.X0Z()),
            new TurnToDirection(() => Direction),
        };

        movementContraints.ForEach(con => agent.movement.Constraints.Add(con));
    }

    public override void EndAct(Agent agent)
    {
        movementContraints.ForEach(con => con.Finished = true);
    }
}

public abstract class MovementConstraint
{
    public bool Finished { get; set; }
    public abstract void Apply(Movement movement);
}

public class VelocityInDirection : MovementConstraint
{
    Direction3F directionF;

    public VelocityInDirection(Direction3F directionF)
    {
        this.directionF = directionF;
    }

    public override void Apply(Movement movement)
    {
        if(Vector3.Dot(movement.velocity, directionF()) <= 0)
        {
            movement.velocity = Vector3.zero;
        }
    }
}

public class TurnToDirection : MovementConstraint
{
    Direction2F directionF;

    public TurnToDirection(Direction2F directionF)
    {
        this.directionF = directionF;
    }

    public override void Apply(Movement movement)
    {
        movement.desiredDirection = directionF();
    }
}

public class TurnToTransform: MovementConstraint
{
    Transform target;

    Vector3 TargetPosition => target != null ? target.position : Vector3.zero;

    public TurnToTransform(Transform target)
    {
        this.target = target;
    }

    public override void Apply(Movement movement)
    {
        movement.desiredDirection = (TargetPosition - movement.transform.position).XZ().normalized;
    }
}

public abstract class VelocityUpdater
{
    /// <summary>
    /// Updates velocity of Movement. Returns true if updating is finished.
    /// </summary>
    public abstract bool UpdateVelocity(Movement movement, float dt);
}

public class DontChangeVelocityUpdater : VelocityUpdater
{
    float duration;
    float t;

    public DontChangeVelocityUpdater(float duration)
    {
        this.duration = duration;
        this.t = 0f;
    }

    public override bool UpdateVelocity(Movement movement, float dt)
    {
        t += dt;
        return t >= duration;
    }

}

public delegate Vector3 Direction3F();
public delegate Vector2 Direction2F();

public class CurveVelocityUpdater : VelocityUpdater
{
    AnimationCurve speedF;
    float duration;
    float t;
    Direction3F directionF;

    bool firstIteration;

    public CurveVelocityUpdater(AnimationCurve speedF, float duration, Direction3F directionF)
    {
        this.speedF = speedF;
        this.duration = duration;
        this.directionF = directionF;
        this.t = 0f;
        firstIteration = true;
    }

    public override bool UpdateVelocity(Movement movement, float dt)
    {
        if (firstIteration)
        {
            var currSpeed = movement.velocity.magnitude;
            movement.velocity = Vector3.zero;
            firstIteration = false;
        }

        t += dt;

        var speed0 = speedF.Evaluate((t - dt) / duration);
        var speed1 = speedF.Evaluate(t / duration);
        var dS = (speed1 - speed0) / duration; // dividing by duration makes traveled distance independent of duration
        movement.Accelerate(directionF().XZ().normalized, dS);
        //movement.velocity += dS * direction;

        return t >= duration;
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
