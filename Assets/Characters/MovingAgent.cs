using Assets;
using System.Collections.Generic;
using UnityEngine;


public class MovingAgent : MonoBehaviour {

	[SerializeField, Range(0f, 100f)]
	float maxSpeed = 10f;

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

	[SerializeField]
	Transform playerInputSpace = default;

	Rigidbody body;

	Vector3 velocity, desiredVelocity;

	Vector3 direction;

	bool desiredJump;

	Vector3 contactNormal, steepNormal;

	int groundContactCount, steepContactCount;

	bool OnGround => groundContactCount > 0;

	bool OnSteep => steepContactCount > 0;

	int jumpPhase;

	float minGroundDotProduct, minStairsDotProduct;

	int stepsSinceLastGrounded, stepsSinceLastJump;

	Vector3 upAxis, rightAxis, forwardAxis;
	bool fixedUpdateHappened;

	public abstract class AgentInstruction
	{
		public abstract void Do(MovingAgent movingAgent);
	}

	public class JumpInstruction : AgentInstruction
	{
		float speed;

		public JumpInstruction(float speed)
		{
			this.speed = speed;
		}

		public override void Do(MovingAgent player)
		{
			player.JumpNoChecks(speed);
		}
	}

	public class MoveInstruction : AgentInstruction
	{
		Vector2 direction;

		public MoveInstruction(Vector2 direction)
		{
			this.direction = direction;
		}

		public override void Do(MovingAgent player)
		{
			player.desiredVelocity =
				new Vector3(direction.x, 0f, direction.y) * player.maxSpeed;
		}
	}


	public abstract class Ability
    {
		public abstract void DoLogic(MovingAgent player);
    }

    public class JumpAbility : Ability
    {
		float speed;

        public JumpAbility(float speed)
        {
            this.speed = speed;
        }

        public override void DoLogic(MovingAgent player)
        {
			player.JumpNoChecks(speed);
        }
    }

    public List<AgentInstruction> instructionQueue;

	void OnValidate () {
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
		minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
	}

	void Awake () {
		body = GetComponent<Rigidbody>();
		upAxis = Vector3.up;
		instructionQueue = new List<AgentInstruction>();
		fixedUpdateHappened = false;
		OnValidate();
		direction = Vector3.forward;
	}

	void Update () {
		/*Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
		playerInput = Vector2.ClampMagnitude(playerInput, 1f);
		if (playerInputSpace != null)
		{
			rightAxis = ExtensionMethods.ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
			forwardAxis = ExtensionMethods.ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
		}
		else
		{
			rightAxis = ExtensionMethods.ProjectDirectionOnPlane(Vector3.right, upAxis);
			forwardAxis = ExtensionMethods.ProjectDirectionOnPlane(Vector3.forward, upAxis);
		}
		desiredVelocity =
		new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

		desiredJump |= Input.GetButtonDown("Jump");*/
	}

	void PreventWallCollision()
    {
		if(body.SweepTest(velocity.normalized, out var info, velocity.magnitude * Time.fixedDeltaTime)){
			velocity -= Vector3.Dot(info.normal, velocity) * info.normal;
		}
    }

	void FixedUpdate ()
	{
		UpdateState();

		foreach (var instruction in instructionQueue)
		{
			instruction.Do(this);
		}
		AdjustVelocity();
		AdjustDirection();

		if (desiredJump) {
			desiredJump = false;
			Jump(Physics.gravity);
		}


		PreventWallCollision();

		body.velocity = velocity;
		ClearState();
		fixedUpdateHappened = true;
	}

	void ClearState () {
		groundContactCount = steepContactCount = 0;
		contactNormal = steepNormal = Vector3.zero;
	}

	void UpdateState () {
		stepsSinceLastGrounded += 1;
		stepsSinceLastJump += 1;
		velocity = body.velocity;
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
		Debug.Log($"Snapped to ground. Steps since last grounded: {stepsSinceLastGrounded}, Steps since last jump: {stepsSinceLastJump}");
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
		if (velocity.sqrMagnitude > 0.01)
		{
			direction = Vector3.Lerp(direction, velocity - new Vector3(0f, velocity.y, 0f), 0.2f);
			body.rotation = Quaternion.FromToRotation(Vector3.forward, direction);
		}
	}

	void AdjustVelocity ()
	{
		rightAxis = ExtensionMethods.ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
		forwardAxis = ExtensionMethods.ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
		Vector3 xAxis = ExtensionMethods.ProjectDirectionOnPlane(rightAxis, contactNormal).normalized;
		Vector3 zAxis = ExtensionMethods.ProjectDirectionOnPlane(forwardAxis, contactNormal).normalized;

		//when moving up, the y vector shouldn't be projected to the plane
		var xzVelocity = velocity - velocity.y * Vector3.up;
		float currentX = Vector3.Dot(xzVelocity, xAxis);
		float currentZ = Vector3.Dot(xzVelocity, zAxis);

		float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		float newX =
			Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
		float newZ =
			Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

		velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
	}

	void Jump (Vector3 gravity) {
		Vector3 jumpDirection;
		if (OnGround) {
			jumpDirection = contactNormal;
		}
		else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps) {
			if (jumpPhase == 0) {
				jumpPhase = 1;
			}
			jumpDirection = contactNormal;
		}
		else {
			Debug.Log("Can't jump because not grounded and too many air jumps.");
			return;
		}

		float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
		JumpNoChecks(jumpSpeed);
	}

	public void JumpNoChecks(float speed)
    {
		stepsSinceLastJump = 0;
		jumpPhase += 1;
		velocity = Vector3.up * speed;
		body.velocity = velocity;
		Debug.Log(velocity);
	}

	public void PerformInstruction(AgentInstruction instruction)
    {
		instructionQueue.Add(instruction);
    }

	public void TryClearInstructions()
    {
        if (fixedUpdateHappened)
        {
			instructionQueue.Clear();
        }
		fixedUpdateHappened = false;
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