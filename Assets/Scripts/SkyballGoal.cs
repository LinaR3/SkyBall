using UnityEngine;

public class SkyballGoal : MonoBehaviour
{
    private bool blueTeamGoal;

    public void SetBlueTeamGoal(bool isBlueGoal)
    {
        blueTeamGoal = isBlueGoal;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only the game ball can score. The controllable player is a separate
        // object (also tagged Player in the current scene) and must not score.
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null) return;
        if (!other.CompareTag("Ball")) return;

        SkyballMatchManager.Instance?.ScoreGoal(!blueTeamGoal, other.transform.position, null);
    }
}
