using System;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [Tooltip("Ground checkers range")]
    public float checkRange = 0.35f;
    
    public bool IsGrounded { get; private set; }

    private float _checkDistance;

    private void Awake()
    {
        _checkDistance = GetComponentInParent<PlayerController>().checkDistance;
    }

    private void Update()
    {
        Vector3 position1 = new Vector3(transform.position.x + checkRange, transform.position.y + checkRange, transform.position.z);
        Vector3 position2 = new Vector3(transform.position.x + checkRange, transform.position.y - checkRange, transform.position.z);
        Vector3 position3 = new Vector3(transform.position.x - checkRange, transform.position.y - checkRange, transform.position.z);
        Vector3 position4 = new Vector3(transform.position.x - checkRange, transform.position.y + checkRange, transform.position.z);

        // Domyślnie false
        IsGrounded = false;

        // Sprawdź każdy z 4 rayów
        if (CheckGroundRay(position1) || CheckGroundRay(position2) ||
            CheckGroundRay(position3) || CheckGroundRay(position4))
        {
            IsGrounded = true;
        }
    }

    private bool CheckGroundRay(Vector3 origin)
    {
        return Physics.Raycast(origin, Vector3.forward, out RaycastHit hit, _checkDistance)
               && hit.collider.CompareTag("Ground");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;

        Vector3 position1 = new Vector3(transform.position.x + checkRange, transform.position.y + checkRange, transform.position.z);
        Vector3 position2 = new Vector3(transform.position.x + checkRange, transform.position.y - checkRange, transform.position.z);
        Vector3 position3 = new Vector3(transform.position.x - checkRange, transform.position.y - checkRange, transform.position.z);
        Vector3 position4 = new Vector3(transform.position.x - checkRange, transform.position.y + checkRange, transform.position.z);

        Gizmos.DrawLine(position1, position1 + Vector3.forward * _checkDistance);
        Gizmos.DrawLine(position2, position2 + Vector3.forward * _checkDistance);
        Gizmos.DrawLine(position3, position3 + Vector3.forward * _checkDistance);
        Gizmos.DrawLine(position4, position4 + Vector3.forward * _checkDistance);
    }
}