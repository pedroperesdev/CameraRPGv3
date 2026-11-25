# CameraRPGv3

A first-person 3D game in Unity (C#), set in a liminal, dreamcore museum. It is a personal project I use to learn Unity 3D, so it is a work in progress.

## How to open

- Unity version: **6000.5.7f1**
- Clone the repo and open the folder in Unity Hub. Unity rebuilds the Library folder on first open.
- Open the scene **Assets/Scenes/SampleScene.unity** and press Play.

## Controls

- **WASD / arrow keys**: walk · **Mouse**: look
- **Left Shift (hold)**: run (uses stamina)
- **Left Ctrl (hold)**: crouch
- **E**: interact (pick up a memory, open/close a locker)
- **1–6 / mouse wheel**: choose the active memory slot
- **Right click (hold)**: aim the camera
- **Left click**: take the photo once the ring is full, which starts combat

Input is set up with the Input System package in `Assets/InputSystem_Actions.inputactions`.

## Core mechanic: camera combat

The player aims the camera at an enemy. While the crosshair stays on the enemy, a ring around it fills up. When the ring is full, a left click takes the photo, which uses up the equipped **memory**. Each memory carries its own minigame, and the player's performance in it decides how much damage the enemy takes.

Memories are picked up in the world and stored in a 6-slot inventory. Without one equipped, the photo can't be taken.

Current minigames:

- **Click minigame:** click repeatedly to fill a bar.
- **Birthday minigame:** blow out the candles at the right moment.

## State machines

All of them use the State pattern: each state is its own class with `EnterState`, `UpdateState`/`Tick` and `ExitState`.

**Player movement:** Idle, Walk, Run, Crouch. Running drains stamina, and once it runs out the player has to recover before running again.

**Player camera (separate from movement):** Lowered and Aiming, each with its own camera position. Aiming also handles the ring charge and the photo.

**Enemies (Tall):**

1. **Patrol:** follows a fixed path of waypoints.
2. **Alerted:** if the player is in its line of sight with no obstacles in between, the enemy jumps briefly.
3. **Chase:** it then chases the player and damages them on contact.
4. **Confused:** placeholder, not implemented yet.

## Pathfinding

Enemies use Unity's **NavMesh**, so they move around objects and obstacles while patrolling and chasing.

## Other systems

- **Lockers:** containers that open and close with E, with sound (`IOpenable`).
- **Events:** `PlayerController` sends C# events (stamina, ring charge, hover info, warnings) that the HUD and other scripts listen to (Observer pattern).
- **Managers:** `GameManager` switches between overworld and minigame mode, `CombatManager` runs a combat round, `AudioManager` plays sound.

## Where to look

- Player states: **[Assets/Scripts/Player/MovementStates/](Assets/Scripts/Player/MovementStates/)** (`PlayerController.cs` is the main player script)
- Enemy states: **[Assets/Scripts/Enemy/](Assets/Scripts/Enemy/)** (`Tall.cs`, with states in `TallStates/`)
- Camera and ring charge: **[Assets/Scripts/Player/CameraStates/](Assets/Scripts/Player/CameraStates/)** (`AimCamState.cs`)
- Combat and minigames: **[Assets/Scripts/](Assets/Scripts/)** (`CombatManager.cs`, `Minigame.cs`, `ClickMinigame.cs`, `BirthdayMinigame.cs`)

## Status

In progress. Next steps: more enemy behaviours (starting with the Confused state) and more minigames.
