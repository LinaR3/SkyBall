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
        if (!other.CompareTag("Player")) return;
        SkyballMatchManager.Instance?.ScoreGoal(!blueTeamGoal);
    }
}
