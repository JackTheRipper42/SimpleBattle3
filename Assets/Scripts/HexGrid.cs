using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class HexGrid : MonoBehaviour
{
    private const int Cells = 15;
    private const float HalfWidth = 0.02f;

    private static Mesh _sharedMesh;

    protected virtual void Awake()
    {
        var gridPosition = GridPosition.FromVector3(transform.position);
        transform.position = GridPosition.ToVector3(gridPosition);

        if (_sharedMesh == null)
        {
            _sharedMesh = new Mesh
            {
                name = "shared hex grid"
            };

            var vertices = new List<Vector3>(4 * Cells * Cells * 12);
            var indices = new List<int>(4 * Cells * Cells * 36);

            for (var u = -Cells; u < Cells; u++)
            {
                for (var v = -Cells; v < Cells; v++)
                {
                    AddCell(u, v, vertices, indices);
                }
            }

            _sharedMesh.SetVertices(vertices);
            _sharedMesh.SetTriangles(indices, 0);
            _sharedMesh.RecalculateNormals();
            _sharedMesh.RecalculateTangents();
        }



        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = _sharedMesh;
    }

    private static void AddCell(int u, int v, ICollection<Vector3> vertices, ICollection<int> indices)
    {
        var center = GridPosition.ToVector3(new GridPosition(u, v));

        var verticesOffset = vertices.Count;
        var angle = 30f;
        for (var index = 0; index < 12; index += 2)
        {
            var x1 = Mathf.Sin(angle * Mathf.Deg2Rad) * (GridPosition.Size - HalfWidth);
            var y1 = Mathf.Cos(angle * Mathf.Deg2Rad) * (GridPosition.Size - HalfWidth);
            var x2 = Mathf.Sin(angle * Mathf.Deg2Rad) * (GridPosition.Size + HalfWidth);
            var y2 = Mathf.Cos(angle * Mathf.Deg2Rad) * (GridPosition.Size + HalfWidth);

            angle += 60f;

            vertices.Add(new Vector3(x1, 0, y1) + center);
            vertices.Add(new Vector3(x2, 0, y2) + center);
        }

        foreach (var index in new[]
        {
            0,1,2,2,1,3,
            2,3,4,4,3,5,
            4,5,6,6,5,7,
            6,7,8,8,7,9,
            8,9,10,10,9,11,
            10,11,0,0,11,1
        })
        {
            indices.Add(index + verticesOffset);
        }
    }
}