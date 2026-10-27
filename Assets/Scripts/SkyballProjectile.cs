using UnityEngine;

public sealed class SkyballProjectile : MonoBehaviour
{
    private Rigidbody ballBody;
    private Vector3 shotDirection;
    private float ballImpulse;
    private static Material impactMaterial;

    public void Initialize(Rigidbody targetBall, Vector3 direction, float impulse)
    {
        ballBody = targetBall;
        shotDirection = direction.normalized;
        ballImpulse = impulse;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || other.attachedRigidbody == ballBody) return;

        CreateImpactBurst(transform.position);

        if (other.CompareTag("Enemy") && ballBody != null)
        {
            // Clear a lane by nudging defenders aside, then transfer the main
            // force to the ball so the shot advances it toward the goal.
            Rigidbody enemyBody = other.attachedRigidbody;
            if (enemyBody != null)
                enemyBody.AddForce(shotDirection * 2.5f, ForceMode.Impulse);
        }

        if (ballBody != null)
            ballBody.AddForce(shotDirection * ballImpulse, ForceMode.Impulse);
        Destroy(gameObject);
    }

    private static void CreateImpactBurst(Vector3 position)
    {
        GameObject burst = new GameObject("Shot Impact Burst");
        burst.transform.position = position;
        ParticleSystem particles = burst.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.duration = 0.12f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.35f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 3.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.1f);
        main.startColor = new Color(0.28f, 0.93f, 1f);
        main.maxParticles = 18;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)14) });
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        if (impactMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader != null)
            {
                impactMaterial = new Material(shader);
                if (impactMaterial.HasProperty("_BaseColor")) impactMaterial.SetColor("_BaseColor", new Color(0.28f, 0.93f, 1f));
                if (impactMaterial.HasProperty("_Color")) impactMaterial.SetColor("_Color", new Color(0.28f, 0.93f, 1f));
            }
        }
        if (impactMaterial != null)
            burst.GetComponent<ParticleSystemRenderer>().sharedMaterial = impactMaterial;
        particles.Play();
        Destroy(burst, 1f);
    }
}
