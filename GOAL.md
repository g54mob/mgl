# GOAL

> Every script is typed by hand. This file defines how.

---

## What

Rebuild **MINEMGL** from `Scripts/Assembly-CSharp/` (~200 scripts).
**100% source behavior** + cleaner architecture + independently testable systems.

## Source of Truth

| What | Where |
|------|-------|
| Logic & behavior | `Scripts/Assembly-CSharp/*.cs` |
| Architecture | `learn/ARCHITECTURE.md` |
| Roadmap | `learn/PhaseMap.md` |
| Reasoning log | `learn/surfer.md` |

---

## Core Principle

**Every script's purpose fits in one sentence. If it doesn't, split it until it does.**
**Every script performs ONLY the service it was assigned — purely, nothing else.**

Every file must have a `→ "I do X"` one-liner. That one-liner is the script's contract. If the script does anything outside that sentence, it's doing too much — move it to the script whose sentence covers it.

```
SO_ShopItemDef         → "I define what a shop item IS"
WShopItem              → "I track what happened to one item this session"
ShopDataService        → "I manage all shop data + cart as a collection"
Field_ShopItem         → "I display one item row"
ShopUIOrchestrator     → "I wire UI fields to data and handle actions"
ShopUI                 → "I open and close the shop panel"
BgUI                   → "I show/hide blur when menus change"
UIManager              → "I report if any menu is open"
EconomyManager         → "I own money"
GameEvents             → "I deliver messages between systems"
Singleton              → "I ensure one instance"
```

End goal: **main source (100%) behavior** with this architecture — consistent, decoupled, independently testable via vertical slice tests.

---

## Folder Structure

Numbered for Unity project panel sorting. One-liner purpose per folder.

```
Scripts/phase-X/
│
├── 0-Core/                     → "everything depends on me, I have zero game logic"
│
├── 1-Managers/                 → "I'm a singleton that owns one domain's state"
│   └── SubManager/             → "I open/close one UI panel and nothing else"
│
├── 2-Data/                     → "everything inside me is independent — testable via new instance, no scene needed"
│   ├── SO_*.cs  (root)         → "I'm a config blueprint — only fields, no methods"
│   ├── Field_*.cs (root)       → "I display UI — SetData/SetState only, no onClick, no logic"
│   ├── Interface/              → "I'm a contract — no implementation"
│   ├── DataWrapper/            → "I wrap an SO with mutable session state (W prefix)"
│   ├── DataService/            → "I manage a collection — build, query, add, remove, snapshot"
│   ├── Enum/                   → "I'm a named set of constants"
│   └── Entities/               → "I'm a grouped small [Serializable] class — entries, save data, structs"
│
├── 3-MonoBehaviours/           → "I have game logic and live in the scene"
│   └── Orchestrator/           → "I wire Field_ prefabs — Instantiate, AddListener, Destroy (only special pattern)"
│
├── 4-Utils/                    → "I'm static — no MonoBehaviour, no state, just helpers and LOG"
│   ├── UtilsPhaseX.cs          → "I'm static extensions + helpers grouped by region (Shop, Economy, Player...)"
│   └── PhaseXLOG.cs            → "I format data collections to JSON for logging — per-collection targeting via .ToNSJson()"
│
└── 5-Tests/                    → "I test one system independently — no other systems needed"
```

### Key Principles

- **2-Data/ is fully independent.** Every class inside can be tested by creating a `new` instance. No scene, no MonoBehaviour, no GameObjects needed. `DEBUG_Check` proves this.
- **Most of 2-Data/ is pure C#** — no Unity dependency. DataWrapper, DataService, Enum, Entities are all plain C# classes (Vector3/Vector2 are fine — they're structs, not Unity components). Only `SO_` (inherits ScriptableObject) and `Field_` (inherits MonoBehaviour) have Unity dependency.
- **3-MonoBehaviours/ has one special subfolder: Orchestrator/.** Everything else is just "MonoBehaviour with game logic." Orchestrator is the only distinct pattern — it wires Field_ instances. Machines, tools, conveyors, buildings are all regular MonoBehaviours grouped by domain when needed (not by pattern).
- **1-Managers/SubManager/ owns its own lifecycle.** UIManager never calls SetActive on SubManagers. They self-init in Start(), disable themselves, listen for GameEvents to re-enable.

### Why This Structure

**Separation by responsibility, not by domain.** Most Unity projects do `Scripts/Shop/`, `Scripts/Player/` — grouping by topic. This structure groups by **what the file does**: `2-Data/` = no logic, `3-MonoBehaviours/` = has logic. When you ask "where does this go?" the answer is about the file's **nature**, not its **topic**.

**What's strong:**
- `2-Data/` independence — testable via `new`, no scene needed. Rare in Unity projects.
- Numbered folders — Unity project panel shows dependency order visually.
- Orchestrator as the only special pattern — no over-categorization.
- Natural MVP: Field_ = View, DataService = Model, Orchestrator = Presenter, ShopUI = Controller.

**What to watch:**
- `3-MonoBehaviours/` will get crowded by Phase E (~80+ files). Add domain subfolders when it gets noisy (`Machine/`, `Tool/`, `Conveyor/`).
- DataService/ grows across phase boundaries — `ShopDataService` (Phase A) + `QuestDataService` (Phase F) coexist. That's fine.

**End goal: 100% of main source functionality** with this architecture to keep it consistent, decoupled, minimal public API, and independently testable via vertical slice tests.

### Where Does It Go?

```
Is it a foundation utility?            → 0-Core/
Is it a singleton owning domain state? → 1-Managers/
Does it only open/close a UI panel?    → 1-Managers/SubManager/
Is it an SO with only fields?          → 2-Data/SO_*.cs
Is it a display-only prefab handle?    → 2-Data/Field_*.cs
Is it a contract with no implementation?→ 2-Data/Interface/
Is it a runtime wrapper around an SO?  → 2-Data/DataWrapper/
Does it manage a collection?           → 2-Data/DataService/
Is it an enum?                         → 2-Data/Enum/
Is it a small serializable struct/class?→ 2-Data/Entities/
Does it wire Field_ + AddListener?     → 3-MonoBehaviours/Orchestrator/
Does it have game logic in the scene?  → 3-MonoBehaviours/
Is it a static helper or LOG formatter?→ 4-Utils/
Is it a test?                          → 5-Tests/
```

---

## Naming

| Kind | Convention | Example |
|------|-----------|---------|
| ScriptableObject | `SO_` prefix | `SO_ShopCategory`, `SO_ShopItemDef` |
| Display-only prefab handle | `Field_` prefix | `Field_ShopItem`, `Field_ShopCartItem` |
| DataWrapper | `W` prefix | `WShopItem`, `WQuest` |
| Interface | `I` prefix | `IInteractable` |
| Dictionary lookups | `DOC__` prefix | `DOC__category_wShopItem`, `DOC__CartItem__Field` |
| List/collection fields | `ALL_CAPS` | `CATEGORY`, `ITEM_DEF`, `CARTITEM` |
| `[SerializeField]` fields | `_` prefix | `[SerializeField] float _defaultMoney` |
| Private non-serialized | `camelCase` | `money`, `isAnyMenuOpen`, `xRot` |
| Event handlers | `Handle...` | `HandleMoneyChanged` |
| GameEvents | `On...` + `Raise...` side by side | `OnMoneyChanged` / `RaiseMoneyChanged()` |
| Singleton access | `.Ins` | `Singleton<EconomyManager>.Ins` |

**Prefix = no logic.** `SO_` = pure data. `Field_` = display only. `W` = session wrapper. No prefix = has logic.

---

## Script Structure

`#region` blocks. One-liner `<summary>` where helpful.

```csharp
public class Example : MonoBehaviour
{
    #region Inspector Fields
    #endregion

    #region private API
    #endregion

    #region public API
    #endregion

    #region Unity Life Cycle
    #endregion
}
```

Nested types (`CartItem`) go inside the class that owns them.

---

## Minimal Methods

**Top priority.** Every script typed by hand — fewer methods = less to type, less to break.

- **Public API:** only expose what another script absolutely needs to call
- **Private methods:** only create when logic is reused or genuinely needs isolation — otherwise inline it
- **Don't over-fragment.** If a 5-line block is used once, keep it inline.

```
ShopUIOrchestrator:  2 public (Init, BuildCategoryView)
                     5 private (only what's needed)
ShopUI:              1 public (GetDataServiceForTest)
                     0 private (lifecycle only)
EconomyManager:      3 public (GetMoney, AddMoney, CanAfford)
SimplePlayerController: 0 public, 3 private handlers
```

---

## Class Responsibilities

### `SO_` — Pure Data (2-Data/)
Only fields. No methods. One file per SO.

### `Field_` — Display Only (2-Data/)
Inspector refs + display setters: `SetData(...)`, `SetButtonInteractable(...)`, `SetPrice(...)`, `SetQty(...)`.
Sets `.text`, `.color`, `.sprite`, `.interactable` — nothing else.
**No** onClick wiring. **No** business logic. **No** singleton access.

### `W` — DataWrapper (2-Data/DataWrapper/)
Plain C# wrapping SO + mutable session state.
`WShopItem` wraps `SO_ShopItemDef` + `isLockedCurr` + `timesPurchased`.
**SO = what it IS. W = what happened to it.**

### DataService (2-Data/DataService/)
One service per domain. Plain C#. Manages ALL collections for that domain.
`ShopDataService` — builds W wrappers, category queries, cart operations, afford check, snapshot.
Nested types live here (`CartItem` inside `ShopDataService`).
Has `GetSnapShotForTest(header)` that combines all PhaseXLOG calls into one formatted output.

### Managers (1-Managers/)
Singleton MonoBehaviours. One domain, one manager.
`EconomyManager` — owns money via `GetMoney()`, `AddMoney()`, `CanAfford()`.
`UIManager` — reports `IsInAnyMenu()`. Nothing else.

### SubManagers (1-Managers/SubManager/)
One panel's lifecycle. **Open/close + init only.**
`ShopUI` — builds DataService on Start, inits Orchestrator, subscribes toggle event. Zero business logic.
`BgUI` — blur on/off via `GameEvents.OnMenuStateChanged`.

### Orchestrator (3-MonoBehaviours/Orchestrator/)
Wires `Field_` prefab instances. All `AddListener` calls live here.
Reads from DataService. Refreshes on events only — never `Update()`.
`ShopUIOrchestrator` — BuildCategoryView, SelectCategoryView, RepopulateShopItemsView, createAndOrchestrateCartItemField, PurchaseCart.

### MonoBehaviours (3-MonoBehaviours/)
World objects, systems, player. Each has minimal public API (ideally zero).

### Utils (4-Utils/)
`UtilsPhaseA` — extensions by region (`formatMoney()`, `formatMoneyShort()`).
`PhaseALOG` — snapshot formatters **targeting specific collections separately**:
```
LIST_CATEGORY__TO__JSON(CATEGORY)           → snapshot of category list
DOC_CATEGORY_ITEM__TO__JSON(DOC)            → snapshot of category→items dictionary
LIST_CARTITEM__TO__JSON(CARTITEM)           → snapshot of cart items
```
All serialization via `.Select()` + anonymous types + `.ToNSJson(pretify: true)` from `SPACE_UTIL`.
PhaseXLOG methods are called by DataService's `GetSnapShotForTest()` — single entry point for test logging.

### Tests (5-Tests/)
**Two levels of testing:**
1. **Data-level** (`DEBUG_Check`) — tests anything in `2-Data/` by creating a plain C# instance. Zero dependency. No UI, no scene, no MonoBehaviour needed. DataService, DataWrapper, Entities — all testable this way.
2. **UI-level** (`ShopUITest`) — tests full UI flow with keyboard shortcuts listed in inspector README field.

Each test is independent. Lists prerequisites in summary or inspector field.

---

## Decoupling

- **`GameEvents`** — static event bus. Event + Raise grouped by domain with `// when X >>` `// << when X` comment blocks.
  Each Raise calls `LogSubscribersCount()`: `[GameEvents] OnX raised for -> N subscribers`
- **Interfaces** — for abstraction (`IInteractable`)
- **Singleton reads** — only for queries (`Singleton<EconomyManager>.Ins.GetMoney()`), never cross-system commands
- **Never** let Script A directly call into unrelated Script B
- **Event-driven refresh** — never poll in `Update()`. Refresh only when state changes.

---

## No Defensive Null Checks

Let it crash. The crash is traceable.
Inspector refs, core singletons, `Field_` components — your responsibility to wire.
Don't hide bugs behind `?.` or `if (x != null)`.

**Exception:** `?.Invoke()` on GameEvents (events can have zero subscribers).

---

## Unity Lifecycle Order

```
Scene loads, GO is active:

  Awake()           ← once, first. Singleton registration, GetComponent caching.
      ↓
  OnEnable()        ← fires immediately after Awake (same frame). 
      ↓               Fires BEFORE Start. Fires every time GO re-enables.
  Start()           ← once, after ALL Awake + OnEnable across scene.
      ↓               Safe to read other singletons. Build data, init, subscribe.
  Update()          ← every frame
  LateUpdate()      ← every frame, after all Update()
      ↓
  OnDisable()       ← fires when GO disabled (SetActive false) or destroyed
  OnDestroy()       ← fires when GO destroyed (scene unload or Destroy())

Re-enable (SetActive true):
  OnEnable()        ← fires again (Start does NOT re-run)
      ↓
  Update() resumes

Key facts:
  - Awake + OnEnable fire together BEFORE Start
  - Start runs ONCE — never again even if re-enabled
  - OnEnable fires EVERY enable (first time + every re-enable)
  - Destroy() auto-cleans onClick/UnityEvent listeners
  - SetActive(false) does NOT clean listeners
```

### SubManager Pattern (ShopUI)

```
Scene loads, ShopUI GO starts ACTIVE:
  Awake()     → (not used)
  OnEnable()  → RaiseMenuStateChanged(true)    // brief — corrected by Start
  Start()     → build data, init orchestrator, subscribe OnOpenShopView, SetActive(false)
  OnDisable() → RaiseMenuStateChanged(false)   // corrects the brief OnEnable pulse
  (GO now inactive, waiting for GameEvents.RaiseOpenShopView)

Player opens shop:
  OnEnable()  → RaiseMenuStateChanged(true)    // cursor unlocks
  Update()    → ESC/E → SetActive(false)
  OnDisable() → RaiseMenuStateChanged(false)   // cursor re-locks
```

### Usage Table

| Hook | Use For | Pair With |
|------|---------|-----------|
| `Awake()` | Singleton registration, `GetComponent` caching | — |
| `Start()` | Build data, init orchestrator, subscribe events, disable self | `OnDestroy()` |
| `OnEnable()` | `RaiseMenuStateChanged(true)`, event subs for toggled panels | `OnDisable()` |
| `OnDisable()` | `RaiseMenuStateChanged(false)`, unsub events | — |
| `Destroy()` | Auto-cleans onClick/UnityEvent listeners | — |
| `SetActive(false)` | Does NOT clean listeners | — |

---

## C# Features

**Allowed:** `$""`, `?.` (on events only), LINQ, `.map()`, `=>` expression-bodied, ternary `? :`

**Not allowed:** `async/await` (use coroutines), `Span<T>`, `Memory<T>`, `ValueTuple` deconstruction

---

## LEGO Independence

Each system works **standalone first**, connects later via GameEvents.
A script must **never** require an unfinished system to compile or run.

```
Type DataService → test with DEBUG_Check (no UI, no scene)
Type ShopUI + Orchestrator → test with ShopUITest (no player, no interaction)
Type InteractionSystem → test with InteractionTest (no shop, no player)
Type SimplePlayerController → test with PlayerControllerTest (no shop, no interaction)
Connect via GameEvents → full phase works
```

---

## Vertical Slice Tests

**Two levels per system:**
1. **Data-level** — test DataService logic without UI (DEBUG_Check, coroutine-based)
2. **UI-level** — test full UI flow with keyboard shortcuts (ShopUITest)

Each test lists prerequisites in inspector README field or summary comment.
Each is independent — doesn't require other systems to run.

---

## Hand-Typing Order

1. `#region` blocks (empty skeleton)
2. Inspector Fields (the data shape)
3. Private fields (state)
4. Unity Lifecycle (Start, OnEnable, OnDisable)
5. Public API (only what's needed — least possible)
6. Private API (only what's needed — least possible)
7. **Compile → Test → Next script**

---

## GUIDE.md Per Phase

Every phase folder has a `GUIDE.md` with:
- **What it looks like when running** (detailed)
- **Folder structure**
- **Script Purpose** — one sentence per script (if it doesn't fit one sentence, split it)
- **Hand-typing order** (compile groups with stop-and-test points)
- **Vertical Slice Test checklist per system** (internal/external prerequisites, NOT required, controls)
- **Scene setup** (step-by-step)
- **Source vs Phase diff** (what original did vs what we changed)

---

## Phase Overview

| Phase | Name | Weight | Difficulty |
|-------|------|--------|------------|
| A | World Interaction + Shop Cart | 7% | Easy |
| A½ | The Mine — Environment & Elevator | 3% | Easy |
| B | Player Controller + Inventory + Tools + Grabbing | 15% | Hard |
| C | Mining & Ore System | 14% | Medium |
| D | Building & Conveyor System | 14% | Hard |
| E | Ore Processing Machines | 18% | Medium |
| F | Quest & Research System | 10% | Medium |
| G | Save/Load System | 8% | Hard |
| H | Sound, Settings & UI Polish | 5% | Easy |
| I | Contracts, World Events & Menus | 4% | Easy |
| J | Debug, Demo & Final Polish | 2% | Easy |
