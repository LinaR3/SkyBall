using UnityEngine;

/// <summary>Renders match score, status, controls, and short pickup feedback.</summary>
public sealed class SkyballMatchHud : MonoBehaviour
{
    private static readonly Color BlueColor = new Color(0.24f, 0.68f, 1f);
    private static readonly Color OrangeColor = new Color(1f, 0.53f, 0.2f);
    private static readonly Color CyanColor = new Color(0.35f, 0.95f, 1f);

    private SkyballMatchManager match;
    private GUIStyle scoreStyle;
    private GUIStyle teamStyle;
    private GUIStyle blueTeamStyle;
    private GUIStyle orangeTeamStyle;
    private GUIStyle statusStyle;
    private GUIStyle goalStyle;
    private GUIStyle helpButtonStyle;
    private GUIStyle helpTitleStyle;
    private GUIStyle helpKeyStyle;
    private GUIStyle helpTextStyle;
    private int styledScreenHeight;
    private bool showControls;

    public void Initialize(SkyballMatchManager matchManager)
    {
        match = matchManager;
    }

    private void OnGUI()
    {
        if (match == null) return;
        EnsureStyles();

        float margin = Mathf.Clamp(Screen.width * 0.025f, 12f, 24f);
        float scoreWidth = Mathf.Min(390f, Screen.width - margin * 2f);
        float scoreHeight = Mathf.Clamp(Screen.height * 0.075f, 50f, 64f);
        Rect scorePlate = new Rect((Screen.width - scoreWidth) * 0.5f, margin, scoreWidth, scoreHeight);

        GUI.color = new Color(0.025f, 0.045f, 0.09f, 0.9f);
        GUI.Box(scorePlate, GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(scorePlate.x + 10f, scorePlate.y, scoreWidth * 0.25f, scoreHeight), "AZUL", blueTeamStyle);
        GUI.Label(new Rect(scorePlate.x + scoreWidth * 0.3f, scorePlate.y, scoreWidth * 0.4f, scoreHeight),
            $"{match.BlueScore}  :  {match.OrangeScore}", scoreStyle);
        GUI.Label(new Rect(scorePlate.x + scoreWidth * 0.72f, scorePlate.y, scoreWidth * 0.25f, scoreHeight),
            "NARANJA", orangeTeamStyle);

        float statusY = scorePlate.y + scoreHeight + 6f;
        DrawWaveStatus(margin, scorePlate, scoreHeight, ref statusY);

        string cameraMode = RotateCamera.Instance != null && RotateCamera.Instance.IsFollowCamera
            ? "SEGUIDORA" : "ARENA";
        GUI.color = new Color(1f, 1f, 1f, 0.86f);
        GUI.Label(new Rect(scorePlate.x, statusY, scorePlate.width, 24f),
            "CÁMARA " + cameraMode + " · C PARA CAMBIAR", statusStyle);
        statusY += 26f;

        PlayerController player = PlayerController.Instance;
        if (player != null)
        {
            if (player.RapidFireRemaining > 0f)
                DrawPowerupStatus(scorePlate, statusY, "RÁFAGA · F / CLIC", player.RapidFireRemaining, CyanColor);
            if (player.PowerupRemaining > 0f)
            {
                float secondStatusY = statusY + (player.RapidFireRemaining > 0f ? 26f : 0f);
                DrawPowerupStatus(scorePlate, secondStatusY, "IMPULSO", player.PowerupRemaining, OrangeColor);
            }

            if (!string.IsNullOrEmpty(player.PickupMessage))
            {
                GUI.color = player.PickupMessage.Contains("IMPULSO") ? OrangeColor : CyanColor;
                GUI.Label(new Rect(margin, Screen.height * 0.4f, Screen.width - margin * 2f, 44f),
                    player.PickupMessage, goalStyle);
            }
        }

        if (match.IsShowingGoalMessage)
        {
            GUI.color = GoalColor(match.GoalMessage);
            GUI.Label(new Rect(margin, Screen.height * 0.25f, Screen.width - margin * 2f, 90f),
                match.GoalMessage, goalStyle);
        }

        DrawControlsHelp(margin, scorePlate);
        GUI.color = Color.white;
    }

    private void DrawWaveStatus(float margin, Rect scorePlate, float scoreHeight, ref float nextStatusY)
    {
        SpawnManager spawner = SpawnManager.Instance;
        if (spawner == null) return;

        string label = $"OLEADA {spawner.WaveNumber} · {spawner.EnemyCount} RIVALES";
        float width = Mathf.Min(180f, Screen.width - margin * 2f);
        if (Screen.width >= 820f)
        {
            Rect wavePlate = new Rect(margin, scorePlate.y, width, scoreHeight);
            GUI.color = new Color(0.025f, 0.045f, 0.09f, 0.9f);
            GUI.Box(wavePlate, GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(wavePlate, label, statusStyle);
            return;
        }

        Rect compactPlate = new Rect((Screen.width - width) * 0.5f, nextStatusY, width, 28f);
        GUI.color = new Color(0.025f, 0.045f, 0.09f, 0.9f);
        GUI.Box(compactPlate, GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(compactPlate, label, statusStyle);
        nextStatusY += 32f;
    }

    private void DrawPowerupStatus(Rect scorePlate, float y, string label, float remaining, Color accent)
    {
        float width = Mathf.Min(310f, scorePlate.width);
        Rect chip = new Rect(scorePlate.center.x - width * 0.5f, y, width, 24f);
        GUI.color = new Color(0.025f, 0.045f, 0.09f, 0.84f);
        GUI.Box(chip, GUIContent.none);
        GUI.color = accent;
        GUI.Label(chip, $"{label} · {remaining:0.0} s", statusStyle);
    }

    private void DrawControlsHelp(float margin, Rect scorePlate)
    {
        const float buttonWidth = 96f;
        const float buttonHeight = 34f;
        float buttonY = Screen.width < 700f
            ? Screen.height - margin - buttonHeight
            : margin;
        Rect button = new Rect(Screen.width - margin - buttonWidth, buttonY, buttonWidth, buttonHeight);
        GUI.color = Color.white;
        if (GUI.Button(button, "AYUDA  ?", helpButtonStyle))
            showControls = !showControls;

        if (!showControls) return;

        float panelWidth = Mathf.Min(310f, Screen.width - margin * 2f);
        const float panelHeight = 256f;
        float panelX = Screen.width - margin - panelWidth;
        float panelY = button.yMax + 8f;
        if (panelY + panelHeight > Screen.height - margin)
            panelY = Mathf.Max(margin, button.y - panelHeight - 8f);
        Rect panel = new Rect(panelX, panelY, panelWidth, panelHeight);

        GUI.color = new Color(0.025f, 0.045f, 0.09f, 0.96f);
        GUI.Box(panel, GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(panel.x + 16f, panel.y + 12f, panel.width - 32f, 30f),
            "CÓMO JUGAR", helpTitleStyle);

        float rowY = panel.y + 50f;
        DrawControlRow(panel, rowY, "W / S", "Avanzar y retroceder");
        DrawControlRow(panel, rowY + 29f, "A / D", "Girar");
        DrawControlRow(panel, rowY + 58f, "ESPACIO", "Saltar");
        DrawControlRow(panel, rowY + 87f, "C", "Cambiar cámara");
        DrawControlRow(panel, rowY + 116f, "1 / 2 / 3", "Cambiar tamaño");
        DrawControlRow(panel, rowY + 145f, "F / CLIC", "Disparar con Ráfaga");
        GUI.color = new Color(1f, 1f, 1f, 0.72f);
        GUI.Label(new Rect(panel.x + 16f, rowY + 180f, panel.width - 32f, 28f),
            "Impulso: toca a los rivales para alejarlos.", helpTextStyle);
        GUI.color = Color.white;
    }

    private void DrawControlRow(Rect panel, float y, string key, string action)
    {
        GUI.Label(new Rect(panel.x + 16f, y, 82f, 26f), key, helpKeyStyle);
        GUI.Label(new Rect(panel.x + 100f, y, panel.width - 116f, 26f), action, helpTextStyle);
    }

    private void EnsureStyles()
    {
        if (scoreStyle != null && styledScreenHeight == Screen.height) return;
        styledScreenHeight = Screen.height;
        scoreStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Clamp(Screen.height / 22, 22, 38),
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        teamStyle = new GUIStyle(scoreStyle)
        {
            fontSize = Mathf.Clamp(Screen.height / 42, 12, 20),
            normal = { textColor = Color.white }
        };
        blueTeamStyle = new GUIStyle(teamStyle)
        {
            normal = { textColor = BlueColor }
        };
        orangeTeamStyle = new GUIStyle(teamStyle)
        {
            normal = { textColor = OrangeColor }
        };
        statusStyle = new GUIStyle(teamStyle)
        {
            fontSize = Mathf.Clamp(Screen.height / 48, 11, 16)
        };
        helpButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.Clamp(Screen.height / 58, 12, 16),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        helpTitleStyle = new GUIStyle(teamStyle)
        {
            fontSize = Mathf.Clamp(Screen.height / 42, 14, 20),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        helpKeyStyle = new GUIStyle(teamStyle)
        {
            fontSize = Mathf.Clamp(Screen.height / 54, 12, 16),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = CyanColor }
        };
        helpTextStyle = new GUIStyle(teamStyle)
        {
            fontSize = Mathf.Clamp(Screen.height / 58, 11, 15),
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true
        };
        goalStyle = new GUIStyle(scoreStyle)
        {
            fontSize = Mathf.Clamp(Screen.height / 12, 34, 68),
            normal = { textColor = Color.white }
        };
    }

    private static Color GoalColor(string message)
    {
        return message.Contains("AZUL") ? BlueColor : OrangeColor;
    }
}
