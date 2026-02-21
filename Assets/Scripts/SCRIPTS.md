# Azoth -- Scripts Reference

Scripts for a 2D tileset-based alchemy game built on Unity 6 (URP) with the new
Input System, Tilemap packages, and a turn-based alchemy combat system.

---

## Architecture Overview

```
GameManager  (singleton, game state: Playing / Paused / Dialogue / Battle)
     |
PlayerController  (movement, sprint, interaction)
     |         \
 Inventory      IInteractable ----+---- NPC             (dialogue, patrol)
     |                            +---- ItemPickup       (world items)
 ItemData (SO)                    +---- BattleEncounter  (triggers fights)
     |
     +-- ItemCategory enum (Catalyst / Gear / Potion / Trinket / Key / Ingredient / MagnumOpus)
     +-- ItemRarity enum  (Common / Uncommon / Rare / Epic / Legendary)
     +-- GearSlot enum    (None / Head / Body / Shield / Weapon / Ring / Amulet)
     +-- Element enum     (Ignis, Aqua, Terra, Ventus, Lux, Umbra)

PlayerCombat  (persistent player HP/MP/stats/skills)
     |
BattleManager  (singleton, turn-based state machine)
     |  events only -- zero UI refs
BattleUI  (event-driven panels, buttons, log)

SkillData (SO)  -- skill definitions
EnemyData (SO)  -- enemy stat templates
Combatant       -- runtime battle participant (plain C#)

CameraFollow  (smooth camera tracking, optional bounds)
```

### Folder Structure

```
Assets/Scripts/
  SCRIPTS.md
  Core/           GameManager.cs, CameraFollow.cs
  Player/         PlayerController.cs, PlayerCombat.cs
  Items/          ItemData.cs, ItemPickup.cs, Inventory.cs
  Battle/         BattleManager.cs, BattleUI.cs, BattleEncounter.cs,
                  Combatant.cs, SkillData.cs, EnemyData.cs
  Interaction/    IInteractable.cs, NPC.cs
  Enums/          Element.cs
```

---

## Scripts

### Enums/Element.cs

**Type:** Enum (standalone, no MonoBehaviour)
**Purpose:** Alchemical elements used by skills, catalysts, and enemies.

| Value | Meaning |
|-------|---------|
| `None` | Unaligned / neutral |
| `Ignis` | Fire |
| `Aqua` | Water |
| `Terra` | Earth |
| `Ventus` | Air |
| `Lux` | Light |
| `Umbra` | Dark |

**Weakness cycle:** Ignis > Ventus > Terra > Aqua > Ignis.  Lux and Umbra are
mutually weak to each other.  None is neutral.

---

### Interaction/IInteractable.cs

**Type:** Interface
**Purpose:** Contract for anything the player can interact with.

| Member | Description |
|--------|-------------|
| `string InteractionPrompt` | Short label (e.g. "Talk to Elder", "Pick up Sword"). |
| `void Interact(PlayerController player)` | Called when the player presses Interact while in range. |

Implemented by `NPC`, `ItemPickup`, and `BattleEncounter`.

---

### Core/GameManager.cs

**Type:** MonoBehaviour (singleton)
**Purpose:** Top-level game state and player reference.

| Inspector Field | Type | Default | Description |
|-----------------|------|---------|-------------|
| Player | PlayerController | -- | Drag the player GameObject here. |

| Public API | Description |
|------------|-------------|
| `static GameManager Instance` | Singleton accessor. |
| `GameState State { get; set; }` | `Playing`, `Paused`, `Dialogue`, or `Battle`. Setting this updates `Time.timeScale` and fires `OnStateChanged`. |
| `event Action<GameState, GameState> OnStateChanged` | Fires with (oldState, newState). |
| `PlayerController Player` | Reference to the player. |

**timeScale rules:** `Playing` and `Battle` keep timeScale at 1 (coroutines
need real time).  `Paused` and `Dialogue` set timeScale to 0.

---

### Player/PlayerController.cs

**Type:** MonoBehaviour
**Required Components:** `Rigidbody2D`, `PlayerInput`, `Inventory`
**Purpose:** Top-down 2D movement, sprinting, and interaction detection.

| Inspector Field | Type | Default | Description |
|-----------------|------|---------|-------------|
| Move Speed | float | 5 | Base movement speed (units/sec). |
| Sprint Multiplier | float | 1.6 | Speed multiplier while Sprint is held. |
| Interact Radius | float | 0.5 | Radius of the interaction overlap circle. |
| Interact Offset | float | 0.8 | Distance from player to interaction circle centre. |
| Interact Layers | LayerMask | Everything | Layers to scan for interactables. |

| Public API | Description |
|------------|-------------|
| `Inventory Inventory` | The player's inventory component. |
| `Vector2 FacingDirection` | Normalised facing direction. |
| `IInteractable GetNearestInteractable()` | Closest interactable in range, or null. |

Movement and interaction are frozen when `GameManager.State != Playing`.

---

### Core/CameraFollow.cs

**Type:** MonoBehaviour
**Purpose:** Smooth camera follow with optional world-bounds clamping.

| Inspector Field | Type | Default | Description |
|-----------------|------|---------|-------------|
| Target | Transform | -- | Transform to follow (drag the player here). |
| Smooth Time | float | 0.15 | Damping time in seconds. |
| Use Bounds | bool | false | Clamp camera inside `World Bounds`. |
| World Bounds | Rect | (-50,-50,100,100) | Camera centre constraint rectangle. |

---

### Items/ItemData.cs

**Type:** ScriptableObject
**Menu:** Assets > Create > Azoth > Item Data
**Purpose:** Immutable definition of an item type with category, rarity, and
optional catalyst/gear properties.

**ItemCategory enum** (defined in this file):

| Value | Meaning |
|-------|---------|
| `Catalyst` | Boost alchemy skills |
| `Gear` | Equippable items with a GearSlot |
| `Potion` | Consumable potions |
| `Trinket` | Miscellaneous collectibles |
| `Key` | Quest / progression keys |
| `Ingredient` | Crafting ingredients (subcategories TBD) |
| `MagnumOpus` | Ultimate alchemical creations |

**GearSlot enum** (defined in this file):

| Value | Meaning |
|-------|---------|
| `None` | Default for non-gear items |
| `Head` | Head armour |
| `Body` | Body armour |
| `Shield` | Off-hand shield |
| `Weapon` | Weapon |
| `Ring` | Finger ring |
| `Amulet` | Neck amulet |

**ItemRarity enum** (defined in this file):

| Value |
|-------|
| `Common` |
| `Uncommon` |
| `Rare` |
| `Epic` |
| `Legendary` |

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Item Name | string | -- | Display name. |
| Description | string | -- | Flavour text. |
| Icon | Sprite | -- | Inventory UI icon. |
| Stackable | bool | true | Share one inventory slot. |
| Max Stack | int | 99 | Maximum units per stack. |
| Category | ItemCategory | Trinket | Item category (see enum above). |
| Rarity | ItemRarity | Common | Item rarity tier. |
| Gear Slot | GearSlot | None | Equipment slot (only relevant when Category = Gear). |
| Element | Element | None | Catalyst element (only when Category = Catalyst). |
| Potency | int | 0 | Catalyst strength (only when Category = Catalyst). |

| Property | Description |
|----------|-------------|
| `bool IsCatalyst` | Shorthand for `category == ItemCategory.Catalyst`. |

**Catalyst items** boost alchemy skills of the matching element during battle.
Each potency point adds +1% to skill base power (stacks additively across all
matching catalysts in the inventory).

---

### Items/ItemPickup.cs

**Type:** MonoBehaviour, implements `IInteractable`
**Required Components:** `Collider2D`
**Purpose:** World representation of an item on the ground.

| Inspector Field | Type | Default | Description |
|-----------------|------|---------|-------------|
| Item Data | ItemData | -- | Which item this pickup represents. |
| Quantity | int | 1 | Units to give on pickup. |

---

### Items/Inventory.cs

**Type:** MonoBehaviour
**Purpose:** Slot-based player inventory.

| Inspector Field | Type | Default | Description |
|-----------------|------|---------|-------------|
| Max Slots | int | 20 | Maximum number of inventory slots. |

| Public API | Description |
|------------|-------------|
| `IReadOnlyList<Slot> Slots` | Current contents (read-only). |
| `event Action OnChanged` | Fires after any add/remove. |
| `int Add(ItemData, int)` | Add items.  Returns count actually added. |
| `int Remove(ItemData, int)` | Remove items.  Returns count actually removed. |
| `int Count(ItemData)` | Total units across all slots. |
| `bool Has(ItemData)` | Whether at least one unit exists. |
| `int GetCatalystBonus(Element)` | Sum of `potency * count` for all catalyst items matching the given element.  Returns 0 for `Element.None`. |

---

### Interaction/NPC.cs

**Type:** MonoBehaviour, implements `IInteractable`
**Required Components:** `Collider2D`
**Purpose:** Named NPCs with dialogue and optional waypoint patrol.

| Inspector Field | Type | Default | Description |
|-----------------|------|---------|-------------|
| Display Name | string | "NPC" | Name shown in prompts and dialogue. |
| Dialogue Lines | string[] | { "..." } | Lines spoken in order (wraps). |
| Waypoints | Transform[] | (empty) | Patrol waypoints (>= 2 to activate). |
| Patrol Speed | float | 2 | Speed between waypoints. |
| Waypoint Pause | float | 1 | Wait time at each waypoint. |

Patrol freezes when `GameManager.State` is not `Playing`.

---

### Battle/SkillData.cs

**Type:** ScriptableObject
**Menu:** Assets > Create > Azoth > Skill Data
**Purpose:** Definition of an alchemy skill usable in battle.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Skill Name | string | -- | Display name. |
| Description | string | -- | Tooltip text. |
| Icon | Sprite | -- | UI icon. |
| Element | Element | None | Elemental affinity. |
| Base Power | int | 10 | Base damage before catalyst bonuses. |
| MP Cost | int | 5 | Mana cost to use. |
| Targets All | bool | false | If true, hits all enemies (or the player for enemy AoE). |

---

### Battle/EnemyData.cs

**Type:** ScriptableObject
**Menu:** Assets > Create > Azoth > Enemy Data
**Purpose:** Template for enemy types used by `BattleEncounter`.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Enemy Name | string | -- | Display name. |
| Sprite | Sprite | -- | Battle sprite. |
| Max HP | int | 30 | Hit points. |
| Max MP | int | 10 | Mana points. |
| Attack | int | 8 | Physical attack stat. |
| Defense | int | 3 | Damage reduction. |
| Speed | int | 5 | Turn order priority. |
| Element | Element | None | Elemental affinity (determines weakness). |
| Known Skills | SkillData[] | -- | Skills the enemy can use in battle. |

---

### Player/PlayerCombat.cs

**Type:** MonoBehaviour
**Purpose:** Persistent player combat stats that carry over between battles.

| Inspector Field | Type | Default | Description |
|-----------------|------|---------|-------------|
| Max HP | int | 100 | Maximum hit points. |
| Max MP | int | 50 | Maximum mana points. |
| Attack | int | 10 | Physical attack stat. |
| Defense | int | 5 | Damage reduction. |
| Speed | int | 10 | Turn order priority. |
| Known Skills | SkillData[] | -- | Skills the player can use in battle. |

| Public API | Description |
|------------|-------------|
| `int CurrentHP { get; set; }` | Runtime HP (clamped to 0..maxHP). |
| `int CurrentMP { get; set; }` | Runtime MP (clamped to 0..maxMP). |
| `void FullRestore()` | Reset HP and MP to max. |

**Setup:** Add alongside `PlayerController` on the player GameObject.

---

### Battle/Combatant.cs

**Type:** Plain C# class (not a MonoBehaviour)
**Purpose:** Runtime battle participant created from `PlayerCombat` or `EnemyData`.

| Factory Method | Description |
|----------------|-------------|
| `Combatant.FromPlayer(PlayerCombat, Inventory)` | Build a player combatant with current HP/MP. |
| `Combatant.FromEnemy(EnemyData)` | Build an enemy combatant at full HP/MP. |

| Property | Description |
|----------|-------------|
| `Name`, `Sprite`, `IsPlayer` | Identity. |
| `MaxHP`, `MaxMP`, `Attack`, `Defense`, `Speed`, `Element` | Stats (immutable). |
| `List<SkillData> Skills` | Available skills. |
| `CurrentHP`, `CurrentMP` | Mutable runtime state. |
| `bool IsAlive` | `CurrentHP > 0`. |

| Method | Description |
|--------|-------------|
| `SyncToPlayer()` | Write HP/MP back to the source `PlayerCombat` component. |

---

### Battle/BattleManager.cs

**Type:** MonoBehaviour (singleton)
**Purpose:** Turn-based combat state machine driven by coroutines.  Communicates
with UI exclusively through events -- holds zero UI references.

| Public API | Description |
|------------|-------------|
| `static BattleManager Instance` | Singleton accessor. |
| `bool IsBattleActive` | Whether a battle is in progress. |
| `Combatant PlayerCombatant` | The player's runtime combatant. |
| `IReadOnlyList<Combatant> Enemies` | Living and dead enemy combatants. |
| `void StartBattle(EnemyData[])` | Begin a battle with the given enemies. |
| `void SubmitPlayerChoice(SkillData, Combatant)` | Called by UI when the player picks a skill and target. Null skill = basic Attack. |

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnBattleStart` | `Action` | Battle has begun; show UI. |
| `OnBattleEnd` | `Action<bool>` | Battle over; true = victory. |
| `OnTurnOrderDecided` | `Action<List<Combatant>>` | Turn order for this round. |
| `OnTurnStart` | `Action<Combatant>` | A combatant's turn is starting. |
| `OnPlayerChooseSkill` | `Action` | Waiting for player input. |
| `OnBattleLog` | `Action<string>` | Narrative message to display. |
| `OnDamageDealt` | `Action<Combatant, int>` | Target and damage amount. |
| `OnCombatantHPChanged` | `Action<Combatant>` | A combatant's HP changed. |
| `OnCombatantDefeated` | `Action<Combatant>` | A combatant reached 0 HP. |

**Damage formula:**
```
effectivePower = skill.basePower * (1 + catalystBonus * 0.01)   [or attacker.Attack for basic Attack]
rawDamage      = effectivePower + attacker.Attack - target.Defense
weakness mult  = 1.5 if element hits weakness, else 1.0
finalDamage    = max(1, floor(rawDamage * weaknessMult))
```

**Battle flow:**
1. `StartBattle` → build Combatants, set `GameState.Battle`, start coroutine
2. Each round: sort by Speed (desc), each alive combatant acts in order
3. Player turn: fire `OnPlayerChooseSkill`, wait for `SubmitPlayerChoice`
4. Enemy turn: pick random affordable skill (or basic Attack)
5. After each action: check victory (all enemies dead) or defeat (player dead)
6. End: sync HP/MP back (victory) or full restore (defeat), set `GameState.Playing`

**Setup:** Create an empty GameObject named "BattleManager", add the component.

---

### Battle/BattleUI.cs

**Type:** MonoBehaviour
**Purpose:** Event-driven battle UI controller.  Subscribes to BattleManager
events and updates serialized UI references.

| Serialized Reference | Type | Description |
|----------------------|------|-------------|
| Battle Panel | GameObject | Root panel (shown/hidden). |
| Player HP Bar | Slider | Player HP bar. |
| Player MP Bar | Slider | Player MP bar. |
| Player HP Text | TMP_Text | "50 / 100" readout. |
| Player MP Text | TMP_Text | "25 / 50" readout. |
| Enemy Container | Transform | Parent for enemy entries. |
| Enemy Entry Prefab | GameObject | Prefab instantiated per enemy. |
| Skill Button Container | Transform | Parent for skill buttons. |
| Skill Button Prefab | GameObject | Prefab instantiated per skill. |
| Attack Button | Button | Always-available basic Attack button. |
| Target Panel | GameObject | Shown when player must pick a target. |
| Target Button Container | Transform | Parent for target buttons. |
| Target Button Prefab | GameObject | Prefab instantiated per living enemy. |
| Battle Log Text | TMP_Text | Scrolling battle narrative. |
| Battle Log Scroll | ScrollRect | Auto-scrolls to bottom. |
| Result Panel | GameObject | Victory/Defeat display. |
| Result Text | TMP_Text | "Victory!" or "Defeat..." label. |

**Setup:** Build the UI in a Canvas, assign all references.  Place on the same
GameObject as the Canvas or a child.

---

### Battle/BattleEncounter.cs

**Type:** MonoBehaviour, implements `IInteractable`
**Required Components:** `Collider2D`
**Purpose:** Overworld trigger that starts a battle via BattleManager.

| Inspector Field | Type | Default | Description |
|-----------------|------|---------|-------------|
| Enemies | EnemyData[] | -- | Enemy roster for this encounter. |
| Trigger On Collision | bool | false | If true, battle starts on contact (no Interact needed). |
| Destroy After Battle | bool | true | If true, this GameObject is destroyed after the battle ends. |
| Prompt | string | "Challenge" | Interaction prompt text. |

**Setup:**
1. Create a GameObject with a `Collider2D` (set as trigger).
2. Add `BattleEncounter`, assign `EnemyData` assets.
3. For automatic triggers (random encounters, boss zones), enable
   `Trigger On Collision`.

---

## Quick-Start Scene Setup

1. **Tilemap** -- Create a Grid with a Tilemap child. Paint tiles.  Add
   `TilemapCollider2D` for walls.

2. **Player** -- Create a GameObject with `SpriteRenderer`.  Add
   `PlayerController` (auto-adds `Rigidbody2D`, `PlayerInput`, `Inventory`).
   Add `PlayerCombat`.  On `PlayerInput`, assign `InputSystem_Actions` and set
   Default Map to "Player".

3. **Camera** -- Add `CameraFollow` to Main Camera, drag player into `Target`.

4. **GameManager** -- Empty GameObject + `GameManager`, drag player into `Player`.

5. **BattleManager** -- Empty GameObject + `BattleManager`.

6. **Battle UI** -- Build a Canvas with all the panels/buttons described in
   BattleUI.  Add `BattleUI` component and wire up references.

7. **Skill & Enemy Data** -- Create `SkillData` and `EnemyData` assets via the
   Create menu.  Assign skills to both `PlayerCombat.knownSkills` and
   `EnemyData.knownSkills`.

8. **Catalyst Items** -- Create `ItemData` assets with Category = Catalyst,
   set the element and potency.  Picking these up boosts matching skills.

9. **Battle Encounters** -- Place GameObjects with `BattleEncounter` +
   trigger collider in the scene.  Assign enemies.

10. **Hit Play.**

---

## Extending the System

- **New interactable types:** Implement `IInteractable` on any MonoBehaviour.
  Add a trigger `Collider2D`.  The player detects it automatically.

- **Dialogue UI:** Replace `Debug.Log` in `NPC.Interact()` with your UI.  Set
  `GameManager.Instance.State = GameState.Dialogue` to freeze movement.

- **Healing skills:** Add a `SkillType` enum (Attack/Heal) to `SkillData` and
  branch in `BattleManager.ExecuteAction`.

- **Status effects:** Add a `List<StatusEffect>` to `Combatant` and apply/tick
  effects at turn start.

- **Inventory UI:** Subscribe to `Inventory.OnChanged` and read
  `Inventory.Slots` to refresh the display.

- **Save/Load:** Serialize `Inventory.Slots`, `PlayerCombat` stats, and
  player position.
