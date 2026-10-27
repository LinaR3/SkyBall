using UnityEngine;

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
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ScoreGoal(bool blueTeamScored)
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
        messageUntil = Time.time + resetDelay;
        resetAt = messageUntil;
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
