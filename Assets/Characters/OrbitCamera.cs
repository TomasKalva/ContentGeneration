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
			return _camUpdater ?? DefaultCamUpdater;
        }
        set
        {
			_camUpdater = value;
			if(value != null)
            {
				_camUpdater.Start(this);
            }
        }
    }

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

	Vector3 cameraPosition, previousCameraPosition;

	Vector3 focusPoint, previousFocusPoint;

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

	Vector3 desiredCameraPosition;

	public CameraUpdater FocusPlayer(Transform player)
    {
		return new FocusPoint(player);
	}

	public CameraUpdater FocusOnEnemy(Transform player, Agent enemy)
	{
		return new LockOn(player, enemy);
	}



	public class CameraUpdater
	{
		public virtual void PreUpdate(OrbitCamera cam)
        {
			cam.desiredCameraPosition = Vector3.one;
			cam.desiredFocusPoint = Vector3.zero;
		}

		public virtual void PostUpdate(OrbitCamera cam)
		{
			cam.desiredCameraPosition = Vector3.one;
			cam.desiredFocusPoint = Vector3.zero;
		}

		/// <summary>
		/// Returns false after these parameters can no longer be used.
		/// </summary>
		public virtual bool Finished() => false;

		public virtual void Start(OrbitCamera cam)
		{

		}
	}

	public class FocusPoint : CameraUpdater
	{
		Transform point;

		Vector2 orbitAngles = new Vector2(45f, 0f);

		Quaternion orbitRotation;

		[SerializeField, Min(0f)]
		float alignDelay = 5f;

		//[SerializeField, Range(1f, 360f)]
		float rotationSpeed = 270f;

		[SerializeField, Range(0f, 90f)]
		float alignSmoothRange = 45f;

		[SerializeField, Range(1f, 10f)]
		float distance = 3f;

		float lastManualRotationTime;

		public FocusPoint(Transform point)
		{
			this.point = point;
		}

		public override void Start(OrbitCamera cam)
		{
			orbitAngles = cam.transform.eulerAngles;
		}

		public override bool Finished() => !point;

		public override void PreUpdate(OrbitCamera cam)
		{
			cam.desiredFocusPoint = point.transform.position;

			//orbitAngles = cam.transform.rotation.eulerAngles;

			ManualRotation(cam);
			AutomaticRotation(cam);

			var constrainedOrbitAngles = cam.ConstrainAngles(orbitAngles);
			orbitAngles = constrainedOrbitAngles;

			orbitRotation = Quaternion.Euler(orbitAngles);

			var focusPoint = point != null ? point.position : Vector3.zero;
			cam.desiredFocusPoint = focusPoint;
		}

		public override void PostUpdate(OrbitCamera cam)
		{
			Vector3 lookDirection = orbitRotation * Vector3.forward;
			var cameraPosition = cam.focusPoint - lookDirection * distance;
			cam.cameraPosition = cameraPosition;
		}

		bool ManualRotation(OrbitCamera cam)
		{

			Vector2 input = 0.05f * new Vector2(
				Input.GetAxis("Vertical Camera"),
				Input.GetAxis("Horizontal Camera")
			);
			const float e = 0.001f;
			if (input.x < -e || input.x > e || input.y < -e || input.y > e)
			{
				orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
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
			float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
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
			orbitAngles.y =
				Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
			return true;
		}
	}

	public class LockOn : CameraUpdater
	{
		Transform from;

		Agent to;

		[SerializeField, Min(0f)]
		float alignDelay = 5f;

		public LockOn(Transform from, Agent to)
		{
			this.from = from;
			this.to = to;
		}

		public override void Start(OrbitCamera cam)
        {

        }

		public override bool Finished() => !from || !to;

		public override void PreUpdate(OrbitCamera cam)
		{
			//
			//  c         
			//   \       f
			//    \   /
			//     a
			var focusPoint = Vector3.Lerp(from.transform.position, to.transform.position + to.CenterOffset * Vector3.up, 0.3f);
			cam.desiredFocusPoint = focusPoint;

			var directionFromTo = ExtensionMethods.ProjectDirectionOnPlane(to.transform.position - from.position, Vector3.up);
			float yaw = GetAngle(new Vector2(directionFromTo.x, directionFromTo.z));
			var cameraPosition = from.position - 3f * (Quaternion.Euler(new Vector3(20f, yaw)) * Vector3.forward);
			cam.desiredCameraPosition = cameraPosition;
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

		previousFocusPoint = Vector3.zero;
		previousCameraPosition = Vector3.one;
		transform.LookAt(previousCameraPosition);
	}

    void LateUpdate()
	{
        if (CamUpdater.Finished())
        {
			CamUpdater = null;
        }

        if (CamUpdater != null)
        {
			CamUpdater.PreUpdate(this);
        }

		UpdateFocusPoint();
		UpdateCameraPosition();

		if (CamUpdater != null)
		{
			CamUpdater.PostUpdate(this);
		}

		transform.position = cameraPosition;
		transform.LookAt(focusPoint);


		Quaternion lookRotation = transform.rotation;
		Vector3 lookDirection = lookRotation * Vector3.forward;

		Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
		Vector3 rectPosition = cameraPosition + rectOffset;
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
			cameraPosition = rectPosition - rectOffset;
		}

		transform.position = cameraPosition;

		var constrainedOrbitAngles = ConstrainAngles(transform.rotation.eulerAngles);
		transform.rotation = Quaternion.Euler(constrainedOrbitAngles);


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

	void UpdateCameraPosition()
    {
		var targetPoint = desiredCameraPosition;
		float t = 0.3f;// Mathf.Pow(1f - 0.99999f, Time.unscaledDeltaTime);
		cameraPosition = Vector3.Lerp(targetPoint, cameraPosition, t);
	}

	Vector2 ConstrainAngles(Vector2 orbitAngles)
	{
		// Angles should be in [-90, 90]
		if (orbitAngles.x > 180f)
			orbitAngles.x -= 360f;

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

		return orbitAngles;
	}

	public static float GetAngle(Vector2 direction)
	{
		float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
		return direction.x < 0f ? 360f - angle : angle;
	}
}
