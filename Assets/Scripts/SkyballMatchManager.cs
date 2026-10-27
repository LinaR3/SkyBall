using UnityEngine;
using System.Collections.Generic;

public sealed class SkyballMatchManager : MonoBehaviour
{
    public static SkyballMatchManager Instance { get; private set; }

    [SerializeField] private float resetDelay = 1.5f;
    private int blueScore;
    private int orangeScore;
    private float resetAt;
    private string goalMessage = "";
    private float messageUntil;
    private GUIStyle scoreStyle;
    private GUIStyle teamStyle;
    private GUIStyle goalStyle;
    private bool goalPending;
    private Material courtMaterial;
    private Material courtLineMaterial;
    private Texture2D courtTexture;
    private Mesh courtTopMesh;

    public static void EnsureInstance()
    {
        if (Instance != null) return;
        var managerObject = new GameObject("SkyBall Match Manager");
        Instance = managerObject.AddComponent<SkyballMatchManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CreateGoals();
        CreateArenaCourt();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (courtMaterial != null) Destroy(courtMaterial);
        if (courtLineMaterial != null) Destroy(courtLineMaterial);
        if (courtTexture != null) Destroy(courtTexture);
        if (courtTopMesh != null) Destroy(courtTopMesh);
    }

    public void ScoreGoal(bool blueTeamScored, Vector3 ballPosition, PlayerController player)
    {
        if (goalPending) return;
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
        CreateGoalExplosion(ballPosition, blueTeamScored ? new Color(0.1f, 0.65f, 1f) : new Color(1f, 0.38f, 0.08f));
        if (player != null) player.HideForGoalExplosion();
        messageUntil = Time.time + resetDelay;
        resetAt = messageUntil;
    }

    private static void CreateGoalExplosion(Vector3 position, Color color)
    {
        GameObject burst = new GameObject("Goal Ball Explosion");
        burst.transform.position = position;
        ParticleSystem particles = burst.AddComponent<ParticleSystem>();
        var main = particles.main;
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

        ParticleSystemRenderer particleRenderer = burst.GetComponent<ParticleSystemRenderer>();
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader != null)
        {
            Material material = new Material(particleShader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            particleRenderer.sharedMaterial = material;
            Destroy(material, 3f);
        }
        particles.Play();
        Destroy(burst, 3f);
    }

    private void Update()
    {
        if (!goalPending || Time.time < resetAt) return;
        goalPending = false;
        GoalReset.ResetAll();
    }

    private void OnGUI()
    {
        EnsureStyles();
        float width = Mathf.Min(390f, Screen.width * 0.58f);
        float height = Mathf.Max(54f, Screen.height * 0.075f);
        var plate = new Rect((Screen.width - width) * 0.5f, 18f, width, height);
        GUI.color = new Color(0.025f, 0.045f, 0.09f, 0.9f);
        GUI.Box(plate, GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(plate.x + 8, plate.y, width * 0.3f, height), "AZUL", teamStyle);
        GUI.Label(new Rect(plate.x + width * 0.28f, plate.y, width * 0.44f, height),
            $"{blueScore}  :  {orangeScore}", scoreStyle);
        GUI.Label(new Rect(plate.x + width * 0.68f - 8, plate.y, width * 0.3f, height), "NARANJA", teamStyle);

        if (goalPending && Time.time < messageUntil)
            GUI.Label(new Rect(0, Screen.height * 0.28f, Screen.width, 100), goalMessage, goalStyle);

        GUI.color = new Color(1f, 1f, 1f, 0.8f);
        string cameraMode = RotateCamera.Instance != null && RotateCamera.Instance.IsFollowCamera ? "SEGUIDORA" : "ARENA";
        GUI.Label(new Rect(plate.x, plate.y + plate.height + 3f, plate.width, 24f), "CÁMARA " + cameraMode + " · C PARA CAMBIAR", teamStyle);
        GUI.Label(new Rect(18f, Screen.height - 54f, Screen.width - 36f, 24f), "1 / 2 / 3  TAMAÑO DE BOLA     ·     ESPACIO  SALTAR     ·     F  DISPARAR CON EL PODER", teamStyle);

        PlayerController player = PlayerController.Instance;
        if (player != null)
        {
            GUI.Label(new Rect(18f, Screen.height - 32f, Screen.width - 36f, 22f), "TAMAÑO: " + player.BallSizeLabel, teamStyle);
            if (player.RapidFireRemaining > 0f)
            {
                GUI.color = new Color(1f, 0.68f, 0.2f);
                GUI.Label(new Rect(plate.x, plate.y + plate.height + 25f, plate.width, 24f),
                    "DISPARO INFINITO · " + player.RapidFireRemaining.ToString("0.0") + " s", teamStyle);
            }
        }
        GUI.color = Color.white;
    }

    private void EnsureStyles()
    {
        if (scoreStyle != null) return;
        scoreStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Clamp(Screen.height / 22, 22, 38),
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        teamStyle = new GUIStyle(scoreStyle)
        {
            fontSize = Mathf.Clamp(Screen.height / 42, 13, 20)
        };
        goalStyle = new GUIStyle(scoreStyle)
        {
            fontSize = Mathf.Clamp(Screen.height / 12, 36, 74),
            normal = { textColor = new Color(1f, 0.84f, 0.28f) }
        };
    }

    private void CreateGoals()
    {
        CreateGoal("Blue Goal", true, 8.35f, new Color(0.08f, 0.65f, 1f));
        CreateGoal("Orange Goal", false, -8.35f, new Color(1f, 0.36f, 0.08f));
    }

    private void CreateArenaCourt()
    {
        GameObject island = GameObject.Find("Island");
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
            if (Mathf.Abs(normal.y) > 0.85f)
            {
                topTriangles.Add(a);
                topTriangles.Add(b);
                topTriangles.Add(c);
            }
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

        GameObject turf = new GameObject("SkyBall Turf");
        turf.transform.SetParent(island.transform, false);
        turf.transform.localPosition = new Vector3(0f, 0.004f, 0f);
        turf.AddComponent<MeshFilter>().sharedMesh = courtTopMesh;
        MeshRenderer turfRenderer = turf.AddComponent<MeshRenderer>();
        turfRenderer.sharedMaterial = CreateCourtMaterial();
        turfRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        turfRenderer.receiveShadows = true;

        float y = islandRenderer.bounds.max.y + 0.045f;
        courtLineMaterial = CreateUnlitMaterial(new Color(0.83f, 0.93f, 0.83f));
        CreateRectLine("Touchline", -6.2f, -6.9f, 6.2f, 6.9f, y);
        CreateLine("Halfway Line", new[] { new Vector3(-6.2f, y, 0f), new Vector3(6.2f, y, 0f) }, 0.055f);
        CreateCircleLine("Center Circle", Vector3.zero, 1.55f, y);

        for (int direction = -1; direction <= 1; direction += 2)
        {
            float sign = direction;
            CreateRectLine("Penalty Area " + direction, -3.4f, sign * 4.45f, 3.4f, sign * 6.9f, y);
            CreateRectLine("Goal Area " + direction, -1.8f, sign * 5.55f, 1.8f, sign * 6.9f, y);
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
        {
            for (int x = 0; x < courtTexture.width; x++)
            {
                float broadVariation = Mathf.PerlinNoise(x * 0.055f, y * 0.055f);
                float fineVariation = Mathf.PerlinNoise(x * 0.32f, y * 0.32f);
                float shade = 0.84f + broadVariation * 0.14f + fineVariation * 0.05f;
                pixels[y * courtTexture.width + x] = new Color(0.1f, 0.34f, 0.23f) * shade;
            }
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

    private void CreateGoal(string goalName, bool blueTeamGoal, float z, Color color)
    {
        GameObject goal = new GameObject(goalName);
        goal.transform.position = new Vector3(0f, 0f, z);
        goal.tag = "Goal";

        float depthDirection = Mathf.Sign(z);
        Color frameColor = new Color(0.9f, 0.95f, 1f);
        Color netColor = Color.Lerp(color, Color.white, 0.42f);
        Material frameMaterial = CreateGoalMaterial(frameColor, 0.72f, 0.78f);
        Material teamMaterial = CreateGoalMaterial(color, 0.48f, 0.7f);
        Material netMaterial = CreateGoalMaterial(netColor, 0.12f, 0.45f);

        // Rounded frame, set slightly behind the scoring plane.
        CreateGoalTube(goal.transform, goalName + " Left Post", new Vector3(-2.15f, 0.05f, 0f), new Vector3(-2.15f, 2.45f, 0f), 0.105f, frameMaterial);
        CreateGoalTube(goal.transform, goalName + " Right Post", new Vector3(2.15f, 0.05f, 0f), new Vector3(2.15f, 2.45f, 0f), 0.105f, frameMaterial);
        CreateGoalTube(goal.transform, goalName + " Crossbar", new Vector3(-2.15f, 2.45f, 0f), new Vector3(2.15f, 2.45f, 0f), 0.105f, frameMaterial);

        // Colored foot bands make the two goals easy to tell apart at a glance.
        CreateGoalTube(goal.transform, goalName + " Left Team Band", new Vector3(-2.15f, 0.23f, 0f), new Vector3(-2.15f, 0.58f, 0f), 0.12f, teamMaterial);
        CreateGoalTube(goal.transform, goalName + " Right Team Band", new Vector3(2.15f, 0.23f, 0f), new Vector3(2.15f, 0.58f, 0f), 0.12f, teamMaterial);

        float backZ = depthDirection * 1.35f;
        CreateGoalTube(goal.transform, goalName + " Back Top", new Vector3(-2.08f, 2.35f, backZ), new Vector3(2.08f, 2.35f, backZ), 0.035f, teamMaterial);
        CreateGoalTube(goal.transform, goalName + " Back Left", new Vector3(-2.08f, 0.08f, backZ), new Vector3(-2.08f, 2.35f, backZ), 0.035f, teamMaterial);
        CreateGoalTube(goal.transform, goalName + " Back Right", new Vector3(2.08f, 0.08f, backZ), new Vector3(2.08f, 2.35f, backZ), 0.035f, teamMaterial);
        CreateGoalTube(goal.transform, goalName + " Depth Left", new Vector3(-2.15f, 2.42f, 0f), new Vector3(-2.08f, 2.35f, backZ), 0.045f, frameMaterial);
        CreateGoalTube(goal.transform, goalName + " Depth Right", new Vector3(2.15f, 2.42f, 0f), new Vector3(2.08f, 2.35f, backZ), 0.045f, frameMaterial);

        // Thin cross-lines form a visible net without turning the goal into a solid wall.
        for (int i = 1; i <= 4; i++)
        {
            float x = Mathf.Lerp(-1.9f, 1.9f, i / 5f);
            CreateGoalTube(goal.transform, goalName + " Net Vertical " + i,
                new Vector3(x, 0.1f, backZ), new Vector3(x, 2.3f, backZ), 0.012f, netMaterial);
        }
        for (int i = 1; i <= 4; i++)
        {
            float y = Mathf.Lerp(0.35f, 2.1f, i / 5f);
            CreateGoalTube(goal.transform, goalName + " Net Horizontal " + i,
                new Vector3(-2.05f, y, backZ), new Vector3(2.05f, y, backZ), 0.012f, netMaterial);
        }


        BoxCollider trigger = goal.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 1.25f, 0f);
        trigger.size = new Vector3(4.1f, 2.4f, 1.3f);
        SkyballGoal goalTrigger = goal.AddComponent<SkyballGoal>();
        goalTrigger.SetBlueTeamGoal(blueTeamGoal);
    }

    private static Material CreateGoalMaterial(Color color, float metallic, float smoothness)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var material = new Material(shader) { color = color };
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        return material;
    }

    private static void CreateGoalTube(Transform parent, string name, Vector3 start, Vector3 end, float radius, Material material)
    {
        GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tube.name = name;
        tube.transform.SetParent(parent, false);
        Vector3 delta = end - start;
        tube.transform.localPosition = (start + end) * 0.5f;
        tube.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        tube.transform.localScale = new Vector3(radius * 2f, delta.magnitude * 0.5f, radius * 2f);
        Collider collider = tube.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        tube.GetComponent<Renderer>().sharedMaterial = material;
    }
}
