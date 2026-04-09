# Estimate — Hand-Typing Timeline

> Based on Phase A taking ~3 days (~14 hours) including architecture design.
> Schedule: 4 hours/weekday, 8 hours/weekend = ~36 hours/week.

---

## Per Phase

| Phase | Scripts | Difficulty | New patterns? | Est. Hours | Est. Days |
|-------|---------|-----------|---------------|-----------|-----------|
| **A** | 22 | Easy | Yes — designed entire architecture from scratch | 14h | 3 days ✅ |
| **A½** | 2 | Easy | No — just 2 MonoBehaviours + scene work | 3h | < 1 day |
| **B** | ~30 | Hard | PlayerController split, inventory orchestrator | 25h | 5-6 days |
| **C** | ~20 | Medium | Object pooling, weighted drops | 15h | 3-4 days |
| **D** | ~18 | Hard | Ghost preview, grid snap, conveyor snap | 18h | 4-5 days |
| **E** | ~25 | Medium | Repetitive — machines follow same I/O pattern | 15h | 3-4 days |
| **F** | ~20 | Medium | Quest tree UI, research tree | 15h | 3-4 days |
| **G** | ~18 | Hard | Save/load serialization, interface wiring | 22h | 5-6 days |
| **H** | ~16 | Easy | Settings UI — familiar orchestrator pattern | 10h | 2-3 days |
| **I** | ~20 | Easy | Contracts — familiar quest-like pattern | 10h | 2-3 days |
| **J** | ~12 | Easy | Debug tools — simple | 6h | 1-2 days |
| **Total** | **~200** | | | **~153h** | |

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
| 1 | A½ + B (start) | 28h | Mine environment + player movement working |
| 2 | B (finish) | 25h | Full player controller + inventory + tools + grab |
| 3 | C + D (start) | 30h | Mining works, ore flows on conveyors |
| 4 | D (finish) + E (start) | 33h | Building placement + first machines |
| 5 | E (finish) + F | 30h | Full factory pipeline + quest system |
| 6 | G | 22h | Save/load working |
| 7 | H + I + J | 26h | Sound, settings, contracts, menus, debug |

**Total: ~7 weeks from now. ~153 hours of hand-typing.**

---

## What Speeds Things Up

- Architecture is locked — no more multi-day design sessions
- Patterns repeat — Phase E machines follow Phase C pattern, Phase I follows Phase F
- LEGO independence — type one system, test it, move on
- `DEBUG_Check` catches DataService bugs before wiring UI
- `PhaseXLOG` snapshots verify data at every step

## What Slows Things Down

- **Phase B** — hardest phase. 888-line PlayerController split into 4+ scripts. Physics math.
- **Phase G** — trickiest. Save/load touches every system. Interface wiring across all phases.
- **Hand-typing** — slower than copy-paste, but you understand every line (pays off in later phases)

---

## The Payoff

```
Week 4-5: fully playable mining/factory game
  → ore flows through conveyors
  → furnaces smelt by majority type
  → automated pipeline runs while you mine
  → quest system guides progression
```

That's the moment it clicks.
