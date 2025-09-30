using UnityEngine;

public class BuildingController : MonoBehaviour
{
    [Header("Sampling Grid")]
    [Tooltip("Rozstaw pomiędzy promieniami (powinien odpowiadać rozmiarowi bloku).")]
    public float gridSpacing = 1f;

    [Tooltip("Tag bloków/podłoża.")]
    public string groundTag = "Ground";

    [Tooltip("Aktualne trafienia 3×3 (góra→dół, lewo→prawo).")]
    public GameObject[] cells = new GameObject[9];

    [Header("Debug")]
    public bool drawGizmos = true;

    private float _checkDistance;

    private void Awake()
    {
        _checkDistance = GetComponentInParent<PlayerController>().checkDistance;
    }


    void Update()
    {
        int idx = 0;
        for (int ry = 1; ry >= -1; ry--)          // góra -> dół
        {
            for (int rx = -1; rx <= 1; rx++)      // lewo -> prawo
            {
                Vector3 origin = transform.position + new Vector3(rx * gridSpacing, ry * gridSpacing, 0f);

                if (Physics.Raycast(origin, Vector3.forward, out RaycastHit hit, _checkDistance)
                    && hit.collider.CompareTag(groundTag))
                {
                    cells[idx] = hit.collider.gameObject;
                }
                else
                {
                    cells[idx] = null;
                }

                idx++;
            }
        }
    }

    public GameObject GetCell(int row, int col)
    {
        row = Mathf.Clamp(row, 0, 2);
        col = Mathf.Clamp(col, 0, 2);
        return cells[row * 3 + col];
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.cyan;
        for (int ry = 1; ry >= -1; ry--)
        {
            for (int rx = -1; rx <= 1; rx++)
            {
                Vector3 origin = transform.position + new Vector3(rx * gridSpacing, ry * gridSpacing, 0f);
                Gizmos.DrawLine(origin, origin + Vector3.forward * _checkDistance);
                Gizmos.DrawSphere(origin, 0.05f);
            }
        }
    }
}