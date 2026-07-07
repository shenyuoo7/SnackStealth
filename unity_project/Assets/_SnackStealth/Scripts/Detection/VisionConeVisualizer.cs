using UnityEngine;

namespace SnackStealth.Detection
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class VisionConeVisualizer : MonoBehaviour
    {
        [SerializeField] private TeacherVisionSensor sensor;
        [SerializeField, Range(8, 64)] private int segments = 32;

        private Mesh coneMesh;

        public void Configure(TeacherVisionSensor newSensor)
        {
            sensor = newSensor;
            Rebuild();
        }

        private void Awake()
        {
            Rebuild();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                Rebuild();
            }
        }

        private void LateUpdate()
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null && sensor != null && meshRenderer.sharedMaterial != null)
            {
                Color color = sensor.CanSeeTarget
                    ? new Color(1f, 0.12f, 0.05f, 0.34f)
                    : new Color(1f, 0.8f, 0.1f, 0.20f);

                meshRenderer.sharedMaterial.color = color;
            }
        }

        private void Rebuild()
        {
            if (sensor == null)
            {
                return;
            }

            coneMesh ??= new Mesh { name = "VisionConeMesh" };
            coneMesh.Clear();

            int vertexCount = segments + 2;
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[segments * 3];

            vertices[0] = Vector3.zero;

            float halfAngle = sensor.HorizontalAngle * 0.5f;
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                vertices[i + 1] = direction * sensor.ViewDistance;
            }

            for (int i = 0; i < segments; i++)
            {
                int triangleIndex = i * 3;
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = i + 1;
                triangles[triangleIndex + 2] = i + 2;
            }

            coneMesh.vertices = vertices;
            coneMesh.triangles = triangles;
            coneMesh.RecalculateBounds();

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = coneMesh;
            }
        }
    }
}
