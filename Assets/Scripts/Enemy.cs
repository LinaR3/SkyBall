using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 3f;
    [SerializeField, Min(0f)] private float acceleration = 9f;
    private Rigidbody enemyRb;
    private Transform player;

    private void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        CachePlayerTarget();
    }

    private void FixedUpdate()
    {
        if (enemyRb == null) return;
        if (player == null) CachePlayerTarget();
        Transform target = player;
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;
        direction.Normalize();

        Vector3 currentVelocity = enemyRb.linearVelocity;
        Vector3 planarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        Vector3 desiredVelocity = direction * speed;
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
