using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns enemy waves and their pickups for the active match.</summary>
public sealed class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject powerupPrefab;
    [SerializeField, Min(1f)] private float spawnRange = 7.2f;
    [SerializeField, Min(1)] private int startingEnemyCount = 4;
    [SerializeField, Min(1)] private int enemiesAddedPerWave = 1;
    [SerializeField, Min(1)] private int maximumEnemyCount = 7;
    [SerializeField, Min(0f)] private float minimumPlayerDistance = 3f;
    [SerializeField, Min(1f)] private float powerupSpawnInterval = 8f;
    [SerializeField, Min(1)] private int maximumActivePowerups = 3;

    [SerializeField] private int enemyCount;
    [SerializeField] private int waveNumber = 1;

    public int EnemyCount => enemyCount;
    public int WaveNumber => waveNumber;

    private readonly List<Enemy> activeEnemies = new List<Enemy>();
    private bool waveActive;
    private float nextPowerupSpawnTime;
    private bool spawnRapidFireNext;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (enemyPrefab == null || enemyPrefab.GetComponent<Enemy>() == null)
        {
            Debug.LogError("SpawnManager requires an enemy prefab with an Enemy component.", this);
            enabled = false;
            return;
        }

        waveNumber = Mathf.Max(1, waveNumber);
        SpawnEnemyWave(Mathf.Clamp(startingEnemyCount, 1, Mathf.Max(1, maximumEnemyCount)));
        SpawnPowerup();
        nextPowerupSpawnTime = Time.time + Mathf.Max(1f, powerupSpawnInterval);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
        enemyCount = activeEnemies.Count;
        if (waveActive && enemyCount == 0)
        {
            waveNumber++;
            int nextWaveCount = Mathf.Min(Mathf.Max(1, maximumEnemyCount),
                Mathf.Max(1, startingEnemyCount + (waveNumber - 1) * Mathf.Max(1, enemiesAddedPerWave)));
            SpawnEnemyWave(nextWaveCount);
        }

        if (Time.time < nextPowerupSpawnTime) return;
        nextPowerupSpawnTime = Time.time + Mathf.Max(1f, powerupSpawnInterval);
        if (GameObject.FindGameObjectsWithTag("Powerup").Length < Mathf.Max(1, maximumActivePowerups))
            SpawnPowerup();
    }

    private void SpawnEnemyWave(int count)
    {
        waveActive = true;
        for (int i = 0; i < count; i++)
        {
            Enemy enemy = Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation)
                .GetComponent<Enemy>();
            if (enemy == null) continue;
            activeEnemies.Add(enemy);
        }

        enemyCount = activeEnemies.Count;
    }

    private Vector3 GenerateSpawnPosition()
    {
        Vector3 playerPosition = PlayerController.Instance != null
            ? PlayerController.Instance.transform.position
            : Vector3.zero;
        Vector3 candidate = Vector3.zero;
        for (int attempt = 0; attempt < 12; attempt++)
        {
            candidate = new Vector3(Random.Range(-spawnRange, spawnRange), 0f,
                Random.Range(-7.8f, 7.8f));
            if (Vector3.Distance(candidate, playerPosition) >= minimumPlayerDistance) break;
        }
        return candidate;
    }

    private void SpawnPowerup()
    {
        if (spawnRapidFireNext || powerupPrefab == null)
            SpawnRapidFirePowerup();
        else
            Instantiate(powerupPrefab, GenerateSpawnPosition(), powerupPrefab.transform.rotation);

        spawnRapidFireNext = !spawnRapidFireNext;
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
        pickup.AddComponent<RapidFirePickup>().Initialize(material);
    }
}
