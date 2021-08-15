using Assets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour {

	[SerializeField, Range(0f, 100f)]
	public float maxSpeed = 10f;

	[SerializeField, Range(0f, 1f)]
	float rotationCoef = 0.2f;

	[SerializeField, Range(0f, 100f)]
	float maxAcceleration = 10f, maxAirAcceleration = 1f;

	[SerializeField, Range(0f, 10f)]
	float jumpHeight = 2f;

	[SerializeField, Range(0, 5)]
	int maxAirJumps = 0;

	[SerializeField, Range(0, 90)]
	float maxGroundAngle = 25f, maxStairsAngle = 50f;

	[SerializeField, Range(0f, 100f)]
	float maxSnapSpeed = 100f;

	[SerializeField, Min(0f)]
	float probeDistance = 1f;

	[SerializeField]
	LayerMask probeMask = -1, stairsMask = -1;

	public Rigidbody body;

	public Vector3 velocity, desiredVelocity;

	public Vector2 direction, desiredDirection;

	Vector3 contactNormal, steepNormal;

	int groundContactCount, steepContactCount;

	bool OnGround => groundContactCount > 0;

	int jumpPhase;

	float minGroundDotProduct, minStairsDotProduct;

	int stepsSinceLastGrounded, stepsSinceLastJump;

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

	public void Jump(float speed)
    {
		//PerformInstruction(new ImpulseInstruction(speed * Vector3.up));
	}

	public void Impulse(Vector3 velocity)
	{
		//PerformInstruction(new ImpulseInstruction(velocity));
	}

	public void Dodge(float speed)
	{
		//PerformInstruction(new ImpulseInstruction(speed * (-AgentForward + 0.1f * Vector3.up)));
	}

	public void Roll(float speed)
	{
		//PerformInstruction(new ImpulseInstruction(speed * (AgentForward + 0.1f * Vector3.up)));
	}

	public void Move(Vector2 direction, bool setDirection = true)
	{
		desiredVelocity = new Vector3(direction.x, 0f, direction.y) * maxSpeed;
		if (setDirection && desiredVelocity.sqrMagnitude > 0.1f)
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
		stepsSinceLastJump += 1;
		velocity = body.velocity;
		/*desiredVelocity = Vector3.zero;
		desiredDirection = Vector2.zero;*/
		if (OnGround || SnapToGround() || CheckSteepContacts()) {
			stepsSinceLastGrounded = 0;
			if (stepsSinceLastJump > 1) {
				jumpPhase = 0;
			}
			if (groundContactCount > 1) {
				contactNormal.Normalize();
			}
		}
		else {
			contactNormal = upAxis;
		}
	}

	bool SnapToGround () {
		if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2) {
			return false;
		}
		float speed = velocity.magnitude;
		if (speed > maxSnapSpeed) {
			return false;
		}
		if (!Physics.Raycast(
			body.position, -upAxis, out RaycastHit hit,
			probeDistance, probeMask
		)) {
			return false;
		}
		float upDot = Vector3.Dot(upAxis, hit.normal);
		if (upDot < GetMinDot(hit.collider.gameObject.layer)) {
			return false;
		}

		groundContactCount = 1;
		contactNormal = hit.normal;
		float dot = Vector3.Dot(velocity, hit.normal);
		if (dot > 0f) {
			velocity = (velocity - hit.normal * dot).normalized * speed;
		}
		//Debug.Log($"Snapped to ground. Steps since last grounded: {stepsSinceLastGrounded}, Steps since last jump: {stepsSinceLastJump}");
		return true;
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
			direction = Vector2.MoveTowards(direction, desiredDirection, rotationCoef);

			// update body rotation
			body.rotation = Quaternion.FromToRotation(Vector3.forward, AgentForward);
		}
	}

	void AdjustVelocity ()
	{
		//when moving up, the y vector shouldn't be projected to the plane
		var xzVelocity = velocity - velocity.y * Vector3.up;

		float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		var newVel = Vector3.MoveTowards(xzVelocity, desiredVelocity, maxSpeedChange);

		velocity += Vector3.right * (newVel.x - velocity.x) + Vector3.forward * (newVel.z - velocity.z);
	}

	public void JumpNoChecks(Vector3 dV)
    {
		stepsSinceLastJump = 0;
		jumpPhase += 1;
		velocity += dV;
		//body.velocity = velocity;
	}

	public void ResetDesiredValues()
    {
		desiredVelocity = Vector3.zero;
		desiredDirection = Vector2.zero;
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