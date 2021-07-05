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

	[SerializeField]
	Transform playerInputSpace = default;

	public Rigidbody body;

	public Vector3 velocity, desiredVelocity;

	public Vector2 direction, desiredDirection;

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

	public VelocityUpdater VelocityUpdater { get; set; }

	public List<MovementConstraint> Constraints { get; private set; }

	Vector3 InpForward
    {
        get
        {
			if(playerInputSpace != null)
            {
				return playerInputSpace.forward;
            }
            else
            {
				return Vector3.forward;
            }
        }
    }

	Vector3 InpRight
	{
		get
		{
			if (playerInputSpace != null)
			{
				return playerInputSpace.right;
			}
			else
			{
				return Vector3.right;
			}
		}
	}

	Vector3 InpForwardHoriz
	{
		get
		{
			return ExtensionMethods.ProjectDirectionOnPlane(InpForward, upAxis);
		}
	}

	Vector3 InpRightHoriz
	{
		get
		{
			return ExtensionMethods.ProjectDirectionOnPlane(InpRight, upAxis);
		}
	}

	public Vector3 AgentForward
    {
        get
        {
			Vector2 normalizedDir = direction.normalized;
			return new Vector3(normalizedDir.x, 0f, normalizedDir.y);
			//return normalizedDir.x * InpRightHoriz + normalizedDir.y * InpForwardHoriz;
		}
    }

	public Vector3 InputToWorld(Vector2 inputVec)
    {
		return inputVec.x * InpRightHoriz + inputVec.y * InpForwardHoriz;
	}

	#region API

	public void Jump(float speed)
    {
		PerformInstruction(new ImpulseInstruction(speed * Vector3.up));
	}

	public void Impulse(Vector3 velocity)
	{
		PerformInstruction(new ImpulseInstruction(velocity));
	}

	public void Dodge(float speed)
	{
		PerformInstruction(new ImpulseInstruction(speed * (-AgentForward + 0.1f * Vector3.up)));
	}

	public void Roll(float speed)
	{
		PerformInstruction(new ImpulseInstruction(speed * (AgentForward + 0.1f * Vector3.up)));
	}

	public void Move(Vector2 direction)
	{
		PerformInstruction(new MoveInstruction(direction));
	}

	public void Turn(Vector2 direction)
	{
		PerformInstruction(new TurnInstruction(direction));
	}

    #endregion

    #region Instructions

    abstract class AgentInstruction
	{
		public abstract void Do(Movement movingAgent);
	}

	class ImpulseInstruction : AgentInstruction
	{
		Vector3 dV;

		public ImpulseInstruction(Vector3 dV)
		{
			this.dV = dV;
		}

		public override void Do(Movement player)
		{
			player.JumpNoChecks(dV);
		}
	}

	class MoveInstruction : AgentInstruction
	{
		Vector2 direction;

		public MoveInstruction(Vector2 direction)
		{
			this.direction = direction;
		}

		public override void Do(Movement player)
		{
			player.desiredVelocity = new Vector3(direction.x, 0f, direction.y) * player.maxSpeed;
			if(player.desiredVelocity.sqrMagnitude > 0.1f)
			{
				player.desiredDirection = direction;
			}
		}
	}

	class TurnInstruction : AgentInstruction
	{
		Vector2 direction;

		public TurnInstruction(Vector2 direction)
		{
			this.direction = direction;
		}

		public override void Do(Movement player)
		{
			player.desiredDirection = direction;
		}
	}

	#endregion

	private List<AgentInstruction> instructionQueue;

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
		Constraints = new List<MovementConstraint>();
	}

	void PreventWallCollision()
    {
		if(body.SweepTest(velocity.normalized, out var info, velocity.magnitude * Time.fixedDeltaTime, QueryTriggerInteraction.Ignore)){
			velocity -= Vector3.Dot(info.normal, velocity) * info.normal;
		}
	}

	void FixedUpdate ()
	{
		UpdateState();

		if (VelocityUpdater != null)
		{
			if (VelocityUpdater.UpdateVelocity(this, Time.fixedDeltaTime))
			{
				VelocityUpdater = null;
			}
		}

		foreach (var instruction in instructionQueue)
		{
			instruction.Do(this);
		}

		if (VelocityUpdater == null)
		{
			AdjustVelocity();
		}

		if (desiredJump) {
			desiredJump = false;
			Jump(Physics.gravity);
		}


		PreventWallCollision();

		foreach(var constraint in Constraints)
        {
			constraint.Constrain(this);
        }
		Constraints.RemoveAll(constr => constr.Finished);

		AdjustDirection();
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
		desiredVelocity = Vector3.zero;
		desiredDirection = Vector2.zero;
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
		if (desiredDirection.sqrMagnitude > 0.01f)
		{
			var inpSpaceA = (Mathf.PI / 2f - Mathf.Atan2(playerInputSpace.forward.z, playerInputSpace.forward.x)) * Mathf.Rad2Deg;
			var globalDesiredDirection = Quaternion.AngleAxis(inpSpaceA, Vector3.up) * new Vector3(desiredDirection.x, 0f, desiredDirection.y);
			direction = Vector2.Lerp(direction, new Vector2(globalDesiredDirection.x, globalDesiredDirection.z), rotationCoef);
			body.rotation = Quaternion.FromToRotation(Vector3.forward, AgentForward);
		}
	}

	void AdjustVelocity ()
	{
		Vector3 xAxis = ExtensionMethods.ProjectDirectionOnPlane(InpRightHoriz, contactNormal).normalized;
		Vector3 zAxis = ExtensionMethods.ProjectDirectionOnPlane(InpForwardHoriz, contactNormal).normalized;

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
		JumpNoChecks(jumpSpeed * Vector3.up);
	}

	public void JumpNoChecks(Vector3 dV)
    {
		stepsSinceLastJump = 0;
		jumpPhase += 1;
		velocity += dV;
		//body.velocity = velocity;
	}

	void PerformInstruction(AgentInstruction instruction)
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