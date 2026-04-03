# Phase A½ — The Mine: Environment & Elevator

> **Goal:** Replace the flat plane from Phase A with an enclosed underground mine room. The player descends via an elevator on scene start, with Perlin noise shake and landing effects. All Phase A systems (shop, interaction) are repositioned inside the mine.

---

## Table of Contents

- [What Phase A½ Looks Like When Running](#what-phase-a-looks-like-when-running)

1. [Folder Structure](#1-folder-structure)
2. [Architecture Overview](#2-architecture-overview)
3. [Script Reference](#3-script-reference)
4. [Unity Scene Setup (Step-by-Step)](#4-unity-scene-setup-step-by-step)
5. [Testing Checklist](#5-testing-checklist)
6. [Source vs Phase A½ — What Changed and Why](#6-source-vs-phase-a---what-changed-and-why)

---

## What Phase A½ Looks Like When Running

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│   You hit Play. The screen is dark — you're underground.    │
│                                                             │
│   You're standing on a platform high above the mine floor.  │
│   The elevator starts descending immediately.               │
│                                                             │
│   During descent:                                           │
│   - Platform shakes side-to-side (Perlin noise on X/Z)     │
│   - Shake is strong at the top, fades as you approach       │
│     the bottom                                              │
│   - Speed is fast at top, decelerates smoothly near bottom  │
│   - A roof collider prevents you from jumping off           │
│   - Camera has subtle ambient sway (barely noticeable)      │
│                                                             │
│   Near the bottom (~1m above floor):                        │
│   - Dust/smoke particle burst plays                         │
│                                                             │
│   Elevator settles at floor level:                          │
│   - Shake stops completely                                  │
│   - Roof collider disables — you can look up freely         │
│   - GameEvents.OnElevatorLanded fires                       │
│                                                             │
│   You're now in an enclosed underground mine room:          │
│   - Rocky walls surround you                                │
│   - Dim point lights (warm orange or cool blue)             │
│   - No sky visible — dark ambient lighting                  │
│   - Tunnel openings in walls (empty, for future mining)     │
│                                                             │
│   Against one wall: the shop terminal (cube from Phase A).  │
│   Walk to it, press E → shop still works exactly as before. │
│   Buy an item → it spawns near the elevator shaft base      │
│   (ShopSpawnPoints repositioned from Phase A).              │
│                                                             │
│   Everything from Phase A works identically —               │
│   just inside a mine instead of on a flat plane.            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 1. Folder Structure

```
learn/phase-a-1/
├── GUIDE.md                              ← You are here
└── Scripts/
    └── World/
        ├── StartingElevator.cs           — Code-driven elevator descent with shake
        └── CameraShaker.cs              — Perlin noise camera sway + view punch

Modified from Phase A:
    phase-a/Scripts/Core/GameEvents.cs    — Added: OnElevatorLanded, OnGamePaused, OnGameUnpaused
```

---

## 2. Architecture Overview

```
Scene Start
    │
    ▼
StartingElevator.OnEnable()
    │
    ├── Teleports player to elevator platform position
    ├── Sets elevator Y to StartingHeight (15m above)
    ├── Activates RoofCollider (prevents jumping out)
    │
    ▼
StartingElevator.Update() — every frame while _isLowering:
    │
    ├── Calculates speed (slows near bottom via InverseLerp)
    ├── Moves Y downward by speed * deltaTime
    ├── Applies Perlin noise shake to X/Z (amplitude shrinks near bottom)
    ├── At Y < EndHeight + 1: activates LandingParticle
    ├── At Y <= EndHeight: stops, disables RoofCollider
    │
    ▼
GameEvents.RaiseElevatorLanded()
    │
    ▼
(Future phases can subscribe to do things after landing)


CameraShaker — runs independently on camera:
    │
    ├── Ambient Perlin noise sway (position + rotation)
    └── ApplyViewPunch(Vector3) — one-shot kick that decays
        (called by StartingElevator landing or future explosions)
```

### How Elevator Movement Works (No .anim, No DOTween)

The elevator is **100% code-driven** in `Update()`. Each frame:

```
1. Calculate progress: how close to EndHeight?
   normalizedProgress = InverseLerp(0.15, 0, distanceRemaining)

2. Calculate speed: fast at top, slow near bottom
   speed = Lerp(1.25, 0.1, normalizedProgress)

3. Calculate shake: strong at top, zero at bottom
   shakeAmount = Lerp(maxShake, 0, normalizedProgress)

4. Apply movement:
   localPos.y -= speed * deltaTime
   localPos.x = PerlinNoise(time * frequency, 0) * shakeAmount
   localPos.z = PerlinNoise(0, time * frequency) * shakeAmount
```

This gives a natural deceleration + shake that dies off as the elevator settles.

---

## 3. Script Reference

### World/StartingElevator.cs

| Item | Detail |
|------|--------|
| **Purpose** | Code-driven elevator that lowers the player into the mine on scene start |
| **Attach to** | Elevator platform GameObject |
| **Inspector** | `StartingHeight` (default 15), `EndHeight` (default 0), `PlayerTeleportPosition`, `RoofCollider`, `LandingParticle` |
| **Method: `TeleportPlayerAndLowerElevator()`** | Moves player to platform and starts descent |
| **Method: `LowerTheElevator()`** | Resets elevator to top and begins lowering |
| **Method: `Update()`** | Moves Y downward with speed + Perlin shake each frame |
| **Events fired** | `GameEvents.RaiseElevatorLanded()` when descent completes |
| **Events listened** | `GameEvents.OnGamePaused` / `OnGameUnpaused` (placeholder for sound pause) |

### World/CameraShaker.cs

| Item | Detail |
|------|--------|
| **Purpose** | Applies ambient Perlin noise sway to camera + one-shot view punch |
| **Attach to** | Player camera |
| **Inspector** | `positionAmplitude`, `rotationAmplitude`, `positionFrequency`, `rotationFrequency` |
| **Method: `Update()`** | Applies noise-based position/rotation offset and decays active punch |
| **Method: `ApplyViewPunch(Vector3)`** | Kicks the camera rotation, smoothly recovers to zero |

### Modified: Core/GameEvents.cs (Phase A)

| Addition | Type | Purpose |
|----------|------|---------|
| `OnElevatorLanded` | `event Action` | Fired when elevator reaches bottom |
| `OnGamePaused` | `event Action` | Fired when game is paused |
| `OnGameUnpaused` | `event Action` | Fired when game is unpaused |
| `RaiseElevatorLanded()` | Method | Invokes OnElevatorLanded |
| `RaiseGamePaused()` | Method | Invokes OnGamePaused |
| `RaiseGameUnpaused()` | Method | Invokes OnGameUnpaused |

---

## 4. Unity Scene Setup (Step-by-Step)

### Step 1: Build the Mine Environment

Replace the flat `Ground` plane from Phase A with an enclosed room:

**Option A: ProBuilder (recommended for prototyping)**
1. Install ProBuilder: **Window > Package Manager > ProBuilder**
2. **Tools > ProBuilder > New Shape > Cube**
3. Create floor (10×1×10), walls (10×5×0.5), ceiling with a hole for the elevator shaft
4. Add a Mesh Collider to each piece
5. Apply dark rock/stone material

**Option B: Simple primitives**
1. Create a floor Plane (scale 3,1,3)
2. Create 4 Cubes for walls (stretch to wall size)
3. Create a ceiling Cube with a gap in the center (elevator shaft)
4. Add Colliders to all

**Either way, you need:**
- An enclosed room (~10×5×10 meters)
- A hole in the ceiling for the elevator shaft
- Tunnel openings in the walls (just dark openings, empty for now)
- Point lights for dim underground lighting
- Ambient light set to dark (Lighting > Environment > Ambient Color: dark grey)

### Step 2: Create the Elevator Platform

1. **Create Empty** → name it `ElevatorPlatform`
2. Position: `(0, 0, 0)` (this is the end position)
3. Add child: **Cube** (scale 3,0.2,3) — the platform the player stands on
4. Add child: **Empty** → name it `PlayerTeleportPosition`, position `(0, 0.5, 0)` on the platform
5. Add child: **Cube** → name it `RoofCollider`, position above the platform, scale to cover the shaft opening. **Initially inactive** (the script enables it)
6. Add child: **Empty** → name it `LandingParticle`, position at floor level
   - Optionally add a **Particle System** with dust/smoke burst. **Initially inactive**
7. Add component: **StartingElevator** to `ElevatorPlatform`
   - `StartingHeight`: 15
   - `EndHeight`: 0
   - `PlayerTeleportPosition`: the child transform
   - `RoofCollider`: the roof cube
   - `LandingParticle`: the particle child

### Step 3: Add Camera Shaker

1. Select `PlayerCamera` (child of Player from Phase A)
2. Add component: **CameraShaker**
3. Set low values for subtle ambient sway:
   - `positionAmplitude`: 0.01
   - `rotationAmplitude`: 0.1
   - `positionFrequency`: 0.15
   - `rotationFrequency`: 0.08

### Step 4: Reposition Phase A Objects

Move these into the mine room:

| Object | New Position | Notes |
|--------|-------------|-------|
| `ShopTerminal` | Against a wall | Player walks to it after elevator lands |
| `ShopSpawnPoint_1/2/3` | Near elevator shaft base | Items "drop" from above |
| `Player` | Remove — StartingElevator handles initial position | Player starts on elevator |

### Step 5: Lighting

1. Delete the default Directional Light (we're underground)
2. Add 3-4 **Point Lights** around the mine room
   - Range: 8-12, Intensity: 0.5-1.0
   - Color: warm orange (torch-like) or cool blue (mineral glow)
3. Set **Lighting > Environment > Ambient Color** to very dark grey `(0.05, 0.05, 0.08)`

### Step 6: Verify Phase A Still Works

Everything from Phase A should still function:
- Walk to terminal → press E → shop opens
- Buy items → they spawn at ShopSpawnPoints
- Money updates on HUD
- ESC closes shop

---

## 5. Testing Checklist

Run the scene and verify:

- [ ] **Elevator starts high** — Platform begins at Y=15 (above the ceiling)
- [ ] **Player on platform** — Player is teleported to elevator at start
- [ ] **Descent** — Elevator lowers smoothly, decelerating near bottom
- [ ] **Shake** — Platform shakes during descent (X/Z Perlin noise)
- [ ] **Shake fades** — Shake amplitude decreases as elevator approaches bottom
- [ ] **Roof collider** — Player can't jump off during descent
- [ ] **Landing particle** — Dust/smoke activates near the bottom
- [ ] **Elevator stops** — Platform settles at EndHeight, shake stops
- [ ] **Roof collider off** — Player can now look/move up freely
- [ ] **Camera sway** — Subtle ambient noise on camera (barely perceptible)
- [ ] **Mine environment** — Enclosed room, no sky visible, dim lighting
- [ ] **Phase A works** — Shop terminal, interaction, cart, purchase all still functional

---

## 6. Source vs Phase A½ — What Changed and Why

| Area | Original Source Did | Phase A½ Does Instead | Why |
|------|-------------------|----------------------|-----|
| **Elevator pause/resume** | `StartingElevator` subscribes to `GameManager.GamePaused`/`GameUnpaused` events via direct singleton reference to pause `SoundPlayer` | Subscribes to `GameEvents.OnGamePaused`/`OnGameUnpaused`. Sound placeholder for Phase H. | Decoupling. No direct reference to GameManager singleton. |
| **Player teleport** | `StartingElevator` calls `FindObjectOfType<PlayerController>().TeleportPlayer()` — depends on full PlayerController | Finds `SimplePlayerController`, disables CharacterController, sets position, re-enables. | Phase A½ uses SimplePlayerController, not the full PlayerController yet (that's Phase B). |
| **New game check** | Original checks `SavingLoadingManager.SceneWasLoadedFromNewGame` and `DebugManager.DevModeEnabled` before lowering | Always lowers on `OnEnable()` — no save system or debug manager exists yet. | Save/load is Phase G, debug is Phase J. Elevator always runs for now. |
| **Camera shaker naming** | Original: `MainMenuCameraShaker` (tied to main menu scene) | Renamed to `CameraShaker` (generic, reusable on any camera) | The original name implies menu-only use. Our version works on gameplay camera too. Future phases can reuse it for explosion shake, etc. |
| **Elevator sound** | `StartingElevator` plays `SoundDefinition` via `SoundPlayer` component | Placeholder comments — sound system is Phase H | No sound system yet. Methods exist as stubs for future wiring. |
| **Elevator landing event** | Original has no explicit event — just sets `_isLowering = false` | Fires `GameEvents.RaiseElevatorLanded()` | Decoupling. Future systems (tutorials, UI, animations) can react without modifying elevator code. |

### What Stayed the Same

- **Elevator movement math** — identical `Mathf.Lerp` speed calculation, `Mathf.InverseLerp` progress, Perlin noise shake on X/Z
- **Roof collider** pattern — enabled during descent, disabled on landing
- **Landing particle** — activated when close to bottom
- **CameraShaker math** — identical Perlin noise position/rotation sway + SmoothDamp view punch decay
- **DefaultExecutionOrder(1000)** — elevator runs after all other scripts initialize
