using Assets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour {

	[SerializeField, Range(0f, 720f)]
	float rotationSpeed = 90f;

	[SerializeField, Range(0, 90)]
	float maxGroundAngle = 25f, maxStairsAngle = 50f;

	[SerializeField, Min(0f)]
	float probeDistance = 1f;

	[SerializeField]
	LayerMask probeMask = -1, stairsMask = -1;

	[SerializeField]
	LayerMask movingPlatformMask;

	public Rigidbody body;

	public Vector3 velocity;

	public Vector2 direction, desiredDirection;

	public Vector3 groundNormal;

	int groundContactCount;

	bool OnGround => groundContactCount > 0;


	float minGroundDotProduct, minStairsDotProduct;

	int stepsSinceLastGrounded;

	bool applyFriction;

	Vector3 upAxis;

	public VelocityUpdater VelocityUpdater { get; set; }

	public List<MovementConstraint> Constraints { get; private set; }

	public Vector3 AgentForward
    {
        get
        {
			Vector2 normalizedDir = direction.normalized;
			return normalizedDir.X0Z();
		}
    }

	#region API

	public void Impulse(Vector3 force)
	{
		var projectedForce = ExtensionMethods.ProjectDirectionOnPlane(force.normalized, groundNormal.normalized) * force.magnitude;
		body.AddForce(projectedForce);
	}

	Vector3 MoveOnGroundDirection(Vector2 moveDirection, Vector3 groundNormal)
	{
		var movePlaneN = new Vector3(moveDirection.y, 0f, -moveDirection.x);
		var groundPlaneN = groundNormal.normalized;
		var moveOnGroundDir = Vector3.Cross(movePlaneN, groundPlaneN).normalized;
		var projectedDir = Vector3.Project(moveDirection.X0Z(), moveOnGroundDir);
		return projectedDir;
	}

	public void Move(Vector2 direction, float speed, bool setDirection = true)
	{
        if (!OnGround)
        {
			return;
        }

		var projectedDir = MoveOnGroundDirection(direction, groundNormal);

		velocity = speed * projectedDir;
		applyFriction = false;
		if (setDirection && velocity.sqrMagnitude > 0.1f)
		{
			desiredDirection = direction;
		}
	}

	public void Accelerate(Vector2 dV, float speed)
	{
		// todo: change it to MoveGroundDirection, but there is problem when touching object and trying to accelerate
		var projectedDir = ExtensionMethods.ProjectDirectionOnPlane(dV.X0Z(), groundNormal.normalized);
		//var projectedDir = MoveGroundDirection(direction, contactNormal);

		velocity += speed * projectedDir;
		applyFriction = false;
	}

	public void Turn(Vector2 direction)
	{
		desiredDirection = direction;
	}

    #endregion

	void OnValidate () {
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
		minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
	}

	void Awake () {
		body = GetComponent<Rigidbody>();
		upAxis = Vector3.up;
		OnValidate();
		direction = Vector2.up;
		Constraints = new List<MovementConstraint>();
	}

	void PreventWallCollision()
    {
		float totalTraveled = velocity.magnitude * Time.fixedDeltaTime;
		if (body.SweepTest(velocity.normalized, out var wallCollision, totalTraveled, QueryTriggerInteraction.Ignore))
		{
			float traveledInWall = totalTraveled - wallCollision.distance;
			velocity += -Vector3.Dot(wallCollision.normal, velocity.normalized * (traveledInWall / Time.fixedDeltaTime)) * wallCollision.normal;
		}
	}

	public void MovementUpdate ()
	{
		UpdateState();

		if (VelocityUpdater != null)
		{
			if (VelocityUpdater.UpdateVelocity(this, Time.fixedDeltaTime))
			{
				VelocityUpdater = null;
			}
		}

		PreventWallCollision();
		SnapToGround();

		foreach(var constraint in Constraints)
        {
			constraint.Apply(this);
        }
		Constraints.RemoveAll(constr => constr.Finished);

		AdjustDirection();
		body.velocity = velocity;

		// gravity
        if (OnGround)
        {
			body.useGravity = false;
			if (applyFriction)
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
	
	void SnapToGround()
    {
		if(Physics.Raycast(transform.position, -upAxis, out var hit, 1.2f, stairsMask, QueryTriggerInteraction.Ignore) &&
			hit.distance > 0.5f)
        {
			//Debug.Log($"Snapping distance: {hit.distance}");
			//transform.position = hit.point;
			//Debug.Log("Snapping to ground");
			velocity += -10f * upAxis;
        }
    }

	void ClearState () {
		groundContactCount = 0;
		groundNormal = Vector3.zero;
	}

	void UpdateState () {
		stepsSinceLastGrounded += 1;
	}

	void AdjustDirection()
	{
		if (desiredDirection.sqrMagnitude > 0.01f)
		{
			var currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			var desiredAngle = Mathf.Atan2(desiredDirection.y, desiredDirection.x) * Mathf.Rad2Deg;

			var newAngle = Mathf.MoveTowardsAngle(currentAngle, desiredAngle, rotationSpeed * Time.fixedDeltaTime) * Mathf.Deg2Rad;

			direction = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));

			// update body rotation
			body.rotation = Quaternion.LookRotation(AgentForward, Vector3.up);//.FromToRotation(Vector3.forward, AgentForward);
		}
	}

	public void MoveTowards(Vector3 velocity)
    {
        if (!OnGround)
        {
			return;
        }

		this.velocity = velocity;
    }

	public void ResetDesiredValues()
    {
		desiredDirection = Vector2.zero;

		velocity = body.velocity;

		applyFriction = true;
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
		//Debug.Log($"Evaluating collisions: collisions: {groundContactCount}");
	}

	float GetMinDot (int layer) {
		return minStairsDotProduct;
	/*return (stairsMask & (1 << layer)) == 0 ?
		minGroundDotProduct : minStairsDotProduct;*/
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
		if (Vector3.Dot(movement.velocity, directionF()) <= 0)
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
		//movement.Accelerate(directionF().XZ().normalized, dS);

		movement.Move(directionF().XZ().normalized, speed1 / duration);
		//movement.velocity += dS * direction;

		return t >= duration;
	}
}