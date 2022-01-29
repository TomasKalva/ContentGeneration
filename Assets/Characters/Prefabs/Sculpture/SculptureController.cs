using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SculptureAgent))]
public class SculptureController : EnemyController<SculptureAgent>
{
	[SerializeField]
	public ColliderDetector leftWideDetector;
	
	[SerializeField]
	public ColliderDetector rightWideDownDetector;

	[SerializeField]
	public ColliderDetector doubleSwipeLeftDetector;

	[SerializeField]
	public ColliderDetector doubleSwipeRightDetector;

	[SerializeField]
	public ColliderDetector overheadDetector;

	[SerializeField]
	public ColliderDetector groundSlamDetector;

    private void Start()
	{
	}
}