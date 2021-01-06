using UnityEngine;

public class MovingSphere : MonoBehaviour {

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

	bool desiredJump;

	public Vector3 contactNormal, steepNormal;

	public int groundContactCount, steepContactCount;

	bool OnGround => groundContactCount > 0;

	bool OnSteep => steepContactCount > 0;

	int jumpPhase;

	float minGroundDotProduct, minStairsDotProduct;

	int stepsSinceLastGrounded, stepsSinceLastJump;

	Vector3 upAxis, rightAxis, forwardAxis;

	void OnValidate () {
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
		minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
	}

	void Awake () {
		body = GetComponent<Rigidbody>();
		upAxis = Vector3.up;
		OnValidate();
	}

	void Update () {
		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
		playerInput = Vector2.ClampMagnitude(playerInput, 1f);
		if (playerInputSpace != null)
		{
			rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
			forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
		}
		else
		{
			rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
			forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
		}
		desiredVelocity =
		new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

		desiredJump |= Input.GetButtonDown("Jump");
	}

	void TryDisableGravity()
    {
		if (!OnGround)
		{
			body.useGravity = true;
		}
		else
		{
			body.useGravity = false;
		}
	}

	void FixedUpdate ()
	{
		TryDisableGravity();
		UpdateState();
		AdjustVelocity();

		if (desiredJump) {
			desiredJump = false;
			Jump(Physics.gravity);
		}

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

	void AdjustVelocity () {
		Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal).normalized;
		Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal).normalized;

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
		//Debug.Log("currentX: " +  currentX + "; currentZ: " +  currentZ);
		//Debug.Log("newX: " + newX + "; newZ: " + newZ);
		//Debug.Log("diffX: " + (newX - currentX) + "; diffZ: " + (newZ - currentZ));
		//Debug.Log("velocity X: " + velocity.x + "; Z: " + velocity.z);

		velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
	}

	void Jump (Vector3 gravity) {
		Vector3 jumpDirection;
		if (OnGround) {
			jumpDirection = contactNormal;
		}
		else if (OnSteep) {
			jumpDirection = steepNormal;
			jumpPhase = 0;
		}
		else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps) {
			if (jumpPhase == 0) {
				jumpPhase = 1;
			}
			jumpDirection = contactNormal;
		}
		else {
			return;
		}

		stepsSinceLastJump = 0;
		jumpPhase += 1;
		float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
		jumpDirection = (jumpDirection + upAxis).normalized;
		float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
		if (alignedSpeed > 0f) {
			jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
		}
		velocity += jumpDirection * jumpSpeed;
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

	Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
	{
		return (direction - Vector3.Dot(direction, normal) * normal).normalized;
	}

	float GetMinDot (int layer) {
	return (stairsMask & (1 << layer)) == 0 ?
		minGroundDotProduct : minStairsDotProduct;
	}
}