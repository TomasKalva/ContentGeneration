using Assets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour {

	[SerializeField, Range(0f, 1f)]
	float rotationSpeed = 0.2f;

	[SerializeField, Range(0, 90)]
	float maxGroundAngle = 25f, maxStairsAngle = 50f;

	[SerializeField, Min(0f)]
	float probeDistance = 1f;

	[SerializeField]
	LayerMask probeMask = -1, stairsMask = -1;

	public Rigidbody body;

	public Vector3 velocity/*, desiredVelocity*/;

	public Vector2 direction, desiredDirection;

	Vector3 contactNormal, steepNormal;

	int groundContactCount, steepContactCount;

	bool OnGround => groundContactCount > 0;


	float minGroundDotProduct, minStairsDotProduct;

	int stepsSinceLastGrounded;

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
		var projectedForce = ExtensionMethods.ProjectDirectionOnPlane(force.normalized, contactNormal) * force.magnitude;
		body.AddForce(projectedForce);
	}

	public void Move(Vector2 direction, float speed, bool setDirection = true)
	{
        if (!OnGround)
        {
			return;
        }

		velocity = speed * direction.X0Z();
		if (setDirection && velocity.sqrMagnitude > 0.1f)
		{
			desiredDirection = direction;
		}
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

		foreach(var constraint in Constraints)
        {
			constraint.Apply(this);
        }
		Constraints.RemoveAll(constr => constr.Finished);

		AdjustDirection();
		body.velocity = velocity;
		ClearState();
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
			direction = Vector2.MoveTowards(direction, desiredDirection, rotationSpeed);

			// update body rotation
			body.rotation = Quaternion.FromToRotation(Vector3.forward, AgentForward);
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
		//desiredVelocity = Vector3.zero;
		desiredDirection = Vector2.zero;

		velocity = body.velocity;
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
	return (stairsMask & (1 << layer)) == 0 ?
		minGroundDotProduct : minStairsDotProduct;
	}
}