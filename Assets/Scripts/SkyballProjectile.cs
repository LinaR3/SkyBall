using UnityEngine;

public sealed class SkyballProjectile : MonoBehaviour
{
    [SerializeField] private float enemyImpulse = 7f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.isTrigger) return;

        if (other.CompareTag("Enemy"))
        {
            Rigidbody enemyBody = other.attachedRigidbody;
            if (enemyBody != null)
            {
                Vector3 direction = enemyBody.worldCenterOfMass - transform.position;
                if (direction.sqrMagnitude > 0.001f)
                    enemyBody.AddForce(direction.normalized * enemyImpulse, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}
