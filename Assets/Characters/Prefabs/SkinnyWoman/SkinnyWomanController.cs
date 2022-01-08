using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnyWomanAgent))]
public class SkinnyWomanController : EnemyController<SkinnyWomanAgent>
{
	[SerializeField]
	public ColliderDetector rushForwardDetector;

	[SerializeField]
	public ColliderDetector castDetector;

	private void Start()
	{
	}
}