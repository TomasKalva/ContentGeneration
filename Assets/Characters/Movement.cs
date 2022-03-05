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

	Vector3 contactNormal, steepNormal;

	int groundContactCount, steepContactCount;

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
		var projectedForce = ExtensionMethods.ProjectDirectionOnPlane(force.normalized, contactNormal.normalized) * force.magnitude;
		body.AddForce(projectedForce);
	}

	Vector3 MoveGroundDirection(Vector2 moveDirection, Vector3 groundNormal)
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

		var projectedDir = MoveGroundDirection(direction, contactNormal);

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
		var projectedDir = ExtensionMethods.ProjectDirectionOnPlane(dV.X0Z(), contactNormal.normalized);
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
		if(body.SweepTest(velocity.normalized, out var info, velocity.magnitude * Time.fixedDeltaTime, QueryTriggerInteraction.Ignore)){
			velocity -= Vector3.Dot(info.normal, velocity) * info.normal;
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

		if (VelocityUpdater == null)
		{
			AdjustVelocity();
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
		if(Physics.Raycast(transform.position, -upAxis, out var hit, 1.2f, movingPlatformMask, QueryTriggerInteraction.Ignore))
        {
			Debug.Log("Snapping to ground");
			velocity += -1f * upAxis;
        }
    }

	void ClearState () {
		groundContactCount = steepContactCount = 0;
		contactNormal = steepNormal = Vector3.zero;
	}

	void UpdateState () {
		stepsSinceLastGrounded += 1;
		CheckSteepContacts();
	}

	bool CheckSteepContacts () {
		if (steepContactCount > 1) {
			steepNormal.Normalize();
			float upDot = Vector3.Dot(upAxis, steepNormal);
			if (upDot >= minGroundDotProduct) {
				steepContactCount = 0;
				groundContactCount = 1;
				contactNormal = steepNormal;
				return true;
			}
		}
		return false;
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

	void AdjustVelocity ()
	{
		/*float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.fixedDeltaTime;

		velocity = Vector3.MoveTowards(velocity, desiredVelocity, maxSpeedChange);*/
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
				contactNormal += normal;
			}
			else if (upDot > -0.01f) {
				steepContactCount += 1;
				steepNormal += normal;
			}
		}
	}

	float GetMinDot (int layer) {
		return minStairsDotProduct;
	/*return (stairsMask & (1 << layer)) == 0 ?
		minGroundDotProduct : minStairsDotProduct;*/
	}
}