using UnityEngine;

/// <summary>Owns match scoring and coordinates the short goal reset.</summary>
public sealed class SkyballMatchManager : MonoBehaviour
{
    public static SkyballMatchManager Instance { get; private set; }

    [SerializeField, Min(0f)] private float resetDelay = 1.5f;

    private int blueScore;
    private int orangeScore;
    private float resetAt;
    private string goalMessage = string.Empty;
    private float messageUntil;
    private bool goalPending;

    public int BlueScore => blueScore;
    public int OrangeScore => orangeScore;
    public string GoalMessage => goalMessage;
    public bool IsShowingGoalMessage => goalPending && Time.time < messageUntil;

    public static void EnsureInstance()
    {
        if (Instance == null)
            new GameObject("SkyBall Match Manager").AddComponent<SkyballMatchManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.AddComponent<SkyballGoalFactory>();
        gameObject.AddComponent<SkyballArenaPresentation>();
        gameObject.AddComponent<SkyballMatchHud>().Initialize(this);
    }

    private void Start()
    {
        PlayerController controller = PlayerController.Instance;
        Transform player = controller != null ? controller.transform : null;
        Transform forward = controller != null ? controller.AimReference : null;
        Collider ground = SkyballArenaPresentation.Instance != null
            ? SkyballArenaPresentation.Instance.GroundCollider
            : null;
        SkyballGameBall.CreateForMatch(player, forward, ground);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ScoreGoal(bool blueTeamScored, SkyballGameBall ball)
    {
        if (goalPending || ball == null) return;

        goalPending = true;
        if (blueTeamScored)
        {
            blueScore++;
            goalMessage = "GOOOL AZUL";
        }
        else
        {
            orangeScore++;
            goalMessage = "GOOOL NARANJA";
        }

        CreateGoalExplosion(ball.transform.position,
            blueTeamScored ? new Color(0.1f, 0.65f, 1f) : new Color(1f, 0.38f, 0.08f));
        ball.HideForGoal();
        messageUntil = Time.time + resetDelay;
        resetAt = messageUntil;
    }

    private void Update()
    {
        if (!goalPending || Time.time < resetAt) return;

        goalPending = false;
        if (PlayerController.Instance != null)
            PlayerController.Instance.ResetForGoal();
        if (SkyballGameBall.Instance != null)
            SkyballGameBall.Instance.ResetToKickoff();
    }

    private static void CreateGoalExplosion(Vector3 position, Color color)
    {
        GameObject burst = new GameObject("Goal Ball Explosion");
        burst.transform.position = position;
        ParticleSystem particles = burst.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.playOnAwake = false;
        main.duration = 0.18f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.07f, 0.17f);
        main.startColor = color;
        main.maxParticles = 90;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.25f;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)80) });
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.28f;

        ParticleSystemRenderer renderer = burst.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader != null)
        {
            Material material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            renderer.sharedMaterial = material;
            Destroy(material, 3f);
        }

        particles.Play();
        Destroy(burst, 3f);
    }
}
