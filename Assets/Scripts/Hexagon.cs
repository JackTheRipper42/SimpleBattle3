using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class Hexagon : MonoBehaviour
{
    private const float HalfWidth = 0.05f;

    protected virtual void Awake()
    {
        var mesh = new Mesh
        {
            name = "hexagon"
        };

        var vertices = new Vector3[12];

        var angle = 30f;
        for (var index = 0; index < 12; index += 2)
        {
            var x1 = Mathf.Sin(angle * Mathf.Deg2Rad) * (GridPosition.Size - HalfWidth);
            var y1 = Mathf.Cos(angle * Mathf.Deg2Rad) * (GridPosition.Size - HalfWidth);
            var x2 = Mathf.Sin(angle * Mathf.Deg2Rad) * (GridPosition.Size + HalfWidth);
            var y2 = Mathf.Cos(angle * Mathf.Deg2Rad) * (GridPosition.Size + HalfWidth);

            angle += 60f;

            vertices[index] = new Vector3(x1, 0, y1);
            vertices[index + 1] = new Vector3(x2, 0, y2);
        }

        var indices = new[]
        {
            0, 1, 2, 2, 1, 3,
            2, 3, 4, 4, 3, 5,
            4, 5, 6, 6, 5, 7,
            6, 7, 8, 8, 7, 9,
            8, 9, 10, 10, 9, 11,
            10, 11, 0, 0, 11, 1
        };

        mesh.vertices = vertices;
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }
}