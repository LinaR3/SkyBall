using UnityEngine;

public sealed class RapidFirePickup : MonoBehaviour
{
    private Vector3 startPosition;
    private Material ownedMaterial;

    public void Initialize(Material pickupMaterial)
    {
        ownedMaterial = pickupMaterial;
        GetComponent<Renderer>().sharedMaterial = ownedMaterial;
    }

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, 110f * Time.deltaTime, Space.World);
        transform.position = startPosition + Vector3.up * (0.12f * Mathf.Sin(Time.time * 3f));
    }

    private void OnDestroy()
    {
        if (ownedMaterial != null) Destroy(ownedMaterial);
    }
}
