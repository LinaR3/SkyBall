using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject powerupPrefab;
    [SerializeField] private float spawnRange = 9f;

    public int enemyCount;
    public int waveNumber;
    private bool waveActive;

    private void Start()
    {
        if (waveNumber < 1)
            waveNumber = 1;
        SpawnEnemyWave(waveNumber);
        SpawnPowerup();
    }

    private void Update()
    {
        enemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        if (waveActive && enemyCount == 0)
        {
            waveNumber++;
            SpawnEnemyWave(waveNumber);
            SpawnPowerup();
        }
    }

    private Vector3 GenerateSpawnPosition()
    {
        return new Vector3(Random.Range(-spawnRange, spawnRange), 0f,
            Random.Range(-spawnRange, spawnRange));
    }

    private void SpawnEnemyWave(int enemiesToSpawn)
    {
        waveActive = true;
        for (int i = 0; i < enemiesToSpawn; i++)
            Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);

        if (waveNumber % 2 == 0)
            SpawnRapidFirePowerup();
    }

    private void SpawnPowerup()
    {
        if (powerupPrefab != null)
            Instantiate(powerupPrefab, GenerateSpawnPosition(), powerupPrefab.transform.rotation);
    }

    private void SpawnRapidFirePowerup()
    {
        Vector3 position = GenerateSpawnPosition() + Vector3.up * 0.8f;
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = "Rapid Fire Powerup";
        pickup.tag = "Powerup";
        pickup.transform.position = position;
        pickup.transform.localScale = Vector3.one * 0.72f;
        pickup.GetComponent<SphereCollider>().isTrigger = true;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader) { color = new Color(1f, 0.55f, 0.08f) };
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(1f, 0.24f, 0.02f) * 1.6f);
        }
        pickup.GetComponent<Renderer>().sharedMaterial = material;
        pickup.AddComponent<RapidFirePickup>();
    }
}
