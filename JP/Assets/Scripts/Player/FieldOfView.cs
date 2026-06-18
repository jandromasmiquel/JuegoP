using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float fov = 90f;
    [SerializeField] private float viewDistance = 15f;
    [SerializeField] private int rayCount = 80;
    [SerializeField] private LayerMask layerMask;

    private Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "Field Of View Mesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void LateUpdate()
    {
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero;

        float startAngle = -fov * 0.5f;
        float angleStep = fov / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector3 localDirection = Quaternion.Euler(0f, 0f, angle) * Vector3.right;
            Vector3 worldDirection = transform.TransformDirection(localDirection);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, worldDirection, viewDistance, layerMask);
            Vector3 worldPoint = hit.collider == null
                ? transform.position + worldDirection * viewDistance
                : (Vector3)hit.point;

            vertices[i + 1] = transform.InverseTransformPoint(worldPoint);

            if (i < rayCount)
            {
                int triangleIndex = i * 3;
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = i + 2;
                triangles[triangleIndex + 2] = i + 1;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public bool CanSeePoint(Vector2 point)
    {
        Vector2 origin = transform.position;
        Vector2 toPoint = point - origin;

        if (toPoint.sqrMagnitude > viewDistance * viewDistance)
        {
            return false;
        }

        float angleToPoint = Vector2.Angle(transform.right, toPoint);
        if (angleToPoint > fov * 0.5f)
        {
            return false;
        }

        RaycastHit2D hit = Physics2D.Raycast(origin, toPoint.normalized, toPoint.magnitude, layerMask);
        return hit.collider == null;
    }
}
