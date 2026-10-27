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
    private GUIStyle controlStyle;
    private GUIStyle goalStyle;
    private int styledScreenHeight;

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

        DrawControlPanel(player, margin);
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

    private void DrawControlPanel(PlayerController player, float margin)
    {
        float panelHeight = Screen.height < 500f ? 90f : 84f;
        Rect panel = new Rect(margin, Screen.height - panelHeight - margin,
            Screen.width - margin * 2f, panelHeight);
        GUI.color = new Color(0.025f, 0.045f, 0.09f, 0.9f);
        GUI.Box(panel, GUIContent.none);
        GUI.color = new Color(1f, 1f, 1f, 0.92f);

        float rowHeight = (panelHeight - 10f) / 3f;
        GUI.Label(new Rect(panel.x + 8f, panel.y + 3f, panel.width - 16f, rowHeight),
            "MOVER W/S  ·  GIRAR A/D  ·  SALTAR ESPACIO", controlStyle);
        string size = player != null ? player.BallSizeLabel : "MEDIANA · NORMAL";
        GUI.Label(new Rect(panel.x + 8f, panel.y + 3f + rowHeight, panel.width - 16f, rowHeight),
            "TAMAÑO 1 / 2 / 3: " + size + "  ·  CÁMARA C", controlStyle);
        GUI.Label(new Rect(panel.x + 8f, panel.y + 3f + rowHeight * 2f, panel.width - 16f, rowHeight),
            "DISPARAR F / CLIC AL RECOGER EL PODER", controlStyle);
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
        controlStyle = new GUIStyle(teamStyle)
        {
            fontSize = Mathf.Clamp(Screen.height / 50, 11, 16),
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
