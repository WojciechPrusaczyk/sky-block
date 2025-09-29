using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Tooltip("Jak daleko w przód (oś Z) sprawdzamy podłoże.")]
    public float checkDistance = 1f;

    public bool IsGrounded { get; private set; }

    private void Update()
    {
        // Ray wzdłuż osi Z (lokalnie: forward)
        Ray ray = new Ray(transform.position, Vector3.forward);
        IsGrounded = Physics.Raycast(ray, out RaycastHit hit, checkDistance)
                     && hit.collider.CompareTag("Ground");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * checkDistance);
    }
}