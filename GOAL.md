# GOAL

> This file defines the high-level projec goal, conventions, and rules for the entire learning build. Read this first before touching any phase.

---

## What This Project Is

Rebuild the **MINEMGL** mining/factory simulation from scratch, phase by phase, as a learning exercise. The original source (`Scripts/Assembly-CSharp/`, ~200 scripts) is the reference for all logic and behavior.

---

## The Goal

Produce a **100% feature-complete** recreation of the original game across 11 phases (A through J), with two key improvements over the original:

1. **No tight coupling** — all cross-system communication via `GameEvents` (static event bus) or interfaces
2. **Clean conventions** — consistent naming, lifecycle patterns, and ScriptableObject usage

---

## Source of Truth

- **Logic & behavior:** Always refer to `Scripts/Assembly-CSharp/*.cs` (the original decompiled source)
- **Architecture docs:** `learn/ARCHITECTURE.md` (full system analysis)
- **Phase plan:** `learn/PhaseMap.md` (all phases, files, modifications, visual targets)
- **Critical thoughts:** `learn/surfer.md` (reasoning log per prompt)

---

## Rules (Always Follow)

### 1. Source Fidelity

Match the original source behavior faithfully. If the original `CastingFurnace` uses majority-type smelting, ours does too. If the original `PlayerController` uses `Mathf.InverseLerp` for slope detection, ours does too. Don't invent new behavior.

### 2. Decoupling

Never let Script A directly call into unrelated Script B via singleton. Instead:

- **`GameEvents`** static event bus for cross-system notifications
- **Interfaces** (`IInteractable`, `IDamageable`, etc.) for abstraction
- **Singleton reads** only for legitimate queries (e.g., reading `EconomyManager.Money` for display)

### 3. Naming Conventions

| Item               | Convention                  | Example                                |
| -------------------------------- | -------------------------------------------- | --------------------------------------------------------------------- |
| ScriptableObject classes & files | Prefix with `SO_`             | `SO_ShopItemDefinition`, `SO_Interaction`             |
| Event handler methods      | `Handle...`, `Toggle...`, `Refresh...` | `HandleMenuStateChanged`, `ToggleShop`, `RefreshOnItemUnlocked` |
| GameEvents event declarations  | `On...` prefix               | `OnMoneyChanged`, `OnMenuStateChanged`              |
| GameEvents raise methods     | `Raise...` prefix             | `RaiseMoneyChanged`, `RaiseToggleShopRequested`          |
| Unity lifecycle methods     | Standard Unity names             | `Awake`, `Start`, `OnEnable`, `OnDisable`, `OnDestroy`   |

### 4. Lifecycle Pattern

| Hook      | Use For                              | Pair With    |
| --------------- | ----------------------------------------------------------------- | --------------- |
| `Awake()`   | Singleton registration,`GetComponent` caching only       | —       |
| `Start()`   | Event subscriptions for permanent objects (all Awake done by now) | `OnDestroy()` |
| `OnEnable()` | Event subscriptions for toggled objects (UI panels)        | `OnDisable()` |
| `OnDestroy()` | Cleanup for Start subscriptions                  | —       |
| `OnDisable()` | Cleanup for OnEnable subscriptions                | —       |
| `GameObject.Destroy()` | Cleans up onClick/UnityEvent listeners automatically  |
| `SetActive(false)`     | Does NOT clean listeners — use OnEnable/OnDisable if you ever want to deactivate here |   |
| `Object pooling`         | Always wire/unwire explicitly in OnEnable/OnDisable  |

### 5. .NET 2.0 Compatibility

- No `$""` string interpolation — use `string.Format()` -> sure
- No `?.` null-conditional — use explicit `if (x != null)` -> sure
- No `async/await` — use coroutines
- No expression-bodied members for complex logic
- `for` loops preferred over LINQ in hot paths -> sure

### 6. Documentation Per Method

Every method gets a one-liner XML `<summary>` comment:

```csharp
/// <summary>Adds the given amount to the player's money.</summary>
public void AddMoney(float amount)
```

seperate script into section such as: 
- public API
- private API
- Unity Life Cycle(if monobehaviour)
- enums etc ....

### 7. Phase GUIDE.md Requirements

Every phase folder must have a `GUIDE.md` containing:

- **"What Phase X Looks Like When Running"** — detailed ASCII walkthrough of the gameplay result
- **Folder structure** — all files in the phase
- **Script reference** — every script with purpose, inspector fields, methods
- **Scene setup** — step-by-step Unity editor instructions
- **Testing checklist** — verify items
- **Source vs Phase X diff** — table showing what original did vs what we did differently and why

### 8. PhaseMap.md Maintenance

`PhaseMap.md` is a living document. When entering a phase:

- Update status from "Planned" to "In Progress" / "Done"
- Add/remove/modify files as implementation reveals needs
- Keep "What It Looks Like" descriptions accurate
- Keep "Modifications to Earlier Phases" tables current

### 9. surfer.md Logging

Append a new entry to `surfer.md` after **every** prompt response. Format:

```
## Prompt N: "short summary"

**What I thought:**
- bullet points of reasoning, tradeoffs, decisions
```

### 10. UI Component Responsibility

MonoBehaviour components attached to UI prefabs (buttons, rows, panels)
are typed prefab handles — inspector field refs + display methods only.

- Inspector refs:        ✅ TextMeshProUGUI, Image, Button, TMP_InputField
- Visual state methods:  ✅ SetSelected(), RefreshUI(), SetData()
- onClick wiring:        ❌ never — owned by the parent orchestrator (e.g. ShopUI)
- Business logic:        ❌ never — owned by the parent orchestrator
- Singleton access:      ❌ never — parent passes what the component needs

The orchestrator instantiates, wires, and destroys. The component only displays.
---

## Phase Overview

| Phase | Name                       | Weight | Difficulty |
| ----- | ------------------------------------------------ | ------ | ---------- |
| A   | World Interaction + Shop Cart          | 7%   | Easy    |
| A½  | The Mine — Environment & Elevator        | 3%   | Easy    |
| B   | Player Controller + Inventory + Tools + Grabbing | 15%  | Hard    |
| C   | Mining & Ore System               | 14%  | Medium   |
| D   | Building & Conveyor System            | 14%  | Hard    |
| E   | Ore Processing Machines             | 18%  | Medium   |
| F   | Quest & Research System             | 10%  | Medium   |
| G   | Save/Load System                 | 8%   | Hard    |
| H   | Sound, Settings & UI Polish           | 5%   | Easy    |
| I   | Contracts, World Events & Menus         | 4%   | Easy    |
| J   | Debug, Demo & Final Polish            | 2%   | Easy    |

See `PhaseMap.md` for full details per phase.

---

## File Map

```
learn/
├── GOAL.md         ← You are here (project rules & goal)
├── ARCHITECTURE.md     ← Full original source analysis
├── PhaseMap.md       ← All phases, files, modifications, visual targets
├── surfer.md        ← Critical thoughts log per prompt
├── phase-a/         ← World Interaction + Shop Cart
│  ├── GUIDE.md
│  └── Scripts/...
├── phase-a-1/        ← The Mine: Environment & Elevator
│  ├── GUIDE.md
│  └── Scripts/...
├── phase-b/         ← (future) Player + Inventory + Tools
├── phase-c/         ← (future) Mining & Ore
├── ...
└── phase-j/         ← (future) Debug & Polish
```

---

## How To Use This

1. **Read `GOAL.md`** (this file) for rules and conventions
2. **Read `PhaseMap.md`** for the full roadmap and current status
3. **Enter a phase folder** (e.g., `phase-a/`) and read its `GUIDE.md`
4. **Follow the scene setup** step-by-step
5. **Run the testing checklist** to verify
6. **Move to the next phase** when all checks pass

When asking for a new phase to be built, just say "start Phase B" — the conventions, source references, and documentation structure are all defined here.



refer: https://claude.ai/share/f27c9018-a032-446d-8ade-1809db75a64d