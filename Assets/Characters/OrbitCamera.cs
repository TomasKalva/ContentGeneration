using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{

	public delegate Vector3 CameraFocus();

	//public CameraFocus Focus;

	public CameraUpdater DefaultCamUpdater { get; set; }

	CameraUpdater _camUpdater;

	public CameraUpdater CamUpdater
    {
        get
        {
			return _camUpdater != null ? _camUpdater : DefaultCamUpdater;
        }
        set
        {
			_camUpdater = value;
        }
    }

	[SerializeField, Range(1f, 20f)]
	float distance = 5f;

	[SerializeField, Min(0f)]
	float focusRadius = 5f;

	[SerializeField, Range(0f, 1f)]
	float focusCentering = 0.5f;

	[SerializeField, Range(-89f, 89f)]
	float minVerticalAngle = -45f, maxVerticalAngle = 45f;

	[SerializeField, Min(0f)]
	float alignDelay = 5f;

	[SerializeField, Range(0f, 90f)]
	float alignSmoothRange = 45f;

	[SerializeField, Min(0f)]
	float upAlignmentSpeed = 360f;

	[SerializeField]
	LayerMask obstructionMask = -1;

	Camera regularCamera;

	Vector3 focusPoint, previousFocusPoint;

	Vector2 orbitAngles = new Vector2(45f, 0f);

	Quaternion orbitRotation;

	Vector3 CameraHalfExtends
	{
		get
		{
			Vector3 halfExtends;
			halfExtends.y =
				regularCamera.nearClipPlane *
				Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
			halfExtends.x = halfExtends.y * regularCamera.aspect;
			halfExtends.z = 0f;
			return halfExtends;
		}
	}

	Vector3 desiredFocusPoint;

	public CameraUpdater FocusPlayer(Transform player)
    {
		return new FocusPoint(player);
	}

	public CameraUpdater FocusOnEnemy(Transform player, Transform enemy)
	{
		return new LockOn(player, enemy);
	}



	public class CameraUpdater
	{
		/// <summary>
		/// Returns false after these parameters can no longer be used.
		/// </summary>
		public virtual bool Update(OrbitCamera cam)
        {
			cam.orbitAngles = Vector2.up;
			cam.desiredFocusPoint = Vector3.zero;
			return true;
        }
	}

	public class FocusPoint : CameraUpdater
	{
		Transform point;


		[SerializeField, Min(0f)]
		float alignDelay = 5f;

		[SerializeField, Range(1f, 360f)]
		float rotationSpeed = 90f;

		[SerializeField, Range(0f, 90f)]
		float alignSmoothRange = 45f;

		float lastManualRotationTime;

		public FocusPoint(Transform point)
		{
			this.point = point;
		}

		public override bool Update(OrbitCamera cam)
		{
			if (!point)
			{
				return false;
			}

			cam.desiredFocusPoint = point.transform.position;
			if (ManualRotation(cam) || AutomaticRotation(cam))
			{
				cam.ConstrainAngles();
				cam.orbitRotation = Quaternion.Euler(cam.orbitAngles);
			}

			return true;
		}

		bool ManualRotation(OrbitCamera cam)
		{
			Vector2 input = new Vector2(
				Input.GetAxis("Vertical Camera"),
				Input.GetAxis("Horizontal Camera")
			);
			const float e = 0.001f;
			if (input.x < -e || input.x > e || input.y < -e || input.y > e)
			{
				cam.orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
				lastManualRotationTime = Time.unscaledTime;
				return true;
			}
			return false;
		}

		bool AutomaticRotation(OrbitCamera cam)
		{
			if (Time.unscaledTime - lastManualRotationTime < alignDelay)
			{
				return false;
			}

			Vector3 alignedDelta =
				(cam.focusPoint - cam.previousFocusPoint);
			Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);
			float movementDeltaSqr = movement.sqrMagnitude;
			if (movementDeltaSqr < 0.0001f)
			{
				return false;
			}

			float headingAngle = OrbitCamera.GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
			float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(cam.orbitAngles.y, headingAngle));
			float rotationChange =
				rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
			if (deltaAbs < alignSmoothRange)
			{
				rotationChange *= deltaAbs / alignSmoothRange;
			}
			else if (180f - deltaAbs < alignSmoothRange)
			{
				rotationChange *= (180f - deltaAbs) / alignSmoothRange;
			}
			cam.orbitAngles.y =
				Mathf.MoveTowardsAngle(cam.orbitAngles.y, headingAngle, rotationChange);
			return true;
		}
	}

	public class LockOn : CameraUpdater
	{
		Transform from;

		Transform to;


		[SerializeField, Min(0f)]
		float alignDelay = 5f;

		[SerializeField, Range(1f, 360f)]
		float rotationSpeed = 270f;

		[SerializeField, Range(0f, 90f)]
		float alignSmoothRange = 45f;

		float lastManualRotationTime;

		public LockOn(Transform from, Transform to)
		{
			this.from = from;
			this.to = to;
		}

		public override bool Update(OrbitCamera cam)
		{
			if (!from || !to)
			{
				return false;
			}

			cam.desiredFocusPoint = Vector3.Lerp(from.transform.position, to.transform.position, 0.1f);
			var directionFromTo = ExtensionMethods.ProjectDirectionOnPlane(to.position - from.position, Vector3.up);
			float yaw = GetAngle(new Vector2(directionFromTo.x, directionFromTo.z));
			float pitch = 30f;
			Vector2 desiredAngle = new Vector2(pitch, yaw);
			Vector2 originalAngle = cam.orbitAngles;
			cam.orbitAngles = MoveTowardsPitchYaw(originalAngle, desiredAngle, rotationSpeed * Time.deltaTime);
			return true;
		}

		/// <summary>
		/// Pitch in [-90, 90]. Yaw in [-180, 180].
		/// </summary>
		Vector2 MoveTowardsPitchYaw(Vector2 from, Vector2 to, float speed)
        {
			return new Vector2(Mathf.MoveTowardsAngle(from.x, to.x, speed), Mathf.MoveTowardsAngle(from.y, to.y, speed));
		}
	}

	void OnValidate()
	{
		if (maxVerticalAngle < minVerticalAngle)
		{
			maxVerticalAngle = minVerticalAngle;
		}
	}

	void Awake()
	{
		regularCamera = GetComponent<Camera>();

		transform.localRotation = orbitRotation = Quaternion.Euler(orbitAngles);
		previousFocusPoint = Vector3.zero;
	}

    void LateUpdate()
	{
        if (!CamUpdater.Update(this))
        {
			CamUpdater = null;
        }
		UpdateFocusPoint();

		ConstrainAngles();
		orbitRotation = Quaternion.Euler(orbitAngles);
		Quaternion lookRotation = orbitRotation;

		Vector3 lookDirection = lookRotation * Vector3.forward;
		Vector3 lookPosition = focusPoint - lookDirection * distance;

		Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
		Vector3 rectPosition = lookPosition + rectOffset;
		Vector3 castFrom = focusPoint;
		Vector3 castLine = rectPosition - castFrom;
		float castDistance = castLine.magnitude;
		Vector3 castDirection = castLine / castDistance;

		if (Physics.BoxCast(
			castFrom, CameraHalfExtends, castDirection, out RaycastHit hit,
			lookRotation, castDistance, obstructionMask
		))
		{
			rectPosition = castFrom + castDirection * hit.distance;
			lookPosition = rectPosition - rectOffset;
		}

		transform.SetPositionAndRotation(lookPosition, lookRotation);
	}

	void UpdateFocusPoint()
	{
		previousFocusPoint = focusPoint;
		var targetPoint = desiredFocusPoint;
		if (focusRadius > 0f)
		{
			float distance = Vector3.Distance(targetPoint, focusPoint);
			float t = 1f;
			if (distance > 0.01f && focusCentering > 0f)
			{
				t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
			}
			if (distance > focusRadius)
			{
				t = Mathf.Min(t, focusRadius / distance);
			}
			focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
		}
		else
		{
			focusPoint = targetPoint;
		}
	}

	void ConstrainAngles()
	{
		orbitAngles.x =
			Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

		if (orbitAngles.y < 0f)
		{
			orbitAngles.y += 360f;
		}
		else if (orbitAngles.y >= 360f)
		{
			orbitAngles.y -= 360f;
		}
	}

	public static float GetAngle(Vector2 direction)
	{
		float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
		return direction.x < 0f ? 360f - angle : angle;
	}
}
