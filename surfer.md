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

---

## Prompt 11: "You haven't updated GUIDE.md for the new SO_ names — do a full sweep"

**What I thought:**

- My mistake — renamed scripts but missed GUIDE.md. This is exactly the kind of inconsistency that trips up a learner following the guide.
- Did a systematic grep for every old name across the entire 869-line GUIDE.md. Found references in 8 sections: folder structure, script reference tables (4 tables), ScriptableObject creation steps, wiring checklist, LEGO-linkability table, diff section, and "What Stayed the Same" list.
- Fixed ~15 individual references across all sections. Added a new row to the Section 12 diff table documenting the SO_ naming convention as a deliberate deviation from the original source.
- Updated the memory to include rule #7: "Always update GUIDE.md when renaming or changing any file/class." This prevents this class of error in future phases.
- The user's reminder about the full convention set (source fidelity + decoupling + naming + Guide.md diff) confirmed these are now locked-in project rules. Memory updated to capture all 7 rules.

---

## Prompt 12: "Does Phase A include grabbing/inventory? Which phases? Create PhaseMap.md"

**What I thought:**

- Phase A explicitly does NOT include grabbing (SpringJoint + LineRenderer) or inventory (hotbar + tool switching). Those are Phase B — the player controller foundation that everything else builds on.
- Planned 10 phases (A-J) by mapping every original source file to a logical group. The key constraint: each phase must be self-contained and testable. You should be able to run the game after each phase and see something working.
- **Phase ordering logic:** A (interaction+shop) → B (player+tools) → C (ore+mining) → D (buildings+conveyors) → E (machines) → F (quests+research) → G (save/load) → H (sound+settings) → I (contracts+menus) → J (debug+polish). Each phase depends on the ones before it.
- Importance percentages based on how much of the final game each phase represents. Phase E (machines) is the biggest at 18% because the factory pipeline is ~30 scripts and the core gameplay loop.
- Total file count: ~197 — closely matches original source's ~200 scripts. This validates the phase plan covers everything.
- The dependency chain diagram is crucial — shows that C/D/E form a linear factory pipeline, while F (quests) can start after B and benefits from C-E existing. G (save/load) needs everything to exist before it can serialize them.
- Included "What It Looks Like" ASCII descriptions for every phase — the user asked for this in Phase A and it was valuable, so I carried it forward.
- Made the PhaseMap.md live in `learn/` root (not inside any phase folder) since it's cross-phase — same reasoning as surfer.md.

---

## Prompt 13: "Does Phase A include grabbing? What about holding items, building preview, all those cases?"

**What I thought:**

- Listed 8 distinct interaction cases from the original source (SpringJoint grab, tool pickup, hotbar switching, view models, ghost preview, delayed swing, magnet multi-grab, mining hat light). All traced to specific lines in the original source. All mapped to Phase B.
- The user is testing whether the phase plan actually accounts for everything. It does — Phase B is deliberately the largest and hardest phase because the player controller + inventory + tools is the foundation for everything else.

---

## Prompt 14: "The game has a mine environment with elevator. Should this be its own phase?"

**What I thought:**

- The user correctly identified that Phase A's flat plane doesn't match the original game's enclosed mine environment with the descending elevator.
- Read `StartingElevator.cs` (Perlin noise shake, landing particle, roof collider) and `MainMenuElevator.cs` (idle sway, accelerating drop). Both are small self-contained scripts.
- Created **Phase A½ — "The Mine"** rather than Phase A-0 or Phase 0. Half-phase naming signals it's small (3%, Easy) but important for visual context. It transforms the flat plane into the actual game environment.
- Deliberately scoped it: no main menu scene (Phase I), no scene loading (Phase G/I). Just the physical mine room + elevator descent + repositioning Phase A objects into the environment.
- This is mostly **scene work** (ProBuilder/terrain, lighting, colliders) with only 2 new scripts. That's why it's Easy difficulty.

---

## Prompt 15: "Create phase-a-1 source. Is elevator .anim or DOTween?"

**What I thought:**

- **Neither .anim nor DOTween.** Read both `StartingElevator.cs` and `MainMenuElevator.cs` — both are pure code in `Update()`. Manual `localPosition.y` manipulation with `Mathf.Lerp` for speed, `Mathf.InverseLerp` for progress, and `Mathf.PerlinNoise` for shake. This is deliberate — no animation asset dependency, fully parametric.
- Renamed `MainMenuCameraShaker` → `CameraShaker` because the original name ties it to the menu scene. Our version is generic and reusable (gameplay camera, explosion shakes in future phases).
- The `StartingElevator` references `SimplePlayerController` (Phase A) not `PlayerController` (Phase B). Teleport logic uses the CharacterController disable/set/enable pattern since `SimplePlayerController` doesn't have a `TeleportPlayer()` method yet.
- Added `GameEvents.OnElevatorLanded` — the original source doesn't have this event (just sets `_isLowering = false`). This is our decoupling improvement: future systems can react to landing without touching elevator code.
- Also added `OnGamePaused`/`OnGameUnpaused` to GameEvents. The original `StartingElevator` subscribes directly to `GameManager.GamePaused` singleton events. We route through GameEvents instead.
- Sound is stubbed out (placeholder comments). The original uses `SoundPlayer.PlaySound(LoweringSoundDefinition)` but we have no sound system until Phase H. Methods exist as stubs for future wiring.

---

## Prompt 16: "Which phase for main menu, options UI, quest tutorial?"

**What I thought:**

- Quick lookup against PhaseMap.md — no new work needed, just answered from the existing plan. Main menu = Phase I, settings UI = Phase H, quests = Phase F.
- Explained the ordering logic: quest tutorial can't exist before the things it teaches (mining C, buildings D, machines E). Main menu can't work without save/load (G) and settings (H).

---

## Prompt 17: "Phase H modifies player movement? Include all cross-phase modifications in PhaseMap"

**What I thought:**

- Clarified: Phase H doesn't change movement logic — it changes where parameter VALUES come from (SettingsManager instead of hardcoded SerializeField) and swaps KeyCode → Input System for rebinding. Same physics, different config source.
- The user's real request: PhaseMap.md should document every file modification across all phases, not just new files. This is a completeness requirement — the roadmap should cover 100%.
- Added "Modifications to Earlier Phases" subsection to every phase B through J. Each has a table: File (with source phase) | Change | Why.
- Phase G (save/load) has the most modifications (9 files across 5 prior phases) because serialization touches everything.
- Phase H has 8 modifications — mostly parameter reads + Input System swap + sound additions.
- Phase E has zero modifications — all machines are self-contained. This validates the architecture.
- The cross-phase modification tables are the most valuable addition to PhaseMap.md. They answer "what existing code do I need to touch?" before entering any phase.

---

## Prompt 18: "OnEnable/Awake ordering conflict — how to handle event subscriptions?"

**What I thought:**

- The user identified a real Unity pitfall: `Awake()` and `OnEnable()` execution order across GameObjects is not guaranteed. If Script A subscribes to GameEvents in `OnEnable()` and the handler reads `Singleton<B>.Instance`, but B's `Awake()` hasn't run yet, you get a null ref.
- Established a clear lifecycle rule:
 - `Awake()` — singleton registration, `GetComponent` caching only
 - `Start()` — event subscriptions for permanent objects (all Awake done by now). Pair with `OnDestroy()`.
 - `OnEnable()/OnDisable()` — event subscriptions ONLY for toggled objects (UI panels). Pair together.
- Fixed `StartingElevator.cs` — moved `GameEvents.OnGamePaused` subscription from `OnEnable→Start` and unsub from `OnDisable→OnDestroy`. The elevator is never toggled, so it should use the permanent pattern.
- Checked all existing Phase A scripts: `ShopUI` correctly splits between `Start()` (toggle event — needs to hear it even when inactive) and `OnEnable()` (refresh events — only when visible). `SimplePlayerController` uses `Start()` — correct. `MoneyDisplay` uses `OnEnable/OnDisable` — acceptable since it's a UI element.
- This rule should be applied consistently in all future phases. Didn't save a memory for it because it's a standard Unity best practice, not a project-specific convention.

---

## Prompt 19: "Implement Handle/Toggle/Refresh naming convention across phase-a and phase-a-1"

**What I thought:**

- Audited all 6 event handlers across 4 scripts. Every `On...` prefixed handler was a leftover from initial implementation — renamed them all to match the convention the user liked:
 - `Handle...` for state-change reactions (HandleMenuStateChanged, HandleMoneyChanged, HandleGamePaused, HandleGameUnpaused)
 - `Toggle...` for on/off switching (ToggleShop)
 - `Refresh...` for UI repopulation (RefreshOnItemUnlocked)
- The `On...` prefix is now reserved exclusively for Unity lifecycle methods (OnEnable, OnDisable, OnDestroy) and GameEvents event declarations (OnMoneyChanged, OnMenuStateChanged). Handler methods that subscribe to those events use Handle/Toggle/Refresh.
- Verified subscription lifecycle patterns were already correct from the previous fix. No additional Start/OnEnable changes needed — just the method names.

---

## Prompt 20: "So Awake/OnEnable/Start are all handled now right?"

**What I thought:**

- Confirmation question. Built a complete table showing every script's lifecycle usage across all 5 lifecycle hooks (Awake, Start, OnEnable, OnDisable, OnDestroy). This serves as a reference for the user and a verification that nothing was missed.
- All patterns are consistent: Singleton.Awake for registration, Start for permanent subs, OnEnable/OnDisable for toggled UI subs, OnDestroy for cleanup.

---

## Prompt 21: "Include everything into surfer.md as said — now and always"

**What I thought:**

- I had fallen behind on surfer.md entries — prompts 19 and 20 were missing. The user's reminder is valid.
- Caught up by appending entries for prompts 19, 20, and this one (21).
- The memory already exists for this (Prompt 5), but the user is reinforcing it. No new memory needed — just need to be more disciplined about appending after every response, not batching them.

---

## Prompt 22: "No 'What It Looks Like When Running' in phase-a-1 GUIDE.md — this is important"

**What I thought:**

- Valid miss. Phase A's GUIDE.md has Section 10 "What Phase A Looks Like When Running" and PhaseMap.md has "What It Looks Like" for every phase — but phase-a-1's GUIDE.md was missing it.
- This is now a mandatory section for every phase's GUIDE.md going forward. The user explicitly said it's important for knowing exactly what the scene should look like when implementation is done.
- Added a detailed ASCII walkthrough covering the full sequence: dark screen → elevator descent with shake → shake fading → landing particle → settling → mine room reveal → shop terminal still works → items spawn at repositioned points.
- Updated the Table of Contents to include the new section at the top (before numbered sections) since it's the first thing you'd want to read.
- Rule for all future phases: GUIDE.md must always have "What Phase X Looks Like When Running" section. PhaseMap.md already has "What It Looks Like" for every phase — GUIDE.md needs a more detailed version of the same thing.

---

## Prompt 23: "Include detailed 'What It Looks Like' in PhaseMap.md for all phases too"

**What I thought:**

- PhaseMap.md had short 3-5 line descriptions for phases C through J. Phase A, A½, and B already had detailed ones. Expanded all remaining phases to the same detail level.
- For each phase, described the exact user experience step-by-step: what you see, what you do, what happens. Referenced the original source for accuracy (e.g., CastingFurnace majority-type smelting logic, OreLimitManager tiered throttling, SavingLoadingManager atomic write process).
- Phase E got the longest expansion — it's the factory pipeline walkthrough showing every machine in sequence. This is the core gameplay loop that the user needs to visualize.
- Phase F describes the full progression loop: Mine → Sell → Quest → Unlock → Build → Research → Repeat.
- Phase G details exactly what gets saved (11 bullet points) and the load process.
- Phase H lists every settings slider/toggle and the keybind rebinding flow.
- Phase I covers contracts workflow + all world objects + full main menu flow.
- Phase J covers the hidden dev mode with all debug keys listed.
- These descriptions may change as implementation reveals new details — that's expected. They're the current best plan from the original source.

---

## Prompt 24: "Create GOAL.md — high-level project goal and rules, similar to CLAUDE.md"

**What I thought:**

- The user wants a single source-of-truth file that defines "what are we doing and how" — similar to how Claude Code uses CLAUDE.md as a project instruction file.
- Consolidated all conventions, rules, and patterns accumulated across 23 prompts into one file: source fidelity, decoupling, SO_ naming, handler naming (Handle/Toggle/Refresh), lifecycle pattern (Awake/Start/OnEnable), .NET 2.0 compat, per-method summaries, GUIDE.md requirements, PhaseMap.md maintenance, surfer.md logging.
- Included a phase overview table (quick reference) and file map showing the learn/ folder structure.
- Added a "How To Use This" section at the bottom — gives a new reader (or future-me) the exact workflow: read GOAL → read PhaseMap → enter phase folder → follow GUIDE → test → next phase.
- Placed at `learn/GOAL.md` (root level, alongside ARCHITECTURE.md, PhaseMap.md, surfer.md) since it's the entry point for the entire project.

---

## Prompt 25: "Read and analyze entire project — refer to GOAL.md, PhaseMap.md, surfer.md"

**What I thought:**

- This is an onboarding/context-load request. The user wants me to internalize the full project state before future work begins. Read every key doc: GOAL.md, PhaseMap.md, ARCHITECTURE.md, surfer.md, both phase GUIDE.md files, and the original source file listing.
- Project is a phased rebuild of MINEMGL (a Unity first-person mining/factory sim) from ~200 decompiled scripts. Two phases done (A, A½), eight remaining (B through J).
- Key rules are locked in: source fidelity, decoupling via GameEvents, SO_ prefix, Handle/Toggle/Refresh naming, lifecycle patterns (Awake/Start/OnEnable), .NET 2.0 compat, one-liner XML summaries, GUIDE.md structure requirements, PhaseMap.md maintenance, surfer.md logging.
- Phase A delivered 20 scripts (interaction, shop, economy, UI, simple player controller). Phase A½ added 2 scripts (StartingElevator, CameraShaker) plus GameEvents modifications.
- Next up is Phase B (Player Controller + Inventory + Tools + Grabbing) — the biggest and hardest phase at 15% weight, ~25 scripts. This replaces SimplePlayerController with the full 888-line PlayerController equivalent.
- The original source has ~269 .cs files in Assembly-CSharp/. The PhaseMap accounts for ~197 scripts, which is close — the delta is likely utility/helper classes and Unity-generated files.
- No questions needed — the documentation is thorough and internally consistent. Ready to proceed with whatever phase the user wants next.

---

## Prompt 26: "Which phases can run in parallel agents? Add to PhaseMap.md"

**What I thought:**

- Traced the full dependency graph. Only 2 parallel windows exist: Wave 4 (C ∥ F) and Wave 9 (I ∥ J). Everything else is strictly sequential due to data/system dependencies.
- C ∥ F works because both only need B. F "benefits from" C-E but doesn't structurally depend on them — quest code can be written against event contracts without the actual ore/machine implementations. Noted the stub-testing caveat.
- I ∥ J works because their file modifications don't overlap at all. I touches UIManager/StartingElevator/PauseMenu; J touches PlayerController/EconomyManager/ShopUI. Clean split.
- Identified merge conflicts: Wave 4 shares `GameEvents.cs` (both add independent event declarations — trivial merge). Wave 9 has no shared files.
- C → D → E cannot be parallelized — each layer physically depends on the previous (conveyors need ore, machines need conveyors). G is a serialization bottleneck. H → I/J is the only late-stage parallelism.
- Added the visual ASCII timeline to make the wave schedule immediately scannable. The critical path drops from 11 sequential phases to 9 waves with 2 agents.
- Also fixed the dependency chain diagram — it was missing Phase A½ and showed I/J as sequential when they can actually be parallel.

---

## Prompt 27: "What's the Fresnel highlighter? Isn't it just an outline?"

**What I thought:**

- Read `FresnelHighlighter.cs` (151 lines) and `HighlightStyle.cs` (50 lines) from the original source. It's NOT an outline — it's a Fresnel rim glow using a custom shader injected via CommandBuffer.
- Key distinction: outlines are geometric (inverted hull or edge detection), Fresnel is a dot-product effect on surface normals. Edges glow, flat faces stay transparent.
- The shader `Hidden/Focus/FresnelAdd` uses additive blending (One+One), no depth write, configurable ZTest for x-ray mode. 4 properties: \_Color, \_Power, \_Intensity, \_ZTest.
---

## Prompt 28: "What kind of shader does it use?"

**What I thought:**

- Found the compiled shader JSON at `Assets/Shader/Hidden_Focus_FresnelAdd.json`. Shader is baked (`m_ShaderIsBaked: true`), so actual HLSL isn't available — but all metadata is.
- Reconstructed the shader logic from properties, render state, and constant buffer layout. Fragment shader does `pow(1 - saturate(dot(V, N)), _Power) * _Color * _Intensity` blended additively.
- For Phase B, we need to write this shader from scratch (~20 lines ShaderLab) or use Highlight Plus instead.

---

## Prompt 29: "How about using Highlight Plus package instead?"

**What I thought:**

- Highlight Plus (Kronnect) is a perfect fit — inner glow mode replicates the Fresnel rim exactly, with better URP support and zero shader code needed.
- The user then asked to keep the API identical (`_fresnel.ClearAll()`, `_fresnel.HighlightObject(obj, _fresnel.ToolPreset)`). Designed a wrapper class that maintains the exact same public API but delegates to HighlightEffect components internally.
- HighlightStyle struct stays identical to the original. FresnelHighlighter becomes a thin adapter.

---

## Prompt 30: "Implement entire Phase B"

**What I thought:**

- Largest phase: 25 scripts across 8 categories. Read all original source files first to ensure 100% source fidelity.
- Key decisions:
 - Used legacy `Input.GetKey/GetAxis` instead of Input System — Phase H adds rebinding. Same logic, different API.
 - Stubbed all SoundManager/SettingsManager/QuestManager/DebugManager calls — those systems don't exist yet. Logic is in place, just commented.
 - `BaseHeldTool._interactions` uses `List<SO_Interaction>` (our SO_ prefix convention) instead of original's `List<Interaction>`.
 - FresnelHighlighter is the Highlight Plus wrapper with commented-out HP calls — uncomment when package is imported.
 - Added `GameEvents.OnToolSwitched`, `OnItemPickedUp`, `OnItemDropped`, `OnToolPickedUp` to replace direct singleton calls the original uses for quest triggers.
 - Modified UIManager: added `IsInAnyMenuExceptInventory()`, `IsInInventory()`, `IsInEditTextPopup()`, `UpdateOnScreenControls()`, `ShowBuildingInfo()`, `HideBuildingInfo()`.
 - PlayerController caches `_anyMenuOpen` via GameEvents instead of polling `UIManager.IsInAnyMenu()` every frame — our decoupling improvement.
 - Preserved the original's `FindObjectOfType<PlayerInventory>()` in BaseHeldTool.DropItem() for source fidelity, even though it's a known perf issue. Documented in the diff section.
- ToolBuilder is partial — placement logic (grid snap, ghost, conveyor snap) completed in Phase D.
- ToolHammer and ToolSupportsWrench have raycast structure but BuildingObject calls are stubbed for Phase D.
- ToolResourceScanner has the identification raycast loop but specific type checks (OreNode, OrePiece, etc.) are stubbed for Phase C.