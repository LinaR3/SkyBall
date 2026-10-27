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
    }

    private void SpawnPowerup()
    {
        if (powerupPrefab != null)
            Instantiate(powerupPrefab, GenerateSpawnPosition(), powerupPrefab.transform.rotation);
    }
}
