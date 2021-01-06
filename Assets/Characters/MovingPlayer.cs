using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * D E P R E C A T E D
 * 
 * public class MovingPlayer : MonoBehaviour
{
    public Transform inputSpace;
	Rigidbody rigidbody;
	public float groundAngle;
	private int groundContactCount;
	public float maxSpeed;
	public float maxAcceleration;
	private Vector3 desiredVelocity;
	private Vector3 velocity;
	public float jumpHeight;
	private float jumpAcceleration;
	bool desiredJump;

	private Vector3 forward;
	private Vector3 right;
	Vector3 contactNormal;
	public LayerMask probeMask;

	// Start is called before the first frame update
	void Start()
    {
		jumpAcceleration = Mathf.Sqrt(2f * (-Physics.gravity.y) * jumpHeight);
		rigidbody = GetComponent<Rigidbody>();
	}

	Vector3 ProjectToPlane(Vector3 vec, Vector3 normal)
    {
		return (vec - Vector3.Dot(vec, normal) * normal).normalized;
    }

	void OnCollisionEnter(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void OnCollisionStay(Collision collision)
	{
		EvaluateCollision(collision);
	}

	float MinDot => Mathf.Cos(groundAngle * Mathf.Deg2Rad);

	void EvaluateCollision(Collision collision)
	{
		float minDot = MinDot;
		for (int i = 0; i < collision.contactCount; i++)
		{
			Vector3 normal = collision.GetContact(i).normal;
			float upDot = Vector3.Dot(normal, Vector3.up);
			if (upDot >= minDot)
			{
				groundContactCount += 1;
				contactNormal += normal;
			}
		}
		contactNormal /= collision.contactCount;
	}

	Vector3 UpVector()
    {
		if(groundContactCount > 0 && Vector3.Dot(contactNormal, Vector3.up) < MinDot)
        {
			return contactNormal;
        }
        else
        {
			return Vector3.up;
        }
    }

	void UpdateSpeed()
    {

    }

	// Update is called once per frame
	void Update()
	{
		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");


		desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
		desiredJump |= Input.GetButtonDown("Jump");
    }

	bool Grounded => Physics.Raycast(GetComponent<Rigidbody>().position, -Vector3.up, out RaycastHit hit, 1f, probeMask);

	void Jump()
    {
		if(Grounded)
		{
			velocity += jumpAcceleration * Vector3.up;
		}
    }

    void FixedUpdate()
	{
		var upVector = UpVector();
		forward = ProjectToPlane(inputSpace.forward, upVector);
		right = ProjectToPlane(inputSpace.right, upVector);

		velocity = GetComponent<Rigidbody>().velocity;

		var xzVelocity = velocity - velocity.y * Vector3.up;
		var yVel = Vector3.Dot(velocity, forward);
		var xVel = Vector3.Dot(velocity, right);
		var newVelX = Mathf.MoveTowards(xVel, desiredVelocity.x, maxAcceleration);
		var newVelY = Mathf.MoveTowards(yVel, desiredVelocity.z, maxAcceleration);
		velocity += (newVelX - xVel) * right + (newVelY - yVel) * forward;

		if(groundContactCount > 0)
        {
			rigidbody.useGravity = false;
        }
        else
        {
			rigidbody.useGravity = true;
        }

        if (desiredJump)
        {
			Jump();
			desiredJump = false;
        }


		rigidbody.velocity = velocity;
		groundContactCount = 0;
	}
}
*/