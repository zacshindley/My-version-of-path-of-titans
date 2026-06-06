using UnityEngine;

public class TedGameDebugHud : MonoBehaviour
{
    public const string TedBuildLabel = "TED UNITY DINO v2026-06-06.5 - normal follow camera";

    private GUIStyle style;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (Object.FindFirstObjectByType<TedGameDebugHud>() != null)
        {
            return;
        }

        GameObject hud = new GameObject("Ted Debug HUD - confirms loaded version");
        hud.AddComponent<TedGameDebugHud>();
        Object.DontDestroyOnLoad(hud);
    }

    private void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 18;
            style.normal.textColor = Color.white;
            style.padding = new RectOffset(12, 12, 10, 10);
        }

        Terrain terrain = Terrain.activeTerrain;
        DinoPlayerController dino = Object.FindFirstObjectByType<DinoPlayerController>();
        string terrainText = terrain != null && terrain.terrainData != null
            ? $"Terrain: {terrain.terrainData.size.x:0} x {terrain.terrainData.size.z:0}, height {terrain.terrainData.size.y:0}"
            : "Terrain: NONE";

        string dinoText = dino != null
            ? $"Dino: pos {dino.transform.position.x:0},{dino.transform.position.y:0},{dino.transform.position.z:0} forward {dino.transform.forward.x:0.00},{dino.transform.forward.z:0.00}"
            : "Dino: NONE";

        string cameraText = Camera.main != null
            ? $"Camera: {Camera.main.name}"
            : "Camera: NONE";

        GUI.Box(
            new Rect(12, 12, 620, 118),
            TedBuildLabel + "\n" + terrainText + "\n" + dinoText + "\n" + cameraText,
            style
        );
    }
}
