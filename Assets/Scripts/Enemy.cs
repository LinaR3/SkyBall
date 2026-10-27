using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;
    private Rigidbody enemyRb;
    private Transform player;

    private void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        FindPlayerTarget();
    }

    private void FixedUpdate()
    {
        if (enemyRb == null) return;
        if (player == null) FindPlayerTarget();
        Transform target = player;
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;
        direction.Normalize();
        enemyRb.AddForce(direction * speed);

        if (transform.position.y < -10f)
            Destroy(gameObject);
    }

    private void FindPlayerTarget()
    {
        PlayerController controller = PlayerController.Instance;
        if (controller != null)
        {
            player = controller.transform;
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }
}
