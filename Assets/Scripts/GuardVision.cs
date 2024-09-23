using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GuardVision : MonoBehaviour
{
    public float visionRange = 5f;
    public float visionAngle = 45f;
    public int rayCount = 30;
    public LayerMask obstructionMask;
    public string visionMaterialPath = "VisionConeMaterial";
    public float headHeight = 1.8f;
    public float updateInterval = 0.1f; // Time interval for updates

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material visionMaterial;
    private NavMeshAgent agent;
    private Mesh visionConeMesh;
    private float lastUpdateTime = 0f;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        agent = GetComponent<NavMeshAgent>();
        visionMaterial = Resources.Load<Material>(visionMaterialPath);
        if (visionMaterial != null)
        {
            meshRenderer.material = visionMaterial;
        }
        else
        {
            Debug.LogError("Vision material not found in Resources folder.");
        }

        visionConeMesh = new Mesh();
        meshFilter.mesh = visionConeMesh;
        CreateVisionCone();
    }

    void Update()
    {
        // Update the vision cone at fixed intervals
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            CreateVisionCone();
            lastUpdateTime = Time.time;
        }
    }

    void CreateVisionCone()
    {
        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        Vector3 headPosition = transform.position + Vector3.up * headHeight;
        vertices[0] = transform.InverseTransformPoint(headPosition);

        float angleStep = visionAngle / rayCount;
        float startingAngle = -visionAngle / 2;

        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = startingAngle + angleStep * i;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            RaycastHit hit;

            if (Physics.Raycast(headPosition, direction, out hit, visionRange, obstructionMask))
            {
                vertices[i + 1] = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                vertices[i + 1] = transform.InverseTransformPoint(headPosition + direction * visionRange);
            }

            if (i < rayCount)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        visionConeMesh.Clear();
        visionConeMesh.vertices = vertices;
        visionConeMesh.triangles = triangles;
        visionConeMesh.RecalculateNormals();
    }
}
