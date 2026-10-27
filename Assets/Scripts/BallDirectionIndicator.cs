using UnityEngine;

/// <summary>Shows the player's facing direction as subtle ground chevrons.</summary>
public sealed class BallDirectionIndicator : MonoBehaviour
{
    [SerializeField] private float forwardOffset = 1f;
    [SerializeField] private float groundOffset = 0.035f;

    private Transform player;
    private Transform aimReference;
    private SphereCollider playerCollider;
    private Collider groundCollider;
    private Transform marker;
    private Material markerMaterial;

    public void Initialize(Transform playerTransform, Transform directionSource, Collider groundSurface)
    {
        player = playerTransform;
        aimReference = directionSource;
        playerCollider = player != null ? player.GetComponent<SphereCollider>() : null;
        groundCollider = groundSurface;
        CreateMarker();
    }

    public void SetVisible(bool visible)
    {
        if (marker != null) marker.gameObject.SetActive(visible);
    }

    private void Update()
    {
        if (marker == null || player == null || aimReference == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(aimReference.forward, Vector3.up).normalized;
        float radius = playerCollider != null
            ? playerCollider.radius * Mathf.Abs(player.lossyScale.x)
            : forwardOffset;
        Vector3 markerPosition = player.position + forward * Mathf.Max(forwardOffset, radius + 0.28f);
        Ray groundRay = new Ray(markerPosition + Vector3.up * 2f, Vector3.down);
        if (groundCollider != null && groundCollider.Raycast(groundRay, out RaycastHit hit, 6f))
            markerPosition = hit.point + Vector3.up * groundOffset;
        else
            markerPosition += Vector3.down * 0.72f;

        marker.SetPositionAndRotation(markerPosition, Quaternion.LookRotation(forward, Vector3.up));
    }

    private void CreateMarker()
    {
        marker = new GameObject("Direction Chevrons").transform;
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        markerMaterial = new Material(shader) { color = new Color(0.2f, 0.95f, 0.84f, 1f) };
        markerMaterial.renderQueue = 4000;
        if (markerMaterial.HasProperty("_ZTest"))
            markerMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        if (markerMaterial.HasProperty("_ZWrite")) markerMaterial.SetInt("_ZWrite", 0);

        CreateSegment("Front Chevron Left", new Vector3(-0.3f, 0f, -0.08f), new Vector3(0f, 0f, 0.16f));
        CreateSegment("Front Chevron Right", new Vector3(0f, 0f, 0.16f), new Vector3(0.3f, 0f, -0.08f));
        CreateSegment("Rear Chevron Left", new Vector3(-0.3f, 0f, -0.36f), new Vector3(0f, 0f, -0.12f));
        CreateSegment("Rear Chevron Right", new Vector3(0f, 0f, -0.12f), new Vector3(0.3f, 0f, -0.36f));
    }

    private void CreateSegment(string segmentName, Vector3 start, Vector3 end)
    {
        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        segment.name = segmentName;
        segment.transform.SetParent(marker, false);
        segment.transform.localPosition = (start + end) * 0.5f;
        Vector3 direction = end - start;
        segment.transform.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        segment.transform.localScale = new Vector3(0.095f, 0.035f, direction.magnitude);
        Destroy(segment.GetComponent<Collider>());

        MeshRenderer renderer = segment.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = markerMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
    }

    private void OnDestroy()
    {
        if (markerMaterial != null) Destroy(markerMaterial);
        if (marker != null) Destroy(marker.gameObject);
    }
}
