using UnityEngine;

/// <summary>The physical match ball. The player controls a separate character.</summary>
public sealed class SkyballGameBall : MonoBehaviour
{
    public static SkyballGameBall Instance { get; private set; }

    private Rigidbody body;
    private Renderer ballRenderer;
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

    public static SkyballGameBall CreateForMatch(Transform player, Transform forwardReference)
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
        if (Physics.Raycast(position + Vector3.up * 4f, Vector3.down, out RaycastHit groundHit,
                10f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
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
        Material material = new Material(shader) { color = new Color(0.96f, 0.98f, 1f) };
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.08f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.42f);
        ball.GetComponent<Renderer>().sharedMaterial = material;

        Rigidbody rigidbody = ball.AddComponent<Rigidbody>();
        rigidbody.mass = 0.8f;
        rigidbody.linearDamping = 0.12f;
        rigidbody.angularDamping = 0.08f;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        SkyballGameBall gameBall = ball.AddComponent<SkyballGameBall>();
        return gameBall;
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
    }
}
