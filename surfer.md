# Surfer — Critical Thoughts Log

> This file captures my critical thinking for each prompt — what I considered, why I made certain choices, and what I deliberately avoided. Appended with each new request.

---

## Prompt 1: "Analyse the entire source and provide detailed architecture documentation"

**What I thought:**
- The `Scripts/` folder has ~130 subfolders but only `Assembly-CSharp/` (~200 files) is actual game code — the rest are Unity engine and third-party DLL decompilations. Focused there exclusively.
- Read the largest/most connected files first (SavingLoadingManager at 853 lines, PlayerController at 888 lines) to map the dependency graph before reading leaf scripts.
- The codebase uses Singletons everywhere — I considered whether to frame this as a flaw. Decided it's pragmatic for this scope and said so honestly rather than dogmatically criticizing it.
- `FindObjectOfType` calls in runtime paths (BaseHeldTool.DropItem, etc.) are a genuine perf issue worth calling out. LINQ in hot paths (CastingFurnace) is another.
- The PlayerController is a textbook God Object but splitting it would be a refactor suggestion, not a bug. Framed it as a learning opportunity.

---

## Prompt 2: "Create learn folder with the .md file"

**What I thought:**
- Straightforward file creation. Saved a memory so I always route future generated files to `learn/`.
- No architectural decisions needed.

---

## Prompt 3: "Build phase-a from scratch — interaction + shop cart system"

**What I thought:**
- Scoped to just 2 systems: world interaction (raycast + IInteractable) and shop/cart UI. Resisted adding inventory, save/load, or mining — those are future phases.
- Used Unity's built-in `UnityEngine.UI` (Text, Button, Image, InputField) instead of TextMeshPro to keep .NET 2.0 compatible and avoid extra dependency.
- Avoided `$""` string interpolation, `?.` null-conditional, `async/await`, and expression-bodied members for .NET 2.0 compat. Used `string.Format` and explicit null checks throughout.
- Created `GameEvents` as a static event bus rather than making every system subscribe to specific manager events — this is the right level of decoupling for this scope without introducing a full message broker.
- `ShopUI` passes `this` to `ShopItemButton.Initialize()` — I considered whether this is tight coupling. Decided it's acceptable because they're the same UI subsystem (parent-child). Decoupling this with events would be over-engineering.
- Made `ShopCategory` a ScriptableObject (original uses a plain `[Serializable]` class inside a list). ScriptableObject is cleaner for the editor workflow and more idiomatic for a learning project.
- `ShopItem` is a runtime class, not a ScriptableObject — correct, because it carries mutable state (lock, purchase count) that differs per session.
- Included `ShopSpawnPoint` with gizmo drawing — small touch but important for scene setup discoverability.
- The GUIDE.md is long but necessary. A from-scratch learner needs exact "create this GameObject, add this component, wire this field" instructions. Didn't shortcut it.

---

## Prompt 4: "Make sure no tight coupling — use events/GameEvents where needed"

**What I thought:**
- Audited all 17 scripts. Found exactly 2 real coupling issues:
  1. `ShopTerminal` directly reaching into `UIManager.ShopUI.gameObject.SetActive()` — crosses 2 system boundaries
  2. `SimplePlayerController` polling `UIManager.IsInAnyMenu()` every frame — unnecessary direct dependency
- Everything else is either read-only singleton queries (acceptable), same-subsystem references (necessary), or one-time setup reads (fine).
- Resisted the urge to event-ify `ShopUI → EconomyManager.AddMoney()`. That's a legitimate command — the shop *must* call the economy system to deduct money. Wrapping it in an event would add indirection with zero benefit.
- Resisted event-ifying `ShopItemButton → ShopUI` reference. They're parent-child UI components. Forcing them through GameEvents would make the code harder to follow for a learner.
- The `OnToggleShopRequested` event pattern is clean: ShopTerminal fires, ShopUI subscribes. Neither knows the other exists. If a second shop terminal existed, it'd work automatically.
- `OnMenuStateChanged` with cached `_anyMenuOpen` bool eliminates per-frame singleton access in the player controller. Simple, effective, no over-engineering.

---

## Prompt 5: "Create surfer.md with critical thoughts, keep appending for future prompts"

**What I thought:**
- This is a meta-documentation request. The user wants transparency on my reasoning process.
- Kept entries concise — bullet points of what I actually weighed, not a rehash of the implementation.
- File goes in `learn/` (not `learn/phase-a/`) since it's cross-phase.
- Need to save a memory so I remember to append to this file on every future prompt.

---

## Prompt 6: "Does GUIDE.md cover everything? No tight coupling confirmed? What does it look like? Is it LEGO-linkable?"

**What I thought:**
- Re-read the entire 630-line GUIDE.md end-to-end looking for gaps. Found 3:
  1. Section 9 still showed the OLD coupling patterns (pre-fix code). Updated all 4 examples + added the full event flow diagram.
  2. Missing EventSystem note — Unity UI buttons silently fail without it. A learner would be stuck. Added it.
  3. ScrollRect setup was hand-waved ("Inside a ScrollRect"). A learner wouldn't know how to wire Content/Viewport. Added step-by-step.
- Coupling is confirmed clean. Reviewed the event flow: ShopTerminal→GameEvents→ShopUI, ShopUI→GameEvents→PlayerController, EconomyManager→GameEvents→MoneyDisplay. No script crosses system boundaries.
- "What does it look like?" — The user needs a mental picture before building. Added Section 10 with an ASCII walkthrough of the exact gameplay flow: walk to terminal, press E, browse, cart, purchase, items spawn, money updates.
- "LEGO-linkable?" — This is the most important architectural question. Added Section 11 with a concrete table: every Phase A extension point, what future phases plug into it, and what changes (answer: almost nothing). Included code examples of Phase B (OreNode) and Phase C (QuestManager) showing zero changes to Phase A files.
- The golden rule I articulated: "Adding a new phase = adding new files + new ScriptableObject data. Existing scripts rarely change, and when they do, it's a one-liner." This is the Open-Closed Principle in practice without naming it that way — a learner absorbs the pattern without the jargon.
- Resisted adding a Phase B preview with actual code. The user asked about linkability, not implementation. Kept it conceptual with just enough code to prove the point.

---

## Prompt 7: "What's the chance (0-100%) that I as a pro dev can produce a complete build?"

**What I thought:**
- Gave 95%. The 5% risk is purely Unity editor UX friction (UI anchoring, ScrollRect wiring, missed inspector assignments) — not conceptual.
- Considered 100% but rejected it. Unity UI anchoring genuinely trips up even experienced devs. Honesty over hype.
- Framed the real question as "how fast, not if" — estimated 2-3 hours. The code is done; remaining work is editor clicking.

---

## Prompt 8: "How does the main source handle lots of physics objects without slowing?"

**What I thought:**
- Three techniques, not one. Important to explain them as a coordinated system, not isolated tricks.
- **Object Pooling** (`OrePiecePoolManager`) is the biggest win — eliminates Instantiate/Destroy GC pressure. The composite key `(ResourceType, PieceType, IsPolished)` is clever because it avoids resetting mesh/material on reuse.
- **Self-regulating throttle** (`OreLimitManager`) is the most architecturally interesting. It only counts non-sleeping rigidbodies — this is the key insight. A pile of 500 ore sitting still costs nothing. Only moving objects matter.
- **Amortized cleanup** (`OreManager.Update` checking one ore per frame) is subtle but important. Prevents orphan accumulation without ever doing an O(N) scan in a single frame.
- The user asked "what technique" (singular) but the real answer is three techniques working in concert. Each alone isn't enough — pooling without throttling still crashes at high counts, throttling without pooling still gets GC spikes, cleanup without pooling still leaks.
- Didn't mention Rigidbody.Sleep() explicitly as a "technique" because Unity does that automatically — but it's critical context for understanding why OreLimitManager only counts non-sleeping bodies.

---

## Prompt 9: "Goal is 100% source fidelity + decoupling. Add a diff section to each phase's GUIDE.md"

**What I thought:**
- This crystallizes the project rules: match the original source behavior exactly, but swap tight coupling for GameEvents/interfaces where the original used direct Singleton calls.
- The diff table (Section 12 in GUIDE.md) is the key deliverable. 10 rows covering every deviation: what original did, what we did, why. Plus a "What Stayed the Same" list proving we didn't change things unnecessarily.
- Updated the memory to capture all 5 rules: source fidelity, decoupling, Guide.md diff section, learn/phase-X folder, .NET 2.0 + summaries. This ensures consistency across all future phases.
- The diff section is educational — a learner sees both approaches side by side and understands the tradeoff. It's not just documentation; it's a teaching tool.

---

## Prompt 10: "Prefix all ScriptableObject file and class names with SO_"

**What I thought:**
- 3 ScriptableObjects in Phase A: `Interaction`, `ShopItemDefinition`, `ShopCategory`. All need renaming to `SO_Interaction`, `SO_ShopItemDefinition`, `SO_ShopCategory`.
- `ShopItem` is NOT a ScriptableObject (it's a plain `[Serializable]` runtime class) — correctly left it unchanged.
- Ripple effect: 10 files reference these classes. Updated all of them: IInteractable, InteractionSystem, InteractionWheelUI, ShopTerminal, ShopItem, ShopManager, ShopUI, ShopCategoryButton. ShopItemButton and ShopCartItemButton only reference `ShopItem` (not a SO), so untouched.
- Can't delete old files via tools. User needs to manually delete `Interaction.cs`, `ShopItemDefinition.cs`, `ShopCategory.cs` — the old non-prefixed versions.
- The SO_ prefix is a good convention — instantly tells you a class is data-only (ScriptableObject) vs runtime behavior (MonoBehaviour). Prevents confusion in large projects where both exist for the same concept (e.g., `SO_ShopItemDefinition` = data, `ShopItem` = runtime state).
