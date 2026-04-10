# GOAL

> Every script is typed by hand. This file defines how.

---

## What

Rebuild **MINEMGL** from `Scripts/Assembly-CSharp/` (~200 scripts).
**100% main-source behavior** + cleaner architecture + independently testable systems.

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

**When to create a DataService:**
- Operations are pure logic (add, remove, query, sort, total) — no Unity API needed
- Testable via `new` instance in `DEBUG_Check` — zero scene, zero GameObjects
- Examples: InventoryDataService (slots, switch, stack), ShopDataService (categories, cart, afford)

**When NOT to create a DataService — keep in MonoBehaviour:**
- Operations need Unity physics (SpringJoint, Rigidbody, AddForce, OverlapSphere)
- Operations need Unity lifecycle (Update, FixedUpdate, Destroy, Instantiate)
- Data is transient frame state (velocity, bob counter) not a persistent collection
- Example: ToolMagnet manages `List<Rigidbody>` + SpringJoints — every operation needs Unity. Not a DataService.

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
- **Every `.Raise...()` call must have a `// purpose:` one-liner** explaining why it's fired and who listens:
  ```csharp
  // purpose: cursor lock/unlock for player controller
  GameEvents.RaiseMenuStateChanged(isAnyMenuOpen: true);
  
  // purpose: MoneyOrchestrator updates HUD text
  GameEvents.RaiseMoneyChanged(money);
  ```
- **Interfaces** — for abstraction (`IInteractable`)
- **Singleton reads** — only for queries (`Singleton<EconomyManager>.Ins.GetMoney()`), never cross-system commands
- **Never** let Script A directly call into unrelated Script B
- **Event-driven refresh** — never poll in `Update()`. Refresh only when state changes.

### Cross-Phase File Changes

When a new phase needs to add to an existing file:

- **`GameEvents` (static class)** → use `partial class`. Each phase adds its own events in its own `0-Core/GameEvents.cs`. No modification to earlier phase's file. Phase A's GameEvents must have `partial` keyword.
  ```
  phase-a/0-Core/GameEvents.cs    → OnMoneyChanged, OnMenuStateChanged, OnOpenShopView
  phase-a-1/0-Core/GameEvents.cs  → OnElevatorLanded, OnGamePaused, OnGameUnpaused
  phase-b/0-Core/GameEvents.cs    → OnToolSwitched, OnItemPickedUp, OnItemDropped
  ```
- **MonoBehaviours with `[SerializeField]`** (UIManager, InteractionSystem) → must be **directly modified** because inspector fields can't be added via partial across files. Document in GUIDE.md exactly what to change and why.
- **Rule:** prefer `partial` extend over direct modify. Only modify when `[SerializeField]` or inheritance requires it.
- **Every phase GUIDE.md** lists modifications in a table: `| File | Change | Why |`

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

**Allowed:** `$""`, `?.` (on events only), LINQ, `.Select()`, `=>` expression-bodied, ternary `? :`

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
1. **Data-level** — test DataService logic without UI (`DEBUG_Check` — plain `new` instance, zero scene)
2. **UI-level** — test full system flow with keyboard shortcuts (`ShopUITest`, `PlayerGrabTest`, etc.)

**Each test in GUIDE.md must have ALL of these:**
- **Internal prerequisites** — which scripts must be typed first to compile
- **External prerequisites** — exact scene setup: which GOs, which components, what to assign in inspector
- **NOT required** — explicitly list which OTHER systems are NOT needed (proves LEGO independence)
- **Controls** — keyboard shortcuts for manual testing
- **Checklist** — pass/fail items to verify

**Each test is a standalone scene.** You should be able to create a fresh empty scene, follow the external prerequisites, and the test works without any other system present. If a test requires another system to run, it's not independent — fix the architecture.

**Each Test script (.cs) must contain:**
- **Summary comment** with: prerequisites, NOT required, "How to test" step-by-step (what to do + what to expect), controls
- **`// purpose:` one-liner** on every `Raise` call and `+=` subscription inside the test
- **Console logging** via GameEvents subscription — proves the system fired the right events without manual visual inspection
- **Minimal code** — the test script is a bootstrap. The actual system handles its own input/logic in its own `Update()`. The test just: locks cursor, provides sim keys (M/N for menu), subscribes to events for logging.
- **No duplicate logic** — never re-implement the system's behavior inside the test. If the test needs to trigger something, fire a GameEvent (e.g. `RaiseToolPickupRequested`) rather than calling methods directly.

----

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

----

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

---

## For Future Agents

This section captures hard-won decisions from Phase A + B implementation. Read before building any phase.

### Always Do First
1. **Read the original source file** in `Scripts/Assembly-CSharp/` before writing any script. Match the behavior 100%.
2. **Read GOAL.md** (this file) for architecture rules + naming conventions.
3. **Read the target phase section in PhaseMap.md** for file list + modifications table.
4. **Check existing phases** for patterns — Phase A (shop) and Phase B (player/inventory) are the reference implementations.

### Splitting Rules
- **Split when one sentence isn't enough.** Original `PlayerController.cs` (888 lines) → 4 scripts (Movement, Camera, Grab, Outline). Each fits one sentence.
- **Don't split when the file is small + single purpose.** `ShopTerminal.cs` (22 lines) stays as-is. `ToolMiningHat.cs` (20 lines) stays as-is.
- **Original god-objects WILL need splitting.** `ComputerShopUI` → ShopUI + ShopUIOrchestrator + ShopDataService. `PlayerController` → PlayerMovement + PlayerCamera + PlayerGrab + PlayerOutline. Expect similar splits for future large files.

### Inheritance
- **Keep the original inheritance chain** unless it's genuinely unnecessary. `BasePhysicsObject → BaseSellableItem → BaseHeldTool → ToolPickaxe` exists because Phase C's `OrePiece` shares `BaseSellableItem`. Don't flatten chains that future phases depend on.
- **Interfaces go in 2-Data/Interface/.** Stub them if the full implementation comes in a later phase (e.g. `ISaveLoadableObject` stub in Phase B, expanded in Phase G).

### Cross-Phase Pattern
- **`GameEvents`** — always use `partial class`. Each phase adds events in its own `0-Core/GameEvents.cs`. Phase A's file needs the `partial` keyword.
- **MonoBehaviours** — when modifying existing scripts (adding `[SerializeField]`, changing method body), document it in the GUIDE.md modifications table: `| File | How | Change | Why |`
- **Prefer extending over modifying.** If you can add behavior via GameEvents subscription instead of editing an existing script, do that.

### The User's Coding Style
**Read `learn/phase-a(hand typed)/` to see the ACTUAL code.** Match this style, not generic C#.

Conventions:
- `#region` blocks (not `// ───` comments)
- `W` prefix for DataWrappers (`WShopItem`, not `ShopItem`)
- `DOC__` prefix for Dictionary lookups (`DOC__category_wShopItem`, `DOC__CartItem__Field`)
- `ALL_CAPS` for List/collection fields (`CATEGORY`, `ITEM_DEF`, `CARTITEM`)
- `.Ins` for Singleton access (not `.Instance`)
- `// purpose:` one-liner on every `.Raise...()` call and every `+=` subscription
- `// when X >>` and `// << when X` comment blocks around GameEvents in GameEvents.cs
- No blank lines between `#endregion` and next `#region`
- Least possible public API. Least possible private methods. Don't over-fragment.
- The user types every script by hand — keep files concise.

SPACE_UTIL extensions the user has (use these, don't reinvent):
- `.map()` — same as `.Select()` from LINQ
- `.find()` — same as `.FirstOrDefault()`
- `.all()` / `.any()` — same as LINQ `.All()` / `.Any()`
- `.gc<T>()` — same as `.GetComponent<T>()`
- `.destroyLeaves()` — destroys all children of a Transform
- `.toggle(value)` — same as `.SetActive(value)`
- `.colorTag("color")` — wraps string in rich text color tag for Debug.Log
- `.formatMoney()` / `.formatMoneyShort()` — `$"${amount:#,##0.00}"` / `$"${amount:#,##0.##}"`
- `.parseInt()` — parses string to int
- `.getRandom()` — random element from list
- `.repeat(n)` — repeats a char n times
- `.ToNSJson(pretify: true)` — Newtonsoft JSON serialization
- `C.method(this)` — logs `[ClassName.MethodName]` for debug
- `INPUT.K.InstantDown(KeyCode)` — same as `Input.GetKeyDown`
- `INPUT.UI.SetCursor(isFpsMode)` — cursor lock/visibility
- `LOG.AddLog(string, "json")` — writes to persistent log file

### Tight Coupling Red Flags — NEVER Do These
The user WILL push back hard if you do any of these. Catch them before the user does.

```
❌ FindObjectOfType<AnyType>()          → use [SerializeField], Owner chain, or GameEvents
❌ Singleton<X>.Ins.DoSomething()        → fire GameEvents.Raise...(), let X subscribe
   (Exception: Singleton reads for queries like EconomyManager.Ins.GetMoney() are OK)
❌ Script A calls Script B's method directly (cross-system)
                                         → fire GameEvents, B subscribes
❌ Tool calls FindObjectOfType<InventoryOrchestrator>()
                                         → tool fires GameEvents.RaiseToolPickupRequested(this)
❌ MonoBehaviour accesses another MonoBehaviour via FindObjectOfType
                                         → use [SerializeField] inspector ref or Owner.GetComponent<>()
```

### Public API Obsession — The User's #1 Priority
**Before making ANY method public, ask: "does another script ACTUALLY call this?"**
If no → it's private. If only subclasses → it's protected. If only same class → inline it.

```
❌ WRONG: public void Release() on PlayerGrab
   → nobody calls it externally. Grab state is via GameEvents. Make it private.

❌ WRONG: public void TryAddTool() on InventoryOrchestrator
   → only called from HandleToolPickup event handler. Make it private.

❌ WRONG: public void ToggleLight() on ToolMiningHat
   → only called from own OnEnable/OnDisable. Make it private.

❌ WRONG: public GameObject WorldModel on BaseHeldTool
   → only this class and subclasses use it. Make it [SerializeField].

✅ RIGHT: public void Init() on Orchestrator
   → SubManager.Start() calls it. Genuinely external.

✅ RIGHT: public virtual void PrimaryFire() on BaseHeldTool
   → InventoryOrchestrator calls active.PrimaryFire(). Genuinely external.

✅ RIGHT: public float GetMoney() on EconomyManager
   → multiple scripts query money. Genuinely external.
```

**After writing every script, audit: can any public method be made private/protected?**

### Splitting Judgment — Real Examples

```
SPLIT — ShopUI (260 lines doing 4 things):
  → ShopUI (SubManager: toggle only, 40 lines)
  → ShopUIOrchestrator (wire Field_, 150 lines)
  → ShopDataService (collections, 45 lines)
  → ShopCartService (cart math, 55 lines) — merged into ShopDataService by user
  WHY: 4 distinct responsibilities. SubManager shouldn't wire UI. Data shouldn't need Unity.

SPLIT — PlayerController (888 lines doing 10 things):
  → PlayerMovement (WASD, jump, slope, duck)
  → PlayerCamera (look, FOV, bob)
  → PlayerGrab (SpringJoint, rope)
  → PlayerOutline (FresnelHighlighter)
  WHY: each fits one sentence. Camera bob has nothing to do with grab physics.

DON'T SPLIT — ToolMagnet (108 lines):
  → manages List<Rigidbody> + SpringJoints, but every operation needs Unity physics
  → DataService would be an empty list wrapper — no testable logic
  WHY: data is inseparable from Unity physics. One file, one purpose.

DON'T SPLIT — ShopTerminal (22 lines):
  → implements IInteractable, fires one GameEvent
  WHY: already one sentence. Splitting would create 2 files with 11 lines each.

DON'T SPLIT — ToolMiningHat (20 lines):
  → toggles a light on enable/disable
  WHY: trivially small, one purpose.
```

### Self-Init Lifecycle Pattern (SubManagers)
Every UI SubManager follows this exact pattern. Don't deviate.

```csharp
// SubManager starts ACTIVE in scene (required for Start to run)
private void OnEnable()
{
    // purpose: cursor lock/unlock for player controller
    GameEvents.RaiseMenuStateChanged(isAnyMenuOpen: true);
}
private void Start()
{
    // build data, init orchestrator
    dataService.Build(categories);
    orchestrator.Init(dataService, categories);
    orchestrator.BuildView();
    // subscribe to open event, then disable self
    GameEvents.OnOpenThisPanel += () => this.gameObject.SetActive(true);
    this.gameObject.SetActive(false); // ← self-disable after init
}
private void Update()
{
    // close on ESC/E only
    if (Input.GetKeyDown(KeyCode.Escape)) gameObject.SetActive(false);
}
private void OnDisable()
{
    GameEvents.RaiseMenuStateChanged(isAnyMenuOpen: false);
}
```

**Key facts:**
- OnEnable fires BEFORE Start (first frame — brief true→false pulse, acceptable)
- Start runs ONCE — never again on re-enable
- OnEnable fires EVERY re-enable
- UIManager never calls SetActive on SubManagers — they own their own lifecycle

### UIManager Hybrid Decision
UIManager exists because **priority blocking** requires centralized knowledge:

```
World trigger (terminal)  → GameEvents → panel self-enables       (decoupled)
Keyboard (Q/Tab)          → UIManager checks priority → toggles   (centralized)
ESC                       → UIManager closes panels with logic    (centralized)
Cursor/Blur               → UIManager reads panel states          (centralized)
```

UIManager for Phase A = just `IsInAnyMenu()`. Grows in Phase F+ when Q/Tab/ESC priority logic arrives.

### What's Been Proven (Phase A + B)
- ✅ DataService tested via plain C# instance (`DEBUG_Check`, `DEBUG_CheckB`)
- ✅ Orchestrator pattern (shop UI + inventory)
- ✅ SubManager self-init pattern (ShopUI, InventoryUI, BgUI)
- ✅ `partial` GameEvents across phase folders
- ✅ Field_ display-only components
- ✅ Vertical slice tests per system (7 tests across 2 phases)
- ✅ Player split from 888-line god-object into 4 focused scripts
- ✅ Tool inheritance chain (BasePhysicsObject → BaseSellableItem → BaseHeldTool → concrete tools)

### What's NOT Been Tested Yet
- ❌ `3-MonoBehaviours/` with domain subfolders (expect ~80+ files by Phase E)
- ❌ Cross-DataService communication (Phase F: quests unlock shop items)
- ❌ Scene switching + `OnDestroy` cleanup (Phase I)
- ❌ Save/load serialization of DataService state (Phase G)
- ❌ `SettingsManager` singleton reads replacing hardcoded values (Phase H)
