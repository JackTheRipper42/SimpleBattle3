using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class FilledHexagon : MonoBehaviour
{
    private static Mesh _sharedMesh;

    protected virtual void Awake()
    {
        if (_sharedMesh == null)
        {

            _sharedMesh = new Mesh
            {
                name = "shared filled hexagon"
            };

            var vertices = new Vector3[7];

            var angle = 30f;
            vertices[0] = Vector3.zero;
            for (var index = 0; index < 6; index++)
            {
                var x = Mathf.Sin(angle * Mathf.Deg2Rad) * GridPosition.Size;
                var y = Mathf.Cos(angle * Mathf.Deg2Rad) * GridPosition.Size;

                angle += 60f;

                vertices[index + 1] = new Vector3(x, 0, y);
            }

            var indices = new[]
            {
                0,1,2,
                0,2,3,
                0,3,4,
                0,4,5,
                0,5,6,
                0,6,1
            };

            _sharedMesh.vertices = vertices;
            _sharedMesh.SetTriangles(indices, 0);
            _sharedMesh.RecalculateNormals();
            _sharedMesh.RecalculateTangents();
        }

        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = _sharedMesh;
    }
}