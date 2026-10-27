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

        CreateGoalPart(goal.transform, goalName + " Left Post", new Vector3(-2.15f, 1.25f, 0f), new Vector3(0.2f, 2.5f, 0.2f), color);
        CreateGoalPart(goal.transform, goalName + " Right Post", new Vector3(2.15f, 1.25f, 0f), new Vector3(0.2f, 2.5f, 0.2f), color);
        CreateGoalPart(goal.transform, goalName + " Crossbar", new Vector3(0f, 2.4f, 0f), new Vector3(4.5f, 0.2f, 0.2f), color);

        // A light back frame gives the goal depth and reads as a net from the game camera.
        CreateGoalPart(goal.transform, goalName + " Back Bar", new Vector3(0f, 1.2f, Mathf.Sign(z) * 1.3f), new Vector3(4.3f, 0.12f, 0.12f), color * 0.72f);
        CreateGoalPart(goal.transform, goalName + " Net Top", new Vector3(0f, 2.3f, Mathf.Sign(z) * 1.3f), new Vector3(4.2f, 0.08f, 0.08f), color * 0.72f);
        CreateGoalPart(goal.transform, goalName + " Net Left", new Vector3(-2.05f, 1.2f, Mathf.Sign(z) * 1.3f), new Vector3(0.08f, 2.2f, 0.08f), color * 0.72f);
        CreateGoalPart(goal.transform, goalName + " Net Right", new Vector3(2.05f, 1.2f, Mathf.Sign(z) * 1.3f), new Vector3(0.08f, 2.2f, 0.08f), color * 0.72f);

        BoxCollider trigger = goal.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 1.25f, 0f);
        trigger.size = new Vector3(4.1f, 2.4f, 1.3f);
        SkyballGoal goalTrigger = goal.AddComponent<SkyballGoal>();
        goalTrigger.SetBlueTeamGoal(blueTeamGoal);
    }

    private static void CreateGoalPart(Transform parent, string partName, Vector3 localPosition, Vector3 scale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = scale;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var material = new Material(shader) { color = color };
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.3f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.72f);
        part.GetComponent<Renderer>().sharedMaterial = material;
    }
}
