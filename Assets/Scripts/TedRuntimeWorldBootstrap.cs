using UnityEngine;

public static class TedRuntimeWorldBootstrap
{
    private const float WorldSize = 7200f;
    private const float TerrainHeight = 280f;
    private const int HeightResolution = 257;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureLargePlayableWorld()
    {
        Terrain existingTerrain = Terrain.activeTerrain;
        if (existingTerrain != null && existingTerrain.terrainData != null && existingTerrain.terrainData.size.x >= WorldSize * 0.9f)
        {
            MovePlayerToTerrain(existingTerrain);
            return;
        }

        RemoveOldFlatStarterObjects();
        Terrain terrain = CreateRuntimeTerrain();
        CreateOcean();
        CreateRiver();
        CreatePlantsAndRocks(terrain);
        MovePlayerToTerrain(terrain);
    }

    private static Terrain CreateRuntimeTerrain()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = HeightResolution;
        terrainData.size = new Vector3(WorldSize, TerrainHeight, WorldSize);

        float[,] heights = new float[HeightResolution, HeightResolution];
        for (int z = 0; z < HeightResolution; z++)
        {
            for (int x = 0; x < HeightResolution; x++)
            {
                float u = x / (float)(HeightResolution - 1);
                float v = z / (float)(HeightResolution - 1);
                float wx = (u - 0.5f) * WorldSize;
                float wz = (v - 0.5f) * WorldSize;
                float dist = Mathf.Sqrt(wx * wx + wz * wz);
                float islandMask = Mathf.Clamp01(1f - Mathf.Pow(dist / (WorldSize * 0.49f), 2.4f));
                float largeHills = Mathf.PerlinNoise(u * 5.5f + 13.2f, v * 5.5f + 9.8f) * 0.34f;
                float smallHills = Mathf.PerlinNoise(u * 21f + 3.1f, v * 21f + 8.6f) * 0.08f;
                float ridge = Mathf.PerlinNoise(u * 9f + 51f, v * 3.2f + 17f) * 0.16f;
                float riverCenter = Mathf.Sin(wx / 650f) * 360f;
                float riverDistance = Mathf.Abs(wz - riverCenter);
                float riverCut = Mathf.SmoothStep(1f, 0f, riverDistance / 85f) * 0.16f;
                heights[z, x] = Mathf.Clamp01(((0.05f + largeHills + smallHills + ridge) * islandMask) - riverCut * islandMask);
            }
        }

        terrainData.SetHeights(0, 0, heights);
        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainObject.name = "20x Runtime Dino Island Terrain";
        terrainObject.transform.position = new Vector3(-WorldSize / 2f, 0f, -WorldSize / 2f);

        Terrain terrain = terrainObject.GetComponent<Terrain>();
        terrain.materialTemplate = MakeMaterial("Runtime Terrain Grass", new Color(0.20f, 0.42f, 0.17f));
        return terrain;
    }

    private static void CreateOcean()
    {
        GameObject ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ocean.name = "Runtime Ocean - huge surrounding water";
        ocean.transform.position = new Vector3(0f, 1.4f, 0f);
        ocean.transform.localScale = new Vector3(1300f, 1f, 1300f);
        ocean.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Runtime Ocean Water", new Color(0.05f, 0.30f, 0.52f, 0.62f));
        Object.Destroy(ocean.GetComponent<Collider>());
    }

    private static void CreateRiver()
    {
        Material riverMaterial = MakeMaterial("Runtime River Water", new Color(0.07f, 0.42f, 0.68f, 0.78f));
        GameObject parent = new GameObject("Runtime River - follows valley through island");
        const int pieces = 80;
        const float step = WorldSize / pieces;
        for (int i = 0; i < pieces; i++)
        {
            float x = -WorldSize / 2f + i * step + step * 0.5f;
            float z = Mathf.Sin(x / 650f) * 360f;
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = "River Segment";
            piece.transform.SetParent(parent.transform);
            piece.transform.position = new Vector3(x, 3.5f, z);
            piece.transform.localScale = new Vector3(step * 1.15f, 0.04f, 90f);
            piece.transform.rotation = Quaternion.Euler(0f, Mathf.Cos(x / 650f) * 22f, 0f);
            piece.GetComponent<Renderer>().sharedMaterial = riverMaterial;
            Object.Destroy(piece.GetComponent<Collider>());
        }
    }

    private static void CreatePlantsAndRocks(Terrain terrain)
    {
        Material trunk = MakeMaterial("Runtime Tree Trunk", new Color(0.26f, 0.15f, 0.08f));
        Material leaves = MakeMaterial("Runtime Tree Leaves", new Color(0.10f, 0.34f, 0.12f));
        Material rock = MakeMaterial("Runtime Rock", new Color(0.31f, 0.30f, 0.27f));
        GameObject parent = new GameObject("Runtime Plants and Rocks");

        Random.InitState(1904);
        for (int i = 0; i < 240; i++)
        {
            Vector2 point = Random.insideUnitCircle * (WorldSize * 0.43f);
            Vector3 pos = new Vector3(point.x, 0f, point.y);
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;
            if (pos.y < 8f) continue;

            if (i % 5 == 0)
            {
                GameObject rockObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rockObject.name = "Runtime Terrain Rock";
                rockObject.transform.SetParent(parent.transform);
                rockObject.transform.position = pos + Vector3.up * 0.7f;
                float scale = Random.Range(1.3f, 4.2f);
                rockObject.transform.localScale = new Vector3(scale * 1.4f, scale * 0.65f, scale);
                rockObject.transform.rotation = Random.rotation;
                rockObject.GetComponent<Renderer>().sharedMaterial = rock;
            }
            else
            {
                float scale = Random.Range(0.75f, 1.8f);
                GameObject treeTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                treeTrunk.name = "Runtime Low Poly Tree Trunk";
                treeTrunk.transform.SetParent(parent.transform);
                treeTrunk.transform.position = pos + Vector3.up * (2.2f * scale);
                treeTrunk.transform.localScale = new Vector3(0.45f * scale, 2.2f * scale, 0.45f * scale);
                treeTrunk.GetComponent<Renderer>().sharedMaterial = trunk;

                GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crown.name = "Runtime Low Poly Tree Crown";
                crown.transform.SetParent(parent.transform);
                crown.transform.position = pos + Vector3.up * (5.1f * scale);
                crown.transform.localScale = new Vector3(3.0f * scale, 2.5f * scale, 3.0f * scale);
                crown.GetComponent<Renderer>().sharedMaterial = leaves;
            }
        }
    }

    private static void MovePlayerToTerrain(Terrain terrain)
    {
        DinoPlayerController player = Object.FindFirstObjectByType<DinoPlayerController>();
        if (player == null) return;

        Vector3 position = player.transform.position;
        position.x = Mathf.Clamp(position.x, -WorldSize * 0.35f, WorldSize * 0.35f);
        position.z = Mathf.Clamp(position.z, -WorldSize * 0.35f, WorldSize * 0.35f);
        position.y = terrain.SampleHeight(position) + terrain.transform.position.y + 2.5f;
        player.transform.position = position;
    }

    private static void RemoveOldFlatStarterObjects()
    {
        string[] names =
        {
            "Island Grass",
            "Beach Ring",
            "Sea",
            "Life Size Island Grass",
            "Life Size Beach Ring",
            "Open Sea"
        };

        foreach (string objectName in names)
        {
            GameObject oldObject = GameObject.Find(objectName);
            if (oldObject != null)
            {
                Object.Destroy(oldObject);
            }
        }
    }

    private static Material MakeMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = name;
        material.color = color;
        return material;
    }
}
