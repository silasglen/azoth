# Dragon Encounter Design Document

> Azoth: Alchemists vs Magus -- Dragon Enemy & Encounter Content
> Battle system reference: `BattleController.cs`, `BattleEnums.cs`, `SkillAndItemData.cs`
> Element cycle: Ignis > Ventus > Terra > Aqua > Ignis | Lux <-> Umbra
> Status effects: Poison, Burn, Stun, AtkUp, AtkDown, DefUp, DefDown
> Player archetypes: Alchemist, Magus, Knight

---

## Section 1: 15 Unique Dragons

---

### 1. Pyrevathan, the Cinderthrone

**Element:** Ignis (primary)
**Lore:** Once the personal warbeast of the first Magus Emperor, Pyrevathan was entombed beneath the Imperial Citadel after its master's death three centuries ago. The current Magus Conclave resurrected it to serve as a living siege weapon, its body still fused with the obsidian throne it was chained to in death. It guards the approach to the Citadel's inner sanctum, its molten slag pooling across the corridor as a natural barricade.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| High | VHigh | Med |

**Combat Role:** Glass Cannon / Area Pressure

**Phase Mechanics:**
- **Phase 1 (100%-50% HP):** Pyrevathan attacks with single-target Ignis skills and builds "Slag Stacks" on the battlefield (a counter visible in the UI, 0-5). Each Slag Stack increases its next AoE damage by 15%.
- **Phase 2 (Below 50% HP):** Pyrevathan's throne fractures. It gains +25% ATK and begins alternating between single-target devastation and "Cinderstorm," which hits all party members. Slag Stacks now build twice as fast. At 20% HP, it uses "Extinction Flare" -- a 3-turn-charge ultimate that wipes the party unless the Corruption Crystal is destroyed or enough damage is dealt to interrupt.

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Slag Rake | Ignis | 1.8x DMG single | Burn (2t, 5 dmg, 40%) | Drags a molten claw across one target, leaving scorched wounds. |
| Cinderstorm | Ignis | 1.2x DMG all | Burn (2t, 5 dmg, 30%) | Rains cinders across the entire party. Damage scales with Slag Stacks. |
| Throne Eruption | Ignis | 2.5x DMG single | Stun (1t, 20%) | The obsidian throne detonates outward. Unblockable. |
| Molten Carapace | None | Self-buff | DefUp (2t, 1.40x, 100%) | Coats itself in hardened lava, raising defense. |
| Extinction Flare | Ignis | 4.0x DMG all | Burn (3t, 8 dmg, 100%) | 3-turn charge. Devastating firestorm. Party wipe if not interrupted. |

**Unique Mechanic:** Slag Stack system. Pyrevathan accumulates stacks each turn that amplify its AoE. Players can "clear" one stack by dealing Aqua damage in a single hit exceeding 20% of its max HP. This creates a tug-of-war between offense and stack management.

**Counter-Strategy:** Bring an Alchemist with Aqua skills to exploit Ignis weakness and clear Slag Stacks. A Knight should Block during Throne Eruption turns -- but since it is unblockable, Dodge is actually required (75% evade vs guaranteed hit). Magus provides sustained damage. Antidotes or Burn-cure items are essential for the Cinderstorm phase.

**Loot:**
- Pyrevathan's Slag Core (crafting material -- Ignis-element weapon infusion)
- Cinderthrone Fragment (accessory -- grants +10% Burn resistance)
- Obsidian Emperor's Seal (key item -- unlocks Citadel inner sanctum)

---

### 2. Glacivorn, the Stillwater Leviathan

**Element:** Aqua (primary)
**Lore:** Glacivorn once dwelled in the depths of the Crystalline Abyss, a freshwater sea beneath the northern tundra. It was a peaceful guardian of the underground aquifers until the Magus empire poisoned the waters with binding reagents, driving it mad and chaining its will to the Conclave. Now it patrols the frozen surface, its body encased in living ice that regrows as fast as it shatters. It guards the northern trade routes, cutting off supplies to the resistance.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| VHigh | Med | High |

**Combat Role:** Tank / Attrition

**Phase Mechanics:**
- **Phase 1 (100%-60% HP):** Glacivorn fights defensively, using Aqua attacks with Poison effects (corrupted water) and self-healing. Every 3 turns, it encases one party member in "Permafrost" (Stun 1 turn + DefDown).
- **Phase 2 (Below 60% HP):** The ice shell cracks, revealing corrupted flesh beneath. Glacivorn's ATK increases and it gains "Undertow" -- a pull effect that forces one party member out of their defensive stance (cancels Block/Dodge for that member). It begins using "Abyssal Deluge" every 4 turns.

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Frost Fang | Aqua | 1.5x DMG single | Poison (3t, 4 dmg, 50%) | Bites with teeth dripping corrupted ice water. |
| Permafrost Cage | Aqua | 0.5x DMG single | Stun (1t, 100%) + DefDown (2t, 0.70x, 100%) | Encases a target in ice. Guaranteed stun. |
| Abyssal Deluge | Aqua | 2.0x DMG all | Poison (2t, 4 dmg, 30%) | Floods the arena with tainted water. |
| Glacial Regeneration | None | Heal 15% max HP | DefUp (2t, 1.30x, 100%) | Regrows its ice shell, restoring HP and defense. |
| Undertow | Aqua | 1.0x DMG single | Special: cancels target stance | Drags a defender off-balance, stripping Block/Dodge. |

**Unique Mechanic:** Permafrost targets the party member with the highest DEF first, attempting to neutralize the Knight. Undertow specifically targets whoever is in a defensive stance, making it impossible to safely Block or Dodge every turn. Players must rotate who defends.

**Counter-Strategy:** Terra element is super-effective against Aqua, making an Alchemist with Terra skills the primary damage dealer. Keep Antidotes stocked for Poison. The Knight should alternate between attacking and defending since Undertow punishes constant blocking. Magus can provide supplemental healing with Arcane Shield. Focus burst damage to push through Phase 2 quickly before Abyssal Deluge stacks Poison on the whole party.

**Loot:**
- Leviathan's Frozen Scale (crafting material -- Aqua-element armor infusion)
- Abyssal Pearl (accessory -- grants +15% Poison resistance)
- Corrupted Aquifer Sample (quest item -- evidence of Magus water poisoning)

---

### 3. Zephyrax, the Stormcaller Sovereign

**Element:** Ventus (primary)
**Lore:** Zephyrax was the undisputed lord of the Howling Peaks, a chain of mountains where the wind itself was said to be alive. The Magus empire captured it during a coordinated lightning-rod ritual that grounded its electrical field and left it vulnerable to binding. Now it circles above the empire's eastern border, generating perpetual storms that ground airships and blind scouts. Its shrieks can be heard for miles -- a constant reminder of the Conclave's dominion over nature itself.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| Med | VHigh | Low |

**Combat Role:** Glass Cannon / Speed Controller

**Phase Mechanics:**
- **Phase 1 (100%-55% HP):** Zephyrax attacks twice per turn (two separate intents declared). Its attacks are individually weaker but frequent, and it has a high crit chance (20%). It applies AtkDown to reduce player offense.
- **Phase 2 (Below 55% HP):** Enters "Tempest State." Now attacks THREE times per turn but each hit is weaker. Gains "Static Field" -- a passive that gives all its attacks a 25% Stun chance. At 25% HP, consolidates all attacks into one massive "Cyclone Judgment."

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Gale Slash | Ventus | 1.3x DMG single | None | Lightning-fast wing strike. Used twice per intent phase. |
| Thunder Screech | Ventus | 1.0x DMG all | AtkDown (2t, 0.75x, 40%) | Piercing shriek that weakens resolve. |
| Static Field | None | Passive in Phase 2 | Stun (1t, 25% on all attacks) | Electricity arcs from its body with each strike. |
| Cyclone Judgment | Ventus | 3.5x DMG single | Stun (1t, 60%) | Consolidates all wind power into a single devastating blast. Unblockable. |
| Downdraft | Ventus | 0.8x DMG single | AtkDown (1t, 0.60x, 100%) | Slams a target to the ground with pressurized air. |

**Unique Mechanic:** Multi-intent system. Zephyrax declares 2-3 intents per turn (each targeting potentially different party members), forcing the player to make difficult defensive allocation choices. You cannot Block for everyone.

**Counter-Strategy:** Ignis is strong against Ventus. An Alchemist with Ignis Toss can exploit the weakness and potentially Burn it, which is valuable against a low-DEF target. The Knight should focus on Blocking the highest-damage intent while other members Dodge. Magus Umbra Drain's lifesteal helps sustain through chip damage. Prioritize killing it fast -- its low HP pool means burst damage is rewarded.

**Loot:**
- Stormcaller Pinion (crafting material -- Ventus-element accessory crafting)
- Lightning Conduit Gem (accessory -- grants +5% crit chance)
- Storm Sovereign's Crown (rare helm -- grants minor Ventus resistance)

---

### 4. Terrathos, the Living Mountain

**Element:** Terra (primary)
**Lore:** Terrathos was not born -- it grew. Over millennia, mineral deposits in the Verdant Bastion's deepest caverns coalesced into a dragon-shaped colossus of living stone. It slept for ten thousand years, content to dream in the earth. The Magus Conclave detonated seismic charges to wake it, then embedded binding crystals in its granite skull. Now it marches ahead of the Imperial army as a living battering ram, its footsteps causing localized earthquakes. Every fortress wall crumbles before it.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| Extreme | High | VHigh |

**Combat Role:** Ultra-Tank / Siege Monster

**Phase Mechanics:**
- **Phase 1 (100%-40% HP):** Terrathos attacks slowly (its intent always includes a 1-turn "winding up" tell before the actual strike). It buffs its own DEF repeatedly, becoming nearly impervious to physical damage. Every 3 turns, it uses "Seismic Slam" which hits all party members.
- **Phase 2 (Below 40% HP):** Stone armor cracks, DEF drops by 30%, but ATK rises by 40%. It stops winding up -- all attacks are now immediate. Gains "Tectonic Rage" which makes every attack unblockable. At 15% HP, it burrows underground and resurfaces for a guaranteed "Mountain Fall" (massive all-party damage).

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Boulder Crush | Terra | 2.0x DMG single | DefDown (2t, 0.75x, 30%) | Hurls a chunk of its own body at a target. |
| Seismic Slam | Terra | 1.5x DMG all | Stun (1t, 20%) | Smashes the ground, sending shockwaves through the arena. |
| Granite Skin | None | Self-buff | DefUp (3t, 1.50x, 100%) | Hardens outer layer. Stacks with existing DefUp. |
| Tectonic Rage | None | Passive in Phase 2 | All attacks unblockable | Stone crumbles, revealing the burning core. Attacks cannot be blocked. |
| Mountain Fall | Terra | 3.0x DMG all | Stun (1t, 40%) | Leaps and crashes down on the entire party. Used at 15% HP. |

**Unique Mechanic:** The wind-up telegraph. In Phase 1, Terrathos declares intent one turn in advance but does not act until the following turn. This gives players an extra turn to prepare (heal, buff DEF, choose Dodge). In Phase 2 this advantage disappears, punishing players who relied on the telegraph.

**Counter-Strategy:** Ventus is strong against Terra. Stack Ventus damage to cut through its massive HP pool. In Phase 1, use the telegraph turns to apply AtkDown via Magus Umbra Drain. Do NOT rely on blocking in Phase 2 -- all attacks become unblockable, so Dodge is mandatory. Alchemist should focus on healing/sustain. This is a long war of attrition; bring extra Ethers and Health Potions.

**Loot:**
- Living Granite Heart (crafting material -- Terra-element shield crafting)
- Tectonic Shard (accessory -- grants +20% DEF when below 30% HP)
- Earthblood Ore (rare crafting material -- used for legendary weapon upgrades)

---

### 5. Umbraxis, the Eclipse Wyrm

**Element:** Umbra (primary)
**Lore:** Umbraxis was once the familiar of the legendary Archmagus Severine, a being of pure shadow that could fold space through darkness. When Severine was executed for heresy against the Conclave, Umbraxis was imprisoned in a pocket dimension of eternal twilight. The Conclave dragged it back through a tear in reality, embedding corruption crystals that force it to obey while slowly driving it insane. It now serves as an assassin, slipping through shadows to eliminate resistance leaders in their sleep.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| Med | VHigh | Low |

**Combat Role:** Debuffer / Assassin

**Phase Mechanics:**
- **Phase 1 (100%-50% HP):** Umbraxis fights from the shadows. It has a 40% chance to "phase out" and avoid any incoming attack entirely (separate from player defense -- this is an enemy dodge). It applies AtkDown and DefDown aggressively, weakening the party before striking. Every 2 turns it uses "Eclipse Veil" to become untargetable for 1 turn.
- **Phase 2 (Below 50% HP):** The corruption crystal pulses visibly. Umbraxis becomes permanently visible (loses phase-out chance) but gains "Shadow Duplicate" -- a second set of intents from a clone that mirrors its attacks at 50% damage. The clone cannot be targeted directly; destroying Umbraxis destroys the clone.

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Umbral Fang | Umbra | 2.0x DMG single | AtkDown (2t, 0.75x, 50%) | Bites from the shadows, sapping the target's will to fight. |
| Eclipse Veil | None | Self: untargetable 1 turn | None | Dissolves into darkness. Cannot be hit or targeted next turn. |
| Nightmare Lash | Umbra | 1.5x DMG single | DefDown (2t, 0.70x, 60%) | A tendril of shadow strips away defenses. |
| Shadow Duplicate | None | Passive in Phase 2 | Clone mirrors attacks at 50% DMG | A dark copy emerges, attacking alongside the original. |
| Void Rend | Umbra | 3.0x DMG single | AtkDown (2t, 0.60x, 40%) + DefDown (2t, 0.70x, 40%) | Tears through dimensional fabric. Unblockable. |

**Unique Mechanic:** The untargetable mechanic. During Eclipse Veil turns, players cannot Attack or use offensive Skills on Umbraxis. This forces a defensive/buff turn for the party, creating a rhythm: prepare during Veil, burst during visibility windows. The Shadow Duplicate in Phase 2 effectively doubles incoming intents.

**Counter-Strategy:** Lux is super-effective against Umbra. A Magus with Lux Bolt is the primary damage dealer. During Eclipse Veil turns, use buffs (AtkUp), healing, or items. The Knight should Dodge since many of Umbraxis's attacks are unblockable in Phase 2. Alchemist provides sustain. The fight rewards patience -- wait for visibility windows and burst hard.

**Loot:**
- Umbraxis Shadow Essence (crafting material -- Umbra-element weapon infusion)
- Twilight Pendant (accessory -- grants +15% Umbra resistance)
- Archmagus Severine's Journal Fragment (lore item -- reveals hidden quest)

---

### 6. Luxarion, the Dawnbreaker Wyrm

**Element:** Lux (primary)
**Lore:** Luxarion was a temple guardian, bound to the Sanctum of the First Light by ancient oaths predating the Magus empire. It radiated warmth and healing, and pilgrims traveled for months to bask in its presence. When the Conclave razed the Sanctum to eliminate a resistance stronghold, they captured Luxarion and inverted its light -- twisting its healing radiance into a searing weapon of war. It now patrols the Bleached Wastes, a desert created by its own corrupted brilliance, burning everything in its path.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| High | High | Med |

**Combat Role:** Healer-Turned-Aggressor / Phase Inversion

**Phase Mechanics:**
- **Phase 1 (100%-55% HP):** Luxarion's corrupted light manifests as offensive Lux damage with Burn effects (searing light). It periodically heals itself for 10% max HP. Its intents always show a clear pattern: Attack, Attack, Heal, repeat.
- **Phase 2 (Below 55% HP):** The corruption destabilizes. Luxarion's healing skill now damages itself (it tries to heal but the corruption inverts it). However, its ATK spikes dramatically (+50%). It begins using "Purging Light" -- an all-party attack that strips all buffs (AtkUp/DefUp) from the player party.

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Searing Ray | Lux | 1.8x DMG single | Burn (2t, 6 dmg, 40%) | A concentrated beam of corrupted sunlight. |
| Radiant Burst | Lux | 1.3x DMG all | Burn (1t, 4 dmg, 25%) | Light explodes outward from its body in all directions. |
| Corrupted Mending | None | Heals self 10% HP (Phase 1) / Damages self 5% HP (Phase 2) | None | Attempts to heal, but corruption inverts the effect in Phase 2. |
| Purging Light | Lux | 1.0x DMG all | Strips all buffs from party (100%) | Holy light scours away all magical enhancements. |
| Dawn's Judgment | Lux | 3.5x DMG single | Burn (3t, 8 dmg, 60%) | Channels the memory of the Sanctum's power into one devastating blast. Unblockable. |

**Unique Mechanic:** Phase inversion and self-harm. In Phase 2, Luxarion's heal pattern becomes a self-damage pattern, meaning it helps you kill it faster IF you can survive its massively increased offensive output. The fight becomes a damage race. Purging Light also punishes buff-stacking strategies, forcing players to time their buffs carefully (use AtkUp right before a burst window, not preemptively).

**Counter-Strategy:** Umbra is super-effective against Lux. A Magus with Umbra Drain provides both damage and lifesteal sustain. In Phase 1, do not over-commit to damage -- Luxarion heals, so chip damage is wasted. Save burst for Phase 2 when it self-harms. The Knight should Block during Dawn's Judgment -- but it is unblockable, so Dodge is required. Alchemist manages Burn with healing items and provides Aqua splash for neutral damage.

**Loot:**
- Dawnbreaker Prism (crafting material -- Lux-element weapon infusion)
- Sanctum Guardian's Halo (accessory -- heals 3% max HP at end of each turn)
- Corrupted Light Fragment (key item -- used to restore the Sanctum in a side quest)

---

### 7. Venerath and Corraxis, the Twinbound Serpents

**Element:** Venerath: Aqua / Corraxis: Ignis
**Lore:** These twin serpentine dragons were born from the same clutch in the volcanic hot springs of the Fumerole Rift, where fire and water exist in perfect equilibrium. One breathes scalding steam, the other freezing brine. The Conclave bound them together with a shared corruption crystal lodged between their intertwined tails -- if one dies, the other goes berserk. They guard the Rift, a strategic geothermal energy source the empire uses to power its war forges.

**Stats (each):**
| Unit | HP | ATK | DEF |
|---|---|---|---|
| Venerath (Aqua) | Med | High | Med |
| Corraxis (Ignis) | Med | High | Med |

**Combat Role:** Synergy Duo / Combo Attackers

**Phase Mechanics:**
- **Phase 1 (Both alive, both above 30% HP):** Each dragon declares its own intent. Venerath applies Poison; Corraxis applies Burn. Every 3 turns they execute a combined "Steam Eruption" attack that hits all party members with both Burn and Poison simultaneously.
- **Phase 2 (Either drops below 30% HP):** The wounded twin retreats to heal while the healthy twin enters "Enrage." The enraged twin gains +40% ATK, attacks twice per turn, and its attacks become unblockable. The retreating twin heals 5% max HP per turn. If the retreating twin reaches 50% HP, it rejoins and Phase 1 resumes.

**Signature Skills:**

| Skill Name | Element | User | Effect | Status |
|---|---|---|---|---|
| Brine Lash | Aqua | Venerath | 1.6x DMG single | Poison (3t, 4 dmg, 50%) |
| Ember Coil | Ignis | Corraxis | 1.6x DMG single | Burn (2t, 5 dmg, 50%) |
| Steam Eruption | Aqua+Ignis | Both (combined) | 2.0x DMG all | Burn (2t, 5 dmg, 40%) + Poison (2t, 4 dmg, 40%) |
| Twin Retreat | None | Wounded twin | Self-heal 5% HP/turn, untargetable | None |
| Enraged Frenzy | (Element of remaining twin) | Healthy twin | 1.8x DMG single, 2 attacks | Unblockable in enrage state |

**Unique Mechanic:** The linked HP/retreat system. Players must damage both twins relatively evenly. If you focus one down too fast, the other enrages and becomes extremely dangerous while the first heals back. The optimal strategy is to keep both between 30-40% HP, then burst both down simultaneously. Steam Eruption is the combined attack -- it only fires when both twins are active.

**Counter-Strategy:** Bring both Terra (strong vs Aqua) and Aqua (strong vs Ignis) damage. An Alchemist can cover both with Aqua Splash for Corraxis and should carry Terra skills if available. Magus provides neutral arcane damage. The Knight manages defense during Enraged Frenzy with Dodge (attacks are unblockable). Stock Antidotes for the double-status-effect pressure. Try to burst both twins below 30% on the same turn, then finish them before retreat/rejoin cycling begins.

**Loot:**
- Twin Serpent Scale (crafting material -- dual-element accessory crafting)
- Steam Opal (accessory -- grants +10% resistance to both Burn and Poison)
- Fumerole Rift Geode (key item -- used to access the war forge dungeon)

---

### 8. Lithocron, the Crystal Tyrant

**Element:** Terra (primary), Lux (secondary)
**Lore:** Lithocron's body is composed entirely of living crystal -- a dragon that consumed so many gemstones over its eons-long life that its flesh became mineral. It once served as the living vault of the Gemcutter's Guild, its crystalline body the safest treasury in the world. The Conclave shattered the Guild and claimed Lithocron, embedding binding crystals that are almost indistinguishable from its natural structure. It now serves as a walking fortress, garrisoned with Magus soldiers who ride in hollowed chambers within its body.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| High | Med | Extreme |

**Combat Role:** Defensive Fortress / Reflect Tank

**Phase Mechanics:**
- **Phase 1 (100%-50% HP):** Lithocron has "Crystal Refraction" -- 30% of all incoming damage is reflected back to the attacker. It attacks infrequently but applies heavy DefUp to itself. Every 4 turns it uses "Prismatic Cascade" (Lux AoE).
- **Phase 2 (Below 50% HP):** Crystals fracture. Refraction increases to 50% but its DEF drops by 25%. It begins spawning "Crystal Shards" -- small add enemies (1 per turn, max 3) with low HP that explode for Lux damage when killed near a party member.

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Crystal Ram | Terra | 1.5x DMG single | None | Charges with its crystalline horn. |
| Prismatic Cascade | Lux | 1.3x DMG all | Burn (1t, 3 dmg, 20%) | Refracts light through its body into searing beams. |
| Crystal Refraction | None | Passive | Reflects 30%/50% damage to attacker | Incoming attacks partially bounce back. |
| Diamond Skin | None | Self-buff | DefUp (3t, 1.60x, 100%) | Compresses surface crystals to extreme hardness. |
| Shatter Spawn | None | Summons Crystal Shard (1, max 3) | Shards explode on death (Lux, 15 dmg to nearby) | Fragments break off and become hostile. |

**Unique Mechanic:** Damage reflection forces players to consider the cost of attacking. High-ATK characters like the Knight take significant reflected damage. Players should use multi-hit weak attacks rather than single massive blows, or use skills that apply status effects (Poison, Burn) which bypass reflection. The Crystal Shard adds must be managed but killing them near party members causes AoE damage -- so positioning (choosing when to kill them) matters.

**Counter-Strategy:** Ventus is strong against Terra. Use Ventus skills from the Alchemist to bypass the high DEF. Avoid using the Knight's Power Strike -- reflection will hurt. Instead, the Knight should focus on defense and Shield Bash (low damage, Stun chance). Magus DoT effects (Poison via items, Burn via Alchemist) bypass reflection entirely. Kill Crystal Shards only when the party is healthy enough to absorb the explosion.

**Loot:**
- Living Crystal Matrix (crafting material -- used to craft reflection accessories)
- Prismatic Lens (accessory -- 10% chance to reflect 15% of incoming damage)
- Gemcutter's Guild Seal (key item -- unlocks the Guild vault dungeon)

---

### 9. Nihildrake, the Void Maw

**Element:** Umbra (primary), Aqua (secondary)
**Lore:** Nihildrake is not a dragon that was resurrected -- it is a dragon that was created. The Conclave's most ambitious necromancers wove together fragments of dead dragons' souls with void energy, creating an abomination that exists partially outside reality. It has no memories, no personality, only hunger. The corruption crystal in its chest is the only thing preventing it from consuming everything around it indiscriminately. It is deployed as a weapon of last resort, aimed at cities that refuse to surrender.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| VHigh | High | Med |

**Combat Role:** Lifesteal Sustain / Entropy

**Phase Mechanics:**
- **Phase 1 (100%-45% HP):** Nihildrake's attacks all have 40% lifesteal, making it extremely difficult to whittle down. It applies a unique debuff: "Entropy" (functions as both AtkDown and DefDown simultaneously). Every 3 turns it uses "Void Siphon" to drain HP from all party members and heal itself.
- **Phase 2 (Below 45% HP):** Lifesteal increases to 60%. However, it also begins "Devouring" its own corruption crystal, which deals 3% of its max HP to itself each turn. The fight becomes a race: can you kill it before it heals more than it self-harms? It gains "Reality Tear," a single-target instant-kill attempt (50% chance, can be dodged).

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Void Fang | Umbra | 1.6x DMG single, 40% lifesteal | AtkDown (2t, 0.80x, 35%) | Bites and absorbs the target's life force. |
| Entropy Wave | Umbra | 1.0x DMG all | AtkDown (2t, 0.80x, 30%) + DefDown (2t, 0.80x, 30%) | A wave of nothingness that erodes everything. |
| Void Siphon | Umbra | 0.8x DMG all, 50% lifesteal | None | Drains life from all party members simultaneously. |
| Abyssal Flood | Aqua | 1.8x DMG single | Poison (3t, 5 dmg, 45%) | Corrupted void-water crashes over a target. |
| Reality Tear | Umbra | Instant kill (50% chance) or 2.5x DMG | None | Attempts to erase a target from existence. Can be dodged. |

**Unique Mechanic:** Lifesteal recovery makes sustained chip damage nearly useless. Players must coordinate burst damage windows where multiple party members attack on the same turn to overcome the healing. Entropy Wave's double-debuff weakens the party's ability to burst, creating a genuine tactical puzzle. Reality Tear's instant-kill chance forces the targeted party member to Dodge, even at the expense of offense.

**Counter-Strategy:** Lux is super-effective against Umbra. Magus Lux Bolt is the primary damage source. Coordinate burst: use AtkUp on the Magus, then unload skills in a single turn to outdamage the lifesteal. The Knight should Dodge on Reality Tear turns (shown in intent). Alchemist provides healing and can use Fire Bombs for burst damage that does not trigger the lifesteal reflection. Burn and Poison bypass lifesteal and chip away while you prepare bursts.

**Loot:**
- Void Maw Fragment (crafting material -- Umbra-element weapon, adds lifesteal)
- Entropy Shard (accessory -- attacks have 5% chance to apply AtkDown to target)
- Nihildrake's Core Crystal (rare -- used in endgame crafting for void-element gear)

---

### 10. Galebrand, the Tempest Warden

**Element:** Ventus (primary), Terra (secondary)
**Lore:** Galebrand once roosted atop the Skyward Pillars, a chain of impossibly tall stone columns rising from a vast plain. It was a creature of balance -- wind and stone in harmony, its wings generating the seasonal winds that farmers relied upon. The Conclave captured it by collapsing the Pillars with explosives while it slept, then binding it in the rubble. Now wingless and furious, it has been fitted with magitek prosthetic wings that channel corrupted Ventus energy. It guards the Conclave's aerostat fleet.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| High | Med | High |

**Combat Role:** Control / AoE Debuffer

**Phase Mechanics:**
- **Phase 1 (100%-50% HP):** Galebrand uses a "Weather System" mechanic, rotating between three weather states every 2 turns: Gale (all attacks gain AtkDown 30%), Dust Storm (all attacks gain DefDown 30%), and Calm (basic attacks only, double damage). The weather state is declared during intent phase.
- **Phase 2 (Below 50% HP):** The magitek wings overload. Galebrand locks into "Superstorm" -- all three weather effects active simultaneously. Every attack applies both AtkDown and DefDown. Every 3 turns it uses "Cataclysm" which deals massive AoE and applies Stun.

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Gale Claw | Ventus | 1.4x DMG single | AtkDown (2t, 0.80x, 30%) during Gale weather | Rakes with wind-sharpened talons. |
| Sandblast | Terra | 1.4x DMG single | DefDown (2t, 0.80x, 30%) during Dust Storm | Hurls compacted earth and gravel. |
| Calm Strike | None | 2.0x DMG single | None during Calm weather | A measured, devastating blow. |
| Cataclysm | Ventus+Terra | 2.5x DMG all | Stun (1t, 35%) + AtkDown (1t, 0.75x, 100%) | The superstorm reaches peak intensity. |
| Windwall | Ventus | Self-buff | DefUp (2t, 1.35x, 100%) + heals 5% max HP | Creates a barrier of compressed air. |

**Unique Mechanic:** Weather rotation. Players must read the declared weather state in the intent phase and plan accordingly. During Gale, prioritize offense (your ATK will be debuffed next turn). During Dust Storm, prioritize defense (your DEF will drop). During Calm, definitely Block or Dodge (double damage incoming). Phase 2 removes this pattern and makes every turn dangerous.

**Counter-Strategy:** Ignis is strong against Ventus (primary element). During Calm weather, the Knight must Block or Dodge -- 2.0x base damage from a High-ATK enemy is devastating. Alchemist provides Ignis damage and healing. Magus can use Arcane Shield during weather phases that debuff the party. Try to push into Phase 2 quickly because Superstorm, while dangerous, is at least predictable. Stack DefUp before Cataclysm turns.

**Loot:**
- Tempest Warden Pinion (crafting material -- Ventus/Terra hybrid accessory)
- Magitek Wing Fragment (accessory -- grants +10% dodge bonus)
- Storm-Worn Pillar Stone (key item -- used to rebuild the Skyward Pillars in side quest)

---

### 11. Sanguinax, the Bloodforged Drake

**Element:** Ignis (primary), Umbra (secondary)
**Lore:** Sanguinax was a common fire drake, unremarkable among its kind -- until the Conclave's flesh-weavers got hold of it. Through months of agonizing ritual, they infused it with the blood of executed prisoners saturated with Umbra energy, transforming it into a chimera of fire and shadow. Its veins glow through translucent skin, pulsing with dark fire. It feels constant pain and lashes out at everything near it. The Conclave uses it as a terror weapon, dropping it into civilian areas to sow panic.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| Med | Extreme | Low |

**Combat Role:** Berserker / Self-Destructive Attacker

**Phase Mechanics:**
- **Phase 1 (100%-60% HP):** Sanguinax attacks ferociously with dual-element strikes. Each attack it lands heals it for 15% of damage dealt (lifesteal from pain response). However, each attack also costs it 3% of its max HP in self-damage (the corruption tearing its body apart). It gains AtkUp automatically each turn, stacking.
- **Phase 2 (Below 60% HP):** Enters "Blood Frenzy." Self-damage increases to 6% per attack but lifesteal also increases to 30%. It attacks twice per turn. AtkUp stacks continue. At 20% HP, it uses "Sanguine Nova" -- a suicide attack that deals 4.0x damage to all party members and kills itself.

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Bloodfire Slash | Ignis | 2.2x DMG single, 15% lifesteal | Burn (2t, 6 dmg, 35%) | Claws wreathed in dark fire carve through flesh. |
| Shadow Hemorrhage | Umbra | 1.8x DMG single | AtkDown (2t, 0.70x, 45%) | Dark blood sprays from its wounds onto the target. |
| Blood Frenzy | None | Passive Phase 2 | AtkUp (permanent, stacking 1.10x/turn) | Pain drives it beyond reason. |
| Crimson Shriek | Ignis+Umbra | 1.5x DMG all | Burn (1t, 4 dmg, 30%) + DefDown (1t, 0.80x, 30%) | An agonized roar that scorches and terrifies. |
| Sanguine Nova | Ignis+Umbra | 4.0x DMG all, kills self | Burn (3t, 10 dmg, 100%) | Detonates its own corrupted blood. Suicide attack at 20% HP. |

**Unique Mechanic:** The self-damage/self-heal economy and the ticking bomb of Sanguine Nova. Players can choose to play defensively and let Sanguinax kill itself through self-damage -- but its stacking AtkUp means each passing turn makes Sanguine Nova more lethal. Alternatively, burst it down past 20% and below 0 HP before it can trigger the Nova. The fight rewards decisive commitment to one strategy.

**Counter-Strategy:** Aqua is strong against Ignis (primary). An Alchemist with Aqua Splash can exploit the weakness. Two viable strategies: (1) BURST -- all-out offense to kill it before AtkUp stacks too high, using AtkUp buffs and the Knight's Power Strike; (2) STALL -- Block/Dodge every turn and let self-damage chip it down, but you MUST kill it above 20% HP or survive the Nova (have everyone Block/Dodge and pray). Strategy 1 is recommended. Keep HP high in case Nova triggers.

**Loot:**
- Bloodforged Fang (crafting material -- dual Ignis/Umbra weapon infusion)
- Hemoglobin Crystal (accessory -- grants 5% lifesteal on basic attacks)
- Sanguinax's Binding Crystal (quest item -- evidence of Conclave flesh-weaving atrocities)

---

### 12. Petravar, the Fossilbound Ancient

**Element:** Terra (primary), Umbra (secondary)
**Lore:** Petravar died naturally over fifty thousand years ago, its body slowly fossilizing into stone. It was the oldest dragon ever known to have lived, and its fossil was venerated as a sacred relic by the Earthkeeper Order. The Conclave's necromancers achieved what was thought impossible: reanimating a fossil. Petravar is not truly alive -- it is a puppet of stone and dark magic, its movements jerky, its attacks mechanical. But the power of its ancient bones is immense. It guards the Conclave's necromancy research facility.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| Extreme | Med | High |

**Combat Role:** Summoner / Attrition Boss

**Phase Mechanics:**
- **Phase 1 (100%-55% HP):** Petravar attacks sparingly but summons "Fossil Minions" -- small bone constructs (2 per summon, max 4 active). Minions have low HP/ATK but each one present grants Petravar +5% DEF (stacking). Every 4 turns, Petravar and all minions attack simultaneously.
- **Phase 2 (Below 55% HP):** Stops summoning. Instead, absorbs all living minions, healing 5% max HP per minion absorbed and gaining their accumulated DEF bonus permanently. Then begins direct combat with enhanced stats. Uses "Fossilize" -- a single-target petrification (Stun 2 turns + DefDown).

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Bone Crush | Terra | 1.6x DMG single | DefDown (2t, 0.75x, 30%) | Slams a fossilized limb down on a target. |
| Summon Fossils | None | Summons 2 Fossil Minions (max 4) | None | Fragments of bone assemble into hostile constructs. |
| Mass Assault | Terra | All units attack (Petravar + minions) | None | Coordinated strike from all active units. |
| Fossilize | Terra+Umbra | 1.0x DMG single | Stun (2t, 80%) + DefDown (2t, 0.65x, 80%) | Attempts to turn a target to stone. |
| Ancient Resonance | Umbra | 1.5x DMG all | AtkDown (2t, 0.80x, 40%) | Channels the weight of geological ages against the party. |

**Unique Mechanic:** Minion management creates a strategic dilemma. Killing minions before Phase 2 prevents Petravar from absorbing them for healing and permanent DEF. But killing minions costs actions that could be spent damaging the boss. If you ignore minions, Mass Assault turns become increasingly dangerous. The optimal play is to kill minions right before Phase 2 triggers (around 60% HP), then burst past 55%.

**Counter-Strategy:** Ventus is strong against Terra. AoE Ventus skills can clear minions and damage Petravar simultaneously. Kill minions before the Phase 2 transition to deny the heal/DEF absorption. Magus Lux Bolt is strong against the Umbra secondary element. Knight should use Shield Bash on Petravar to try for Stun and prevent summons. Bring extra items for the long fight.

**Loot:**
- Ancient Fossil Core (crafting material -- Terra/Umbra hybrid armor)
- Earthkeeper's Relic (accessory -- summon attacks deal 10% less damage to wearer)
- Petrified Dragon Tooth (key item -- unlocks a hidden fossil cave with lore)

---

### 13. Prismathra, the Chromatic Aberration

**Element:** Cycles between all four classical elements each turn (Ignis -> Ventus -> Terra -> Aqua -> repeat)
**Lore:** Prismathra was an experiment -- the Conclave's attempt to create an artificial dragon capable of wielding all elements. They succeeded, but the result is unstable: a shimmering, ever-shifting beast whose body constantly changes color and composition. It exists in agony, each elemental shift tearing at its molecular structure. The binding crystal in its skull is the only thing preventing it from dissolving into raw elemental chaos. It patrols the Conclave's experimental research wing.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| High | High | Med |

**Combat Role:** Elemental Puzzle / Adaptive Threat

**Phase Mechanics:**
- **Phase 1 (100%-50% HP):** Prismathra cycles elements in a fixed order (Ignis -> Ventus -> Terra -> Aqua) each turn. Its attacks match its current element. Its elemental weakness ALSO cycles, always matching the standard weakness of its current element. The intent display shows which element it will use, allowing players to prepare the counter-element.
- **Phase 2 (Below 50% HP):** The cycle accelerates -- Prismathra changes element MID-TURN (attacks as one element, but by the time players respond, it has shifted to the next). It also gains "Chromatic Overload" every 5 turns, which hits with ALL four elements simultaneously.

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Elemental Surge | (Current cycle element) | 1.8x DMG single | (Burn if Ignis, Poison if Aqua, Stun if Terra, AtkDown if Ventus, 30%) | Attacks with the current dominant element. |
| Chromatic Shield | (Current cycle element) | Self-buff | DefUp (1t, 1.40x, 100%) | Elemental armor matching current form. |
| Phase Shift | None | Changes element | None | Cycles to the next element. Happens automatically. |
| Chromatic Overload | All Classical | 2.0x DMG all | Burn + Poison + Stun (1t each, 25% each) | ALL elements discharge simultaneously. |
| Unstable Discharge | (Current cycle element) | 2.5x DMG single | Random status effect (100%) | Elemental instability concentrates into a single devastating blast. |

**Unique Mechanic:** The elemental cycle creates a puzzle. Players must track which element Prismathra is currently using and respond with the correct counter-element. In Phase 1 this is straightforward (the intent shows the element). In Phase 2, the mid-turn shift means the "correct" element to use offensively changes by the time the player acts, requiring prediction rather than reaction. Chromatic Overload hits with all elements, meaning no single element can resist it.

**Counter-Strategy:** Requires a party that can deal damage in multiple elements. Alchemist is essential (Ignis Toss for Ventus turns, Aqua Splash for Ignis turns). Track the cycle and prepare the counter-element one turn in advance for Phase 2. Knight provides stable non-elemental damage that is always neutral. Magus can use Lux/Umbra for consistent damage since Prismathra only uses classical elements. Chromatic Overload turns are pure defense turns -- Block or Dodge.

**Loot:**
- Prismatic Scale (crafting material -- allows crafting gear with cycling elemental resistance)
- Chromatic Gem (accessory -- attacks have 10% chance to deal bonus random-element damage)
- Unstable Element Sample (quest item -- the Alchemist's Guild wants to study this)

---

### 14. Morathul, the Plaguebreath Sovereign

**Element:** Aqua (primary), Terra (secondary)
**Lore:** Morathul was the guardian of the Verdant Marsh, a wetland ecosystem so rich in life that it was considered sacred by herbalists and alchemists alike. The Conclave weaponized its breath -- once a nurturing mist that encouraged plant growth -- into a virulent plague that rots everything it touches. Morathul's own body is partially decomposed, held together by the corruption crystal and sheer magical force. It is deployed over farmlands belonging to resistance-aligned villages, turning fields to rot.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| High | Med | Med |

**Combat Role:** DoT Specialist / Healing Denial

**Phase Mechanics:**
- **Phase 1 (100%-50% HP):** Morathul focuses entirely on applying Poison and Burn (acid burns) to the entire party. It rarely deals high direct damage but stacks multiple DoTs. It also has "Miasma Field" -- a persistent effect that reduces all healing received by the party by 50%.
- **Phase 2 (Below 50% HP):** Miasma Field intensifies to 75% healing reduction. Morathul begins using "Pandemic Breath" which applies Poison to any party member NOT already Poisoned, and refreshes the duration on those who are. It gains a self-heal that scales with the number of status effects active on the player party (5% max HP per effect).

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Plague Breath | Aqua | 0.8x DMG all | Poison (3t, 5 dmg, 60%) | Exhales a cloud of virulent disease. |
| Acid Spray | Terra | 1.2x DMG single | Burn (2t, 6 dmg, 45%) + DefDown (2t, 0.80x, 30%) | Corrosive bile that eats through armor. |
| Miasma Field | None | Passive | Reduces party healing by 50%/75% | The air itself becomes toxic, hindering recovery. |
| Pandemic Breath | Aqua | 0.5x DMG all | Poison (3t, 5 dmg, 100%) to non-Poisoned | Spreads disease to the healthy. Refreshes existing Poison. |
| Festering Embrace | Terra+Aqua | 2.0x DMG single | Poison (3t, 8 dmg, 80%) + Burn (2t, 6 dmg, 80%) | Wraps a target in rotting tendrils. |

**Unique Mechanic:** Healing denial via Miasma Field. Health Potions, Mending Salve, Arcane Shield -- all healing is cut by 50-75%. This forces players to prevent damage rather than heal through it (Block/Dodge become essential). The fight is won by managing status effects aggressively with Antidotes and curing items, not by out-healing the DoTs. Morathul's self-heal from party status effects means leaving DoTs on your party also heals the boss.

**Counter-Strategy:** Terra is strong against Aqua (primary). Bring maximum Antidotes and any Poison/Burn cure items. The Alchemist's Mending Salve is less effective due to Miasma, so focus on preventing Poison with Antidotes rather than healing through it. Knight should Block frequently to avoid direct damage that stacks with DoTs. Magus Umbra Drain lifesteal is partially mitigated by Miasma but still useful. Rush to Phase 2 and burst before the status-effect self-healing spiral becomes unmanageable.

**Loot:**
- Plaguebreath Gland (crafting material -- Poison-element weapon infusion)
- Miasma Filter (accessory -- reduces incoming Poison damage by 40%)
- Verdant Marsh Seedling (key item -- can be used to begin restoring the marsh in a side quest)

---

### 15. Aeonvrax, the First Flame

**Element:** Ignis (primary), Lux (secondary), Umbra (tertiary)
**Lore:** Aeonvrax is not merely ancient -- it is primordial. The Conclave's archivists believe it was the first dragon to ever draw breath, born from the collision of light and shadow at the dawn of creation. It was found in a sealed chamber beneath the world, dormant for an age beyond counting. The Conclave spent a decade and sacrificed three hundred mages to bind it. Even bound, its power is barely contained -- the corruption crystal in its chest visibly cracks and reforms each second. Aeonvrax guards nothing. It IS the weapon. The Conclave's endgame.

**Stats:**
| HP | ATK | DEF |
|----|-----|-----|
| Extreme | Extreme | High |

**Combat Role:** Superboss / Ultimate Challenge

**Phase Mechanics:**
- **Phase 1 (100%-70% HP):** Aeonvrax tests the party with alternating Ignis, Lux, and Umbra attacks. It has 25% damage reduction from all sources. It declares two intents per turn.
- **Phase 2 (70%-35% HP):** The binding cracks further. Damage reduction drops to 10%, but ATK increases by 30%. It gains "Primordial Authority" -- once per 3 turns, it cancels the player's turn entirely (party acts but all actions fail). Declares three intents per turn.
- **Phase 3 (Below 35% HP):** Full power. No damage reduction. ATK increases by another 30% (total +60% from base). All attacks become unblockable. It uses "Genesis Fire" every 4 turns -- a full-party attack that deals 3.5x damage with all three of its elements and applies Burn, AtkDown, and DefDown simultaneously. If not killed within 10 turns of entering Phase 3, it uses "First and Last Flame" (instant party wipe).

**Signature Skills:**

| Skill Name | Element | Effect | Status | Description |
|---|---|---|---|---|
| Primordial Claw | Ignis | 2.0x DMG single | Burn (2t, 8 dmg, 50%) | A strike that carries the heat of creation. |
| Light of Ages | Lux | 1.8x DMG all | AtkDown (2t, 0.70x, 40%) | Searing radiance that makes weapons feel heavy. |
| Shadow of Ages | Umbra | 1.8x DMG all | DefDown (2t, 0.70x, 40%) | Creeping darkness that erodes armor. |
| Genesis Fire | Ignis+Lux+Umbra | 3.5x DMG all | Burn (2t, 8 dmg) + AtkDown (1t, 0.75x) + DefDown (1t, 0.75x), all 100% | The fire that started everything. |
| Primordial Authority | None | Cancels player turn | All player actions fail this turn | Reality bends to its will. |
| First and Last Flame | Ignis+Lux+Umbra | Instant party wipe | N/A | Enrage timer (10 turns in Phase 3). |

**Unique Mechanic:** Multi-phase scaling with an enrage timer. Primordial Authority forces the party to lose a turn every 3 turns in Phase 2, requiring efficiency in the remaining turns. Phase 3's unblockable attacks and 10-turn enrage timer create extreme pressure. This is the game's ultimate test of all mechanics: element exploitation, status management, burst coordination, and defensive timing.

**Counter-Strategy:** Aqua is strong against Ignis (primary). Both Lux and Umbra are mutually strong (use whichever you have). Full party required: Alchemist for Aqua damage and healing, Magus for Lux/Umbra super-effective damage, Knight for consistent output and emergency Dodges. Stockpile every item. In Phase 2, when Primordial Authority is upcoming (every 3rd turn), preemptively use buffs/healing on the turn before (they will persist). In Phase 3, commit fully to offense -- Dodge only on Genesis Fire turns. AtkUp on the Magus and burst as hard as possible.

**Loot:**
- Heart of the First Flame (legendary crafting material -- ultimate weapon component)
- Aeonvrax's Binding Shard (legendary accessory -- +10% all damage, +10% all resistance)
- Primordial Scale Armor (legendary armor -- highest DEF in the game)

---

## Section 2: 5 Dragon Encounter Types

---

### Encounter Type 1: Solo Dragon Boss

**Setup Rules:**
- Standard battle rules apply. 3-member player party vs. 1 dragon enemy.
- The dragon uses the intent-based combat system: it declares 1+ intents during the EnemyIntent phase, the player sees them, then the player party acts during PlayerTurn, followed by the dragon executing its declared intents during EnemyTurn.
- Phase transitions trigger when the dragon's HP drops below specific thresholds, changing its moveset and behavior.
- No special rules beyond standard `BattleController` logic.

**How It Differs from Standard Battles:**
- Dragons have higher stats than normal enemies, often with VHigh or Extreme in at least one stat category.
- Dragons always have at least 2 phases with mechanically distinct behavior.
- Dragons may declare multiple intents per turn (targeting different party members), unlike standard enemies which declare one.
- Dragons have unique mechanics (Slag Stacks, Weather Rotation, etc.) tracked via UI counters.

**Example Encounter:** Pyrevathan, the Cinderthrone (Dragon #1). A single Ignis dragon with Slag Stack management and a charged ultimate. The fight tests elemental awareness (Aqua is strong), defensive timing (Throne Eruption is unblockable), and resource management (long fight with multiple phases).

**Special Rewards:**
- Guaranteed unique dragon material drop (used for crafting dragon-tier equipment).
- First-time clear bonus: elemental gemstone matching the dragon's element (permanent party buff).
- Achievement/title for defeating each solo dragon.

---

### Encounter Type 2: Dragon + Handler

**Setup Rules:**
- 3-member player party vs. 2 enemies: the Dragon and the Magus Handler.
- The Handler is a humanoid enemy (UnitType.Magus) with Med HP, Med ATK, Low DEF.
- While the Handler is alive, the dragon receives the following bonuses:
  - +25% ATK (the Handler channels power into the dragon)
  - +25% DEF (the Handler maintains a magical barrier around the dragon)
  - The Handler can heal the dragon for 10% max HP once every 3 turns
  - The Handler can apply AtkUp/DefUp to the dragon
- When the Handler dies, the dragon loses all Handler bonuses. Additionally, the dragon becomes "Disoriented" for 2 turns (AtkDown 0.70x, no skills, basic attacks only). After disorientation ends, the dragon fights at base stats with no Handler support.
- The Handler declares their own intents (heal dragon, buff dragon, or attack party with Lux/Umbra skills).

**How It Differs from Standard Battles:**
- Two-target priority management. Killing the Handler first is usually optimal but the Handler often hides behind the dragon (has a "Guarded" status that redirects 50% of attacks aimed at the Handler to the dragon instead while the dragon is above 50% HP).
- The dragon's phase transitions may behave differently without Handler support (some skills require the Handler to be alive to use).
- Creates a genuine strategic choice: attack the dragon (direct approach, long fight) or attack the Handler first (shorter fight, but the Handler is hard to hit while guarded).

**Example Encounter:** Glacivorn (Dragon #2) with Handler "Magus Cryomancer Valdris." Valdris channels Aqua energy into Glacivorn, boosting its already-high DEF to extreme levels. Valdris heals the dragon and applies DefUp. To reach Valdris, players must either push Glacivorn below 50% HP (removing the Guard effect) or use skills that bypass the redirect (status effects like Poison applied directly to the Handler always work).

**Special Rewards:**
- Dragon material drop + Handler's equipment (Magus-type weapon or accessory).
- Handler's personal journal (lore item revealing Conclave secrets).
- Bonus reward if the Handler is killed before the dragon takes any damage (skill-based challenge reward: rare crafting material).

---

### Encounter Type 3: Twin Dragons

**Setup Rules:**
- 3-member player party vs. 2 dragons simultaneously.
- The twin dragons are always complementary elements (one strong against the element the other is weak to).
- Both dragons declare intents each turn (total of 2+ intents to manage).
- Linked HP mechanic: if one twin's HP drops below 30% while the other is above 60%, the wounded twin retreats and the healthy twin enrages (see Venerath and Corraxis, Dragon #7 for the detailed mechanic).
- Enrage bonuses: +40% ATK, double intents, unblockable attacks.
- The retreated twin heals 5% max HP per turn and rejoins at 50% HP.
- To avoid the retreat/enrage cycle, players must damage both twins relatively evenly.

**How It Differs from Standard Battles:**
- Two high-priority targets that must be managed simultaneously.
- HP balancing adds a layer of strategy beyond simple "kill one first."
- Double the status effect pressure (Burn from one, Poison from the other).
- Combined attacks (like Steam Eruption) only occur when both twins are active, incentivizing keeping one retreated but at the cost of the other enraging.
- Players must split elemental damage types (need two different counter-elements).

**Example Encounter:** Venerath and Corraxis (Dragon #7). Aqua twin applies Poison, Ignis twin applies Burn. Combined Steam Eruption hits all party members with both. Players need Terra (for Aqua twin) and Aqua (for Ignis twin). The fight tests party composition breadth and HP tracking discipline.

**Special Rewards:**
- Twin material drops (one from each dragon -- used together to craft dual-element gear).
- "Twin Slayer" title if both are killed on the same turn.
- Linked Binding Crystal fragment (used in the Dragon Liberation mechanic -- freeing twins grants a unique dual-element summon).

---

### Encounter Type 4: Siege Dragon

**Setup Rules:**
- The dragon is attacking a fortress (the "Wall"). The Wall is a non-combatant entity with its own HP bar (starts at 100%, tracked in UI).
- 3-member player party fights the dragon, but the dragon splits its intents between attacking the party AND the Wall.
- Each turn, the dragon declares at least one intent targeting the Wall (dealing "Siege Damage" that reduces the Wall's HP).
- If the Wall's HP reaches 0%, the fortress falls: battle lost (even if the party is alive).
- If the dragon is killed before the Wall falls: victory.
- The party can use one action per turn to "Fortify" the Wall (restoring 10% Wall HP), but this costs that party member's entire turn.
- The fight has a turn limit (12 turns). If the dragon is not dead by turn 12, the Wall automatically falls.

**How It Differs from Standard Battles:**
- Split attention: the dragon is not focused solely on the party, but the Wall is constantly in danger.
- The "Fortify" action creates a tension between offense and defense -- spending actions to repair the Wall means less damage on the dragon.
- The turn limit adds urgency that standard fights lack.
- The dragon may have siege-specific skills (battering ram charges, fire breath aimed at the Wall) that the party cannot Block or Dodge -- they must either kill the dragon or Fortify to counteract the damage.
- Positioning element: the party can choose to "intercept" a Wall-targeting attack by having a party member Dodge in front of the Wall (a special option replacing normal Dodge). If successful, the party member takes the hit instead of the Wall.

**Example Encounter:** Terrathos, the Living Mountain (Dragon #4) besieging the resistance fortress of Haldenwatch. Terrathos uses Seismic Slam to damage both the party and the Wall. Its Mountain Fall targets the Wall for 25% Wall damage. Players must balance between attacking Terrathos and Fortifying the Wall. The Knight can intercept Wall-targeting attacks. Alchemist provides damage and healing. Magus deals Ventus damage (strong vs Terra).

**Special Rewards:**
- Dragon material drop + "Fortress Defender" title.
- Bonus reward scaling with remaining Wall HP (100% Wall HP at victory = rare crafting material; below 30% = standard rewards only).
- Haldenwatch citizens offer a permanent shop discount as gratitude.
- Unique defensive accessory: Fortress Ward (reduces incoming siege damage by 15% in future siege encounters).

---

### Encounter Type 5: Ancient Wyrm Superboss

**Setup Rules:**
- Optional endgame encounter. The strongest dragon in the game.
- Full 3-member party required (all must be alive at battle start; no empty slots).
- No items can be used during the fight (the wyrm's aura nullifies alchemical items). Skills and basic attacks only.
- The wyrm has 3 phases (100%-70%, 70%-35%, 35%-0%) with escalating mechanics.
- The wyrm has a 10-turn enrage timer in the final phase. If not killed within 10 turns of entering Phase 3, it uses an instant party-wipe skill.
- Permanent death within the encounter: if a party member dies, they cannot be revived during this battle (Phoenix Down is an item, and items are disabled).
- The wyrm is immune to Stun. Poison and Burn effects have their damage halved. Only AtkUp/AtkDown/DefUp/DefDown work at full strength.

**How It Differs from Standard Battles:**
- No items forces pure skill/ability usage -- no safety net.
- The permanent death rule means every point of damage matters and every defensive decision is critical.
- Stun immunity removes the Knight's Shield Bash stun cheese strategy.
- Halved DoT damage reduces the Alchemist's attrition tools.
- The enrage timer in Phase 3 creates a hard DPS check -- the party must be able to output enough damage.
- Multi-intent with 2-3 attacks per turn, some unblockable.
- Phase 2's "Primordial Authority" (turn cancellation) means effective turns are even fewer than the timer suggests.

**Example Encounter:** Aeonvrax, the First Flame (Dragon #15). The ultimate challenge. Triple-element attacks, turn cancellation, 10-turn enrage timer, unblockable Phase 3 attacks. Players must master every battle mechanic: element exploitation, Dodge timing, AtkUp/DefUp buff management, and burst damage coordination. This fight is designed to take multiple attempts.

**Special Rewards:**
- Legendary crafting materials (Heart of the First Flame, Primordial Scale Armor).
- Aeonvrax's Binding Shard (best accessory in the game).
- "Dragonsbane" ultimate title.
- Unlocks the true ending (Aeonvrax, freed, reveals the location of the Conclave's binding master crystal -- the source of all dragon slavery).
- Unique post-battle scene: Aeonvrax speaks for the first and only time, acknowledging the party as equals.

---

## Section 3: Dragon Liberation Mechanic

---

### How the Magus Empire Controls Dragons

The Conclave's dragon binding process involves three components:

1. **The Subjugation Ritual:** A circle of Magus channelers performs a synchronized incantation that forcibly severs the dragon's connection to its elemental source. This leaves the dragon weakened and disoriented -- a window of vulnerability lasting approximately one hour, during which the physical binding must be completed.

2. **The Corruption Crystal:** A specially grown crystal infused with concentrated Umbra energy. During the vulnerability window, the crystal is surgically embedded in the dragon's body (typically the chest, skull, or between shoulder blades). The crystal creates a feedback loop: the dragon's own magical energy is siphoned through the crystal, filtered through Umbra corruption, and fed back as a control signal that overrides the dragon's will. The dragon remains conscious and aware -- it simply cannot disobey commands routed through the crystal. This is why freed dragons remember their captivity and are grateful (or furious).

3. **The Command Lattice:** A network of smaller crystals embedded along the dragon's spine and wings, connected to the primary Corruption Crystal. These amplify the control signal and allow fine-grained commands (attack this target, fly here, stop). Destroying the primary crystal severs the entire lattice, but the lattice crystals are inert without the primary -- they do not need to be individually removed.

The binding is not permanent by nature -- the Corruption Crystal degrades over time as the dragon's natural magic resists it. The Conclave must periodically "recharge" the crystal during maintenance rituals. A dragon that has been bound for a very long time (like Aeonvrax) has partially adapted to the crystal, making liberation harder but also making the crystal more unstable.

---

### Alternative Victory Condition: Liberation

In any dragon encounter, players can choose to **free the dragon** instead of killing it. This is achieved by destroying the **Corruption Crystal** -- a targetable weak point that appears under specific conditions.

**How It Works:**

1. **Crystal Exposure:** The Corruption Crystal becomes targetable when the dragon enters Phase 2 (below the first HP threshold). The crystal is shown as a separate targetable entity in the enemy list, with its own HP bar.

2. **Crystal Stats:** The crystal has moderate HP (approximately 25% of the dragon's max HP), zero DEF, and zero ATK. It does not act or declare intents. However, the dragon will prioritize protecting the crystal once it is exposed -- 50% of attacks aimed at the crystal are redirected to the dragon's body (similar to the Handler's Guard mechanic).

3. **Destroying the Crystal:** When the crystal's HP reaches 0, the dragon is immediately freed. The battle ends in a special "Liberation Victory" state. The dragon does not die -- it regains its senses, the corruption lattice shatters, and a unique cutscene plays.

4. **Dragon HP Matters:** If the dragon's HP reaches 0 before the crystal is destroyed, the dragon dies (standard kill victory). You cannot free a dead dragon. This creates a tension: attacking the dragon to push into Phase 2 (to expose the crystal) risks killing it. Players who want to liberate must be precise with their damage.

5. **Dragon Becomes Hostile to Crystal:** Once exposed, the dragon's AI changes subtly -- it occasionally attacks the crystal itself (trying to resist the binding), dealing small damage to it. Patient players can let the dragon help destroy its own crystal, but this is slow.

**Design Note:** Liberation requires more skill than killing. Players must manage their damage output carefully, switch targets to the crystal while the dragon continues attacking them, and survive without the option to just burst the dragon down. It is always easier to kill than to free.

---

### Freed Dragons as Summon Abilities

Each freed dragon becomes available as a **Dragon Summon** -- a powerful once-per-battle ability that can be activated during the PlayerTurn phase. Summoning a dragon costs the entire party's turn (all AP from all remaining party members for that turn round is consumed).

**Summon Effects:**
- The freed dragon appears and performs a single powerful action (damage, heal, buff, or status effect).
- The effect is always themed to the dragon's element and personality.
- Summons cannot be used in the same battle where the dragon was freed.
- Only one summon can be used per battle.
- Summons are NOT available in the Ancient Wyrm Superboss fight (Aeonvrax's aura suppresses them).

---

### Dragon Bond System

Beyond summons, each freed dragon grants a **permanent passive bonus** to the entire party. These bonuses are cumulative -- freeing more dragons makes the party progressively stronger.

**Bond Tiers:**
- **1-3 freed dragons:** Each individual dragon's passive bonus is active.
- **4-7 freed dragons:** All individual bonuses are active, plus a "Draconic Resonance" bonus (+5% max HP to all party members).
- **8-11 freed dragons:** Draconic Resonance increases to +10% max HP, plus skill resource costs reduced by 1.
- **12-14 freed dragons:** Draconic Resonance at +15% max HP, resource cost reduction, plus all elemental damage dealt by the party increases by 10%.
- **15 freed dragons (all):** "Dragonlord" tier. All bonuses above, plus party gains +5% crit chance and +10% dodge bonus. Unlocks the true ending path.

---

### 5 Example Freed Dragon Bonuses

1. **Freed Pyrevathan (Ignis Drake):** *Cinderthrone's Resolve* -- Party deals +15% Ignis damage. Burn effects applied by the party last 1 additional turn. Summon: "Slag Cascade" -- deals 2.5x Ignis damage to all enemies and applies Burn (3t, 8 dmg, 100%).

2. **Freed Glacivorn (Aqua Leviathan):** *Stillwater's Blessing* -- Party gains +20% Poison resistance. Healing items and skills restore 10% more HP. Summon: "Abyssal Tide" -- heals all party members for 40% max HP and cures all Poison effects.

3. **Freed Umbraxis (Umbra Wyrm):** *Eclipse Shroud* -- Party gains +10% dodge bonus (additive with base and gear). Once per battle, when a party member would be reduced to 0 HP, they survive with 1 HP instead (auto-trigger, no action required). Summon: "Shadow Step" -- makes the entire party untargetable for 1 full enemy turn.

4. **Freed Prismathra (Chromatic Aberration):** *Chromatic Attunement* -- All elemental damage dealt by the party gains +10% effectiveness (stacks with element advantage). Status effects applied by the party have +10% chance to land. Summon: "Prismatic Storm" -- hits all enemies with a random classical element at 2.0x damage, cycling through all four elements in rapid succession (4 hits total).

5. **Freed Aeonvrax (Primordial Superboss):** *Heart of the First Flame* -- All party members gain +10% to all stats (HP, ATK, DEF). All party members gain +5% crit chance. Elemental damage of all types deals +10% bonus damage. Summon: "Genesis Flame" -- deals 4.0x damage of the target's elemental weakness to all enemies. If the target has no weakness, deals 4.0x Ignis+Lux+Umbra combined damage. The ultimate summon.
