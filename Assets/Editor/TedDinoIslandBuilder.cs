using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TedDinoIslandBuilder
{
    [MenuItem("Ted/Create Dino Island Starter Scene")]
    public static void CreateStarterScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Dino Island Starter";

        Material grass = MakeMaterial("Ted_Grass", new Color(0.22f, 0.45f, 0.18f));
        Material sand = MakeMaterial("Ted_Sand", new Color(0.74f, 0.62f, 0.36f));
        Material water = MakeMaterial("Ted_Water", new Color(0.08f, 0.36f, 0.65f, 0.55f));
        Material dinoHide = MakeMaterial("Ted_DinoHide", new Color(0.28f, 0.34f, 0.22f));
        Material belly = MakeMaterial("Ted_DinoBelly", new Color(0.67f, 0.60f, 0.42f));
        Material dark = MakeMaterial("Ted_DarkDetails", new Color(0.08f, 0.10f, 0.08f));

        GameObject sun = new GameObject("Sun");
        Light light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 2.2f;
        sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

        GameObject island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        island.name = "Island Grass";
        island.transform.position = Vector3.zero;
        island.transform.localScale = new Vector3(24f, 0.25f, 24f);
        island.GetComponent<Renderer>().sharedMaterial = grass;

        GameObject beach = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beach.name = "Beach Ring";
        beach.transform.position = new Vector3(0f, -0.08f, 0f);
        beach.transform.localScale = new Vector3(30f, 0.18f, 30f);
        beach.GetComponent<Renderer>().sharedMaterial = sand;

        GameObject sea = GameObject.CreatePrimitive(PrimitiveType.Plane);
        sea.name = "Sea";
        sea.transform.position = new Vector3(0f, -0.2f, 0f);
        sea.transform.localScale = new Vector3(16f, 1f, 16f);
        sea.GetComponent<Renderer>().sharedMaterial = water;

        GameObject player = new GameObject("Playable Dino Placeholder");
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2.2f;
        controller.radius = 0.55f;
        controller.center = new Vector3(0f, 1.1f, 0f);
        player.AddComponent<DinoPlayerController>();
        BuildBetterPlaceholderDino(player.transform, dinoHide, belly, dark);

        GameObject camera = new GameObject("Follow Camera");
        Camera cam = camera.AddComponent<Camera>();
        cam.tag = "MainCamera";
        camera.transform.position = new Vector3(0f, 4.5f, -8f);
        ThirdPersonCamera follow = camera.AddComponent<ThirdPersonCamera>();
        follow.target = player.transform;
        player.GetComponent<DinoPlayerController>().cameraTransform = camera.transform;

        GameObject markerParent = new GameObject("Collectibles");
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8f;
            GameObject food = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            food.name = "Food Marker";
            food.transform.SetParent(markerParent.transform);
            food.transform.position = new Vector3(Mathf.Cos(angle) * 8f, 0.65f, Mathf.Sin(angle) * 8f);
            food.transform.localScale = Vector3.one * 0.45f;
            food.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Ted_Food", new Color(0.28f, 0.85f, 0.22f));
        }

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/DinoIslandStarter.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeGameObject = player;
        Debug.Log("Ted created DinoIslandStarter.unity. Press Play and move with WASD/arrows, Shift sprint, Space jump.");
    }

    private static void BuildBetterPlaceholderDino(Transform root, Material hide, Material belly, Material dark)
    {
        AddPart(root, "Body", PrimitiveType.Capsule, new Vector3(0f, 1.15f, 0f), new Vector3(1.25f, 2.4f, 0.85f), Quaternion.Euler(0f, 0f, 90f), hide);
        AddPart(root, "Belly", PrimitiveType.Capsule, new Vector3(0.2f, 0.92f, 0f), new Vector3(0.5f, 1.7f, 0.55f), Quaternion.Euler(0f, 0f, 90f), belly);
        AddPart(root, "Hip", PrimitiveType.Sphere, new Vector3(-1.2f, 1.08f, 0f), new Vector3(1.0f, 0.75f, 0.75f), Quaternion.identity, hide);
        AddPart(root, "Neck", PrimitiveType.Capsule, new Vector3(1.38f, 1.45f, 0f), new Vector3(0.45f, 1.2f, 0.45f), Quaternion.Euler(0f, 0f, -55f), hide);
        AddPart(root, "Head", PrimitiveType.Capsule, new Vector3(2.18f, 1.75f, 0f), new Vector3(0.58f, 1.05f, 0.58f), Quaternion.Euler(0f, 0f, 83f), hide);
        AddPart(root, "Snout", PrimitiveType.Capsule, new Vector3(2.82f, 1.65f, 0f), new Vector3(0.34f, 1.0f, 0.38f), Quaternion.Euler(0f, 0f, 84f), belly);
        AddPart(root, "Tail", PrimitiveType.Capsule, new Vector3(-2.55f, 1.03f, 0f), new Vector3(0.45f, 2.75f, 0.45f), Quaternion.Euler(0f, 0f, 92f), hide);

        for (int side = -1; side <= 1; side += 2)
        {
            AddPart(root, "Thigh", PrimitiveType.Capsule, new Vector3(-0.75f, 0.55f, side * 0.42f), new Vector3(0.42f, 0.98f, 0.42f), Quaternion.Euler(0f, 0f, -12f), dark);
            AddPart(root, "Shin", PrimitiveType.Capsule, new Vector3(-0.18f, 0.28f, side * 0.42f), new Vector3(0.25f, 0.82f, 0.25f), Quaternion.Euler(0f, 0f, 22f), dark);
            AddPart(root, "Foot", PrimitiveType.Capsule, new Vector3(0.35f, 0.12f, side * 0.42f), new Vector3(0.18f, 0.8f, 0.24f), Quaternion.Euler(0f, 0f, 90f), dark);
            AddPart(root, "Arm", PrimitiveType.Capsule, new Vector3(1.15f, 0.82f, side * 0.48f), new Vector3(0.14f, 0.7f, 0.14f), Quaternion.Euler(side * 25f, 0f, -35f), dark);
        }
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
