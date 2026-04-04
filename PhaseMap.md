# PhaseMap — Full Build Roadmap

> Maps every phase from start to finish. Each phase shows: what it looks like when done, all .cs files to create/modify, importance weight, and dependencies. Phases are ordered so each one builds on the last like LEGO.
>
> **This plan may evolve** — new files may be added, phases may split or merge as implementation reveals needs.

---

## Overview

| Phase     | Name                       | Importance | Cumulative | Difficulty | Status |
| ------------- | ------------------------------------------------ | ---------- | ---------- | ---------- | ------- |
| **A**  | World Interaction + Shop Cart          | 7%     | 7%     | Easy    | Done  |
| **A½** | The Mine — Environment & Elevator        | 3%     | 10%    | Easy    | Done  |
| **B**  | Player Controller + Inventory + Tools + Grabbing | 15%    | 25%    | Hard    | Done  |
| **C**  | Mining & Ore System               | 14%    | 39%    | Medium   | Planned |
| **D**  | Building & Conveyor System            | 14%    | 53%    | Hard    | Planned |
| **E**  | Ore Processing Machines             | 18%    | 71%    | Medium   | Planned |
| **F**  | Quest & Research System             | 10%    | 81%    | Medium   | Planned |
| **G**  | Save/Load System                 | 8%     | 89%    | Hard    | Planned |
| **H**  | Sound, Settings & UI Polish           | 5%     | 94%    | Easy    | Planned |
| **I**  | Contracts, World Events & Menus         | 4%     | 98%    | Easy    | Planned |
| **J**  | Debug, Demo & Final Polish            | 2%     | 100%    | Easy    | Planned |

---

## Phase A — World Interaction + Shop Cart (8%) DONE

### What It Looks Like

```
First-person player on a flat plane. Walk to a cube (shop terminal),
press E. Shop panel opens with category tabs, item list, cart.
Add items, adjust quantity, purchase. Items spawn near terminal.
Money updates on HUD. ESC closes shop, cursor re-locks.
```

### Files (17 scripts)

| File                 | Type | Status |
| ------------------------------------- | ---- | ------ |
| `Core/Singleton.cs`         | New | Done  |
| `Core/GameEvents.cs`        | New | Done  |
| `Interaction/IInteractable.cs`   | New | Done  |
| `Interaction/SO_Interaction.cs`   | New | Done  |
| `Interaction/InteractionSystem.cs` | New | Done  |
| `Interaction/InteractionWheelUI.cs` | New | Done  |
| `Economy/EconomyManager.cs`     | New | Done  |
| `Shop/SO_ShopItemDefinition.cs`   | New | Done  |
| `Shop/SO_ShopCategory.cs`      | New | Done  |
| `Shop/ShopItem.cs`         | New | Done  |
| `Shop/ShopManager.cs`        | New | Done  |
| `Shop/ShopTerminal.cs`       | New | Done  |
| `Shop/ShopSpawnPoint.cs`      | New | Done  |
| `Shop/UI/ShopUI.cs`         | New | Done  |
| `Shop/UI/ShopCategoryButton.cs`   | New | Done  |
| `Shop/UI/ShopItemButton.cs`     | New | Done  |
| `Shop/UI/ShopCartItemButton.cs`   | New | Done  |
| `Player/SimplePlayerController.cs` | New | Done  |
| `UI/UIManager.cs`          | New | Done  |
| `UI/MoneyDisplay.cs`        | New | Done  |

### Original Source Reference

`ComputerShopUI.cs`, `ComputerTerminal.cs`, `ShopItemDefinition.cs`, `ShopCategory.cs`, `ShopItem.cs`, `ShopItemButton.cs`, `ShopCartItemButton.cs`, `ShopCategoryButton.cs`, `ShopSpawnPoint.cs`, `EconomyManager.cs`, `IInteractable.cs`, `Interaction.cs`, `InteractionWheelUI.cs`, `UIManager.cs`, `Singleton.cs`

---

## Phase A½ — The Mine: Environment & Elevator (3%)

### What It Looks Like

```
No more flat plane. The player starts inside an enclosed underground
mine room — rocky walls, dim lighting, industrial feel.

New Game → StartingElevator lowers the player down from above.
Elevator shakes with Perlin noise, landing particle plays on arrival.
Roof collider prevents jumping out during descent.

The mine floor has:
- Shop terminal (computer) against a wall
- ShopSpawnPoints near the elevator shaft (items drop from above)
- Tunnel openings leading to mining areas (empty for now)
- Basic lighting (point lights, ambient)

The shop terminal and spawn points from Phase A are repositioned
into this environment. Everything from Phase A still works —
just inside a proper mine instead of a flat plane.
```

### Files (~5 scripts + scene work)

| File               | Type         | Original Source             |
| --------------------------------- | -------------------- | --------------------------------------- |
| `World/StartingElevator.cs`   | New         | `StartingElevator.cs`         |
| `World/MainMenuCameraShaker.cs` | New         | `MainMenuCameraShaker.cs`       |
| `Core/GameEvents.cs`      | **Modify**   | Add:`OnElevatorLanded`        |
| Scene: Mine environment      | **Scene work** | Terrain/ProBuilder, lighting, colliders |
| Scene: Reposition Phase A objects | **Scene work** | Move terminal + spawn points into mine |

### Key Systems Introduced

- **StartingElevator** — Lerps Y position from `StartingHeight` to `EndHeight`, Perlin noise shake on X/Z, roof collider during descent, landing particle on arrival
- **Scene design** — Enclosed mine room built with ProBuilder or terrain, tunnel openings for future mining areas
- **Spawn point placement** — ShopSpawnPoints positioned so purchased items appear to "drop" from the elevator shaft above

### What This Phase Is NOT

- No main menu scene yet (that's Phase I)
- No scene loading/transitions (that's Phase G/I)
- The elevator just runs on scene start for new games
- Focus is purely on the physical environment the player inhabits

### Original Source Reference

`StartingElevator.cs`, `MainMenuCameraShaker.cs`, Scene layout from `Assets/Scenes/`

---

## Phase B — Player Controller + Inventory + Tools + Grabbing (15%)

### What It Looks Like

```
Full FPS controller: walk, sprint, duck, jump, slope sliding.
Look around with mouse, FOV widens when sprinting.

Walk up to a dropped pickaxe on the ground → press E → it goes
into your hotbar. Press 1-0 to switch tools. Scroll wheel cycles.
Active tool shows as a view model (first-person hands).

Hold right-click on a physics cube → SpringJoint grabs it,
a LineRenderer rope connects you to the object. Move mouse to
drag it around. Click again to release. Object bounces naturally.

Equip pickaxe → hold left-click → swing animation plays,
delayed raycast hits world objects.

Equip magnet tool → hold right-click → nearby physics objects
fly toward you via spring joints. Left-click to launch them.
R to drop gently. Q to cycle grab mode.

FresnelHighlighter outlines whatever you're looking at
(tools glow cyan, grabbables glow, buildings glow when hammer equipped).
```

### Files (~30 scripts)

| File                  | Type                 | Original Source                        |
| -------------------------------------- | ------------------------------------- | ------------------------------------------------------------- |
| `Player/PlayerController.cs`     | New (replaces SimplePlayerController) | `PlayerController.cs` (888 lines)              |
| `Player/PlayerInventory.cs`     | New                  | `PlayerInventory.cs`                    |
| `Player/PlayerFootsteps.cs`     | New                  | `PlayerFootsteps.cs`                    |
| `Player/PlayerSpawnPoint.cs`     | New                  | `PlayerSpawnPoint.cs`                    |
| `Tools/BaseHeldTool.cs`       | New                  | `BaseHeldTool.cs`                      |
| `Tools/BaseHeldToolSaveData.cs`   | New                  | `BaseHeldToolSaveData.cs`                  |
| `Tools/ToolPickaxe.cs`        | New                  | `ToolPickaxe.cs`                      |
| `Tools/ToolMagnet.cs`        | New                  | `ToolMagnet.cs`                       |
| `Tools/ToolMagnetSaveData.cs`    | New                  | `ToolMagnetSaveData.cs`                   |
| `Tools/ToolHammer.cs`        | New                  | `ToolHammer.cs`                       |
| `Tools/ToolMiningHat.cs`       | New                  | `ToolMiningHat.cs`                     |
| `Tools/ToolSupportsWrench.cs`    | New                  | `ToolSupportsWrench.cs`                   |
| `Tools/ToolResourceScanner.cs`    | New                  | `ToolResourceScanner.cs`                  |
| `Tools/ToolBuilder.cs`        | New                  | `ToolBuilder.cs` (partial — placement logic in Phase D)  |
| `Physics/BasePhysicsObject.cs`    | New                  | `BasePhysicsObject.cs`                   |
| `Physics/BaseSellableItem.cs`    | New (pulled forward)         | `BaseSellableItem.cs` — BaseHeldTool inherits from this  |
| `Physics/PhysicsUtils.cs`      | New                  | `PhysicsUtils.cs`                      |
| `Physics/PhysicsSoundPlayer.cs`   | New                  | `PhysicsSoundPlayer.cs`                   |
| `Physics/PhysicsGib.cs`       | New                  | `PhysicsGib.cs`                       |
| `Rendering/FresnelHighlighter.cs`  | New                  | `FresnelHighlighter.cs` (Highlight Plus wrapper)      |
| `Rendering/HighlightStyle.cs`    | New                  | `HighlightStyle.cs`                     |
| `UI/InventoryUIManager.cs`      | New                  | `InventoryUIManager.cs` (inferred)             |
| `UI/InventorySlotUI.cs`       | New                  | `InventorySlotUI.cs`                    |
| `UI/InventoryItemPreview.cs`     | New                  | `InventoryItemPreview.cs`                  |
| `Data/SO_FootstepSoundDefinition.cs` | New                  | `FootstepSoundDefinition.cs`                |
| `Enums/MagnetToolSelectionMode.cs`  | New (pulled forward)         | `MagnetToolSelectionMode.cs` — ToolMagnet requires this  |
| `Enums/SavableObjectID.cs`      | New (stub, expanded Phase G)     | `SavableObjectID.cs` — ISaveLoadableObject requires this  |
| `Interfaces/IIconItem.cs`      | New (pulled forward)         | Inferred — BaseHeldTool implements this           |
| `Interfaces/ISaveLoadableObject.cs` | New (stub, expanded Phase G)     | `ISaveLoadableObject.cs` — BaseHeldTool implements this  |
| `Core/GameEvents.cs`         | **Modify**           | Add:`OnToolSwitched`, `OnItemPickedUp`, `OnItemDropped`, `OnToolPickedUp` |
| `UI/UIManager.cs`          | **Modify**           | Add inventory panel check, on-screen controls, building info |

> **Note:** 5 files were pulled forward from future phases because `BaseHeldTool` inherits `BaseSellableItem` and implements `ISaveLoadableObject` + `IIconItem` in the original source. Without these stubs, the inheritance chain doesn't compile. `MagnetToolSelectionMode` is similarly required by `ToolMagnet`. `SavableObjectID` is a minimal stub — expanded with full IDs in Phase G.

### Modifications to Earlier Phases

| File (from earlier phase)        | Change                                | Why                    |
| ---------------------------------------- | -------------------------------------------------------------------- | ------------------------------------------ |
| `Core/GameEvents.cs` (A)        | Add `OnToolSwitched`, `OnItemPickedUp`, `OnItemDropped` events | Decoupled tool/inventory notifications   |
| `UI/UIManager.cs` (A)         | Add inventory panel check to `IsInAnyMenu()`            | Cursor unlock when inventory open     |
| `Player/SimplePlayerController.cs` (A) | **Replaced** by `PlayerController.cs`            | Full controller supersedes the minimal one |
| `World/StartingElevator.cs` (A½)   | Update player teleport to use `PlayerController.TeleportPlayer()` | New controller has proper teleport method |

### Key Systems Introduced

- **SpringJoint grab** — Player creates SpringJoint on grabbed rigidbody, LineRenderer draws rope
- **Tool hierarchy** — `BaseHeldTool` base class, concrete tools override `PrimaryFire()`, `SecondaryFire()`, etc.
- **View/World model toggle** — Equipped tools show ViewModel, dropped tools show WorldModel
- **Hotbar + extended inventory** — 10 hotbar slots + 30 extended, scroll/number keys to switch
- **FresnelHighlighter** — CommandBuffer-based GPU outline on looked-at objects

---

## Phase C — Mining & Ore System (15%)

### What It Looks Like

```
You walk into a mine tunnel from the starting room.
Glowing ore nodes embedded in the walls/floor — different
colors for Iron (grey), Gold (yellow), Copper (orange), Coal (black).

Equip pickaxe from hotbar → hold left-click:
 - Swing animation plays
 - 0.2s delay, then raycast hits the node
 - Particle sparks fly from impact point
 - Node health bar decreases
 - After 3-4 hits, node shatters:
  → 2-4 ore pieces fly out with random velocity
  → Pieces bounce and roll on the ground (Rigidbody physics)
  → Break particle burst plays
  → Node disappears permanently (position saved for persistence)

Ore pieces on the ground:
 - Grabbable with hand (right-click SpringJoint from Phase B)
 - Pullable with magnet tool (Phase B)
 - Each has ResourceType (Iron, Gold, etc.) + PieceType (Ore, Crushed, etc.)
 - Random mesh variant + slight scale variation for visual variety
 - Random price multiplier (0.9x–1.1x)

AutoMiner placed at a node → rotates continuously, spawns ore
on a timer. Probability-based (80% default). Rate adjustable.

SellerMachine (trigger volume) → ore enters → waits 2s →
money increases → ore returns to pool.

With 500+ ore pieces active, OreLimitManager kicks in:
 - UI warning appears
 - Auto-miner spawn rate slows down
 - At 2000+ moving objects, spawning blocks entirely

OrePiecePoolManager recycles all ore — zero Instantiate/Destroy
after initial pool warmup. Smooth performance.
```

### Files (~20 scripts)

| File                    | Type       | Original Source                     |
| ------------------------------------------ | ---------------- | -------------------------------------------------------- |
| `Ore/OrePiece.cs`            | New       | `OrePiece.cs` (443 lines)               |
| `Ore/OreNode.cs`             | New       | `OreNode.cs`                      |
| `Ore/OreManager.cs`           | New       | `OreManager.cs`                    |
| `Ore/OrePiecePoolManager.cs`       | New       | `OrePiecePoolManager.cs`                |
| `Ore/OreLimitManager.cs`         | New       | `OreLimitManager.cs`                  |
| `Ore/OrePieceKey.cs`           | New       | `OrePieceKey.cs`                    |
| `Ore/OrePieceEntry.cs`          | New       | `OrePieceEntry.cs`                   |
| `Ore/DamageableOrePiece.cs`       | New       | `DamageableOrePiece.cs`                |
| `Data/SO_ResourceDescription.cs`     | New       | `ResourceDescription.cs`                |
| `Data/SO_AutoMinerResourceDefinition.cs` | New       | `AutoMinerResourceDefinition.cs`            |
| `Data/SO_WeightedOreChance.cs`      | New       | `WeightedOreChance.cs`                 |
| `Data/SO_WeightedNodeDrop.cs`      | New       | `WeightedNodeDrop.cs`                 |
| `Enums/ResourceType.cs`         | New       | `ResourceType.cs`                   |
| `Enums/PieceType.cs`           | New       | `PieceType.cs`                     |
| `Interfaces/IDamageable.cs`       | New       | `IDamageable.cs`                    |
| `Machines/SellerMachine.cs`       | New       | `SellerMachine.cs`                   |
| `Machines/AutoMiner.cs`         | New       | `AutoMiner.cs`                     |
| `Items/BaseSellableItem.cs`       | New       | `BaseSellableItem.cs`                 |
| `Particles/ParticleManager.cs`      | New       | `ParticleManager.cs`                  |
| `UI/PhysicsLimitUIWarning.cs`      | New       | `PhysicsLimitUIWarning.cs`               |
| `Core/GameEvents.cs`           | **Modify** | Add:`OnOreMined`, `OnOreSold`, `OnOreLimitChanged` |

### Modifications to Earlier Phases

| File (from earlier phase) | Change                             | Why                      |
| -------------------------- | --------------------------------------------------------------- | ---------------------------------------------- |
| `Core/GameEvents.cs` (A) | Add `OnOreMined`, `OnOreSold`, `OnOreLimitChanged` events | Quest system (Phase F) will subscribe to these |

### Key Systems Introduced

- **Object pooling** — `OrePiecePoolManager` with `Dictionary<OreKey, Queue<OrePiece>>`
- **Performance throttle** — `OreLimitManager` counts non-sleeping rigidbodies, throttles spawn rates
- **Amortized cleanup** — `OreManager` checks one ore per frame (round-robin)
- **Weighted random drops** — Cumulative weight selection for node drops and sieve outputs
- **OrePiece transformation chain** — Crush, smelt, roll, polish pipeline via prefab references

---

## Phase D — Building & Conveyor System (15%)

### What It Looks Like

```
Open shop → buy "Conveyor Belt" → a crate spawns near elevator.
Walk to crate, press E → "Take" → goes into hotbar as ToolBuilder.

Equip the conveyor tool from hotbar:
 - A transparent green ghost of the conveyor belt follows
  your camera aim, snapped to a 1m world grid
 - Move mouse → ghost slides along grid positions
 - Look at invalid spot (overlapping another building) →
  ghost turns red
 - Press R → ghost rotates 90°
 - Press Q → ghost mirrors (for L/R variants)
 - Place near another conveyor → auto-snaps input→output
  (tests 4 rotations, picks best alignment)
 - Left-click → real conveyor belt instantiates at ghost position
 - Tool quantity decreases. At 0, tool is consumed.

Conveyor belt in the world:
 - Ore pieces that touch the belt trigger get pushed forward
  via physics velocity in FixedUpdate
 - Place multiple belts end-to-end → ore flows along the line
 - Belt has visual texture scroll (ConveyorRenderer)

Equip hammer → look at any placed building:
 - FresnelHighlighter outlines it in cyan
 - Press E → interaction wheel: "Take" or "Pack"
 - Take → building goes back into inventory as ToolBuilder
 - Pack → building becomes a crate on the ground

Buildings on uneven ground:
 - Modular scaffolding legs raycast downward
 - Legs spawn dynamically to reach the floor
 - Toggle supports on/off with wrench tool
```

### Files (~18 scripts)

| File                    | Type       | Original Source               |
| ------------------------------------------ | ---------------- | -------------------------------------------- |
| `Building/BuildingManager.cs`      | New       | `BuildingManager.cs` (404 lines)      |
| `Building/BuildingObject.cs`       | New       | `BuildingObject.cs` (271 lines)      |
| `Building/BuildingPlacementNode.cs`   | New       | `BuildingPlacementNode.cs`         |
| `Building/BuildingCrate.cs`       | New       | `BuildingCrate.cs`             |
| `Building/BuildingRotationInfo.cs`    | New       | `BuildingRotationInfo.cs`         |
| `Building/ModularBuildingSupports.cs`  | New       | `ModularBuildingSupports.cs`        |
| `Building/ScaffoldingSupportLeg.cs`   | New       | `ScaffoldingSupportLeg.cs`         |
| `Building/BaseModularSupports.cs`    | New       | `BaseModularSupports.cs`          |
| `Data/SO_BuildingInventoryDefinition.cs` | New       | `BuildingInventoryDefinition.cs`      |
| `Data/SO_BuildingObjectEntry.cs`     | New       | `BuildingObjectEntry.cs`          |
| `Enums/CanPlaceBuilding.cs`       | New       | `CanPlaceBuilding.cs`           |
| `Enums/PlacementNodeRequirement.cs`   | New       | `PlacementNodeRequirement.cs`       |
| `Enums/SupportType.cs`          | New       | `SupportType.cs`              |
| `Conveyor/ConveyorBelt.cs`        | New       | `ConveyorBelt.cs`             |
| `Conveyor/ConveyorBeltManager.cs`    | New       | `ConveyorBeltManager.cs`          |
| `Conveyor/ConveyorRenderer.cs`      | New       | `ConveyorRenderer.cs`           |
| `Conveyor/ConveyorSoundSource.cs`    | New       | `ConveyorSoundSource.cs`          |
| `Tools/ToolBuilder.cs`          | **Modify** | Complete placement logic (grid, ghost, snap) |

### Modifications to Earlier Phases

| File (from earlier phase)       | Change                                    | Why                                  |
| ------------------------------------- | ----------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| `Tools/ToolBuilder.cs` (B)     | Complete placement logic — grid snap, ghost preview, conveyor snap detection | Phase B creates partial ToolBuilder; Phase D finishes it        |
| `Shop/SO_ShopItemDefinition.cs` (A) | Add `SO_BuildingInventoryDefinition` field                 | Links shop items to building data (crate → tool → building pipeline) |
| `Shop/ShopUI.cs` (A)        | Update `TrySpawnItem()` to handle BuildingCrate spawning          | Buildings spawn as crates instead of raw prefabs            |

### Key Systems Introduced

- **Grid-based placement** — `GetClosestGridPosition()` snaps to 1m grid
- **Ghost preview** — Instantiate prefab as ghost, swap materials, disable scripts
- **Conveyor snap detection** — Test 4 rotations × all I/O points, pick best by frequency
- **Physics conveyor** — `ConveyorBelt.FixedUpdate()` applies velocity to `BasePhysicsObject` on belt
- **Modular supports** — Raycast downward, spawn scaffolding legs dynamically

---

## Phase E — Ore Processing Machines (18%)

### What It Looks Like

```
You now have all the machines to build a complete factory.
A typical layout in the mine:

AutoMiner (placed at ore node) → ore spawns on a timer,
drops onto a conveyor belt below it.
  ↓
Conveyor carries ore to CrusherMachine → ore enters trigger,
2× crushed pieces pop out the other side. Crushing sound plays.
  ↓
Conveyor carries crushed ore to CastingFurnace:
 - Crushed pieces fall into the furnace basin
 - Liquid plane rises as pieces accumulate
 - Display shows contents bar (colored by resource type)
 - When enough material: furnace animates, pours into molds
 - Output depends on majority type:
  All Iron → Iron Ingot. Iron + Coal → Steel. Mixed → Slag.
 - 3 mold slots → 3 output pieces per smelt cycle
  ↓
Conveyor carries ingots to shaping machines:
 - RollingMill → ingot becomes plate (flat)
 - PipeRoller → ingot becomes pipe (round)
 - RodExtruder → ingot becomes rod (thin)
 - ThreadingLathe → rod becomes threaded rod
  ↓
PolishingMachine → ore piece sits on the polisher,
polish % gradually increases (0→100%). At 100%, piece
transforms into polished variant (shinier material, higher sell price).
  ↓
SorterMachine → ore enters, routes to different output
conveyors based on ResourceType. Configure filters per output.
  ↓
PackagerMachine → individual ore pieces boxed into a
BoxObject container. Box tracks contents (type, count, polish).
  ↓
SellerMachine → box or loose ore enters trigger → sold after
2s delay → money increases → quest progress updates.

Advanced conveyor pieces:
 - ConveyorSplitter → splits flow into 2 directions
 - ConveyorBlocker → stops flow (toggle on/off)
 - RoutingConveyor → configurable output direction
 - RollerSplitter → alternates items between outputs

DepositBox → animated bucket elevator, buckets move up/down,
motor sound pitch scales with speed, auto-stops when idle.

The player can now build a fully automated ore-to-money pipeline
that runs while they mine more nodes or expand the factory.
```

### Files (~30 scripts)

| File                       | Type | Original Source             |
| ------------------------------------------------ | ---- | --------------------------------------- |
| `Machines/CastingFurnace.cs`          | New | `CastingFurnace.cs` (456 lines)    |
| `Machines/CastingFurnaceCoalInput.cs`     | New | `CastingFurnaceCoalInput.cs`     |
| `Machines/CastingFurnaceInteractionHandler.cs` | New | `CastingFurnaceInteractionHandler.cs` |
| `Machines/CastingFurnaceMoldArea.cs`      | New | `CastingFurnaceMoldArea.cs`      |
| `Machines/BlastFurnace.cs`           | New | `BlastFurnace.cs`           |
| `Machines/RollingMill.cs`           | New | `RollingMill.cs`           |
| `Machines/PipeRoller.cs`            | New | `PipeRoller.cs`            |
| `Machines/RodExtruder.cs`           | New | `RodExtruder.cs`           |
| `Machines/ThreadingLathe.cs`          | New | `ThreadingLathe.cs`          |
| `Machines/PolishingMachine.cs`         | New | `PolishingMachine.cs`         |
| `Machines/CrusherMachine.cs`          | New | `CrusherMachine.cs`          |
| `Machines/ClusterBreaker.cs`          | New | `ClusterBreaker.cs`          |
| `Machines/ShakerTable.cs`           | New | `ShakerTable.cs`           |
| `Machines/SorterMachine.cs`          | New | `SorterMachine.cs`          |
| `Machines/BulkSorter.cs`            | New | `BulkSorter.cs`            |
| `Machines/PackagerMachine.cs`         | New | `PackagerMachine.cs`         |
| `Machines/DepositBox.cs`            | New | `DepositBox.cs`            |
| `Machines/RapidAutoMiner.cs`          | New | `RapidAutoMiner.cs`          |
| `Machines/RapidAutoMinerDrillBit.cs`      | New | `RapidAutoMinerDrillBit.cs`      |
| `Machines/OreAnalyzer.cs`           | New | `OreAnalyzer.cs`           |
| `Conveyor/ConveyorBlocker.cs`         | New | `ConveyorBlocker.cs`         |
| `Conveyor/ConveyorBlockerT2.cs`        | New | `ConveyorBlockerT2.cs`        |
| `Conveyor/ConveyorSplitterT2.cs`        | New | `ConveyorSplitterT2.cs`        |
| `Conveyor/RollerSplitter.cs`          | New | `RollerSplitter.cs`          |
| `Conveyor/RoutingConveyor.cs`         | New | `RoutingConveyor.cs`         |
| `Items/BoxObject.cs`              | New | `BoxObject.cs`            |
| `Items/BoxContents.cs`             | New | `BoxContents.cs`           |
| `Items/BoxContentEntry.cs`           | New | `BoxContentEntry.cs`         |
| `Items/BaseBasket.cs`             | New | `BaseBasket.cs`            |
| `Items/SorterFilterBasket.cs`         | New | `SorterFilterBasket.cs`        |
| `Items/Hopper.cs`               | New | `Hopper.cs`              |
| `Data/SO_CastingFurnaceRecipie.cs`       | New | `CastingFurnaceRecipie.cs`      |
| `Data/SO_CastingFurnaceMoldRecipieSet.cs`   | New | `CastingFurnaceMoldRecipieSet.cs`   |
| `Enums/CastingMoldType.cs`           | New | `CastingMoldType.cs`         |

### Modifications to Earlier Phases

None — all new machine scripts. Each machine is self-contained with trigger/collision-based I/O.

---

## Phase F — Quest & Research System (10%)

### What It Looks Like

```
Bottom-left of HUD shows active quest:
 "Sell 10 Iron Ingots (3/10)" ← progress bar updates live

Sell ore at SellerMachine → quest counter increments.
When all requirements met → quest completes automatically:
 - Notification appears
 - Shop items unlock (e.g. "Polishing Machine" now available)
 - Money reward added
 - Research tickets awarded
 - Next quest in chain auto-activates

Press Q to open Quest Tree UI:
 - Full tree view of all quests
 - Completed quests: green checkmark
 - Available quests: white, clickable to activate
 - Locked quests: greyed out (prerequisites not met)
 - Click a quest → see requirements, rewards, description
 - Can pause/unpause active quests

Quest types you'll see:
 - "Mine your first ore" (TriggeredQuestRequirement)
 - "Sell 10 Iron Ingots" (ResourceQuestRequirement)
 - "Research the Conveyor Belt" (UnlockResearchQuestRequirement)
 - "Buy an Auto-Miner" (ShopItemQuestRequirement)

Research Tree (separate tab from Quest Tree):
 - Grid of research items with prerequisites
 - Each costs research tickets + optionally money
 - Completed items: colored. Locked: greyed.
 - Click to research → tickets deducted → item unlocked
 - Unlocked research enables new shop items or features

This is the progression loop:
Mine → Sell → Complete Quest → Unlock Items → Build Factory
→ Earn More → Research → Unlock Advanced Machines → Repeat
```

### Files (~20 scripts)

| File                    | Type       | Original Source                             |
| ------------------------------------------- | ---------------- | ----------------------------------------------------------------------- |
| `Quest/QuestManager.cs`          | New       | `QuestManager.cs`                           |
| `Quest/Quest.cs`             | New       | `Quest.cs`                              |
| `Quest/SO_QuestDefinition.cs`       | New       | `QuestDefinition.cs`                         |
| `Quest/QuestRequirement.cs`        | New       | `QuestRequirement.cs`                         |
| `Quest/ResourceQuestRequirement.cs`    | New       | `ResourceQuestRequirement.cs`                     |
| `Quest/TriggeredQuestRequirement.cs`   | New       | `TriggeredQuestRequirement.cs`                    |
| `Quest/TimedQuestRequirement.cs`     | New       | `TimedQuestRequirement.cs`                      |
| `Quest/UnlockResearchQuestRequirement.cs` | New       | `UnlockResearchQuestRequirement.cs`                  |
| `Quest/ShopItemQuestRequirement.cs`    | New       | `ShopItemQuestRequirement.cs`                     |
| `Quest/ActiveQuestEntry.cs`        | New       | `ActiveQuestEntry.cs`                         |
| `Quest/ResourceQuestRequirementEntry.cs` | New       | `ResourceQuestRequirementEntry.cs`                  |
| `Research/ResearchManager.cs`       | New       | `ResearchManager.cs`                         |
| `Research/SO_ResearchItemDefinition.cs`  | New       | `ResearchItemDefinition.cs`                      |
| `UI/QuestHud.cs`             | New       | `QuestHud.cs`                             |
| `UI/QuestTreeUI.cs`            | New       | `QuestTreeUI.cs`                           |
| `UI/QuestTreeItemButton.cs`        | New       | `QuestTreeItemButton.cs`                       |
| `UI/QuestTreeQuestInfoUI.cs`       | New       | `QuestTreeQuestInfoUI.cs`                       |
| `UI/ResearchTreeUI.cs`          | New       | `ResearchTreeUI.cs`                          |
| `UI/ResearchItemButton.cs`        | New       | `ResearchItemButton.cs`                        |
| `Enums/QuestID.cs`            | New       | `QuestID.cs`                             |
| `Enums/TriggeredQuestRequirementType.cs` | New       | `TriggeredQuestRequirementType.cs`                  |
| `Core/GameEvents.cs`           | **Modify** | Add:`OnQuestCompleted`, `OnQuestActivated`, `OnResearchCompleted` |
| `UI/UIManager.cs`             | **Modify** | Add quest tree panel to `IsInAnyMenu()`                |

### Modifications to Earlier Phases

| File (from earlier phase)  | Change                                 | Why                  |
| --------------------------- | ----------------------------------------------------------------------- | -------------------------------------- |
| `Core/GameEvents.cs` (A) | Add `OnQuestCompleted`, `OnQuestActivated`, `OnResearchCompleted` | Decoupled quest/research notifications |
| `UI/UIManager.cs` (A)   | Add quest tree panel to `IsInAnyMenu()`                | Cursor unlock when quest tree open   |
| `Shop/ShopManager.cs` (A) | Quest completion calls `UnlockShopItem()` via events         | Quests unlock shop items on completion |

---

## Phase G — Save/Load System (8%)

### What It Looks Like

```
Press ESC → Pause Menu appears (game time freezes).
FPS capped to 50 while paused (saves GPU).

Click "Save Game":
 - Screen briefly captures a JPG screenshot
 - "Auto-saving..." warning appears on HUD
 - JSON file written atomically (write to .tmp, rename)
 - Backup .bak created before overwriting
 - Save includes:
  • Every tool in inventory (position, slot, custom data)
  • Every placed building (position, rotation, supports)
  • Every ore piece in world (position, rotation, scale,
   mesh ID, resource type, piece type, polish %)
  • Player position + rotation
  • Money, research tickets
  • All quest progress (completed + active + counters)
  • Shop purchase history
  • Destroyed ore node positions
  • World events (explosions, etc.)
  • Total play time

Click "Load Game":
 - All existing objects destroyed
 - Scene reloaded
 - Every saved object reinstantiated from prefab lookup
 - Player teleported to saved position
 - Economy/quests/research restored
 - Destroyed nodes re-destroyed

Auto-save runs every 5 minutes (configurable):
 - Shows "Auto-saving..." warning briefly
 - Same atomic write process

Save file format: versioned JSON (version 15).
Backward compatible from version 4+.
Legacy save migration from old folder structure.
```

### Files (~18 scripts)

| File                      | Type | Original Source             |
| ---------------------------------------------- | ---- | --------------------------------------- |
| `SaveLoad/SavingLoadingManager.cs`      | New | `SavingLoadingManager.cs` (853 lines) |
| `SaveLoad/SaveFile.cs`            | New | `SaveFile.cs`             |
| `SaveLoad/SaveEntry.cs`           | New | `SaveEntry.cs`            |
| `SaveLoad/SaveFileHeader.cs`         | New | `SaveFileHeader.cs`          |
| `SaveLoad/SaveFileHeaderFileCombo.cs`    | New | `SaveFileHeaderFileCombo.cs`     |
| `SaveLoad/AutoSaveManager.cs`        | New | `AutoSaveManager.cs`         |
| `SaveLoad/SaveFileScreenshotCamera.cs`    | New | `SaveFileScreenshotCamera.cs`     |
| `Interfaces/ISaveLoadableObject.cs`     | New | `ISaveLoadableObject.cs`       |
| `Interfaces/ISaveLoadableBuildingObject.cs` | New | `ISaveLoadableBuildingObject.cs`   |
| `Interfaces/ISaveLoadableStaticBreakable.cs` | New | `ISaveLoadableStaticBreakable.cs`   |
| `Interfaces/ISaveLoadableWorldEvent.cs`   | New | `ISaveLoadableWorldEvent.cs`     |
| `Interfaces/ICustomSaveDataProvider.cs`   | New | `ICustomSaveDataProvider.cs`     |
| `Data/WorldEventEntry.cs`          | New | `WorldEventEntry.cs`         |
| `Data/ShopPurchases.cs`           | New | `ShopPurchases.cs`          |
| `Enums/SavableObjectID.cs`          | New | `SavableObjectID.cs`         |
| `Enums/SavableWorldEventType.cs`       | New | `SavableWorldEventType.cs`      |
| `UI/SaveFileButton.cs`            | New | `SaveFileButton.cs`          |
| `UI/AutoSavingWarning.cs`          | New | `AutoSavingWarning.cs`        |

### Modifications to Earlier Phases

| File (from earlier phase)      | Change                            | Why                          |
| ----------------------------------- | ------------------------------------------------------------ | ----------------------------------------------------- |
| `Tools/BaseHeldTool.cs` (B)    | Add `ISaveLoadableObject` interface            | Tools save/load position, inventory slot, custom data |
| `Building/BuildingObject.cs` (D) | Add `ISaveLoadableBuildingObject` interface        | Buildings save/load position, rotation, support state |
| `Ore/OreNode.cs` (C)       | Add `ISaveLoadableStaticBreakable` interface        | Broken nodes persist across saves           |
| `Machines/AutoMiner.cs` (C)    | Add `ICustomSaveDataProvider` interface          | Save on/off state                   |
| `Machines/CastingFurnace.cs` (E) | Add `ICustomSaveDataProvider` interface          | Save coal amount, mold types             |
| `Economy/EconomyManager.cs` (A)  | Add `SetMoney()` for load, wire to save file        | Restore money on load                 |
| `Shop/ShopItem.cs` (A)      | Add `ShopPurchases` tracking with `SavableObjectID` keys | Persistent purchase history              |
| `Quest/QuestManager.cs` (F)    | Add `LoadFromSaveFile()` method              | Restore quest progress                |
| `Research/ResearchManager.cs` (F) | Add `LoadFromSaveFile()` method              | Restore research progress               |

---

## Phase H — Sound, Settings & UI Polish (5%)

### What It Looks Like

```
The game now has full audio and is fully configurable.

Sounds you hear:
 - Pickaxe: swing whoosh, impact thud on nodes, clang on world
 - Ore: breaking crunch, crushing sound, furnace sizzle
 - Conveyor: ambient hum (spatial, N closest to player)
 - Machines: processing loops, pour sounds, motor sounds
 - Footsteps: surface-dependent (stone, metal, etc.)
 - UI: button hover, click, purchase chime, error buzz
 - Elevator: rumble during descent

SoundManager pools 30 AudioSources. Sounds too far from
player are culled (sqrMagnitude check). Zero allocation.

Press ESC → Pause Menu → Settings:
 - Mouse Sensitivity slider
 - FOV slider (60–120)
 - Master Volume slider
 - Camera Bob slider (0–200%)
 - Viewmodel Bob slider (0–200%)
 - VSync toggle
 - FPS Limit (30/60/120/144/240/300/Unlimited)
 - Physics Object Limit (500/1000/2000/5000/10000)
 - Display Mode (Windowed/Borderless/Fullscreen)
 - Resolution dropdown
 - Invert Mouse X/Y toggles
 - Toggle Ducking (hold vs toggle)
 - Reverse Hotbar Scrolling toggle

Keybind Rebinding:
 - List of all actions with current key shown
 - Click an action → "Press a key..." → press new key
 - Saved to keybinds.json in persistent data
 - Reset to Default button per binding
 - UI text auto-updates: "Press [Interact] to open" shows
  actual bound key via token replacement

All settings persist via PlayerPrefs. Applied on game start.
```

### Files (~15 scripts)

| File                | Type | Original Source      |
| ---------------------------------- | ---- | ------------------------- |
| `Sound/SoundManager.cs`     | New | `SoundManager.cs`    |
| `Sound/SoundPlayer.cs`      | New | `SoundPlayer.cs`    |
| `Sound/SO_SoundDefinition.cs`  | New | `SoundDefinition.cs`  |
| `Sound/LoopingSoundPlayer.cs`  | New | `LoopingSoundPlayer.cs` |
| `Sound/LoopingSoundFader.cs`   | New | `LoopingSoundFader.cs` |
| `Settings/SettingsManager.cs`  | New | `SettingsManager.cs`  |
| `Settings/SettingsMenu.cs`    | New | `SettingsMenu.cs`    |
| `Settings/SettingSlider.cs`   | New | `SettingSlider.cs`   |
| `Settings/SettingToggle.cs`   | New | `SettingToggle.cs`   |
| `Settings/SettingKeybind.cs`   | New | `SettingKeybind.cs`   |
| `Settings/ResolutionSetting.cs` | New | `ResolutionSetting.cs` |
| `Settings/DisplayModeSetting.cs` | New | `DisplayModeSetting.cs` |
| `Input/KeybindManager.cs`    | New | `KeybindManager.cs`   |
| `Input/KeybindAction.cs`     | New | `KeybindAction.cs`   |
| `Input/KeybindEntry.cs`     | New | `KeybindEntry.cs`    |
| `Input/KeybindTokenText.cs`   | New | `KeybindTokenText.cs`  |

### Modifications to Earlier Phases

| File (from earlier phase)        | Change                                                                  | Why                             |
| ---------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------- |
| `Player/PlayerController.cs` (B)    | Read `SettingsManager.Instance.MouseSensitivity` / `DesiredFOV` / `CameraBobScale` instead of hardcoded `[SerializeField]` values | Settings become user-configurable instead of inspector-only |
| `Player/PlayerController.cs` (B)    | Replace `Input.GetKeyDown(KeyCode.X)` → `PlayerInputActions` from Input System                            | All keybinds become rebindable               |
| `Interaction/InteractionSystem.cs` (A) | Replace `KeyCode.E` → Input System action                                               | Interact key becomes rebindable               |
| `Shop/ShopUI.cs` (A)          | Add purchase/add/remove sound calls via `SoundManager`                                         | UI sounds on cart actions                  |
| `World/StartingElevator.cs` (A½)   | Wire `SoundPlayer` for elevator descent sound                                              | Elevator sound during descent                |
| `Machines/*.cs` (E)          | Add processing sounds to each machine                                                   | Machine audio feedback                   |
| `Tools/ToolPickaxe.cs` (B)       | Add swing/hit sounds via `SoundManager`                                                 | Mining audio feedback                    |
| `UI/UIManager.cs` (A)         | Add `PauseMenu` panel reference + check in `IsInAnyMenu()`                                      | Pause menu integration                   |

---

## Phase I — Contracts, World Events & Menus (4%)

### What It Looks Like

```
Contracts Terminal (in-world, near shop):
 - Walk up, press E → Contracts UI opens
 - List of available contracts:
  "Deliver 20 Polished Iron Plates" — Reward: $2,000
  "Deliver 10 Steel Ingots" — Reward: $3,500
 - Accept one contract at a time
 - Pack ore into BoxObjects via PackagerMachine
 - Carry/convey boxes to ContractSellTrigger
 - Box contents checked against contract requirements
 - Progress updates. Complete → claim reward → money added.

World Objects:
 - DetonatorBuySign → buy dynamite for money
 - DetonatorTrigger → place near ore nodes, interact to detonate
 - Explosion blasts ore nodes, spawns ore pieces
 - BreakableCrate → hit with pickaxe → random loot drops
 - EditableSign → interact → type custom text → sign displays it
 - WaterVolume → player enters water trigger (swim/slow)
 - ExtinguishableFire → visual fire, can be put out

Main Menu (separate scene):
 - Title screen with logo
 - MainMenuElevator sways gently (Perlin noise)
 - "New Game" → map selection → elevator drops → fade → load scene
 - "Load Game" → save file browser with JPG thumbnails
  Shows: save name, date, play time, money, game version
 - "Settings" → same settings menu from Phase H
 - "Quit" → exits application
 - Loading screen during scene transitions
```

### Files (~20 scripts)

| File                  | Type | Original Source      |
| -------------------------------------- | ---- | -------------------------- |
| `Contracts/ContractsManager.cs`   | New | `ContractsManager.cs`  |
| `Contracts/SO_ContractDefinition.cs` | New | `ContractDefinition.cs` |
| `Contracts/ContractInstance.cs`   | New | `ContractInstance.cs`  |
| `Contracts/ContractsTerminal.cs`   | New | `ContractsTerminal.cs`  |
| `Contracts/ContractSellTrigger.cs`  | New | `ContractSellTrigger.cs` |
| `UI/ContractsTerminalUI.cs`     | New | `ContractsTerminalUI.cs` |
| `UI/ContractInfoUI.cs`        | New | `ContractInfoUI.cs`   |
| `World/DetonatorExplosion.cs`    | New | `DetonatorExplosion.cs` |
| `World/DetonatorTrigger.cs`     | New | `DetonatorTrigger.cs`  |
| `World/DetonatorBuySign.cs`     | New | `DetonatorBuySign.cs`  |
| `World/BreakableCrate.cs`      | New | `BreakableCrate.cs`   |
| `World/EditableSign.cs`       | New | `EditableSign.cs`    |
| `World/ExtinguishableFire.cs`    | New | `ExtinguishableFire.cs` |
| `World/WaterVolume.cs`        | New | `WaterVolume.cs`     |
| `Menus/MainMenu.cs`         | New | `MainMenu.cs`      |
| `Menus/LoadingMenu.cs`        | New | `LoadingMenu.cs`     |
| `Menus/NewGameMenu.cs`        | New | `NewGameMenu.cs`     |
| `Menus/PauseMenu.cs`         | New | `PauseMenu.cs`      |
| `Menus/MapSelectButton.cs`      | New | `MapSelectButton.cs`   |
| `UI/EditTextPopup.cs`        | New | `EditTextPopup.cs`    |

### Modifications to Earlier Phases

| File (from earlier phase)      | Change                                  | Why                     |
| ----------------------------------- | ------------------------------------------------------------------------ | -------------------------------------------- |
| `UI/UIManager.cs` (A)       | Add `ContractsTerminalUI` panel reference + check in `IsInAnyMenu()` | Cursor unlock when contracts UI open     |
| `World/StartingElevator.cs` (A½) | Wire to `SavingLoadingManager.SceneWasLoadedFromNewGame` check     | Only lower elevator on new game, not on load |
| `Menus/PauseMenu.cs` (H)     | Add save/load file browser integration                  | Pause menu gets full save/load UI      |

---

## Phase J — Debug, Demo & Final Polish (2%)

### What It Looks Like

```
Developer Mode (hidden):
 - Type "shaftmaster" during gameplay → dev mode activates
 - Sound plays on activation
 - Debug keys become active:
  V → toggle noclip (fly through walls)
  M → add $1,000
  I → unlock all shop items
  U → toggle unlimited building (no quantity consumed)
  Z → complete next active quest
  -/= → halve/double time scale
  Backspace → reset time scale to 1.0
 - Debug shop categories become visible
 - DebugOreSpawner → spawn any ore type at cursor

Error Handling:
 - Runtime errors/exceptions caught via Application.logMessageReceived
 - Error popup appears in-game with message + stack trace
 - "Don't show again this session" option

Version & Level Management:
 - Version number displayed on HUD and main menu
 - LevelManager tracks current scene/level ID
 - DemoManager restricts locked items in demo builds

Visual Polish:
 - DisplacementMeshGenerator → terrain detail meshes
 - VertexPainter → hand-painted vertex colors on mine walls
 - DecalDestroyer → cleanup decals when objects are removed
 - Particle tuning across all systems

The game is now feature-complete.
```

### Files (~12 scripts)

| File                  | Type | Original Source         |
| --------------------------------------- | ---- | -------------------------------- |
| `Debug/DebugManager.cs`        | New | `DebugManager.cs`       |
| `Debug/DebugOreSpawner.cs`      | New | `DebugOreSpawner.cs`      |
| `Debug/ToolDebugSpawnTool.cs`     | New | `ToolDebugSpawnTool.cs`    |
| `Core/VersionManager.cs`       | New | `VersionManager.cs`      |
| `Core/LevelManager.cs`        | New | `LevelManager.cs`       |
| `Core/LevelInfo.cs`          | New | `LevelInfo.cs`         |
| `Core/DemoManager.cs`         | New | `DemoManager.cs`        |
| `Visual/DisplacementMeshGenerator.cs` | New | `DisplacementMeshGenerator.cs` |
| `Visual/VertexPainter.cs`       | New | `VertexPainter.cs`       |
| `Visual/DecalDestroyer.cs`      | New | `DecalDestroyer.cs`      |
| `UI/ErrorMessagePopup.cs`       | New | `ErrorMessagePopup.cs`     |
| `UI/InfoMessagePopup.cs`       | New | `InfoMessagePopup.cs`     |

### Modifications to Earlier Phases

| File (from earlier phase)     | Change                      | Why                |
| ---------------------------------- | ------------------------------------------------ | --------------------------------- |
| `Player/PlayerController.cs` (B) | Add noclip toggle on V key (dev mode only)    | Debug flight mode         |
| `Economy/EconomyManager.cs` (A) | Add `UnlockAllShopItems()` call from debug key | Dev shortcut to unlock everything |
| `Shop/ShopUI.cs` (A)       | Show debug categories when dev mode is active  | Debug shop items visible     |

---

## Dependency Chain

```
Phase A ─── foundation (interaction, shop, economy, events)
 │
 ▼
Phase A½ ── mine environment, elevator descent
 │
 ▼
Phase B ─── player, inventory, tools, grabbing, physics, highlighting
 │
 ├──► Phase C ─── ore, mining, pooling, selling
 │    │
 │    ▼
 │  Phase D ─── buildings, conveyors, grid placement
 │    │
 │    ▼
 │  Phase E ─── processing machines (full factory pipeline)
 │
 ├──► Phase F ─── quests, research (can start after B, benefits from C-E)
 │
 ▼
Phase G ─── save/load (needs all above systems to exist)
 │
 ▼
Phase H ─── sound, settings, keybinds (polish layer)
 │
 ├──► Phase I ─── contracts, world events, menus (content layer)
 │
 └──► Phase J ─── debug, demo, final polish
```

---

## Parallel Execution Windows

> If running multiple agents, these are the phases that can be built simultaneously. Each "wave" completes before the next begins. Parallel phases within a wave have no logical dependency on each other.

### Wave Schedule

| Wave | Agents | Phases | Why Parallel Works |
| ---- | ------ | ------ | ------------------ |
| 1 | 1 | **A** | Foundation — nothing else can start without interaction, shop, economy, GameEvents |
| 2 | 1 | **A½** | Needs A's GameEvents for `OnElevatorLanded`, `OnGamePaused` |
| 3 | 1 | **B** | Sole dependency on A + A½. Replaces SimplePlayerController, adds inventory, tools, physics |
| **4** | **2** | **C** ∥ **F** | Both depend only on B. C = ore/mining/pooling, F = quests/research. No logical overlap |
| 5 | 1 | **D** | Needs C — ore pieces must exist for conveyor transport testing |
| 6 | 1 | **E** | Needs D — conveyors must exist for machine I/O pipeline |
| 7 | 1 | **G** | Needs B through F — serializes every system |
| 8 | 1 | **H** | Needs G — sound definitions referenced in save format |
| **9** | **2** | **I** ∥ **J** | I = contracts/menus, J = debug/polish. No shared modifications |

### Visual Timeline

```
Wave: 1  2  3    4     5  6  7  8    9
   ┌──┐ ┌──┐ ┌────┐ ┌────┐  ┌──┐ ┌──┐ ┌──┐ ┌──┐ ┌────┐
Agent1 │ A│ │A½│ │ B │ │ C │───►│ D│ │ E│ │ G│ │ H│ │ I │
   └──┘ └──┘ └────┘ ├────┤  └──┘ └──┘ └──┘ └──┘ ├────┤
Agent2          │ F │             │ J │
            └────┘             └────┘
```

### Merge Conflicts to Resolve After Parallel Waves

**Wave 4 (C ∥ F) — shared file: `GameEvents.cs`**

| Agent | Adds to `GameEvents.cs` |
| ----- | ----------------------- |
| C | `OnOreMined`, `OnOreSold`, `OnOreLimitChanged` |
| F | `OnQuestCompleted`, `OnQuestActivated`, `OnResearchCompleted` |

Both also modify `Core/GameEvents.cs` from Phase A. After both agents finish, merge the event declarations into one file. No logic conflicts — both add independent `event Action` fields.

F also modifies `UI/UIManager.cs` (adds quest tree panel check) and `Shop/ShopManager.cs` (quest unlock wiring). C modifies only `GameEvents.cs`. No overlap beyond GameEvents.

**Wave 9 (I ∥ J) — shared file: `ShopUI.cs`**

| Agent | Modifies |
| ----- | -------- |
| I | `UI/UIManager.cs` (contracts panel), `World/StartingElevator.cs` (new game check), `Menus/PauseMenu.cs` (save/load browser) |
| J | `Player/PlayerController.cs` (noclip), `Economy/EconomyManager.cs` (unlock all), `Shop/ShopUI.cs` (debug categories) |

No file overlap — clean parallel. I touches UIManager/StartingElevator/PauseMenu, J touches PlayerController/EconomyManager/ShopUI.

### Phase F Caveat

F can be **coded** after B, but full **testing** of resource-based quest requirements (e.g., "Sell 10 Iron Ingots") requires C's SellerMachine and OrePiece. Two strategies:

1. **Stub testing** — F agent writes quest logic with mock event invocations, full integration test after C merges
2. **Delayed testing** — F agent writes code in Wave 4, integration testing happens in Wave 5 after C is merged

### Critical Path

The longest sequential chain determines minimum total time:

```
A → A½ → B → C → D → E → G → H → I (or J)
1  2  3  4  5  6  7  8  9 waves
```

Without parallelism: **11 sequential phases**.
With 2 agents: **9 waves** (saving C∥F and I∥J).
**Speedup: 2 phase-durations saved.**

### What Cannot Be Parallelized

| Sequence | Why |
| -------- | --- |
| **C → D → E** | Each machine layer depends on the previous: D needs ore physics from C, E needs conveyors from D |
| **G alone** | Save/load must serialize every system — needs B through F complete |
| **H after G** | Sound definitions get referenced in save format; settings modify controller parameters that G serializes |

---

## Total File Count Estimate

| Phase      | New Files   | Modified Files | Total     |
| --------------- | -------------- | -------------- | -------------- |
| A        | 17       | 0       | 17       |
| B        | 28       | 2       | 30       |
| C        | 19       | 1       | 20       |
| D        | 17       | 1       | 18       |
| E        | 30       | 0       | 30       |
| F        | 20       | 2       | 22       |
| G        | 17       | 0       | 17       |
| H        | 16       | 0       | 16       |
| I        | 20       | 0       | 20       |
| J        | 12       | 0       | 12       |
| **Total** | **~191** | **~6**  | **~197** |

This closely matches the original source's ~200 scripts in `Assembly-CSharp/`.

---

> **Note:** This roadmap may evolve. Files may be added, split, or merged as implementation reveals new needs. The phase boundaries are designed so each phase is self-contained and testable independently.