# Ted's Unity Dino Starter

This repo now includes the CC0 Quaternius Animated LowPoly Dinosaurs pack:

- Source: https://quaternius.itch.io/animated-lowpoly-dinosaurs
- License: CC0 1.0 Universal / public domain
- Included here under `Assets/External/Quaternius/`

## Current playable dino

The scene builder now tries to use the imported Quaternius animated T-Rex FBX instead of the old primitive placeholder. It creates a simple animator controller from the FBX clips and drives it from player movement speed.

If Unity has not finished importing the FBX files yet, the builder may briefly fall back to the old primitive dino. Wait for Unity to finish compiling/importing, then run the Ted menu again.

## In Unity

1. Pull/sync the latest repo changes.
2. Open the project in Unity.
3. Wait for Unity to finish importing/compiling.
4. In the top menu, click `Ted > Create Dino Island Starter Scene`.
5. Press Play.
6. Controls: W/Up = forward where the dino nose points, S/Down = reverse, A/D = steer, Shift+W = sprint, Space = jump.

The builder creates a large procedural terrain island with hills, a river, plants, rocks, collectibles and surrounding ocean.

## Debugging what Zac sees

The game shows a top-left Ted debug HUD in Play mode. If Ted and Zac are seeing different things, send Ted a screenshot/video showing that HUD plus the Unity Console.
