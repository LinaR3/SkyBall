using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Owns pickup effects, temporary abilities, and rapid-fire projectiles.</summary>
public sealed class PlayerAbilityController : MonoBehaviour
{
    [SerializeField] private float speedBoostMultiplier = 1.75f;
    [SerializeField] private float powerupDuration = 5f;
    [SerializeField] private float powerupStrength = 15f;
    [SerializeField] private float rapidFireDuration = 1f;
    [SerializeField] private float rapidFireInterval = 0.09f;
    [SerializeField] private float projectileSpeed = 24f;
    [SerializeField] private float shotImpulse = 0.07f;
    [SerializeField, Range(3, 7)] private int projectilesPerBurst = 5;
    [SerializeField] private float projectileSpreadDegrees = 14f;

    private Transform aimReference;
    private GameObject powerupIndicator;
    private Renderer playerRenderer;
    private Color originalPlayerColor = Color.white;
    private Coroutine powerupRoutine;
    private Coroutine feedbackRoutine;
    private float rapidFireExpiresAt;
    private float powerupExpiresAt;
    private float pickupMessageExpiresAt;
    private float nextShotAt;
    private Material projectileMaterial;

    public bool HasPowerup { get; private set; }
    public float SpeedMultiplier => HasPowerup ? speedBoostMultiplier : 1f;
    public float RapidFireRemaining => Mathf.Max(0f, rapidFireExpiresAt - Time.time);
    public float PowerupRemaining => Mathf.Max(0f, powerupExpiresAt - Time.time);
    public string PickupMessage { get; private set; }

    public void Initialize(Transform directionSource, GameObject indicator)
    {
        aimReference = directionSource;
        powerupIndicator = indicator;
        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null && playerRenderer.sharedMaterial != null)
            originalPlayerColor = playerRenderer.sharedMaterial.color;
        if (powerupIndicator != null)
            powerupIndicator.SetActive(HasPowerup);
    }

    private void Update()
    {
        if (powerupIndicator != null && powerupIndicator.activeSelf)
            powerupIndicator.transform.position = transform.position + Vector3.down * 0.5f;
        if (Time.time >= pickupMessageExpiresAt) PickupMessage = string.Empty;

        if (RapidFireRemaining > 0f)
            TryFireRapidShot();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Powerup")) return;

        RapidFirePickup rapidFire = other.GetComponent<RapidFirePickup>();
        Destroy(other.gameObject);
        if (rapidFire != null)
        {
            rapidFireExpiresAt = Time.time + rapidFireDuration;
            nextShotAt = Time.time;
            ShowPickupFeedback("¡DISPARO POTENCIADO!", new Color(0.15f, 0.9f, 1f));
            return;
        }

        HasPowerup = true;
        powerupExpiresAt = Time.time + powerupDuration;
        ShowPickupFeedback("¡IMPULSO ACTIVADO!", new Color(1f, 0.57f, 0.12f));
        if (powerupIndicator != null) powerupIndicator.SetActive(true);
        if (powerupRoutine != null) StopCoroutine(powerupRoutine);
        powerupRoutine = StartCoroutine(PowerupCountdownRoutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!HasPowerup || !collision.gameObject.CompareTag("Enemy")) return;
        Rigidbody enemyBody = collision.rigidbody;
        if (enemyBody == null) return;

        Vector3 awayFromPlayer = enemyBody.worldCenterOfMass - transform.position;
        awayFromPlayer.y = 0f;
        if (awayFromPlayer.sqrMagnitude > 0.001f)
            enemyBody.AddForce(awayFromPlayer.normalized * powerupStrength, ForceMode.Impulse);
    }

    private void TryFireRapidShot()
    {
        if (Time.time < nextShotAt || aimReference == null) return;
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        bool firing = keyboard != null && keyboard.fKey.isPressed || mouse != null && mouse.leftButton.isPressed;
        if (!firing) return;

        Vector3 forward = Vector3.ProjectOnPlane(aimReference.forward, Vector3.up).normalized;
        Vector3 goalDirection = transform.position.z >= 0f ? Vector3.back : Vector3.forward;
        Vector3 shotDirection = Vector3.Slerp(forward, goalDirection, 0.7f).normalized;
        SphereCollider sphere = GetComponent<SphereCollider>();
        float radius = sphere != null ? sphere.radius * Mathf.Abs(transform.lossyScale.x) : 0.75f;
        Vector3 spawnPosition = transform.position + shotDirection * (radius + 0.45f) + Vector3.up * 0.1f;

        EnsureProjectileMaterial();
        int count = Mathf.Max(1, projectilesPerBurst);
        for (int i = 0; i < count; i++)
        {
            float spread = count == 1 ? 0f : Mathf.Lerp(-projectileSpreadDegrees, projectileSpreadDegrees, i / (float)(count - 1));
            Vector3 direction = Quaternion.AngleAxis(spread, Vector3.up) * shotDirection;
            Vector3 position = spawnPosition + direction * (i * 0.035f);
            SpawnProjectilePellet(position, direction);
        }
        nextShotAt = Time.time + rapidFireInterval;
    }

    private void EnsureProjectileMaterial()
    {
        if (projectileMaterial != null) return;
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        projectileMaterial = new Material(shader) { color = new Color(0.3f, 0.95f, 1f) };
        if (!projectileMaterial.HasProperty("_EmissionColor")) return;
        projectileMaterial.EnableKeyword("_EMISSION");
        projectileMaterial.SetColor("_EmissionColor", new Color(0.1f, 0.8f, 1f) * 2f);
    }

    private void SpawnProjectilePellet(Vector3 position, Vector3 direction)
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "Rapid Fire Pellet";
        projectile.transform.position = position;
        projectile.transform.localScale = Vector3.one * 0.2f;
        projectile.GetComponent<SphereCollider>().isTrigger = true;
        projectile.GetComponent<Renderer>().sharedMaterial = projectileMaterial;

        Rigidbody projectileBody = projectile.AddComponent<Rigidbody>();
        projectileBody.useGravity = false;
        projectileBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        projectileBody.linearVelocity = direction * projectileSpeed;
        SkyballProjectile projectileLogic = projectile.AddComponent<SkyballProjectile>();
        Rigidbody ballBody = SkyballGameBall.Instance != null ? SkyballGameBall.Instance.Body : null;
        projectileLogic.Initialize(ballBody, direction, shotImpulse);
        Destroy(projectile, 2.5f);
    }

    private void ShowPickupFeedback(string message, Color color)
    {
        PickupMessage = message;
        pickupMessageExpiresAt = Time.time + 1.4f;
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(PickupFeedbackRoutine(color));
    }

    private IEnumerator PickupFeedbackRoutine(Color color)
    {
        if (playerRenderer == null) yield break;
        Material material = playerRenderer.material;
        bool usesBaseColor = material.HasProperty("_BaseColor");
        SetPlayerColor(material, usesBaseColor, color);

        float elapsed = 0f;
        while (elapsed < 1.1f)
        {
            elapsed += Time.deltaTime;
            float pulse = 0.65f + Mathf.Sin(elapsed * 16f) * 0.2f;
            SetPlayerColor(material, usesBaseColor, Color.Lerp(originalPlayerColor, color, pulse));
            yield return null;
        }

        SetPlayerColor(material, usesBaseColor, originalPlayerColor);
        feedbackRoutine = null;
    }

    private static void SetPlayerColor(Material material, bool usesBaseColor, Color color)
    {
        if (usesBaseColor) material.SetColor("_BaseColor", color);
        else material.color = color;
    }

    private IEnumerator PowerupCountdownRoutine()
    {
        yield return new WaitForSeconds(powerupDuration);
        HasPowerup = false;
        powerupExpiresAt = 0f;
        if (powerupIndicator != null) powerupIndicator.SetActive(false);
        powerupRoutine = null;
    }

    public void ResetAbilities()
    {
        HasPowerup = false;
        rapidFireExpiresAt = 0f;
        powerupExpiresAt = 0f;
        nextShotAt = 0f;
        if (powerupRoutine != null)
        {
            StopCoroutine(powerupRoutine);
            powerupRoutine = null;
        }
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }
        if (powerupIndicator != null) powerupIndicator.SetActive(false);
        if (playerRenderer != null)
            SetPlayerColor(playerRenderer.material, playerRenderer.material.HasProperty("_BaseColor"), originalPlayerColor);
    }

    private void OnDestroy()
    {
        if (projectileMaterial != null) Destroy(projectileMaterial);
    }
}
