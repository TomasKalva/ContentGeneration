using UnityEngine;

/// <summary>
/// Why not use cinemachine?
/// </summary>
[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{

	public delegate Vector3 CameraFocus();

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

	[SerializeField]
	LayerMask obstructionMask = -1;

	Vector3 cameraPosition, previousCameraPosition;

	Vector3 focusPoint, previousFocusPoint;

	Camera regularCamera;

	Vector3 CameraHalfExtents
	{
		get
		{
			Vector3 halfExtents;
			halfExtents.y =
				regularCamera.nearClipPlane *
				Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
			halfExtents.x = halfExtents.y * regularCamera.aspect;
			halfExtents.z = 0f;
			return halfExtents;
		}
	}

	Vector3 desiredFocusPoint;

	Vector3 desiredCameraPosition;

	public CameraUpdater FocusPlayer(Transform player)
    {
		return new FocusPoint(player);
	}

	public abstract class CameraUpdater
	{
		public abstract void PreUpdate(OrbitCamera cam);

		public abstract void PostUpdate(OrbitCamera cam);

		/// <summary>
		/// Returns false after these parameters can no longer be used.
		/// </summary>
		public abstract bool Finished();

		public abstract void Start(OrbitCamera cam);

		public abstract void UpdateCameraPosition(OrbitCamera cam);
	}

	public class FocusPoint : CameraUpdater
	{
		Transform focusPoint;

		Vector2 orbitAngles = new Vector2(45f, 0f);

		Quaternion orbitRotation;

		float alignDelay = 5f;

		float rotationSpeed = 270f;

		float alignSmoothRange = 45f;

		float distance = 3f;

		float lastManualRotationTime;

		public FocusPoint(Transform point)
		{
			this.focusPoint = point;
		}

		public override void Start(OrbitCamera cam)
		{
			orbitAngles = cam.transform.eulerAngles;
		}

		public override bool Finished() => !focusPoint;

		public override void PreUpdate(OrbitCamera cam)
		{
			if (!this.focusPoint)
				return;

			cam.desiredFocusPoint = this.focusPoint.position;

			ManualRotation(cam);
			AutomaticRotation(cam);

			var constrainedOrbitAngles = cam.ConstrainAngles(orbitAngles);
			orbitAngles = constrainedOrbitAngles;

			orbitRotation = Quaternion.Euler(orbitAngles);

			var focusPoint = this.focusPoint != null ? this.focusPoint.position : Vector3.zero;
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

			float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
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

		public override void UpdateCameraPosition(OrbitCamera cam)
		{
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
        if (CamUpdater != null &&
			CamUpdater.Finished())
        {
			CamUpdater = null;
        }

        if (CamUpdater != null)
        {
			CamUpdater.PreUpdate(this);
        }

		UpdateFocusPoint();
		if(CamUpdater != null)
		{
			CamUpdater.UpdateCameraPosition(this);
		}

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
			castFrom, CameraHalfExtents, castDirection, out RaycastHit hit,
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
