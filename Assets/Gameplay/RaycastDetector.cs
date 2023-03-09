using UnityEngine;

public class RaycastDetector : MonoBehaviour
{
    [SerializeField]
    LayerMask detectionMask = -1;

    [SerializeField]
    public float distance;

    public Collider other;

    public bool Triggered => other;

    private void FixedUpdate()
    {
        other = Physics.Raycast(
            transform.position, 
            transform.TransformDirection(Vector3.forward), 
            out var hit, distance, 
            detectionMask)
            ? hit.collider : null;
    }
}
