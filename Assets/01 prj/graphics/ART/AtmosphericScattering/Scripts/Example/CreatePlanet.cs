using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CreatePlanet : MonoBehaviour
{
    public int resolution = 25;
    public AtmosphereRenderer atmosphere;
    public bool update = false;

    void Start()
    {
        Create();
    }

    private void Update()
    {
        if (update)
        {
            Create();
            update = false;
        }
    }

    void Create()
    {
        SphereMesh meshOutput = SphereMesher.GenerateSphere(resolution, Vertex);
        MeshFilter filter = GetComponent<MeshFilter>();
        if (!filter.sharedMesh)
            filter.sharedMesh = new Mesh();

        filter.sharedMesh.Clear();
        filter.sharedMesh.name = "Planet Mesh";
        filter.sharedMesh.vertices = meshOutput.vertices;
        filter.sharedMesh.normals = meshOutput.normals;
        filter.sharedMesh.uv = meshOutput.uvs;
        filter.sharedMesh.colors = meshOutput.colors;
        filter.sharedMesh.triangles = meshOutput.triangles;
        filter.sharedMesh.RecalculateBounds();
    }

    private void Vertex(Vector3 dir, out Vector3 vertPos, out Vector3 vertNormal, out Vector2 uvs, out Color vertColor)
    {
        vertPos = dir * atmosphere.planetRadius;
        vertNormal = dir;
        uvs = Vector2.zero;
        vertColor = Color.white;
    }
}
