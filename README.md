# SkyBall

SkyBall is a single-player arcade sports prototype built with Unity 6.3 LTS. Move the cyan player ball, influence the gold match ball with collisions and shots, and score in either goal while waves of enemy balls pursue you.

## Gameplay

- Guide the separate match ball toward a goal; goals update the blue or orange score and reset play after a short pause.
- Survive enemy waves. Each wave adds pursuers up to a limit of five, keeping the arena playable.
- Change the player ball's size to trade agility for weight.
- Switch between arena and follow camera views.
- Collect two power-ups. One boosts movement and knocks enemies away on contact; the other enables rapid fire, whose projectiles push the match ball or enemies.
- Power-ups appear at the start and then about every 8 seconds, while fewer than three remain on the arena. The two types alternate.

The **Ayuda ?** button in the game HUD opens the controls when needed, keeping the play area clear during a match.

## Controls

| Action | Controls |
| --- | --- |
| Move forward or backward | `W` / `S` or `↑` / `↓` |
| Turn the arena and adjust heading | `A` / `D` or `←` / `→` |
| Jump | `Space` |
| Switch camera view | `C` |
| Set player size | `1` small, `2` medium, `3` large |
| Fire while rapid fire is active | Hold `F` or left-click |

## Open and play

1. Open the project in Unity Hub with **Unity 6.3 LTS (`6000.3.10f1`)** and WebGL Build Support installed.
2. Open `Assets/Scenes/Prototype 4.unity`.
3. Press **Play** in the Editor.

## Build for WebGL

The project includes a reusable profile at `Assets/Settings/Build Profiles/WebGL.asset`.

1. Open Unity's **Build Profiles** window and select **WebGL**.
2. Build to an output folder, then deploy the complete folder to a web server.

The current WebGL build uses Brotli compression. The host must serve `.br` files with the correct `Content-Encoding: br` header. Open the build through a web server; launching `index.html` directly with `file://` will not work reliably.

## Project layout

- `Assets/Scenes/` — playable and sample scenes.
- `Assets/Scripts/` — player, enemies, match flow, goals, abilities, camera, HUD, and spawning.
- `Assets/Prefab/` — enemy and power-up prefabs.
- `Assets/Settings/Build Profiles/` — platform build profiles.
