# Ted's Unity Dino Starter

This repo has three starter scripts:

- `Assets/Scripts/DinoPlayerController.cs` — creature-style WASD movement, sprint, jump, smoother acceleration, slope lean and simple leg motion.
- `Assets/Scripts/ThirdPersonCamera.cs` — smooth follow camera that looks at the dino head.
- `Assets/Editor/TedDinoIslandBuilder.cs` — Unity editor menu that builds the starter island scene.

## In Unity

1. Pull/sync the latest repo changes.
2. Open the project in Unity.
3. In the top menu, click `Ted > Create Dino Island Starter Scene`.
4. Press Play.
5. Controls: W/Up = forward where the dino nose points, S/Down = reverse, A/D = steer, Shift+W = sprint, Space = jump.

The builder now creates a much larger procedural terrain island with hills, a river, plants, rocks, collectibles and surrounding ocean. If you already created the old scene, run `Ted > Create Dino Island Starter Scene` again to rebuild it.

This is still a placeholder dinosaur. For true photorealistic visuals and proper walking/running over hills, the next real step is importing a rigged dinosaur model with animations and blending those animations in Unity.
