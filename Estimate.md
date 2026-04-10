# Estimate — Hand-Typing Timeline

> Based on Phase A taking ~3 days (~14 hours) including architecture design.
> Architecture is now locked (GOAL.md). Patterns repeat across phases.
> Schedule: 4 hours/weekday, 8 hours/weekend = ~36 hours/week.

---

## Per Phase

| Phase | Scripts | Difficulty | What's new | Est. Hours | Est. Days |
|-------|---------|-----------|-----------|-----------|-----------|
| **A** | 22 | Easy | Designed entire architecture: DataService, Orchestrator, SubManager, Field_, GameEvents, vertical slice tests | 14h | 3 days ✅ |
| **A½** | 3 | Easy | 2 MonoBehaviours + partial GameEvents + scene work | 3h | < 1 day |
| **B** | 35 | Hard | PlayerController split (888→4 scripts), inventory DataService, tool inheritance chain, 7 tools, partial GameEvents, zero FindObjectOfType | 28h | 6-7 days |
| **C** | ~22 | Medium | Object pooling (OrePiecePoolManager), weighted drops, IDamageable, OreDataService | 15h | 3-4 days |
| **D** | ~18 | Hard | Ghost preview, grid snap, conveyor snap detection, BuildingDataService | 18h | 4-5 days |
| **E** | ~25 | Medium | Repetitive — machines follow same trigger I/O pattern. Each is one MonoBehaviour. | 12h | 3 days |
| **F** | ~22 | Medium | QuestDataService, QuestOrchestrator, quest requirements (polymorphic) | 15h | 3-4 days |
| **G** | ~18 | Hard | SaveDataService, ISaveLoadable interface wiring across ALL phases | 22h | 5-6 days |
| **H** | ~16 | Easy | SoundManager, SettingsManager, SettingsOrchestrator — familiar patterns | 10h | 2-3 days |
| **I** | ~20 | Easy | ContractDataService — follows quest pattern. Main menu scene. | 10h | 2-3 days |
| **J** | ~12 | Easy | DebugManager, VersionManager — simple singletons | 6h | 1-2 days |
| **Total** | **~213** | | | **~153h** | |

---

## Why It's Faster Now (After Phase A + B Architecture)

| Advantage | Impact |
|-----------|--------|
| **Architecture locked** | No more multi-day design sessions. GOAL.md defines everything. |
| **Patterns repeat** | Phase E machines = same as Phase C. Phase I contracts = same as Phase F quests. Just different data. |
| **DataService first** | Type DataService → test with `DEBUG_Check` (plain C# `new` instance) → then wire UI. Bugs caught early. |
| **Orchestrator pattern** | Every UI system follows same: SubManager (toggle) → Orchestrator (wire Field_) → DataService (data). Copy the shape, change the content. |
| **partial GameEvents** | Each phase adds events in its own `0-Core/GameEvents.cs`. No merge conflicts. No modifying Phase A. |
| **OwnerCamRay / protected helpers** | Tools are ~10 lines each. Base class does the heavy lifting. |
| **Vertical slice tests** | Test each system independently. Never wonder "did I break something else?" |
| **Zero FindObjectOfType** | No hidden dependencies. Inspector refs + GameEvents only. |

---

## Weekly Schedule

```
Weekday:  ~4 hours/day × 5 = 20 hours
Weekend:  ~8 hours/day × 2 = 16 hours
Weekly total: ~36 hours
```

## Timeline

| Week | Phases | Hours | Milestone |
|------|--------|-------|-----------|
| 1 | A½ + B (start) | 31h | Mine environment + player movement + grab working |
| 2 | B (finish) | 28h | Full player: movement + camera + grab + tools + inventory |
| 3 | C + D (start) | 30h | Mining works, ore spawns, conveyors carry ore |
| 4 | D (finish) + E (start) | 30h | Building placement + first machines (crusher, furnace) |
| 5 | E (finish) + F | 27h | Full factory pipeline + quest system |
| 6 | G | 22h | Save/load working — game persists |
| 7 | H + I + J | 26h | Sound, settings, contracts, menus, debug — feature complete |

**Total: ~7 weeks. ~153 hours of hand-typing.**

---

## What Slows Things Down

- **Phase B** — hardest. 35 scripts. 888-line split. Tool inheritance. Inventory system. But architecture is proven.
- **Phase G** — trickiest. Save/load touches every system. ISaveLoadable interface wiring across all phases.
- **Hand-typing** — slower than copy-paste, but you understand every line. Pays off when debugging.

---

## The Payoff

```
Week 4-5: fully playable mining/factory game
  → mine ore nodes with pickaxe
  → ore flows through conveyors automatically
  → furnaces smelt by majority type
  → machines shape, polish, sort, package
  → seller machine converts to money
  → quest system guides progression
  → every system tested independently via vertical slice
  → zero tight coupling — add/remove any system without breaking others
```

That's the moment it clicks — and the architecture proves itself.
