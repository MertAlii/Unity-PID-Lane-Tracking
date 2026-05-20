using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedurally generates environment decorations (trees and houses) around the road.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(RoadSpline))]
public class EnvironmentGenerator : MonoBehaviour
{
    [Header("Environment Settings")]
    public int houseCount = 40;
    public int treeCount = 80;
    public float minDistance = 15f;
    public float maxDistance = 60f;

    private GameObject _envContainer;
    public Material treeMaterial;
    public Material houseMaterial;

    private void OnEnable()
    {
        GenerateEnvironment();
    }

    private List<Vector3> _spawnedPositions = new List<Vector3>();
    private Material _cachedUrpMaterial;

    [ContextMenu("Force Regenerate Environment")]
    public void GenerateEnvironment()
    {
        // Cleanup old environment
        Transform oldEnv = transform.Find("Environment");
        if (oldEnv != null)
        {
            DestroyImmediate(oldEnv.gameObject);
        }
        if (_envContainer != null)
        {
            DestroyImmediate(_envContainer);
        }

        // Create new container
        _envContainer = new GameObject("Environment");
        _envContainer.transform.SetParent(transform);
        _envContainer.transform.localPosition = Vector3.zero;

        RoadSpline spline = GetComponent<RoadSpline>();
        if (spline == null || spline.controlPoints.Count < 4) return;

        // Initialize Materials
        if (treeMaterial == null || houseMaterial == null)
        {
            Shader std = Shader.Find("Universal Render Pipeline/Lit");
            if (std != null)
            {
                if (treeMaterial == null) { treeMaterial = new Material(std); treeMaterial.SetColor("_BaseColor", new Color(0.1f, 0.6f, 0.1f)); }
                if (houseMaterial == null) { houseMaterial = new Material(std); houseMaterial.SetColor("_BaseColor", new Color(0.8f, 0.8f, 0.8f)); }
            }
        }

        _spawnedPositions.Clear();

        // ── Generate Neighborhoods (Clusters of Houses) ──
        int neighborhoodCount = 10;
        int housesPerNeighborhood = houseCount / neighborhoodCount;

        for (int i = 0; i < neighborhoodCount; i++)
        {
            Vector3 center = GetValidSpawnCenter(spline, 20f);
            if (center != Vector3.zero)
            {
                for (int j = 0; j < housesPerNeighborhood; j++)
                {
                    Vector3 offset = new Vector3(Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f));
                    TrySpawnProp("House", center + offset, false, spline);
                }
            }
        }

        // ── Generate Forests (Clusters of Trees) ──
        int forestCount = 15;
        int treesPerForest = treeCount / forestCount;

        for (int i = 0; i < forestCount; i++)
        {
            Vector3 center = GetValidSpawnCenter(spline, 15f);
            if (center != Vector3.zero)
            {
                for (int j = 0; j < treesPerForest; j++)
                {
                    Vector3 offset = new Vector3(Random.Range(-15f, 15f), 0, Random.Range(-15f, 15f));
                    TrySpawnProp("Tree", center + offset, true, spline);
                }
            }
        }
        
        // ── Generate Start Gate ──
        GenerateStartGate(spline);
    }
    
    private void GenerateStartGate(RoadSpline spline)
    {
        Vector3 startPos = spline.EvaluateCatmullRom(0, 0f);
        Vector3 forward = spline.EvaluateTangent(0, 0f);
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        
        GameObject gate = new GameObject("StartGate");
        gate.transform.SetParent(_envContainer.transform);
        gate.transform.position = startPos;
        gate.transform.rotation = Quaternion.LookRotation(forward);
        
        Color metalColor = new Color(0.15f, 0.15f, 0.15f); // Dark metal
        Color accentColor = new Color(0.9f, 0.1f, 0.1f);   // Red accent
        Color whitePanel = Color.white;
        
        float width = 6f; // distance from center to pillar
        float height = 7f; // height of the arch

        // ── Left Truss Pillar ──
        CreatePrimitivePart(gate.transform, PrimitiveType.Cylinder, new Vector3(-width + 0.5f, height/2f,  0.5f), new Vector3(0.3f, height/2f, 0.3f), metalColor);
        CreatePrimitivePart(gate.transform, PrimitiveType.Cylinder, new Vector3(-width - 0.5f, height/2f,  0.5f), new Vector3(0.3f, height/2f, 0.3f), metalColor);
        CreatePrimitivePart(gate.transform, PrimitiveType.Cylinder, new Vector3(-width + 0.5f, height/2f, -0.5f), new Vector3(0.3f, height/2f, 0.3f), metalColor);
        CreatePrimitivePart(gate.transform, PrimitiveType.Cylinder, new Vector3(-width - 0.5f, height/2f, -0.5f), new Vector3(0.3f, height/2f, 0.3f), metalColor);
        
        // ── Right Truss Pillar ──
        CreatePrimitivePart(gate.transform, PrimitiveType.Cylinder, new Vector3(width + 0.5f, height/2f,  0.5f), new Vector3(0.3f, height/2f, 0.3f), metalColor);
        CreatePrimitivePart(gate.transform, PrimitiveType.Cylinder, new Vector3(width - 0.5f, height/2f,  0.5f), new Vector3(0.3f, height/2f, 0.3f), metalColor);
        CreatePrimitivePart(gate.transform, PrimitiveType.Cylinder, new Vector3(width + 0.5f, height/2f, -0.5f), new Vector3(0.3f, height/2f, 0.3f), metalColor);
        CreatePrimitivePart(gate.transform, PrimitiveType.Cylinder, new Vector3(width - 0.5f, height/2f, -0.5f), new Vector3(0.3f, height/2f, 0.3f), metalColor);

        // ── Main Overhead Beams ──
        CreatePrimitivePart(gate.transform, PrimitiveType.Cube, new Vector3(0f, height, 0f), new Vector3(width * 2f + 2f, 1f, 1.2f), metalColor);
        CreatePrimitivePart(gate.transform, PrimitiveType.Cube, new Vector3(0f, height + 1.2f, 0f), new Vector3(width * 2f + 2f, 0.2f, 1f), accentColor);
        
        // ── "START" Panels (White boards) ──
        CreatePrimitivePart(gate.transform, PrimitiveType.Cube, new Vector3(0f, height + 0.6f, 0.65f), new Vector3(6f, 1.2f, 0.1f), whitePanel);
        CreatePrimitivePart(gate.transform, PrimitiveType.Cube, new Vector3(0f, height + 0.6f, -0.65f), new Vector3(6f, 1.2f, 0.1f), whitePanel);

        // ── Starting Line on the Ground (Checkered Pattern) ──
        CreatePrimitivePart(gate.transform, PrimitiveType.Cube, new Vector3(0f, 0.02f, 0f), new Vector3(width * 2f - 1f, 0.01f, 1f), whitePanel);
        for(float x = -width + 1.5f; x < width - 1f; x += 1f) {
            CreatePrimitivePart(gate.transform, PrimitiveType.Cube, new Vector3(x, 0.03f, 0.25f), new Vector3(0.5f, 0.01f, 0.5f), Color.black);
            CreatePrimitivePart(gate.transform, PrimitiveType.Cube, new Vector3(x + 0.5f, 0.03f, -0.25f), new Vector3(0.5f, 0.01f, 0.5f), Color.black);
        }
    }

    private Vector3 GetValidSpawnCenter(RoadSpline spline, float safeRadius)
    {
        for (int attempts = 0; attempts < 50; attempts++)
        {
            float t = Random.value;
            Vector3 point = spline.GetPointAtNormalizedDistance(t);
            Vector2 randDir = Random.insideUnitCircle.normalized;
            Vector3 center = point + new Vector3(randDir.x, 0, randDir.y) * Random.Range(minDistance + safeRadius, maxDistance);
            
            center.y = 0f;

            spline.GetNearestPoint(center, out Vector3 nearestPoint, out float lateralError, out Vector3 forwardDir);
            if (Mathf.Abs(lateralError) > minDistance + safeRadius)
            {
                return center;
            }
        }
        return Vector3.zero;
    }

    private void TrySpawnProp(string type, Vector3 spawnPos, bool isTree, RoadSpline spline)
    {
        float propRadius = isTree ? 3f : 12f;
        spawnPos.y = 0f;

        // Check road distance
        spline.GetNearestPoint(spawnPos, out Vector3 nearestPoint, out float lateralError, out Vector3 forwardDir);
        if (Mathf.Abs(lateralError) < minDistance)
            return;

        // Check overlap
        foreach (var pos in _spawnedPositions)
        {
            if (Vector3.Distance(pos, spawnPos) < propRadius)
                return;
        }

        _spawnedPositions.Add(spawnPos);

        GameObject prop = new GameObject(type + "_" + Random.Range(1000, 9999));
        prop.transform.SetParent(_envContainer.transform);
        prop.transform.position = spawnPos;

        if (isTree)
        {
            float scale = Random.Range(1f, 2.5f);
            prop.transform.localScale = Vector3.one * scale;

            // Trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(prop.transform);
            trunk.transform.localPosition = new Vector3(0, 1.5f, 0);
            trunk.transform.localScale = new Vector3(0.6f, 3f, 0.6f);
            SetMaterial(trunk, new Color(0.35f, 0.2f, 0.1f));

            // Leaves
            Color[] leafColors = new Color[] {
                new Color(0.15f, 0.7f, 0.2f),
                new Color(0.2f, 0.85f, 0.1f),
                new Color(0.3f, 0.6f, 0.2f),
                new Color(1.0f, 0.6f, 0.7f), // Sakura Pink
                new Color(0.9f, 0.5f, 0.1f)  // Autumn Orange
            };
            Color leafColor = leafColors[Random.Range(0, leafColors.Length)];
            
            CreatePrimitivePart(prop.transform, PrimitiveType.Sphere, new Vector3(0, 4f, 0), new Vector3(4f, 4f, 4f), leafColor);
            CreatePrimitivePart(prop.transform, PrimitiveType.Sphere, new Vector3(-1.2f, 3.5f, 1.2f), new Vector3(3f, 3f, 3f), leafColor);
            CreatePrimitivePart(prop.transform, PrimitiveType.Sphere, new Vector3(1.2f, 3.5f, -1.2f), new Vector3(3f, 3f, 3f), leafColor);
        }
        else
        {
            prop.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            float scale = Random.Range(0.8f, 1.5f);
            prop.transform.localScale = Vector3.one * scale;

            Color[] houseColors = new Color[] {
                Color.HSVToRGB(Random.Range(0f, 1f), 0.5f, 0.9f),
                Color.HSVToRGB(Random.Range(0f, 1f), 0.6f, 1.0f)
            };
            Color houseColor = houseColors[Random.Range(0, houseColors.Length)];
            Color roofColor = new Color(0.2f, 0.2f, 0.2f);
            Color garageColor = new Color(0.7f, 0.7f, 0.7f);

            // Main Body
            CreatePrimitivePart(prop.transform, PrimitiveType.Cube, new Vector3(0, 3f, 0), new Vector3(8f, 6f, 6f), houseColor);
            CreatePrimitivePart(prop.transform, PrimitiveType.Cube, new Vector3(0, 6.1f, 0), new Vector3(8.4f, 0.2f, 6.4f), roofColor);

            // Second Floor
            CreatePrimitivePart(prop.transform, PrimitiveType.Cube, new Vector3(-1f, 7.5f, 0f), new Vector3(5f, 3f, 5f), houseColor);
            CreatePrimitivePart(prop.transform, PrimitiveType.Cube, new Vector3(-1f, 9.1f, 0f), new Vector3(5.4f, 0.2f, 5.4f), roofColor);

            // Garage
            CreatePrimitivePart(prop.transform, PrimitiveType.Cube, new Vector3(5.5f, 2f, 0.5f), new Vector3(5f, 4f, 5f), garageColor);
            CreatePrimitivePart(prop.transform, PrimitiveType.Cube, new Vector3(5.5f, 4.1f, 0.5f), new Vector3(5.4f, 0.2f, 5.4f), roofColor);
        }
    }

    private void CreatePrimitivePart(Transform parent, PrimitiveType type, Vector3 localPos, Vector3 scale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.transform.SetParent(parent);
        part.transform.localPosition = localPos;
        part.transform.localScale = scale;
        SetMaterial(part, color);
    }

    private void SetMaterial(GameObject obj, Color color)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            if (_cachedUrpMaterial == null)
            {
                Shader urpShader = Shader.Find("UI/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
                _cachedUrpMaterial = new Material(urpShader);
            }
            
            r.sharedMaterial = _cachedUrpMaterial;
            
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            r.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", color);
            propBlock.SetColor("_Color", color);
            r.SetPropertyBlock(propBlock);
        }
    }
}
