# Phase A — World Interaction & Shop Cart System

> **Goal:** Build a first-person game where the player walks around, interacts with objects in the world, opens a shop terminal, browses categories, adds items to a cart, and purchases them. Items spawn in the world after purchase.

---

## Table of Contents

1. [Folder Structure](#1-folder-structure)
2. [Architecture Overview](#2-architecture-overview)
3. [Script Reference](#3-script-reference)
4. [Unity Scene Setup (Step-by-Step)](#4-unity-scene-setup-step-by-step)
5. [Creating ScriptableObject Data](#5-creating-scriptableobject-data)
6. [Prefab Setup](#6-prefab-setup)
7. [Wiring It All Together](#7-wiring-it-all-together)
8. [Testing Checklist](#8-testing-checklist)
9. [How Systems Communicate (No Tight Coupling)](#9-how-systems-communicate-no-tight-coupling)

---

## 1. Folder Structure

```
learn/phase-a/
├── GUIDE.md                          ← You are here
└── Scripts/
    ├── Core/
    │   ├── Singleton.cs              — Generic singleton base
    │   └── GameEvents.cs             — Static event bus for decoupled messaging
    ├── Interaction/
    │   ├── IInteractable.cs          — Interface for interactable world objects
    │   ├── SO_Interaction.cs         — ScriptableObject: interaction option data
    │   ├── InteractionSystem.cs      — Raycast-based interaction detection
    │   └── InteractionWheelUI.cs     — Radial menu for multi-option interactions
    ├── Economy/
    │   └── EconomyManager.cs         — Singleton: player money management
    ├── Shop/
    │   ├── SO_ShopItemDefinition.cs  — ScriptableObject: purchasable item data
    │   ├── SO_ShopCategory.cs        — ScriptableObject: category grouping
    │   ├── ShopItem.cs               — Runtime item wrapper (lock/purchase state)
    │   ├── ShopManager.cs            — Singleton: shop state and item registry
    │   ├── ShopTerminal.cs           — In-world interactable that opens shop UI
    │   ├── ShopSpawnPoint.cs         — Marks spawn locations for purchased items
    │   └── UI/
    │       ├── ShopUI.cs             — Main shop panel controller (categories, items, cart)
    │       ├── ShopCategoryButton.cs — Category tab button
    │       ├── ShopItemButton.cs     — Item listing button with "Add to Cart"
    │       └── ShopCartItemButton.cs — Cart entry with quantity editing
    ├── Player/
    │   └── SimplePlayerController.cs — Minimal FPS controller for testing
    └── UI/
        ├── UIManager.cs              — Singleton: global UI state management
        └── MoneyDisplay.cs           — HUD money text (event-driven updates)
```

---

## 2. Architecture Overview

```
┌────────────────────────────────────────────────────┐
│              STATIC EVENT BUS                       │
│              GameEvents.cs                          │
│  OnMoneyChanged · OnItemPurchased                  │
│  OnShopItemUnlocked · OnMenuStateChanged           │
└───────┬──────────────┬──────────────┬──────────────┘
        │              │              │
   ┌────▼────┐   ┌─────▼─────┐  ┌────▼────────┐
   │Economy  │   │ShopManager│  │  UIManager   │
   │Manager  │   │           │  │              │
   │(money)  │   │(items,    │  │(menu state,  │
   │         │   │ categories│  │ cursor)      │
   └────┬────┘   │ locks)    │  └──────────────┘
        │        └─────┬─────┘
        │              │
   ┌────▼──────────────▼─────┐
   │        ShopUI            │
   │  ┌──────────────────┐   │
   │  │ Category Buttons  │   │
   │  │ Item Buttons      │   │
   │  │ Cart Items        │   │
   │  │ Purchase Button   │   │
   │  └──────────────────┘   │
   └──────────────────────────┘
        ▲
        │ (Interact)
   ┌────┴──────────────┐
   │  InteractionSystem │──raycast──► ShopTerminal (IInteractable)
   │  (on Player)       │
   └────────────────────┘
```

### Communication Pattern

**No script directly references another manager.** Instead:

- **`GameEvents`** (static event bus) — Systems fire events, other systems subscribe
- **`Singleton<T>.Instance`** — For reading state (money, menu state) — read-only access pattern
- **`IInteractable` interface** — InteractionSystem doesn't know about ShopTerminal; it only knows the interface

---

## 3. Script Reference

### Core/Singleton.cs

| Item                          | Detail                                        |
| ----------------------------- | --------------------------------------------- |
| **Purpose**             | Generic MonoBehaviour singleton base class    |
| **Attach to**           | Never directly — inherited by managers       |
| **Key property**        | `static T Instance` — global access point  |
| **Method: `Awake()`** | Registers first instance, destroys duplicates |

### Core/GameEvents.cs

| Item                                    | Detail                                                        |
| --------------------------------------- | ------------------------------------------------------------- |
| **Purpose**                       | Static event bus for decoupled system-to-system communication |
| **Attach to**                     | Nothing — it's a static class                                |
| **Event: `OnMoneyChanged`**     | `Action<float>` — new money total                          |
| **Event: `OnItemPurchased`**    | `Action` — an item was bought                              |
| **Event: `OnShopItemUnlocked`** | `Action<ShopItem>` — an item was unlocked                  |
| **Event: `OnMenuStateChanged`** | `Action<bool>` — true if any menu is open                  |

### Interaction/IInteractable.cs

| Item                                              | Detail                                                        |
| ------------------------------------------------- | ------------------------------------------------------------- |
| **Purpose**                                 | Interface for any world object the player can interact with   |
| **Method: `ShouldUseInteractionWheel()`** | Return true to show radial menu, false for direct interaction |
| **Method: `GetInteractions()`**           | Returns list of available SO_Interaction options              |
| **Method: `GetObjectName()`**             | Display name for the interaction prompt                       |
| **Method: `Interact(SO_Interaction)`**    | Executes the chosen interaction                               |

### Interaction/SO_Interaction.cs

| Item              | Detail                                                |
| ----------------- | ----------------------------------------------------- |
| **Purpose** | ScriptableObject defining a single interaction option |
| **Create**  | Assets > Create > Interactions > SO_Interaction          |
| **Fields**  | `Name`, `Description`, `Icon` (Sprite)          |

### Interaction/InteractionSystem.cs

| Item                                      | Detail                                                                    |
| ----------------------------------------- | ------------------------------------------------------------------------- |
| **Purpose**                         | Raycast-based interaction detection from player camera                    |
| **Attach to**                       | Player GameObject                                                         |
| **Inspector**                       | Assign `_playerCamera`, `_interactLayerMask`, `_interactionWheelUI` |
| **Method: `Update()`**            | Checks for interact key (E) press each frame                              |
| **Method: `TryInteract()`**       | Raycasts, finds IInteractable, triggers interaction or opens wheel        |
| **Method: `GetLookedAtObject()`** | Returns the GameObject the player is looking at                           |

### Interaction/InteractionWheelUI.cs

| Item                                             | Detail                                                                          |
| ------------------------------------------------ | ------------------------------------------------------------------------------- |
| **Purpose**                                | Radial menu that appears when an object has multiple interactions               |
| **Attach to**                              | A UI Panel (initially inactive)                                                 |
| **Inspector**                              | Assign `_interactionButtonPrefab`, `_contentTransform`, `_objectNameText` |
| **Method: `PopulateInteractionWheel()`** | Creates buttons for each interaction on the object                              |
| **Method: `CloseWheel()`**               | Clears buttons and hides the panel                                              |
| **Method: `ClearInteractionWheel()`**    | Destroys all spawned buttons                                                    |

### Economy/EconomyManager.cs

| Item                                     | Detail                                      |
| ---------------------------------------- | ------------------------------------------- |
| **Purpose**                        | Singleton owning the player's money         |
| **Attach to**                      | Empty GameObject named `[EconomyManager]` |
| **Inspector**                      | Set starting `_money` value (default 500) |
| **Property: `Money`**            | Get/set with automatic event firing         |
| **Method: `AddMoney(float)`**    | Adds or subtracts money                     |
| **Method: `CanAfford(float)`**   | Returns true if player has enough           |
| **Method: `FormatMoney(float)`** | Static — formats as "$1,234.56"            |

### Shop/SO_ShopItemDefinition.cs

| Item              | Detail                                                                                                                    |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------- |
| **Purpose** | ScriptableObject defining a purchasable item                                                                              |
| **Create**  | Assets > Create > Shop > SO_ShopItemDefinition                                                                               |
| **Fields**  | `Name`, `Description`, `Price` (int), `Icon` (Sprite), `PrefabToSpawn`, `IsLockedByDefault`, `MaxStackSize` |

### Shop/SO_ShopCategory.cs

| Item              | Detail                                                                |
| ----------------- | --------------------------------------------------------------------- |
| **Purpose** | ScriptableObject grouping items into a category tab                   |
| **Create**  | Assets > Create > Shop > SO_ShopCategory                                 |
| **Fields**  | `CategoryName`, `ShopItemDefinitions` (list), `HideIfAllLocked` |
| **Runtime** | `ShopItems` list populated by ShopManager on Start                  |

### Shop/ShopItem.cs

| Item                                    | Detail                                                             |
| --------------------------------------- | ------------------------------------------------------------------ |
| **Purpose**                       | Runtime wrapper around SO_ShopItemDefinition with lock/purchase state |
| **Created by**                    | ShopManager during Start()                                         |
| **Method: `GetPrice()`**        | Returns price from definition                                      |
| **Method: `IsNewlyUnlocked()`** | True if was locked, now unlocked, and never purchased              |
| **Method: `AddPurchase(int)`**  | Increments purchase counter                                        |

### Shop/ShopManager.cs

| Item                                           | Detail                                                    |
| ---------------------------------------------- | --------------------------------------------------------- |
| **Purpose**                              | Singleton managing all shop categories and items          |
| **Attach to**                            | Empty GameObject named `[ShopManager]`                  |
| **Inspector**                            | Assign `_allShopCategories` list of SO_ShopCategory assets |
| **Method: `GetAvailableCategories()`** | Returns all categories                                    |
| **Method: `FindItemByDefinition()`**   | Finds runtime ShopItem by definition                      |
| **Method: `UnlockShopItem()`**         | Unlocks a specific item by definition                     |

### Shop/ShopTerminal.cs

| Item                             | Detail                                                      |
| -------------------------------- | ----------------------------------------------------------- |
| **Purpose**                | In-world IInteractable that opens the shop UI               |
| **Attach to**              | 3D object with a Collider (on Interactable layer)           |
| **Inspector**              | Assign an SO_Interaction ScriptableObject to `_interactions` |
| **Method: `Interact()`** | Toggles the ShopUI active state                             |

### Shop/ShopSpawnPoint.cs

| Item                                        | Detail                                                     |
| ------------------------------------------- | ---------------------------------------------------------- |
| **Purpose**                           | Marks a transform as a valid purchased-item spawn location |
| **Attach to**                         | Empty GameObjects positioned near the shop terminal        |
| **Method: `GetRandomSpawnPoint()`** | Static — picks a random point from the scene              |

### Shop/UI/ShopUI.cs

| Item                                    | Detail                                                                |
| --------------------------------------- | --------------------------------------------------------------------- |
| **Purpose**                       | Main shop panel controller — categories, items, cart, purchase       |
| **Attach to**                     | Root shop UI Panel (initially inactive)                               |
| **Inspector**                     | Assign all container transforms, prefabs, text references, and colors |
| **Method: `SetupCategories()`** | Creates category tab buttons                                          |
| **Method: `OpenCategory()`**    | Switches displayed items to selected category                         |
| **Method: `AddToCart()`**       | Adds item to cart (merges if existing)                                |
| **Method: `PurchaseCart()`**    | Buys all cart items, spawns prefabs, deducts money                    |
| **Method: `CanAffordCart()`**   | Returns true if player can afford total cart                          |

### Shop/UI/ShopCategoryButton.cs

| Item                                    | Detail                                              |
| --------------------------------------- | --------------------------------------------------- |
| **Purpose**                       | Category tab button in the shop UI                  |
| **Attach to**                     | Prefab: Button with Text child                      |
| **Inspector**                     | Assign `_backgroundImage`, `_nameText`, colors  |
| **Method: `Initialize()`**      | Sets category data and label                        |
| **Method: `SetSelected()`**     | Changes background color for selected state         |
| **Method: `OnButtonPressed()`** | Fires `OnPressed` event — wire to Button.OnClick |

### Shop/UI/ShopItemButton.cs

| Item                               | Detail                                                        |
| ---------------------------------- | ------------------------------------------------------------- |
| **Purpose**                  | Item listing button with name, price, icon, and "Add to Cart" |
| **Attach to**                | Prefab: Panel with Button, Texts, Image                       |
| **Inspector**                | Assign all text/image references and colors                   |
| **Method: `Initialize()`** | Configures from ShopItem data                                 |
| **Method: `UpdateUI()`**   | Refreshes affordability and lock state visuals                |

### Shop/UI/ShopCartItemButton.cs

| Item                                   | Detail                                                    |
| -------------------------------------- | --------------------------------------------------------- |
| **Purpose**                      | Cart entry with quantity editing, +/- buttons, and remove |
| **Attach to**                    | Prefab: Panel with Texts, InputField, Buttons             |
| **Method: `Initialize()`**     | Sets item data and initial quantity                       |
| **Method: `ChangeQuantity()`** | Clamps to affordable range, removes if zero               |
| **Method: `AddQuantity()`**    | Increments/decrements quantity                            |
| **Method: `RemoveFromCart()`** | Sets quantity to 0 (triggers removal)                     |

### Player/SimplePlayerController.cs

| Item                | Detail                                                             |
| ------------------- | ------------------------------------------------------------------ |
| **Purpose**   | Minimal FPS controller for testing                                 |
| **Attach to** | Player GameObject with CharacterController                         |
| **Inspector** | Assign `_playerCamera`, set speeds                               |
| **Behavior**  | WASD move, mouse look, auto cursor lock/unlock based on menu state |

### UI/UIManager.cs

| Item                                      | Detail                                                  |
| ----------------------------------------- | ------------------------------------------------------- |
| **Purpose**                         | Singleton managing global UI state                      |
| **Attach to**                       | Empty GameObject named `[UIManager]`                  |
| **Inspector**                       | Assign `ShopUI` and `InteractionWheelUI` references |
| **Method: `IsInAnyMenu()`**       | Returns true if any menu panel is active                |
| **Method: `SetBackgroundBlur()`** | Toggles blur overlay                                    |

### UI/MoneyDisplay.cs

| Item                    | Detail                                                 |
| ----------------------- | ------------------------------------------------------ |
| **Purpose**       | HUD text showing current money (event-driven)          |
| **Attach to**     | UI Text object on the HUD canvas                       |
| **Inspector**     | Assign `_moneyText`                                  |
| **Subscribes to** | `GameEvents.OnMoneyChanged` — updates automatically |

---

## 4. Unity Scene Setup (Step-by-Step)

### Step 1: Create a New Unity Project

1. Open Unity Hub → New Project → **3D (Built-in Render Pipeline)**
2. Name it `MineGameLearn` or similar
3. Copy all scripts from `phase-a/Scripts/` into your project's `Assets/Scripts/` folder

### Step 2: Create Layers

Go to **Edit > Project Settings > Tags and Layers** and add:

- Layer 6: `Interactable`

### Step 3: Build the Scene Ground

1. **GameObject > 3D Object > Plane** — name it `Ground`
2. Scale: `(10, 1, 10)` — gives a 100×100 unit floor
3. Add a material with a dark color

### Step 4: Create the Player

1. **Create Empty** → name it `Player`
2. Position: `(0, 1.5, 0)`
3. Add component: **CharacterController** (Height: 2, Center: 0,0,0)
4. Add component: **SimplePlayerController**
5. **Create child Camera** → name it `PlayerCamera`
   - Position: `(0, 0.8, 0)` (eye height)
   - Tag: `MainCamera`
6. Add component: **InteractionSystem** to `Player`
   - Assign `PlayerCamera` to `_playerCamera`
   - Set `_interactLayerMask` to include `Interactable` layer
7. Add component: **AudioListener** to `PlayerCamera` (remove from any other camera)

### Step 5: Create Manager GameObjects

Create these **empty GameObjects** at root:

| Name                 | Components                                     |
| -------------------- | ---------------------------------------------- |
| `[EconomyManager]` | `EconomyManager` (set starting money to 500) |
| `[ShopManager]`    | `ShopManager` (assign categories later)      |
| `[UIManager]`      | `UIManager` (assign UI references later)     |

### Step 6: Create the Shop Terminal (World Object)

1. **GameObject > 3D Object > Cube** → name it `ShopTerminal`
2. Position near `(3, 0.5, 3)`
3. Scale: `(1, 1, 0.5)` — a desk/monitor shape
4. Set Layer to **Interactable**
5. Add component: **ShopTerminal**
6. Create an Interaction ScriptableObject:
   - **Assets > Create > Interactions > Interaction**
   - Name: `UseComputer`, set Name field to "Use"
7. Add the `UseComputer` interaction to the ShopTerminal's `_interactions` list

### Step 7: Create Shop Spawn Points

1. Create 2-3 **Empty GameObjects** near the terminal
2. Name them `ShopSpawnPoint_1`, `ShopSpawnPoint_2`, etc.
3. Add component: **ShopSpawnPoint** to each
4. Position them slightly in front of the terminal (where items should appear)

### Step 8: Build the UI Canvas

1. **GameObject > UI > Canvas** → name it `UICanvas`
   - Canvas Scaler: Scale With Screen Size, Reference: 1920×1080
2. Add component: **Canvas Group** (for potential fading)
3. Unity should auto-create an **EventSystem** GameObject. If not, create one: **GameObject > UI > Event System**. Without this, no UI buttons will respond to clicks.

#### 8a. HUD Panel

1. Create **Panel** child of `UICanvas` → name it `HUD`
2. Anchor: Top-Left
3. Add a **Text** child → name it `MoneyText`
   - Text: "$500.00", Font Size: 24, Color: Green
   - Anchor: Top-Right corner of HUD
4. Add component: **MoneyDisplay** to `MoneyText`
   - Assign itself as `_moneyText`

#### 8b. Background Blur (Optional)

1. Create **Panel** child of `UICanvas` → name it `BackgroundBlur`
2. Set color to `(0, 0, 0, 0.5)` semi-transparent black
3. **Initially inactive** (unchecked in inspector)

#### 8c. Interaction Wheel Panel

1. Create **Panel** child of `UICanvas` → name it `InteractionWheelPanel`
2. Center it on screen, size ~300×400
3. Add a **Text** child → `ObjectNameText` (top of panel)
4. Add a **Vertical Layout Group** child → `ButtonsContainer`
5. Add component: **InteractionWheelUI**
   - Assign `_contentTransform` = `ButtonsContainer`
   - Assign `_objectNameText` = `ObjectNameText`
6. **Initially inactive**

##### Interaction Button Prefab

1. Create a **Button** inside `ButtonsContainer` temporarily
2. Set it up with a Text child showing "Action Name"
3. Drag it to `Assets/Prefabs/` to make a prefab
4. Delete the instance from the scene
5. Assign the prefab to InteractionWheelUI's `_interactionButtonPrefab`

#### 8d. Shop UI Panel

1. Create **Panel** child of `UICanvas` → name it `ShopPanel`
2. Fill most of the screen, add a dark background
3. **Initially inactive**

**Layout inside ShopPanel:**

```
┌──────────────────────────────────────────────────────┐
│ [Money: $500.00]                              [X]    │
├──────────┬───────────────────────┬───────────────────┤
│ Category │     Item List         │     Cart          │
│ Tabs     │                      │                    │
│ ┌──────┐ │ ┌──────────────────┐ │ ┌───────────────┐ │
│ │Tools │ │ │ Item Name    $50 │ │ │ Item  qty  $  │ │
│ │------│ │ │ Description      │ │ │ [+] [-] [X]   │ │
│ │Build │ │ │ [Add to Cart]    │ │ │               │ │
│ │------│ │ ├──────────────────┤ │ ├───────────────┤ │
│ │Misc  │ │ │ Item Name   $100 │ │ │               │ │
│ └──────┘ │ │ Description      │ │ │               │ │
│          │ │ [Add to Cart]    │ │ │               │ │
│          │ └──────────────────┘ │ │               │ │
│          │                      │ │               │ │
├──────────┴──────────────────────┴─┤               │ │
│ Cart Total: $150    [Purchase]    │               │ │
└───────────────────────────────────┴───────────────┘ │
```

**Create these containers as child GameObjects:**

| Name                  | Type                           | Layout Component                | Notes         |
| --------------------- | ------------------------------ | ------------------------------- | ------------- |
| `CategoryContainer` | Empty with VerticalLayoutGroup | Content Size Fitter (Preferred) | Left column   |
| `ItemListContainer` | Empty with VerticalLayoutGroup | Inside a ScrollRect (see below) | Center column |
| `CartListContainer` | Empty with VerticalLayoutGroup | Inside a ScrollRect (see below) | Right column  |

**Setting up ScrollRect for ItemList and Cart:**

1. Create an empty `ItemListScrollArea` as child of ShopPanel
2. Add **ScrollRect** component to it, uncheck Horizontal
3. Create `ItemListContainer` as child of `ItemListScrollArea`
4. Add **VerticalLayoutGroup** + **ContentSizeFitter** (Vertical Fit: Preferred Size) to `ItemListContainer`
5. Assign `ItemListContainer` as the ScrollRect's **Content** field
6. Repeat the same for `CartListScrollArea` / `CartListContainer`
   | `MoneyText` | Text | — | Top bar, shows current money |
   | `CartTotalText` | Text | — | Bottom bar, shows cart total |
   | `PurchaseButton` | Button | — | Bottom bar, calls `ShopUI.PurchaseCart()` |
7. Add component: **ShopUI** to `ShopPanel`
8. Assign all inspector references (containers, texts, button, prefabs)
9. Wire `PurchaseButton.OnClick` → `ShopUI.PurchaseCart()`

#### 8e. Create UI Prefabs

##### Category Button Prefab

1. Create a **Button** with:
   - `Image` component (background)
   - Child `Text` (category name)
2. Add component: **ShopCategoryButton**
   - Assign `_backgroundImage` and `_nameText`
   - Set selected/normal colors
3. Wire `Button.OnClick` → `ShopCategoryButton.OnButtonPressed()`
4. Save as prefab: `CategoryButtonPrefab`

##### Shop Item Button Prefab

1. Create a **Panel** with:
   - Child `Text` → `ItemNameText`
   - Child `Text` → `ItemDescriptionText`
   - Child `Text` → `ItemPriceText`
   - Child `Image` → `ItemIcon`
   - Child `Button` → with child `Text` ("Add to Cart") → `ButtonText`
2. Add component: **ShopItemButton**
   - Assign all references
   - Set colors
3. Save as prefab: `ShopItemButtonPrefab`

##### Cart Item Button Prefab

1. Create a **Panel** with:
   - Child `Text` → `ItemNameText`
   - Child `Text` → `ItemPriceText`
   - Child `Image` → `ItemIcon`
   - Child `InputField` → for quantity editing
   - Child `Button` ("+") → OnClick calls `ShopCartItemButton.AddQuantity(1)`
   - Child `Button` ("-") → OnClick calls `ShopCartItemButton.AddQuantity(-1)`
   - Child `Button` ("X") → OnClick calls `ShopCartItemButton.RemoveFromCart()`
2. Add component: **ShopCartItemButton**
   - Assign all references
3. Save as prefab: `CartItemButtonPrefab`

### Step 9: Wire UIManager References

Select `[UIManager]` and assign:

- `ShopUI` → the `ShopPanel` (which has ShopUI component)
- `InteractionWheelUI` → the `InteractionWheelPanel`
- `_hudObject` → the `HUD` panel
- `_backgroundBlur` → the `BackgroundBlur` panel

### Step 10: Wire InteractionSystem

Select `Player` → InteractionSystem component:

- `_interactionWheelUI` → the `InteractionWheelPanel`

---

## 5. Creating ScriptableObject Data

### Create Interaction Assets

1. **Assets > Create > Interactions > SO_Interaction**
   - Name: `UseComputer`, field Name: "Use"

### Create Shop Item Definitions

1. **Assets > Create > Shop > SO_ShopItemDefinition**
   - Example: `Pickaxe` — Name: "Pickaxe", Price: 50, Icon: any sprite, PrefabToSpawn: a cube prefab
   - Example: `Lantern` — Name: "Lantern", Price: 30, Icon: any sprite, PrefabToSpawn: a sphere prefab
   - Example: `Dynamite` — Name: "Dynamite", Price: 100, Icon: any sprite, IsLockedByDefault: true

### Create Shop Categories

1. **Assets > Create > Shop > SO_ShopCategory**
   - Example: `Tools` — CategoryName: "Tools", add Pickaxe + Lantern definitions
   - Example: `Explosives` — CategoryName: "Explosives", add Dynamite, HideIfAllLocked: true

### Create Placeholder Prefabs (for spawning)

1. Create simple 3D objects (Cube, Sphere, Cylinder)
2. Add a Rigidbody to each
3. Save as prefabs in `Assets/Prefabs/`
4. Assign them to the SO_ShopItemDefinition's `PrefabToSpawn` field

### Wire Categories to ShopManager

1. Select `[ShopManager]`
2. Add your SO_ShopCategory assets to `_allShopCategories` list

---

## 6. Prefab Setup

| Prefab                      | Components                                                     | Purpose                  |
| --------------------------- | -------------------------------------------------------------- | ------------------------ |
| `CategoryButtonPrefab`    | Button, Image, Text,**ShopCategoryButton**               | Category tab in shop     |
| `ShopItemButtonPrefab`    | Panel, Button, Texts, Image,**ShopItemButton**           | Item listing entry       |
| `CartItemButtonPrefab`    | Panel, Texts, InputField, Buttons,**ShopCartItemButton** | Cart entry               |
| `InteractionButtonPrefab` | Button, Text                                                   | Interaction wheel option |
| `Pickaxe_Prefab`          | Cube + Rigidbody                                               | Spawned item example     |
| `Lantern_Prefab`          | Sphere + Rigidbody                                             | Spawned item example     |

---

## 7. Wiring It All Together

### Final Inspector Checklist

| GameObject           | Component              | Field                   | Assign To                 |
| -------------------- | ---------------------- | ----------------------- | ------------------------- |
| `Player`           | InteractionSystem      | `_playerCamera`       | `PlayerCamera`          |
| `Player`           | InteractionSystem      | `_interactionWheelUI` | `InteractionWheelPanel` |
| `Player`           | InteractionSystem      | `_interactLayerMask`  | `Interactable` layer    |
| `Player`           | SimplePlayerController | `_playerCamera`       | `PlayerCamera`          |
| `[EconomyManager]` | EconomyManager         | `_money`              | 500                       |
| `[ShopManager]`    | ShopManager            | `_allShopCategories`  | Your category assets      |
| `[UIManager]`      | UIManager              | `ShopUI`              | `ShopPanel`             |
| `[UIManager]`      | UIManager              | `InteractionWheelUI`  | `InteractionWheelPanel` |
| `[UIManager]`      | UIManager              | `_backgroundBlur`     | `BackgroundBlur` panel  |
| `[UIManager]`      | UIManager              | `_hudObject`          | `HUD` panel             |
| `ShopPanel`        | ShopUI                 | all containers          | Respective UI containers  |
| `ShopPanel`        | ShopUI                 | prefabs                 | Your UI prefabs           |
| `ShopPanel`        | ShopUI                 | `_purchaseButton`     | Purchase button           |
| `ShopTerminal`     | ShopTerminal           | `_interactions`       | `UseComputer` asset     |
| `MoneyText`        | MoneyDisplay           | `_moneyText`          | Self (the Text component) |

---

## 8. Testing Checklist

Run the scene and verify:

- [ ] **Player movement** — WASD moves, mouse looks around
- [ ] **Cursor** — Locked during gameplay, free when shop is open
- [ ] **Interaction** — Walk to terminal, press E → shop opens
- [ ] **Categories** — Tabs appear, clicking switches item list
- [ ] **Item buttons** — Show name, price, description, icon
- [ ] **Locked items** — Show "Locked" text, button disabled
- [ ] **Add to cart** — Click "Add to Cart" → entry appears in cart panel
- [ ] **Cart quantity** — +/- buttons work, input field accepts typed numbers
- [ ] **Cart total** — Updates as items are added/removed
- [ ] **Can't afford** — Purchase button disabled when total > money, red total text
- [ ] **Purchase** — Click Purchase → money decreases, items spawn at spawn points
- [ ] **Cart clears** — Purchased items removed from cart
- [ ] **Money display** — HUD text updates immediately on purchase
- [ ] **Close shop** — Press ESC or E → shop closes, cursor re-locks
- [ ] **Interaction wheel** — If you add multiple IInteractable components, wheel appears

---

## 9. How Systems Communicate (No Tight Coupling)

Every cross-system communication uses either **GameEvents** (static event bus) or the **IInteractable interface**. No script directly references an unrelated system.

### Problem: How does MoneyDisplay know money changed?

**NOT this (tight coupling):**

```csharp
// MoneyDisplay.Update()
text = FindObjectOfType<EconomyManager>().Money; // BAD: polls every frame, direct reference
```

**Instead (event-driven via GameEvents):**

```csharp
// EconomyManager.Money setter → GameEvents.RaiseMoneyChanged(amount)
// MoneyDisplay subscribes to GameEvents.OnMoneyChanged on OnEnable
// MoneyDisplay updates text only when the event fires — zero polling
```

### Problem: How does ShopUI know an item was unlocked?

**NOT this (tight coupling):**

```csharp
// ShopManager calls shopUI.RefreshItems() directly // BAD: ShopManager knows about ShopUI
```

**Instead (event-driven):**

```csharp
// ShopManager.UnlockShopItem() → GameEvents.RaiseShopItemUnlocked(item)
// ShopUI subscribes to GameEvents.OnShopItemUnlocked on OnEnable
// ShopUI refreshes its item list when the event fires
// ShopManager has zero knowledge of ShopUI's existence
```

### Problem: How does the player controller know a menu is open?

**NOT this (tight coupling):**

```csharp
// Every frame: player polls UIManager directly
bool open = Singleton<UIManager>.Instance.IsInAnyMenu(); // BAD: direct dependency
```

**Instead (event-driven via GameEvents):**

```csharp
// ShopUI.OnEnable()  → GameEvents.RaiseMenuStateChanged(true)
// ShopUI.OnDisable() → GameEvents.RaiseMenuStateChanged(false)
// SimplePlayerController subscribes to GameEvents.OnMenuStateChanged in Start()
// Caches _anyMenuOpen bool — zero per-frame singleton access
// Player has zero knowledge of UIManager or ShopUI
```

### Problem: How does the ShopTerminal open the shop?

**NOT this (tight coupling):**

```csharp
// ShopTerminal directly reaches into UIManager → ShopUI → toggles it
Singleton<UIManager>.Instance.ShopUI.gameObject.SetActive(true); // BAD: knows 2 unrelated classes
```

**Instead (event-driven):**

```csharp
// ShopTerminal.Interact() → GameEvents.RaiseToggleShopRequested()
// ShopUI subscribes to GameEvents.OnToggleShopRequested in Start()
// ShopUI toggles itself — ShopTerminal has zero knowledge of ShopUI
```

### Problem: How does InteractionSystem trigger things?

**Through the IInteractable interface (abstraction):**

```csharp
// InteractionSystem raycasts → finds IInteractable → calls Interact()
// It doesn't know it's a ShopTerminal — could be any IInteractable
// Future phases: OreNode, Machine, BuildingObject all just implement IInteractable
// InteractionSystem code never changes
```

### Full Event Flow Diagram

```
ShopTerminal.Interact()
    → GameEvents.RaiseToggleShopRequested()
        → ShopUI.OnToggleShopRequested()        [toggles panel]

ShopUI.OnEnable()
    → GameEvents.RaiseMenuStateChanged(true)
        → SimplePlayerController.OnMenuStateChanged()  [unlocks cursor]

ShopUI.OnDisable()
    → GameEvents.RaiseMenuStateChanged(false)
        → SimplePlayerController.OnMenuStateChanged()  [locks cursor]

EconomyManager.Money setter
    → GameEvents.RaiseMoneyChanged(amount)
        → MoneyDisplay.OnMoneyChanged()          [updates HUD text]

ShopManager.UnlockShopItem()
    → GameEvents.RaiseShopItemUnlocked(item)
        → ShopUI.OnShopItemUnlocked()            [refreshes item list]

ShopUI.PurchaseCart()
    → GameEvents.RaiseItemPurchased()
        → (future: quest system, analytics, etc.)

```

---

## 10. What Phase A Looks Like When Running

When you hit Play, this is what you should experience:

```
┌─────────────────────────────────────────────────────────┐
│                                                         │
│   You're a first-person player on a flat ground plane.  │
│   WASD to walk, mouse to look around.                   │
│                                                         │
│   In front of you: a cube representing a shop terminal. │
│   Walk up to it, press E.                               │
│                                                         │
│   A shop panel fills the screen:                        │
│   - Left: category tabs (Tools, Explosives, etc.)       │
│   - Center: scrollable item list with icons and prices  │
│   - Right: your cart (empty at first)                   │
│                                                         │
│   Click "Add to Cart" on a Pickaxe ($50).               │
│   Cart shows: Pickaxe x1  $50                           │
│   Use +/- or type a number to change quantity.           │
│                                                         │
│   Add a Lantern ($30). Cart total: $80.                 │
│   Your money: $500. Purchase button is green.            │
│                                                         │
│   Click Purchase.                                       │
│   → Money drops to $420 (HUD updates instantly)         │
│   → A cube and sphere spawn near the terminal           │
│   → Cart clears                                         │
│                                                         │
│   Press ESC or E to close shop. Cursor re-locks.        │
│   Walk around — the spawned objects are on the ground.   │
│                                                         │
│   Locked items show "Locked" with greyed-out buttons.    │
│   If you spend too much, "Can't Afford" appears in red. │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

**Visually it's simple** — placeholder cubes/spheres, basic Unity UI panels. The value is in the *systems* working correctly: interaction, categories, cart math, purchase flow, event-driven updates, cursor management.

---

## 11. LEGO-Linkability to Future Phases

Phase A is designed so future phases snap on without modifying existing code. Here's exactly how:

### Extension Points Built Into Phase A

| Phase A Piece                                  | Future Phase Uses It By...                                                               | What Changes in Phase A?                                          |
| ---------------------------------------------- | ---------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| **`IInteractable`**                    | OreNode, Machine, BuildingObject all `implement IInteractable`                         | Nothing. InteractionSystem already handles any IInteractable.     |
| **`GameEvents`**                       | Add new events:`OnOreMined`, `OnBuildingPlaced`, `OnQuestCompleted`                | Only add new lines — never modify existing events.               |
| **`Singleton<T>`**                     | New managers:`SavingLoadingManager`, `QuestManager`, `BuildingManager`             | Nothing. They just inherit from Singleton.                        |
| **`EconomyManager`**                   | Quest rewards call `AddMoney()`, selling ore calls `AddMoney()`                      | Nothing. Already has the API.                                     |
| **`ShopManager.UnlockShopItem()`**     | Quest system calls it when player completes a quest                                      | Nothing. Already wired to fire `GameEvents.OnShopItemUnlocked`. |
| **`SO_ShopItemDefinition.PrefabToSpawn`** | Point it at a BuildingCrate prefab instead of a plain cube                            | Just change the ScriptableObject data — zero code changes.       |
| **`InteractionSystem`**                | Works for machines, ore nodes, terminals, anything with a collider on Interactable layer | Nothing. Already generic.                                         |
| **`UIManager.IsInAnyMenu()`**          | Add new panels: PauseMenu, QuestTree, Inventory. Just check them in `IsInAnyMenu()`.   | Add one `if` check per new panel.                               |
| **`GameEvents.OnMenuStateChanged`**    | New menus fire it on enable/disable, player cursor auto-responds                         | Nothing. Player already subscribes.                               |
| **`GameEvents.OnItemPurchased`**       | Quest system subscribes to track "Buy 5 items" quests                                    | Nothing. Event already fires.                                     |

### What a Phase B Addition Looks Like (Example: Mining)

```csharp
// New file: OreNode.cs — Phase B
// Zero changes to any Phase A file

public class OreNode : MonoBehaviour, IInteractable, IDamageable
{
    // Implements IInteractable → InteractionSystem already detects it
    // When mined → GameEvents.RaiseOreMined(resourceType)
    // Quest system subscribes → tracks mining progress
    // Economy untouched, shop untouched, player untouched
}
```

### What a Phase C Addition Looks Like (Example: Quest Unlocking Shop Items)

```csharp
// New file: QuestManager.cs — Phase C
// Zero changes to any Phase A file

private void OnQuestCompleted(Quest quest)
{
    // Uses existing Phase A API:
    Singleton<ShopManager>.Instance.UnlockShopItem(quest.RewardItem);
    Singleton<EconomyManager>.Instance.AddMoney(quest.RewardMoney);
    // ShopUI auto-refreshes via GameEvents.OnShopItemUnlocked
    // MoneyDisplay auto-refreshes via GameEvents.OnMoneyChanged
}
```

### The Rule

> **Adding a new phase should mean adding new files and new ScriptableObject data. Existing Phase A scripts should rarely need modification — and when they do, it's a one-line addition (like adding a panel check to `IsInAnyMenu`), never a rewrite.**

---

## 12. Source vs Phase A — What Changed and Why

This section documents every place where Phase A deviates from the original source, and why.

| Area | Original Source Did | Phase A Does Instead | Why |
|------|-------------------|---------------------|-----|
| **ShopTerminal → ShopUI** | `ComputerTerminal.Interact()` directly calls `Singleton<UIManager>.Instance.ComputerShopUI.gameObject.SetActive()` — reaches across 2 system boundaries | `ShopTerminal.Interact()` fires `GameEvents.RaiseToggleShopRequested()`. ShopUI subscribes and toggles itself. | Decoupling. ShopTerminal has zero knowledge of UIManager or ShopUI. Adding a second terminal requires zero code changes. |
| **Player cursor management** | `PlayerController.Update()` polls `Singleton<UIManager>.Instance.IsInAnyMenu()` every frame — direct dependency on UIManager | `SimplePlayerController` subscribes to `GameEvents.OnMenuStateChanged` in `Start()`, caches `_anyMenuOpen` bool. Zero per-frame singleton access. | Decoupling. Player has zero knowledge of UIManager. Any new menu just fires the same event. |
| **Money change notification** | `EconomyManager` has a local `event Action<float> OnMoneyUpdated`. Subscribers must hold a reference to EconomyManager. | EconomyManager fires both local event AND `GameEvents.RaiseMoneyChanged()`. MoneyDisplay subscribes to the static event — no reference needed. | Decoupling. MoneyDisplay doesn't need to know EconomyManager exists. |
| **Shop item unlocking** | `EconomyManager.UnlockShopItem()` fires local `event Action<ShopItem> ShopItemUnlocked`. `ComputerShopUI` subscribes directly to `EconomyManager.Instance.ShopItemUnlocked`. | `ShopManager.UnlockShopItem()` fires `GameEvents.RaiseShopItemUnlocked()`. ShopUI subscribes to the static event. | Decoupling. ShopUI doesn't subscribe to a specific manager instance. |
| **SO_ShopCategory** | Plain `[Serializable]` class stored in a `List<ShopCategory>` on EconomyManager. | `ScriptableObject` (`SO_ShopCategory`) created via `Assets > Create > Shop > SO_ShopCategory`. | Better editor workflow. Each category is its own asset file — easier to manage, reorder, and assign in inspector. |
| **Shop ownership** | Shop items, categories, and purchases all live on `EconomyManager` — single god-manager. | Split into `EconomyManager` (money only) and `ShopManager` (items, categories, locks). | Single Responsibility. Economy doesn't need to know about shop categories. |
| **Sound on actions** | `ComputerShopUI` plays sounds via `Singleton<SoundManager>.Instance.PlaySoundAtLocation()` on add/remove/purchase. | Phase A omits sound system entirely — will be added in a later phase. | Scope. Phase A focuses on interaction + cart logic. Sound is a separate system. |
| **SO_ShopItemDefinition** | Original `ShopItemDefinition` has `BuildingInventoryDefinition` reference, `UseNameAndDescriptionOfBuildingDefinition` flag, demo lock logic, and `SavableObjectID` lookup. | Simplified to core fields: `Name`, `Description`, `Price`, `Icon`, `PrefabToSpawn`, `IsLockedByDefault`, `MaxStackSize`. Renamed to `SO_ShopItemDefinition`. | Phase A doesn't have buildings, save/load, or demo mode yet. Fields will be added in future phases when those systems exist. |
| **ShopItem purchase tracking** | Tracked in a separate `ShopPurchases` class with `SavableObjectID` keys for save/load persistence. | Tracked as a simple `_timesPurchased` int on `ShopItem`. | Phase A has no save/load. Will be replaced with persistent tracking when save system is added. |
| **Interaction prompt text** | Original uses `KeybindTokenText` with `[ActionName]` token replacement to show rebindable key names. | Phase A uses hardcoded `KeyCode.E` for interaction. | Phase A has no keybind system. Will be swapped to Input System + rebinding in a later phase. |
| **SO_ naming convention** | Original source uses plain names: `Interaction`, `ShopItemDefinition`, `ShopCategory` for ScriptableObject classes. | All ScriptableObject classes prefixed with `SO_`: `SO_Interaction`, `SO_ShopItemDefinition`, `SO_ShopCategory`. | Convention makes it instantly clear which classes are data-only ScriptableObjects vs runtime MonoBehaviours. Prevents naming confusion in larger projects. |

### What Stayed the Same

These patterns matched the original source exactly — no changes needed:

- **`Singleton<T>`** base class — identical implementation
- **`IInteractable`** interface — same 4 methods
- **`SO_Interaction`** ScriptableObject — same fields (Name, Description, Icon)
- **`InteractionWheelUI`** — same dynamic button spawning pattern
- **`ShopUI` cart logic** — same add/merge/remove/purchase flow
- **`ShopItemButton` UI update logic** — same affordability + lock state checks
- **`ShopCartItemButton` quantity clamping** — same max-affordable calculation
- **`ShopSpawnPoint`** — same `FindObjectsOfType` + random selection

---

## Next Phase Preview

Phase B will add:

- Mining system (OreNode, OrePiece, pickaxe tool)
- Inventory system (hotbar, tool equipping)
- Building placement system (grid-based, ghost preview)
- Save/Load foundation
