using UnityEngine;

public sealed class SkyballProjectile : MonoBehaviour
{
    private Rigidbody ballBody;
    private Vector3 shotDirection;
    private float ballImpulse;

    public void Initialize(Rigidbody targetBall, Vector3 direction, float impulse)
    {
        ballBody = targetBall;
        shotDirection = direction.normalized;
        ballImpulse = impulse;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || other.attachedRigidbody == ballBody) return;

        if (other.CompareTag("Enemy") && ballBody != null)
        {
            // Clear a lane by nudging defenders aside, then transfer the main
            // force to the ball so the shot advances it toward the goal.
            Rigidbody enemyBody = other.attachedRigidbody;
            if (enemyBody != null)
                enemyBody.AddForce(shotDirection * 2.5f, ForceMode.Impulse);
        }

        if (ballBody != null)
            ballBody.AddForce(shotDirection * ballImpulse, ForceMode.Impulse);
        Destroy(gameObject);
    }
}
