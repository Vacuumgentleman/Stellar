using UnityEngine;

namespace bullets
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class Cylinders : MonoBehaviour
    {
        private void Start()
        {
            var mesh = new Mesh
            {
                name = "Fire3DCylinder",
                vertices = GenerateVertices(),
                triangles = GenerateTriangles(),
                uv = GenerateUVs(GenerateVertices())
            };

            mesh.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = mesh;
        }

        private Vector3[] GenerateVertices()
        {
            float coef = 0.3f;
            float coef2 = 0.2f;

            float radius = Mathf.Cos(Mathf.PI / 4f);
            float rHalf = radius * 0.4f;

            return new Vector3[] {
                new Vector3(-0.4f * coef2, -2.5f, 0),
                new Vector3(-rHalf * coef2, -2.5f, rHalf * coef2),
                new Vector3(0, -2.5f, 0.4f * coef2),
                new Vector3(rHalf * coef2, -2.5f, rHalf * coef2),
                new Vector3(0.4f * coef2, -2.5f, 0),
                new Vector3(rHalf * coef2, -2.5f, -rHalf * coef2),
                new Vector3(0, -2.5f, -0.4f * coef2),
                new Vector3(-rHalf * coef2, -2.5f, -rHalf * coef2),

                new Vector3(-0.4f * coef, -0.5f, 0),
                new Vector3(-rHalf * coef, -0.5f, rHalf * coef),
                new Vector3(0, -0.5f, 0.4f * coef),
                new Vector3(rHalf * coef, -0.5f, rHalf * coef),
                new Vector3(0.4f * coef, -0.5f, 0),
                new Vector3(rHalf * coef, -0.5f, -rHalf * coef),
                new Vector3(0, -0.5f, -0.4f * coef),
                new Vector3(-rHalf * coef, -0.5f, -rHalf * coef)
            };
        }

        private int[] GenerateTriangles()
        {
            return new int[] {
                8, 0, 1,
                8, 1, 9,
                9, 1, 2,
                9, 2, 10,
                10, 2, 3,
                10, 3, 11,
                11, 3, 4,
                11, 4, 12,
                12, 4, 5,
                12, 5, 13,
                13, 5, 6,
                13, 6, 14,
                14, 6, 7,
                14, 7, 15,
                15, 7, 8,
                8, 7, 0
            };
        }

        private Vector2[] GenerateUVs(Vector3[] vertices)
        {
            Vector2[] uvs = new Vector2[vertices.Length];
            float height = 2f;

            for (int i = 0; i < vertices.Length; i++)
            {
                float u = Mathf.Atan2(vertices[i].z, vertices[i].x) / (2 * Mathf.PI);
                float v = (vertices[i].y + height * 0.5f) / height;
                uvs[i] = new Vector2(u, v);
            }

            return uvs;
        }
    }
}
