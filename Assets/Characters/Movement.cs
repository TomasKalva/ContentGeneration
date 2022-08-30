using Assets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour {

	[SerializeField, Range(0f, 720f)]
	float rotationSpeed = 90f;

	[SerializeField]
	LayerMask groundMask = -1;

	public Rigidbody body;

	public Vector2 direction;

	public Vector3 groundNormal;

	int groundContactCount;

	bool OnGround => groundContactCount > 0;

	float maxGroundAngle = 60f;
	float minGroundDotProduct;

	Vector3 upAxis;


	public Vector3 AgentForward
    {
        get
        {
			Vector2 normalizedDir = direction.normalized;
			return normalizedDir.X0Z();
		}
    }

	#region Utils

	/// <summary>
	/// The agent moves in moveDirection projected to ground given by groundNormal.
	/// </summary>
	Vector3 MoveOnGroundDirection(Vector2 moveDirection, Vector3 groundNormal)
	{
		var movePlaneN = new Vector3(moveDirection.y, 0f, -moveDirection.x);
		var groundPlaneN = groundNormal.normalized;
		var moveOnGroundDir = Vector3.Cross(movePlaneN, groundPlaneN).normalized;
		var projectedDir = Vector3.Project(moveDirection.X0Z(), moveOnGroundDir);
		return projectedDir;
	}

	#endregion


	#region API

	public Vector3 Velocity { get; set; }

	public Vector2 DesiredDirection { get; set; }

	public bool ApplyFriction { get; set; }

	public VelocityUpdater VelocityUpdater { get; set; }

	List<MovementConstraint> Constraints { get; set; }

	/// <summary>
	/// Adds the movement constraints.
	/// </summary>
	public void AddMovementConstraints(params MovementConstraint[] constraints)
    {
		Constraints.AddRange(constraints);
	}

	/// <summary>
	/// Adds the movement constraint.
	/// </summary>
	public void AddMovementConstraint(MovementConstraint constraint)
	{
		Constraints.Add(constraint);
	}

	/// <summary>
	/// Removes the movement constraint.
	/// </summary>
	public void RemoveMovementConstraint(MovementConstraint constraint)
	{
		Constraints.Remove(constraint);
	}

	/// <summary>
	/// Pushes the agent by force.
	/// </summary>
	public void Impulse(Vector3 force)
	{
		var projectedForce = ExtensionMethods.ProjectDirectionOnPlane(force.normalized, groundNormal.normalized) * force.magnitude;
		body.AddForce(projectedForce);
	}

	/// <summary>
	/// Move on ground in direction with speed.
	/// </summary>
	/// <param name="setDirection">If true, agent will turn to the direction.</param>
	public void Move(Vector2 direction, float speed, bool setDirection = true)
	{
        if (!OnGround)
        {
			return;
        }

		var projectedDir = MoveOnGroundDirection(direction, groundNormal);

		Velocity = speed * projectedDir;
		ApplyFriction = false;
		if (setDirection && Velocity.sqrMagnitude > 0.1f)
		{
			DesiredDirection = direction;
		}
	}

	/// <summary>
	/// Agent turns to direction.
	/// </summary>
	public void Turn(Vector2 direction)
	{
		DesiredDirection = direction;
	}

    #endregion

	void Awake () {
		body = GetComponent<Rigidbody>();
		upAxis = Vector3.up;
		direction = Vector2.up;
		Constraints = new List<MovementConstraint>();
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
	}

	/// <summary>
	/// Removes the velocity component that goes inside of a wall.
	/// </summary>
	void PreventWallCollision()
    {
		// total distance traveled by the testing ray
		float totalTraveled = Velocity.magnitude * Time.fixedDeltaTime;
		if (body.SweepTest(Velocity.normalized, out var wallCollision, totalTraveled, QueryTriggerInteraction.Ignore))
		{
			// distance traveled by the ray once reaching the wall
			float traveledInWall = totalTraveled - wallCollision.distance;
			// amount of velocity inside of the wall
			float velocityInWall = traveledInWall / Time.fixedDeltaTime;
			// subtract velocity in the wall from velocity
			Velocity += -Vector3.Dot(wallCollision.normal, Velocity.normalized * velocityInWall) * wallCollision.normal;
		}
	}

	/// <summary>
	/// Main update method.
	/// </summary>
	public void MovementUpdate ()
	{
		// Update velocity using the current strategy
		if (VelocityUpdater != null)
		{
			if (VelocityUpdater.UpdateVelocity(this, Time.fixedDeltaTime))
			{
				VelocityUpdater = null;
			}
		}

		//
		PreventWallCollision();
		SnapToGround();

		foreach (var constraint in Constraints)
        {
			constraint.Apply(this);
        }
		Constraints.RemoveAll(constr => constr.Finished);


		AdjustDirection();
		body.velocity = Velocity;

		// gravity
        if (OnGround)
        {
			body.useGravity = false;
			if (ApplyFriction)
			{
				body.velocity *= 0.7f;
			}
        }
        else
        {
			body.useGravity = true;
        }

		ClearState();
	}
	
	/// <summary>
	/// Put agent back to ground. Used mainly for not jumping when walking downstairs from straight platform.
	/// </summary>
	void SnapToGround()
    {
		if(Physics.Raycast(transform.position, -upAxis, out var hit, 1.2f, groundMask, QueryTriggerInteraction.Ignore) &&
			hit.distance > 0.5f)
        {
			Velocity += -10f * upAxis;
        }
    }

	/// <summary>
	/// Resets variables used during collision checking.
	/// </summary>
	void ClearState () {
		groundContactCount = 0;
		groundNormal = Vector3.zero;
	}

	/// <summary>
	/// Rotate towards desired direction.
	/// </summary>
	void AdjustDirection()
	{
		if (DesiredDirection.sqrMagnitude > 0.01f)
		{
			var currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			var desiredAngle = Mathf.Atan2(DesiredDirection.y, DesiredDirection.x) * Mathf.Rad2Deg;

			var newAngle = Mathf.MoveTowardsAngle(currentAngle, desiredAngle, rotationSpeed * Time.fixedDeltaTime) * Mathf.Deg2Rad;

			direction = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));

			// update body rotation
			body.rotation = Quaternion.LookRotation(AgentForward, Vector3.up);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public void ResetDesiredValues()
    {
		DesiredDirection = Vector2.zero;
		Velocity = body.velocity;
		ApplyFriction = true;
	}

	void OnCollisionEnter (Collision collision) {
		EvaluateCollision(collision);
	}

	void OnCollisionStay (Collision collision) {
		EvaluateCollision(collision);
	}

	void EvaluateCollision (Collision collision) {

		float minDot = GetMinDot(collision.gameObject.layer);
		for (int i = 0; i < collision.contactCount; i++) {
			Vector3 normal = collision.GetContact(i).normal;
			float upDot = Vector3.Dot(normal, upAxis);
			if (upDot >= minDot) {
				groundContactCount += 1;
				groundNormal += normal;
			}
		}
		groundNormal = groundNormal.normalized;
	}

	float GetMinDot (int layer) {
		return minGroundDotProduct;
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
		if (Vector3.Dot(movement.Velocity, directionF()) <= 0)
		{
			movement.Velocity = Vector3.zero;
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
		movement.DesiredDirection = directionF();
	}
}

public class TurnToTransform : MovementConstraint
{
	Transform target;

	Vector3 TargetPosition => target != null ? target.position : Vector3.zero;

	public TurnToTransform(Transform target)
	{
		this.target = target;
	}

	public override void Apply(Movement movement)
	{
		movement.DesiredDirection = (TargetPosition - movement.transform.position).XZ().normalized;
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
			var currSpeed = movement.Velocity.magnitude;
			movement.Velocity = Vector3.zero;
			firstIteration = false;
		}

		t += dt;

		var speed0 = speedF.Evaluate((t - dt) / duration);
		var speed1 = speedF.Evaluate(t / duration);
		var dS = (speed1 - speed0) / duration; // dividing by duration makes traveled distance independent of duration
		//movement.Accelerate(directionF().XZ().normalized, dS);

		movement.Move(directionF().XZ().normalized, speed1 / duration);
		//movement.velocity += dS * direction;

		return t >= duration;
	}
}