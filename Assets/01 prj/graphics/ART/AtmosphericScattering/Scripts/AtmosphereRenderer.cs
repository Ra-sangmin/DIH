using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[DisallowMultipleComponent]
public class AtmosphereRenderer : MonoBehaviour
{
    public int resolution = 25;
    public float planetRadius = 10;
    public float scale = 1.5f;
    public bool updateOnAwake = true;
    public bool useSunDirection = false;
    public bool dynamicSun = false;
    public Transform sunObject;
    
    private MeshRenderer meshRenderer;
    private bool active = false;
    private MaterialPropertyBlock materialProperties;
    private SphereMesh meshOutput = null;
    private bool generating = false;

    private static float initExposure = -1;

    void Awake()
    {
        if (initExposure < 0)
            initExposure = RenderSettings.skybox.GetFloat("_Exposure");
        meshOutput = null;
        generating = false;
        if (updateOnAwake) UpdateBounds();
    }

    void OnDestroy()
    {
        if(active)
            RenderSettings.skybox.SetFloat("_Exposure", initExposure);
    }

    void Update()
    {
	Camera cam = Camera.main;
	if(!cam) return;

        float exposure = 1f;
        Vector3 delta = cam.transform.position - transform.position;
        float distance = delta.sqrMagnitude;
        float outRadius = planetRadius * scale;
        bool inBounds = distance < outRadius * outRadius;
        if (inBounds)
        {
            active = true;
            distance = Mathf.Sqrt(distance);
            delta = delta / distance;
            
            float dot = 1f - (Vector3.Dot(delta, -GetSunDirection()) * 0.5f + 0.5f);
            float he = 1f - Mathf.Clamp01((distance - planetRadius) / (outRadius - planetRadius));

            exposure -= dot * he;
            exposure = Mathf.Clamp01(exposure);
        }
        if (active)
        {
            RenderSettings.skybox.SetFloat("_Exposure", exposure * initExposure);
            if (!inBounds) active = false;
        }
        UpdateMesh();
    }

    [ContextMenu("Update Bounds")]
    public void UpdateBounds()
    {
        if (generating) return;
        meshOutput = null;
        generating = true;
        if (Application.isPlaying)
            System.Threading.ThreadPool.QueueUserWorkItem(GenerateMeshThread);
        else
        {
            GenerateMeshThread(null);
            UpdateMesh();
        }
    }

    private void GenerateMeshThread(object obj)
    {
        meshOutput = SphereMesher.GenerateSphere(resolution, Vertex);
    }

    private void UpdateMesh()
    {
        if (generating && meshOutput != null)
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            if (!filter.sharedMesh)
                filter.sharedMesh = new Mesh();

            filter.sharedMesh.Clear();
            filter.sharedMesh.name = "Atmosphere Bounds";
            filter.sharedMesh.vertices = meshOutput.vertices;
            filter.sharedMesh.normals = meshOutput.normals;
            filter.sharedMesh.uv = meshOutput.uvs;
            filter.sharedMesh.colors = meshOutput.colors;
            filter.sharedMesh.triangles = meshOutput.triangles;
            filter.sharedMesh.RecalculateBounds();
            meshRenderer = GetComponent<MeshRenderer>();
            if (materialProperties == null) materialProperties = new MaterialPropertyBlock();
            materialProperties.SetFloat("_OuterRadius", planetRadius * scale);
            materialProperties.SetFloat("_InnerRadius", planetRadius);
            materialProperties.SetFloat("_RadiusRange", planetRadius * scale - planetRadius);
            materialProperties.SetVector("_SunDirection", GetSunDirection());
            meshRenderer.SetPropertyBlock(materialProperties);
            generating = false;
            meshOutput = null;
        }
        else if (dynamicSun && meshRenderer && materialProperties != null)
        {
            materialProperties.SetVector("_SunDirection", GetSunDirection());
            meshRenderer.SetPropertyBlock(materialProperties);
        }
    }

    private void Vertex(Vector3 dir, out Vector3 vertPos, out Vector3 vertNormal, out Vector2 uvs, out Color vertColor)
    {
        vertPos = dir * planetRadius * scale;
        vertNormal = dir;
        uvs = Vector2.zero;
        vertColor = Color.white;
    }

    private Vector3 GetSunDirection()
    {
        return useSunDirection ? -sunObject.forward : (sunObject.position - transform.position).normalized;
    }
}
