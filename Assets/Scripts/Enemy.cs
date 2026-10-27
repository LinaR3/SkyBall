using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 2.8f;
    [SerializeField, Min(0f)] private float acceleration = 7f;
    private Rigidbody enemyRb;
    private Transform player;
    private Vector3 pursuitOffset;

    public void SetPursuitOffset(Vector3 offset)
    {
        pursuitOffset = offset;
    }

    private void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            enemyRb.mass = 0.65f;
            enemyRb.linearDamping = 0.25f;
            enemyRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        CachePlayerTarget();
    }

    private void FixedUpdate()
    {
        if (enemyRb == null) return;
        if (player == null) CachePlayerTarget();
        Transform target = player;
        if (target == null) return;

        Vector3 pursuitTarget = target.position + pursuitOffset;
        pursuitTarget.x = Mathf.Clamp(pursuitTarget.x, -7f, 7f);
        pursuitTarget.z = Mathf.Clamp(pursuitTarget.z, -8f, 8f);
        Vector3 direction = pursuitTarget - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;
        Vector3 desiredVelocity = distance < 0.08f
            ? Vector3.zero
            : direction.normalized * Mathf.Min(speed, distance * 2f);

        Vector3 currentVelocity = enemyRb.linearVelocity;
        Vector3 planarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        Vector3 velocityChange = Vector3.ClampMagnitude(
            desiredVelocity - planarVelocity, acceleration * Time.fixedDeltaTime);
        enemyRb.AddForce(velocityChange, ForceMode.VelocityChange);

        if (transform.position.y < -10f)
            Destroy(gameObject);
    }

    private void CachePlayerTarget()
    {
        PlayerController controller = PlayerController.Instance;
        if (controller != null) player = controller.transform;
    }
}
