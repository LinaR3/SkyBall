using UnityEngine;

/// <summary>The physical match ball. The player controls a separate character.</summary>
public sealed class SkyballGameBall : MonoBehaviour
{
    public static SkyballGameBall Instance { get; private set; }

    private Rigidbody body;
    private Renderer ballRenderer;
    private Material markingMaterial;
    private Vector3 kickoffPosition;
    private Quaternion kickoffRotation;

    public Rigidbody Body => body;

    private void Awake()
    {
        Instance = this;
        body = GetComponent<Rigidbody>();
        ballRenderer = GetComponent<Renderer>();
        kickoffPosition = transform.position;
        kickoffRotation = transform.rotation;
    }

    private void Update()
    {
        if (transform.position.y < -10f)
            ResetToKickoff();
    }

    public static SkyballGameBall CreateForMatch(Transform player, Transform forwardReference, Collider groundCollider)
    {
        if (Instance != null) return Instance;

        Vector3 forward = forwardReference != null ? forwardReference.forward : Vector3.forward;
        forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
        Vector3 goalDirection = player != null && player.position.z >= 0f ? Vector3.back : Vector3.forward;
        forward = Vector3.Slerp(forward, goalDirection, 0.7f).normalized;

        Vector3 position = player != null
            ? player.position + forward * 2.1f
            : new Vector3(0f, 0.6f, 0f);
        Ray groundRay = new Ray(position + Vector3.up * 4f, Vector3.down);
        if (groundCollider != null && groundCollider.Raycast(groundRay, out RaycastHit groundHit, 10f))
            position.y = groundHit.point.y + 0.58f;
        else
            position.y = Mathf.Max(position.y, 0.58f);

        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Match Ball";
        ball.tag = "Ball";
        ball.transform.position = position;
        ball.transform.localScale = Vector3.one * 1.1f;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader) { color = new Color(1f, 0.79f, 0.12f) };
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.08f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.32f);
        ball.GetComponent<Renderer>().sharedMaterial = material;

        Rigidbody rigidbody = ball.AddComponent<Rigidbody>();
        rigidbody.mass = 0.8f;
        rigidbody.linearDamping = 0.12f;
        rigidbody.angularDamping = 0.08f;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Shader markingShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (markingShader == null) markingShader = Shader.Find("Unlit/Color");
        SkyballGameBall gameBall = ball.AddComponent<SkyballGameBall>();
        gameBall.markingMaterial = new Material(markingShader) { color = new Color(0.08f, 0.16f, 0.24f) };
        CreateBallRing(ball.transform, "Ball Pattern Equator", 0);
        CreateBallRing(ball.transform, "Ball Pattern Meridian A", 1);
        CreateBallRing(ball.transform, "Ball Pattern Meridian B", 2);

        return gameBall;
    }

    private static void CreateBallRing(Transform parent, string ringName, int plane)
    {
        const int segments = 48;
        const float radius = 0.554f;
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float cosine = Mathf.Cos(angle) * radius;
            float sine = Mathf.Sin(angle) * radius;
            points[i] = plane switch
            {
                0 => new Vector3(cosine, 0f, sine),
                1 => new Vector3(cosine, sine, 0f),
                _ => new Vector3(0f, cosine, sine)
            };
        }

        GameObject ringObject = new GameObject(ringName);
        ringObject.transform.SetParent(parent, false);
        LineRenderer ring = ringObject.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.positionCount = points.Length;
        ring.SetPositions(points);
        ring.startWidth = 0.024f;
        ring.endWidth = 0.024f;
        ring.numCapVertices = 2;
        ring.sharedMaterial = parent.GetComponent<SkyballGameBall>().markingMaterial;
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows = false;
    }

    public void HideForGoal()
    {
        if (ballRenderer != null) ballRenderer.enabled = false;
    }

    public void ResetToKickoff()
    {
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = kickoffPosition;
            body.rotation = kickoffRotation;
        }
        else
        {
            transform.SetPositionAndRotation(kickoffPosition, kickoffRotation);
        }
        if (ballRenderer != null) ballRenderer.enabled = true;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (ballRenderer != null && ballRenderer.sharedMaterial != null)
            Destroy(ballRenderer.sharedMaterial);
        if (markingMaterial != null) Destroy(markingMaterial);
    }
}
