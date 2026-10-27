using System.Collections.Generic;
using UnityEngine;

/// <summary>Creates the two runtime goals and owns their shared materials.</summary>
public sealed class SkyballGoalFactory : MonoBehaviour
{
    private readonly List<Material> ownedMaterials = new List<Material>();

    private void Awake()
    {
        CreateGoal("Blue Goal", true, 10.2f, new Color(0.08f, 0.65f, 1f));
        CreateGoal("Orange Goal", false, -10.2f, new Color(1f, 0.36f, 0.08f));
    }

    private void OnDestroy()
    {
        foreach (Material material in ownedMaterials)
            if (material != null) Destroy(material);
        ownedMaterials.Clear();
    }

    private void CreateGoal(string goalName, bool blueTeamGoal, float z, Color color)
    {
        GameObject goal = new GameObject(goalName);
        goal.transform.SetParent(transform, false);
        goal.transform.position = new Vector3(0f, 0f, z);
        goal.tag = "Goal";

        float depthDirection = Mathf.Sign(z);
        Color frameColor = new Color(0.9f, 0.95f, 1f);
        Color netColor = Color.Lerp(color, Color.white, 0.42f);
        Material frameMaterial = CreateGoalMaterial(frameColor, 0.72f, 0.78f);
        Material teamMaterial = CreateGoalMaterial(color, 0.48f, 0.7f);
        Material netMaterial = CreateGoalMaterial(netColor, 0.12f, 0.45f);

        CreateGoalTube(goal.transform, goalName + " Left Post", new Vector3(-2.15f, 0.05f, 0f), new Vector3(-2.15f, 2.45f, 0f), 0.105f, frameMaterial);
        CreateGoalTube(goal.transform, goalName + " Right Post", new Vector3(2.15f, 0.05f, 0f), new Vector3(2.15f, 2.45f, 0f), 0.105f, frameMaterial);
        CreateGoalTube(goal.transform, goalName + " Crossbar", new Vector3(-2.15f, 2.45f, 0f), new Vector3(2.15f, 2.45f, 0f), 0.105f, frameMaterial);
        CreateGoalTube(goal.transform, goalName + " Left Team Band", new Vector3(-2.15f, 0.23f, 0f), new Vector3(-2.15f, 0.58f, 0f), 0.12f, teamMaterial);
        CreateGoalTube(goal.transform, goalName + " Right Team Band", new Vector3(2.15f, 0.23f, 0f), new Vector3(2.15f, 0.58f, 0f), 0.12f, teamMaterial);

        float backZ = depthDirection * 1.35f;
        CreateGoalTube(goal.transform, goalName + " Back Top", new Vector3(-2.08f, 2.35f, backZ), new Vector3(2.08f, 2.35f, backZ), 0.035f, teamMaterial);
        CreateGoalTube(goal.transform, goalName + " Back Left", new Vector3(-2.08f, 0.08f, backZ), new Vector3(-2.08f, 2.35f, backZ), 0.035f, teamMaterial);
        CreateGoalTube(goal.transform, goalName + " Back Right", new Vector3(2.08f, 0.08f, backZ), new Vector3(2.08f, 2.35f, backZ), 0.035f, teamMaterial);
        CreateGoalTube(goal.transform, goalName + " Depth Left", new Vector3(-2.15f, 2.42f, 0f), new Vector3(-2.08f, 2.35f, backZ), 0.045f, frameMaterial);
        CreateGoalTube(goal.transform, goalName + " Depth Right", new Vector3(2.15f, 2.42f, 0f), new Vector3(2.08f, 2.35f, backZ), 0.045f, frameMaterial);

        for (int i = 1; i <= 4; i++)
        {
            float x = Mathf.Lerp(-1.9f, 1.9f, i / 5f);
            CreateGoalTube(goal.transform, goalName + " Net Vertical " + i,
                new Vector3(x, 0.1f, backZ), new Vector3(x, 2.3f, backZ), 0.012f, netMaterial);
        }
        for (int i = 1; i <= 4; i++)
        {
            float y = Mathf.Lerp(0.35f, 2.1f, i / 5f);
            CreateGoalTube(goal.transform, goalName + " Net Horizontal " + i,
                new Vector3(-2.05f, y, backZ), new Vector3(2.05f, y, backZ), 0.012f, netMaterial);
        }

        BoxCollider trigger = goal.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 1.25f, 0f);
        trigger.size = new Vector3(4.1f, 2.4f, 1.3f);
        goal.AddComponent<SkyballGoal>().SetBlueTeamGoal(blueTeamGoal);
    }

    private Material CreateGoalMaterial(Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader) { color = color };
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        ownedMaterials.Add(material);
        return material;
    }

    private static void CreateGoalTube(Transform parent, string name, Vector3 start, Vector3 end, float radius, Material material)
    {
        GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tube.name = name;
        tube.transform.SetParent(parent, false);
        Vector3 delta = end - start;
        tube.transform.localPosition = (start + end) * 0.5f;
        tube.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        tube.transform.localScale = new Vector3(radius * 2f, delta.magnitude * 0.5f, radius * 2f);
        Collider collider = tube.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        tube.GetComponent<Renderer>().sharedMaterial = material;
    }
}
