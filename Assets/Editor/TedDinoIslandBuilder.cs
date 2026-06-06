using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TedDinoIslandBuilder
{
    private const string QuaterniusTrexPath = "Assets/External/Quaternius/Dinosaur Animated Pack - Dec 2018/FBX/Trex.fbx";
    private const string GeneratedAnimatorPath = "Assets/Generated/TedTrexAnimator.controller";
    private const float WorldSize = 7200f; // 20x bigger than the previous 360-ish playable feel.
    private const float TerrainHeight = 280f;
    private const int HeightResolution = 513;

    [MenuItem("Ted/Create Dino Island Starter Scene")]
    public static void CreateStarterScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Dino Island Starter";

        Material grass = MakeMaterial("Ted_Grass", new Color(0.20f, 0.42f, 0.17f));
        Material sand = MakeMaterial("Ted_Sand", new Color(0.74f, 0.62f, 0.36f));
        Material water = MakeMaterial("Ted_Water", new Color(0.05f, 0.30f, 0.52f, 0.62f));
        Material river = MakeMaterial("Ted_River", new Color(0.07f, 0.42f, 0.68f, 0.78f));
        Material trunk = MakeMaterial("Ted_Trunk", new Color(0.26f, 0.15f, 0.08f));
        Material leaves = MakeMaterial("Ted_Leaves", new Color(0.10f, 0.34f, 0.12f));
        Material rock = MakeMaterial("Ted_Rock", new Color(0.31f, 0.30f, 0.27f));
        Material dinoHide = MakeMaterial("Ted_DinoHide", new Color(0.24f, 0.29f, 0.20f));
        Material belly = MakeMaterial("Ted_DinoBelly", new Color(0.63f, 0.57f, 0.39f));
        Material dark = MakeMaterial("Ted_DarkDetails", new Color(0.07f, 0.09f, 0.07f));

        RenderSettings.ambientIntensity = 0.55f;

        GameObject sun = new GameObject("Sun");
        Light light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 2.45f;
        sun.transform.rotation = Quaternion.Euler(46f, -32f, 0f);

        Terrain terrain = BuildTerrain(grass, sand);
        TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
        if (terrainCollider != null)
        {
            terrainCollider.terrainData = terrain.terrainData;
        }

        GameObject ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ocean.name = "Ocean - huge surrounding water";
        ocean.transform.position = new Vector3(0f, 1.4f, 0f);
        ocean.transform.localScale = new Vector3(1300f, 1f, 1300f);
        ocean.GetComponent<Renderer>().sharedMaterial = water;

        BuildRiver(river);
        BuildForestAndRocks(trunk, leaves, rock);

        float spawnY = terrain.SampleHeight(Vector3.zero) + terrain.transform.position.y + 2.5f;
        GameObject player = new GameObject("Playable Quaternius T-Rex");
        player.transform.position = new Vector3(0f, spawnY, 0f);
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 3.8f;
        controller.radius = 0.9f;
        controller.center = new Vector3(0f, 1.9f, 0f);
        DinoPlayerController dinoController = player.AddComponent<DinoPlayerController>();
        Transform head = BuildPlayableDinoVisual(player.transform, dinoHide, belly, dark);

        GameObject camera = new GameObject("Follow Camera");
        Camera cam = camera.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.farClipPlane = 9500f;
        cam.fieldOfView = 62f;
        camera.transform.position = player.transform.position + new Vector3(0f, 5.5f, -10f);
        ThirdPersonCamera follow = camera.AddComponent<ThirdPersonCamera>();
        follow.target = player.transform;
        dinoController.cameraTransform = camera.transform;

        GameObject markerParent = new GameObject("Collectibles");
        Material foodMaterial = MakeMaterial("Ted_Food", new Color(0.28f, 0.85f, 0.22f));
        for (int ringIndex = 0; ringIndex < 5; ringIndex++)
        {
            int markerCount = 12 + ringIndex * 8;
            float markerRadius = 160f + ringIndex * 520f;
            for (int i = 0; i < markerCount; i++)
            {
                float angle = (i + ringIndex * 0.37f) * Mathf.PI * 2f / markerCount;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * markerRadius, 0f, Mathf.Sin(angle) * markerRadius);
                pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y + 0.75f;
                GameObject food = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                food.name = "Food Marker";
                food.transform.SetParent(markerParent.transform);
                food.transform.position = pos;
                food.transform.localScale = Vector3.one * 1.2f;
                food.GetComponent<Renderer>().sharedMaterial = foodMaterial;
            }
        }

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/DinoIslandStarter.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeGameObject = player;
        Debug.Log("Ted created a 20x larger procedural dino island with terrain, hills, river, plants and an imported animated Quaternius T-Rex when available.");
    }

    private static Transform BuildPlayableDinoVisual(Transform root, Material hide, Material belly, Material dark)
    {
        GameObject trexPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(QuaterniusTrexPath);
        if (trexPrefab == null)
        {
            Debug.LogWarning("Ted could not find the Quaternius T-Rex FBX yet, so he used the old primitive dino fallback. Wait for Unity to import the FBX files, then run Ted > Create Dino Island Starter Scene again.");
            return BuildBetterPlaceholderDino(root, hide, belly, dark);
        }

        GameObject trex = (GameObject)PrefabUtility.InstantiatePrefab(trexPrefab);
        trex.name = "Quaternius Animated T-Rex - playable visual";
        trex.transform.SetParent(root, false);
        trex.transform.localPosition = Vector3.zero;
        trex.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        trex.transform.localScale = Vector3.one * 1.8f;

        Animator animator = trex.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = trex.AddComponent<Animator>();
        }

        AnimatorController controller = BuildTrexAnimatorController();
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }

        Transform head = FindChildContaining(trex.transform, "head");
        return head != null ? head : trex.transform;
    }

    private static AnimatorController BuildTrexAnimatorController()
    {
        const string generatedFolder = "Assets/Generated";
        if (!AssetDatabase.IsValidFolder(generatedFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Generated");
        }

        AssetDatabase.DeleteAsset(GeneratedAnimatorPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(GeneratedAnimatorPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        AnimationClip idle = FindClip("Idle", "Idl");
        AnimationClip walk = FindClip("Walk");
        AnimationClip run = FindClip("Run");

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        machine.states = new ChildAnimatorState[0];

        AnimatorState idleState = machine.AddState("Idle");
        idleState.motion = idle != null ? idle : walk;
        machine.defaultState = idleState;

        AnimatorState walkState = machine.AddState("Walk");
        walkState.motion = walk != null ? walk : idleState.motion;

        AnimatorState runState = machine.AddState("Run");
        runState.motion = run != null ? run : walkState.motion;

        AddSpeedTransition(idleState, walkState, 0.25f, true);
        AddSpeedTransition(walkState, idleState, 0.2f, false);
        AddSpeedTransition(walkState, runState, 8f, true);
        AddSpeedTransition(runState, walkState, 7.5f, false);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static void AddSpeedTransition(AnimatorState from, AnimatorState to, float threshold, bool greater)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.18f;
        transition.AddCondition(greater ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less, threshold, "Speed");
    }

    private static AnimationClip FindClip(params string[] nameParts)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(QuaterniusTrexPath);
        foreach (Object asset in assets)
        {
            AnimationClip clip = asset as AnimationClip;
            if (clip == null) continue;
            string lower = clip.name.ToLowerInvariant();
            foreach (string part in nameParts)
            {
                if (lower.Contains(part.ToLowerInvariant()))
                {
                    return clip;
                }
            }
        }

        return null;
    }

    private static Transform FindChildContaining(Transform parent, string namePart)
    {
        string lowerPart = namePart.ToLowerInvariant();
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (child.name.ToLowerInvariant().Contains(lowerPart))
            {
                return child;
            }
        }

        return null;
    }

    private static Terrain BuildTerrain(Material grass, Material sand)
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

                float height = (0.05f + largeHills + smallHills + ridge) * islandMask;
                height -= riverCut * islandMask;
                height = Mathf.Clamp01(height);
                heights[z, x] = height;
            }
        }

        terrainData.SetHeights(0, 0, heights);

        const string generatedFolder = "Assets/Generated";
        const string terrainAssetPath = "Assets/Generated/TedProceduralDinoIslandTerrain.asset";
        if (!AssetDatabase.IsValidFolder(generatedFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Generated");
        }
        AssetDatabase.DeleteAsset(terrainAssetPath);
        AssetDatabase.CreateAsset(terrainData, terrainAssetPath);

        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainObject.name = "20x Procedural Dino Island Terrain";
        terrainObject.transform.position = new Vector3(-WorldSize / 2f, 0f, -WorldSize / 2f);
        Terrain terrain = terrainObject.GetComponent<Terrain>();
        terrain.materialTemplate = grass;
        return terrain;
    }

    private static void BuildRiver(Material riverMaterial)
    {
        GameObject parent = new GameObject("River - follows valley through island");
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
            Object.DestroyImmediate(piece.GetComponent<Collider>());
        }
    }

    private static void BuildForestAndRocks(Material trunk, Material leaves, Material rock)
    {
        GameObject forest = new GameObject("Procedural Plants and Rocks");
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null) return;

        Random.InitState(1904);
        for (int i = 0; i < 360; i++)
        {
            Vector2 point = Random.insideUnitCircle * (WorldSize * 0.43f);
            Vector3 pos = new Vector3(point.x, 0f, point.y);
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;
            if (pos.y < 8f) continue;

            if (i % 5 == 0)
            {
                AddRock(forest.transform, pos, rock);
            }
            else
            {
                AddTree(forest.transform, pos, trunk, leaves);
            }
        }
    }

    private static void AddTree(Transform parent, Vector3 position, Material trunkMaterial, Material leafMaterial)
    {
        float scale = Random.Range(0.75f, 1.8f);
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Low Poly Tree Trunk";
        trunk.transform.SetParent(parent);
        trunk.transform.position = position + Vector3.up * (2.2f * scale);
        trunk.transform.localScale = new Vector3(0.45f * scale, 2.2f * scale, 0.45f * scale);
        trunk.GetComponent<Renderer>().sharedMaterial = trunkMaterial;

        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "Low Poly Tree Crown";
        crown.transform.SetParent(parent);
        crown.transform.position = position + Vector3.up * (5.1f * scale);
        crown.transform.localScale = new Vector3(3.0f * scale, 2.5f * scale, 3.0f * scale);
        crown.GetComponent<Renderer>().sharedMaterial = leafMaterial;
    }

    private static void AddRock(Transform parent, Vector3 position, Material material)
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "Terrain Rock";
        rock.transform.SetParent(parent);
        rock.transform.position = position + Vector3.up * 0.7f;
        float scale = Random.Range(1.3f, 4.2f);
        rock.transform.localScale = new Vector3(scale * 1.4f, scale * 0.65f, scale);
        rock.transform.rotation = Random.rotation;
        rock.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static Transform BuildBetterPlaceholderDino(Transform root, Material hide, Material belly, Material dark)
    {
        GameObject visual = new GameObject("Dino Visual - nose points Unity forward");
        visual.transform.SetParent(root);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.Euler(0f, -90f, 0f); // Old model points +X; this makes the nose point +Z.

        AddPart(visual.transform, "Body", PrimitiveType.Capsule, new Vector3(0f, 1.15f, 0f), new Vector3(1.25f, 2.4f, 0.85f), Quaternion.Euler(0f, 0f, 90f), hide);
        AddPart(visual.transform, "Belly", PrimitiveType.Capsule, new Vector3(0.2f, 0.92f, 0f), new Vector3(0.5f, 1.7f, 0.55f), Quaternion.Euler(0f, 0f, 90f), belly);
        AddPart(visual.transform, "Hip", PrimitiveType.Sphere, new Vector3(-1.2f, 1.08f, 0f), new Vector3(1.0f, 0.75f, 0.75f), Quaternion.identity, hide);
        AddPart(visual.transform, "Neck", PrimitiveType.Capsule, new Vector3(1.38f, 1.45f, 0f), new Vector3(0.45f, 1.2f, 0.45f), Quaternion.Euler(0f, 0f, -55f), hide);
        Transform head = AddPart(visual.transform, "Head", PrimitiveType.Capsule, new Vector3(2.18f, 1.75f, 0f), new Vector3(0.58f, 1.05f, 0.58f), Quaternion.Euler(0f, 0f, 83f), hide).transform;
        AddPart(visual.transform, "Snout", PrimitiveType.Capsule, new Vector3(2.82f, 1.65f, 0f), new Vector3(0.34f, 1.0f, 0.38f), Quaternion.Euler(0f, 0f, 84f), belly);
        AddPart(visual.transform, "Tail", PrimitiveType.Capsule, new Vector3(-2.55f, 1.03f, 0f), new Vector3(0.45f, 2.75f, 0.45f), Quaternion.Euler(0f, 0f, 92f), hide);

        for (int side = -1; side <= 1; side += 2)
        {
            AddPart(visual.transform, "Thigh", PrimitiveType.Capsule, new Vector3(-0.75f, 0.55f, side * 0.42f), new Vector3(0.42f, 0.98f, 0.42f), Quaternion.Euler(0f, 0f, -12f), dark);
            AddPart(visual.transform, "Shin", PrimitiveType.Capsule, new Vector3(-0.18f, 0.28f, side * 0.42f), new Vector3(0.25f, 0.82f, 0.25f), Quaternion.Euler(0f, 0f, 22f), dark);
            AddPart(visual.transform, "Foot", PrimitiveType.Capsule, new Vector3(0.35f, 0.12f, side * 0.42f), new Vector3(0.18f, 0.8f, 0.24f), Quaternion.Euler(0f, 0f, 90f), dark);
            AddPart(visual.transform, "Arm", PrimitiveType.Capsule, new Vector3(1.15f, 0.82f, side * 0.48f), new Vector3(0.14f, 0.7f, 0.14f), Quaternion.Euler(side * 25f, 0f, -35f), dark);
        }

        return head;
    }

    private static GameObject AddPart(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation;
        part.transform.localScale = localScale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(part.GetComponent<Collider>());
        return part;
    }

    private static Material MakeMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = name;
        material.color = color;
        return material;
    }
}
