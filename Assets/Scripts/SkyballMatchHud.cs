using UnityEngine;

/// <summary>Renders match score, status, controls, and short pickup feedback.</summary>
public sealed class SkyballMatchHud : MonoBehaviour
{
    private SkyballMatchManager match;
    private GUIStyle scoreStyle;
    private GUIStyle teamStyle;
    private GUIStyle goalStyle;

    public void Initialize(SkyballMatchManager matchManager)
    {
        match = matchManager;
    }

    private void OnGUI()
    {
        if (match == null) return;
        EnsureStyles();
        float width = Mathf.Min(390f, Screen.width * 0.58f);
        float height = Mathf.Max(54f, Screen.height * 0.075f);
        Rect plate = new Rect((Screen.width - width) * 0.5f, 18f, width, height);
        GUI.color = new Color(0.025f, 0.045f, 0.09f, 0.9f);
        GUI.Box(plate, GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(plate.x + 8f, plate.y, width * 0.3f, height), "AZUL", teamStyle);
        GUI.Label(new Rect(plate.x + width * 0.28f, plate.y, width * 0.44f, height),
            $"{match.BlueScore}  :  {match.OrangeScore}", scoreStyle);
        GUI.Label(new Rect(plate.x + width * 0.68f - 8f, plate.y, width * 0.3f, height), "NARANJA", teamStyle);

        if (match.IsShowingGoalMessage)
            GUI.Label(new Rect(0f, Screen.height * 0.28f, Screen.width, 100f), match.GoalMessage, goalStyle);

        GUI.color = new Color(1f, 1f, 1f, 0.8f);
        string cameraMode = RotateCamera.Instance != null && RotateCamera.Instance.IsFollowCamera
            ? "SEGUIDORA" : "ARENA";
        GUI.Label(new Rect(plate.x, plate.y + plate.height + 3f, plate.width, 24f),
            "CÁMARA " + cameraMode + " · C PARA CAMBIAR", teamStyle);

        SpawnManager spawner = SpawnManager.Instance;
        if (spawner != null)
            GUI.Label(new Rect(18f, 18f, 220f, 30f),
                $"OLEADA {spawner.WaveNumber} · RIVALES {spawner.EnemyCount}", teamStyle);

        GUI.Label(new Rect(18f, Screen.height - 54f, Screen.width - 36f, 24f),
            "1 / 2 / 3  TAMAÑO DE BOLA     ·     ESPACIO  SALTAR     ·     F  DISPARAR CON EL PODER", teamStyle);

        PlayerController player = PlayerController.Instance;
        if (player == null)
        {
            GUI.color = Color.white;
            return;
        }

        GUI.Label(new Rect(18f, Screen.height - 32f, Screen.width - 36f, 22f),
            "TAMAÑO: " + player.BallSizeLabel, teamStyle);
        if (!string.IsNullOrEmpty(player.PickupMessage))
        {
            GUI.color = new Color(0.45f, 0.95f, 1f);
            GUI.Label(new Rect(0f, Screen.height * 0.42f, Screen.width, 44f), player.PickupMessage, goalStyle);
        }

        float powerupRemaining = Mathf.Max(player.RapidFireRemaining, player.PowerupRemaining);
        if (powerupRemaining > 0f)
        {
            string label = player.RapidFireRemaining > 0f
                ? "DISPARO POTENCIADO · F / CLIC" : "IMPULSO ACTIVO";
            GUI.color = new Color(1f, 0.68f, 0.2f);
            GUI.Label(new Rect(plate.x, plate.y + plate.height + 25f, plate.width, 24f),
                label + " · " + powerupRemaining.ToString("0.0") + " s", teamStyle);
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
}
