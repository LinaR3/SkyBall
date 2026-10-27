using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;
    [SerializeField] private Transform playerGoal;
    private Rigidbody enemyRb;
    private Transform player;

    private void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        if (playerGoal == null)
        {
            GameObject goalObject = GameObject.Find("Player Goal");
            if (goalObject != null) playerGoal = goalObject.transform;
        }
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    private void FixedUpdate()
    {
        if (enemyRb == null) return;
        Transform target = playerGoal != null ? playerGoal : player;
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        enemyRb.AddForce(direction * speed);

        if (transform.position.y < -10f)
            Destroy(gameObject);
    }
}
