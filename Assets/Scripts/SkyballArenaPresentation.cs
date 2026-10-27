using System.Collections.Generic;
using UnityEngine;

/// <summary>Builds and owns the runtime turf and field markings.</summary>
public sealed class SkyballArenaPresentation : MonoBehaviour
{
    public static SkyballArenaPresentation Instance { get; private set; }

    private Material courtMaterial;
    private Material courtLineMaterial;
    private Texture2D courtTexture;
    private Mesh courtTopMesh;
    private GameObject turfObject;

    public Collider GroundCollider { get; private set; }

    private void Awake()
    {
        Instance = this;
        GameObject island = GameObject.Find("Island");
        GroundCollider = island != null ? island.GetComponent<Collider>() : null;
        CreateArenaCourt(island);
    }

    private void OnDestroy()
    {
        if (courtMaterial != null) Destroy(courtMaterial);
        if (courtLineMaterial != null) Destroy(courtLineMaterial);
        if (courtTexture != null) Destroy(courtTexture);
        if (courtTopMesh != null) Destroy(courtTopMesh);
        if (turfObject != null) Destroy(turfObject);
        if (Instance == this) Instance = null;
    }

    private void CreateArenaCourt(GameObject island)
    {
        if (island == null) return;
        MeshFilter sourceFilter = island.GetComponent<MeshFilter>();
        Renderer islandRenderer = island.GetComponent<Renderer>();
        if (sourceFilter == null || sourceFilter.sharedMesh == null || islandRenderer == null) return;

        Mesh sourceMesh = sourceFilter.sharedMesh;
        Vector3[] vertices = sourceMesh.vertices;
        int[] sourceTriangles = sourceMesh.triangles;
        List<int> topTriangles = new List<int>();
        for (int i = 0; i + 2 < sourceTriangles.Length; i += 3)
        {
            int a = sourceTriangles[i];
            int b = sourceTriangles[i + 1];
            int c = sourceTriangles[i + 2];
            Vector3 normal = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]).normalized;
            if (Mathf.Abs(normal.y) <= 0.85f) continue;
            topTriangles.Add(a);
            topTriangles.Add(b);
            topTriangles.Add(c);
        }
        if (topTriangles.Count == 0) return;

        Bounds localBounds = sourceMesh.bounds;
        Vector2[] uvs = new Vector2[vertices.Length];
        float width = Mathf.Max(0.01f, localBounds.size.x);
        float depth = Mathf.Max(0.01f, localBounds.size.z);
        for (int i = 0; i < vertices.Length; i++)
            uvs[i] = new Vector2((vertices[i].x - localBounds.min.x) / width * 7f,
                (vertices[i].z - localBounds.min.z) / depth * 7f);

        courtTopMesh = new Mesh { name = "SkyBall Turf Surface" };
        courtTopMesh.vertices = vertices;
        courtTopMesh.uv = uvs;
        courtTopMesh.SetTriangles(topTriangles, 0);
        courtTopMesh.RecalculateNormals();
        courtTopMesh.RecalculateBounds();

        turfObject = new GameObject("SkyBall Turf");
        turfObject.transform.SetParent(island.transform, false);
        turfObject.transform.localPosition = new Vector3(0f, 0.004f, 0f);
        turfObject.AddComponent<MeshFilter>().sharedMesh = courtTopMesh;
        MeshRenderer turfRenderer = turfObject.AddComponent<MeshRenderer>();
        turfRenderer.sharedMaterial = CreateCourtMaterial();
        turfRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        turfRenderer.receiveShadows = true;

        float y = islandRenderer.bounds.max.y + 0.045f;
        courtLineMaterial = CreateUnlitMaterial(new Color(0.83f, 0.93f, 0.83f));
        CreateRectLine("Touchline", -7.4f, -8.6f, 7.4f, 8.6f, y);
        CreateLine("Halfway Line", new[] { new Vector3(-7.4f, y, 0f), new Vector3(7.4f, y, 0f) }, 0.055f);
        CreateCircleLine("Center Circle", Vector3.zero, 1.75f, y);

        for (int direction = -1; direction <= 1; direction += 2)
        {
            float sign = direction;
            CreateRectLine("Penalty Area " + direction, -4f, sign * 5.3f, 4f, sign * 8.6f, y);
            CreateRectLine("Goal Area " + direction, -2.1f, sign * 6.9f, 2.1f, sign * 8.6f, y);
        }
    }

    private Material CreateCourtMaterial()
    {
        courtTexture = new Texture2D(96, 96, TextureFormat.RGB24, true)
        {
            name = "SkyBall Turf Grain",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear
        };
        Color[] pixels = new Color[courtTexture.width * courtTexture.height];
        for (int y = 0; y < courtTexture.height; y++)
        for (int x = 0; x < courtTexture.width; x++)
        {
            float broadVariation = Mathf.PerlinNoise(x * 0.055f, y * 0.055f);
            float fineVariation = Mathf.PerlinNoise(x * 0.32f, y * 0.32f);
            float shade = 0.84f + broadVariation * 0.14f + fineVariation * 0.05f;
            pixels[y * courtTexture.width + x] = new Color(0.1f, 0.34f, 0.23f) * shade;
        }
        courtTexture.SetPixels(pixels);
        courtTexture.Apply(true, true);

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader) { color = Color.white };
        material.mainTexture = courtTexture;
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", courtTexture);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.02f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.18f);
        courtMaterial = material;
        return material;
    }

    private void CreateRectLine(string name, float minX, float minZ, float maxX, float maxZ, float y)
    {
        CreateLine(name, new[]
        {
            new Vector3(minX, y, minZ), new Vector3(maxX, y, minZ),
            new Vector3(maxX, y, maxZ), new Vector3(minX, y, maxZ),
            new Vector3(minX, y, minZ)
        }, 0.055f);
    }

    private void CreateCircleLine(string name, Vector3 center, float radius, float y)
    {
        const int segments = 48;
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            points[i] = new Vector3(center.x + Mathf.Cos(angle) * radius, y, center.z + Mathf.Sin(angle) * radius);
        }
        CreateLine(name, points, 0.055f);
    }

    private void CreateLine(string name, Vector3[] positions, float width)
    {
        GameObject lineObject = new GameObject(name);
        lineObject.transform.SetParent(transform, false);
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = positions.Length;
        line.SetPositions(positions);
        line.startWidth = width;
        line.endWidth = width;
        line.numCornerVertices = 2;
        line.numCapVertices = 2;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.sharedMaterial = courtLineMaterial;
    }

    private static Material CreateUnlitMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        return new Material(shader) { color = color };
    }
}
