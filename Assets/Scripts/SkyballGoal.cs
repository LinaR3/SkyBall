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
        if (!other.CompareTag("Ball")) return;
        SkyballGameBall ball = other.GetComponentInParent<SkyballGameBall>();
        if (ball == null) return;

        SkyballMatchManager.Instance?.ScoreGoal(!blueTeamGoal, ball);
    }
}
