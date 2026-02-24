# Battle System Extensions & Improvements -- Agent Instruction Document

This document is a comprehensive reference for all planned battle system extensions, enemy designs, items, progression mechanics, and quality-of-life features for the turn-based JRPG battle system. Use the Quick Reference Index below to scan and select what to implement.

---

## Quick Reference Index

### A. Enemy Bestiary (65 enemies)
| # | Faction | Count | Highlights |
|---|---------|-------|------------|
| A1 | Magus Empire | 11 | Debuffers, DoT specialists, glass cannons, aura support, healer, area denial |
| A2 | Hired Knights & Mercenaries | 9 | Tanks, assassins, summoners, setup DPS |
| A3 | Dragonkin | 7 | Aerial attackers, dragon support, swarm units, bruiser, healer, ambusher |
| A4 | Corrupted Creatures | 9 | Chargers, DoT punishers, pack hunters, hazards, elites, fire/wind corrupted |
| A5 | Arcane Constructs | 9 | Walls, reflectors, swarm with revive, AoE casters, mini-bosses, fire/water constructs |
| A6 | Undead Legion | 10 | Fodder with revive, necro support, phasing wraiths, elites, fire/light undead |
| A7 | Elite / Boss Enemies | 10 | Multi-phase bosses with unique mechanics and AI |

### B. Enemy Tactics & AI Patterns (49 tactics)
| # | Category | Count | Highlights |
|---|----------|-------|------------|
| B1 | Individual AI Patterns | 17 | Berserker, Tactician, Mimic, Commander, Elemental Shifter |
| B2 | Team Compositions | 12 | Tank+Healer Wall, Resurrection Duo, Sacrifice Engine |
| B3 | Boss Mechanics | 12 | Enrage Timer, Phase Shift, Soul Link, Countdown Doomsday |
| B4 | Environmental Tactics | 8 | Ambush, Weather, Terrain, Reinforcement Waves |

### C. Dragon Encounters (15 dragons + 5 encounter types + liberation mechanic)
| # | Sub-Section | Count | Highlights |
|---|-------------|-------|------------|
| C1 | Dragon Roster | 15 | Solo, duo, cycling elements, superboss |
| C2 | Encounter Types | 5 | Solo boss, handler, twin, siege, ancient wyrm |
| C3 | Liberation Mechanic | 1 | Free dragons as summons, Dragon Bond tiers |

### D. Items, Loot & Crafting (42 items, 15 materials, 10 recipes)
| # | Category | Count | Highlights |
|---|----------|-------|------------|
| D1 | Consumable Items | 20 | HP/MP/resource restore, status cures, revive, party heals |
| D2 | Offensive Items | 10 | Elemental bombs, AoE grenades, status-inflicting items |
| D3 | Tactical Items | 12 | Evasion, element shifting, scan, aggro, dispel, reflect, extra actions |
| D4 | Enemy Drop Materials | 15 | Faction-specific drops for crafting |
| D5 | Crafting Recipes | 10 | Ranks 1-4, combine drops into high-tier items |

### E. Experience & Progression (35+ mechanics)
| # | Category | Count | Highlights |
|---|----------|-------|------------|
| E1 | Experience Types | 6 | Combat XP, Element Mastery, Class Proficiency, Skill Mastery, Bond XP, Boss Tokens |
| E2 | Stat Growth | 6 | Level-up distributions, class promotion, equipment, attribute points |
| E3 | Skill Trees | 9 branches | 3 classes x 3 branches, skill evolution system |
| E4 | Meta-Progression | 5+3 | Bestiary, achievements, difficulty, alchemy, dragon bonds (3 deferred to post-launch) |
| E5 | Battle Rewards | 6 | Victory screen, bonus conditions, streak, loot tables |

### F. Battle System Extensions (55 features)
| # | Category | Count | Highlights |
|---|----------|-------|------------|
| F1 | Combat Mechanic Extensions | 16 | Weakness/resistance, counters, combos, limit break, formations, speed/initiative, flee, multi-hit |
| F2 | New Status Effects | 14 | Regen, Haste, Silence, Reflect, Doom, Charm, Fear |
| F3 | Party System Extensions | 6 | Mid-battle swap, summons, dual attacks, guest NPCs |
| F4 | UI/UX Extensions | 11 | Damage popups, timeline, scan, speed controls, victory screen, HP bars, status icons, tooltips |
| F5 | Quality of Life | 8 | Auto-battle, repeat action, undo, skip animations, retry |

---

## A. Enemy Bestiary

65 enemies across 7 factions. Stats use shorthand: L = Low, M = Medium, H = High, VH = Very High.

---

### A1. Magus Empire (11 enemies)

| Name | UnitType | Element | HP/ATK/DEF | Role | Signature Skills | AI Notes |
|------|----------|---------|------------|------|-----------------|----------|
| Inquisitor Censor | Magus | Umbra | M/M/M | Debuffer | "Censor's Brand" Umbra+AtkDown, "Silence Decree" Stun | Targets highest-ATK player; rotates debuffs rather than stacking same one |
| Tome Sentinel | Magus | Lux | L/H/L | Glass Cannon | "Arcane Volley" Lux AoE, "Page Storm" multi-hit | Burns through MP fast, fragile; self-destructs if MP hits 0 |
| Hexweaver Adept | Magus | Umbra | M/M/L | DoT Specialist | "Hex of Decay" Poison+Burn, "Dark Tether" lifesteal | Stacks DoTs on different targets to spread pressure, then focuses lowest-HP |
| Veilstalker Spy | Knight | Ventus | L/VH/L | Assassin | "Shadowstep Strike" high single+Stun, "Vanish" untargetable 1 turn | Opens with Vanish, then strikes lowest HP; retreats if alone |
| Binding Circle Ritualist | Magus | Lux/Umbra | M/L/H | Support | "Empowerment Sigil" AtkUp allies, "Warding Circle" DefUp allies | Always buffs before attacking; prioritizes buffing strongest ally |
| Ruinscribe Igniteur | Alchemist | Ignis | M/H/M | Elemental DPS | "Ruinfire" Ignis 2.0x+Burn, "Glyph Trap" delayed AoE | Alternates setup and burst; places Glyph Trap on healer position |
| Nethervault Warden | Knight | Umbra | H/M/H | Tank | "Void Shield" self DefUp, "Suppression Slam" physical+AtkDown | Protects other Magus units; redirects attacks targeting Magus-type allies |
| Mind-Chain Psion | Magus | Umbra | L/M/M | Disruptor | "Psychic Lance" Umbra+DefDown, "Mind Fog" AoE chance Stun | Prioritizes stunning party members who are charging skills or buffing |
| Resonance Amplifier | Magus | Lux | L/L/H | Aura Support | "Arcane Resonance" +15% damage to all Magus allies, "Lux Spark" weak Lux attack | Kill first to weaken other Magus enemies; flees to back if targeted |
| Tideweaver Initiate | Magus | Aqua | M/M/M | Healer/Control | "Healing Torrent" heals ally 30HP+Regen, "Riptide Grasp" Aqua+Slow | Heals most-wounded ally first; uses Riptide on players who just buffed |
| Geomantic Sentry | Alchemist | Terra | H/L/H | Area Denial | "Earthen Barrier" creates terrain reducing ranged damage 30%, "Quake Pulse" AoE Terra+chance Stun | Establishes barriers first turn, then pulses; retreats behind barrier if damaged |

---

### A2. Hired Knights & Mercenaries (9 enemies)

| Name | UnitType | Element | HP/ATK/DEF | Role | Signature Skills | AI Notes |
|------|----------|---------|------------|------|-----------------|----------|
| Ironvow Shieldbearer | Knight | Terra | H/L/VH | Tank | "Fortress Guard" DefUp+intercepts attacks on allies, "Shield Rush" physical+Stun | Draws attacks |
| Silveredge Duelist | Knight | Ventus | M/H/M | Finisher | "Riposte Stance" counters next melee, "Deathblow" bonus damage below 50% HP | Dangerous when allies are low |
| Blackpowder Sapper | Alchemist | Ignis | M/M/L | AoE Damage | "Barrel Bomb" Ignis AoE+Burn, "Smoke Cover" grants evasion to allies | Opens with Smoke Cover |
| Ironheart Berserker | Knight | None | H/VH/L | Glass Cannon Melee | "Frenzy Strikes" 3-hit random, "Blood Rage" AtkUp when below 50% | Gets more dangerous as HP drops |
| Contract Archer | Knight | Ventus | L/M/L | Ranged | "Piercing Shot" ignores 50% DEF, "Poison Arrow" Ventus+Poison | Always targets lowest DEF |
| Field Surgeon | Alchemist | None | M/L/M | Healer | "Combat Triage" heals ally 40HP, "Stimulant Injection" AtkUp+cures debuffs on ally | Always heals most-wounded ally |
| War Hound Handler | Knight | Terra | M/M/M | Pack Fighter | "Release the Pack" summons 2 War Hounds, "Command Attack" all War Hounds attack same target | Kill Handler to demoralize hounds |
| Bounty Hunter | Knight | None | M/H/M | Target Specialist | "Death Mark" marks one player for bonus damage, "Execute" massive damage on marked target | Always marks, then executes next turn |
| Siege Engineer | Alchemist | Terra | M/M/H | Setup DPS | "Construct Ballista" creates a static turret that attacks each turn, "Fortify Position" DefUp to turret | Turret is a separate targetable entity |

---

### A3. Dragonkin (7 enemies)

| Name | UnitType | Element | HP/ATK/DEF | Role | Signature Skills | AI Notes |
|------|----------|---------|------------|------|-----------------|----------|
| Ashscale Wyvern | Knight | Ignis | M/M/L | Aerial Attacker | "Dive Bomb" high single+Burn, "Wing Buffet" AoE weak+AtkDown | Dive Bombs first, then harasses with Wing Buffet; retreats to aerial state after 2 melee hits |
| Frost Drake | Knight | Aqua | M/M/H | Harasser | "Ice Breath" Aqua cone+DefDown, "Tail Sweep" AoE physical | Often in pairs; alternates breath and sweep to keep DefDown active |
| Wyrmcult Zealot | Magus | Ignis | L/M/L | Dragon Support | "Dragon's Blessing" heals dragon ally+AtkUp, "Martyrdom" on death grants AtkUp to all allies | Priority target in dragon fights; always heals dragon over self |
| Stormwing Fledgling | Knight | Ventus | L/L/L | Swarm | "Gust Slash" Ventus, "Screech" AoE AtkDown | Always in groups of 3-4; gains +15% damage per fledgling alive |
| Stonehide Basilisk | Knight | Terra | H/H/M | Bruiser | "Petrifying Gaze" single Terra+chance Petrify, "Tail Crush" high physical | Opens with Petrifying Gaze, then focuses petrified targets with Tail Crush |
| Radiant Amphiptere | Magus | Lux | M/H/L | Burst Healer | "Sunburst Pulse" Lux AoE medium, "Luminous Mend" heals all dragonkin allies 25% HP | Heals when any dragonkin drops below 50%; otherwise uses Sunburst Pulse on clustered players |
| Shadowscale Raptor | Knight | Umbra | L/VH/L | Ambusher | "Dusk Pounce" Umbra high single (2.5x from stealth), "Fade" enters stealth for 1 turn | Always opens with Fade then Dusk Pounce; re-enters stealth every 3rd turn |

---

### A4. Corrupted Creatures (9 enemies)

| Name | UnitType | Element | HP/ATK/DEF | Role | Signature Skills | AI Notes |
|------|----------|---------|------------|------|-----------------|----------|
| Blightback Boar | Knight | Terra | H/M/M | Charger | "Tainted Gore" physical+Poison, "Stampede" AoE weak | Aggressive when Poison active on player |
| Vitriol Toad | Alchemist | Aqua | M/L/H | DoT | "Acid Glob" Aqua+Burn, "Toxic Skin" counter-Poisons melee attackers | Punishes melee |
| Gloomwing Moth | Magus | Umbra | L/M/L | Disruptor | "Duskpowder" AoE Umbra+chance Stun, "Eclipse Flutter" untargetable 1 turn | Fragile but disruptive |
| Thornmaw Vine | Knight | Terra | M/L/H | Area Denial | "Constrict" physical+Stun, "Briar Wall" DefUp+protects ally | Always shields allies first |
| Frostfang Wolf | Knight | Aqua | M/H/M | Pack Hunter | "Frozen Bite" Aqua+AtkDown, "Pack Howl" AtkUp all corrupted allies | Always in pairs, both target same player |
| Venomspore Cluster | Alchemist | Terra | L/L/L | Hazard | "Sporadic Burst" AoE Poison, "Split" on death spawns 2 smaller clusters | Must use AoE to clear |
| Riftclaw Bear | Knight | Umbra | H/VH/M | Elite | "Void Maul" very high Umbra, "Abyssal Roar" AoE AtkDown+DefDown | Immune to Stun. Rare encounter. Enrages (AtkUp) if any corrupted ally dies nearby |
| Cindermane Stalker | Knight | Ignis | M/H/L | Aggressive DPS | "Blazing Lunge" Ignis high single+Burn, "Firemane Aura" passive Burn to melee attackers | Charges highest-DEF target to bypass tanks; switches targets each turn |
| Galefeather Harpy | Magus | Ventus | L/M/M | Disruptor/Healer | "Screaming Gust" Ventus AoE+chance Silence, "Windborne Salve" heals one corrupted ally 20% HP | Silences first, then heals wounded allies; flies out of melee range every other turn |

---

### A5. Arcane Constructs (9 enemies)

| Name | UnitType | Element | HP/ATK/DEF | Role | Signature Skills | AI Notes |
|------|----------|---------|------------|------|-----------------|----------|
| Granite Warden | Knight | Terra | VH/L/VH | Wall | "Earthbind Slam" Terra+Stun, "Fortification Protocol" stacking DefUp, "Crumble" AoE on death | Absorbs damage, explodes when killed |
| Sparkcore Automaton | Alchemist | Lux | M/H/L | Burst DPS | "Prismatic Beam" high Lux (2 charges), "Overcharge" AoE Lux+self-Burn | Self-destructive burst pattern |
| Mirrorplate Sentinel | Knight | Lux | M/M/VH | Reflector | "Reflective Shell" reflects 50% magic damage for 2 turns, "Shatter Guard" AoE on shell break | Punishes magic, use physical |
| Voidheart Engine | Magus | Umbra | H/H/M | AoE Caster | "Dark Pulse" AoE Umbra, "Siphon Core" Umbra+MP restore, "Gravity Well" AoE AtkDown+DefDown | High priority target |
| Clockwork Hound | Knight | Ventus | L/M/L | Swarm | "Razor Sprint" double-hit Ventus, "Magnetic Recall" returns 2 turns after death (once) | Must kill twice |
| Obsidian Colossus | Knight | Terra | VH/H/H | Mini-Boss | "Seismic Slam" AoE Terra+Stun, "Magma Core" shifts to Ignis+AtkUp, "Tectonic Armor" flat damage reduction | Phase-shifting mini-boss |
| Galvanic Sprite | Magus | Ventus | L/M/L | Support | "Static Field" DefUp all constructs, "Jolt" Ventus+Stun, "Energize" restores ally charges | Kill early to weaken constructs; prioritizes Energize on depleted constructs |
| Furnace Golem | Knight | Ignis | H/M/M | Sustained DPS | "Slag Fist" Ignis single+Burn, "Overheat" AoE Ignis (self-damages 10%), "Thermal Vent" cures own Burn for AtkUp | Cycles Slag Fist and Overheat; uses Thermal Vent at 3 self-Burn stacks |
| Tidecaster Orb | Magus | Aqua | M/L/H | Healer/Debuffer | "Restorative Mist" heals all construct allies 15% HP, "Corroding Spray" Aqua+DefDown, "Bubble Shield" absorb on ally | Heals when any construct below 60%; otherwise debuffs highest-ATK player |

---

### A6. Undead Legion (10 enemies)

| Name | UnitType | Element | HP/ATK/DEF | Role | Signature Skills | AI Notes |
|------|----------|---------|------------|------|-----------------|----------|
| Shambling Conscript | Knight | Umbra | L/L/L | Fodder | "Clumsy Swing" weak, "Reassemble" revives once at 25% HP | Always in groups of 3-4; targets random player |
| Bone Archer | Knight | Ventus | L/M/L | Ranged | "Blight Arrow" Ventus+Poison, "Skeletal Volley" AoE if 3+ archers alive | Targets lowest DEF; switches to Volley when 3+ alive |
| Wailing Wraith | Magus | Umbra | M/H/L | Magic DPS | "Soul Shriek" AoE Umbra+AtkDown, "Life Drain" Umbra+heals self, "Phase Shift" immune to physical 1 turn | Phases when below 50% HP; uses Life Drain to sustain, Soul Shriek to pressure |
| Grave Knight | Knight | Terra | H/M/H | Tank | "Cursed Blade" physical+Poison, "Unyielding" survives lethal hit once at 1 HP | Survives one killing blow; taunts after Unyielding triggers |
| Blightwretch | Alchemist | Aqua | M/M/L | DoT Spreader | "Plague Vomit" AoE Poison+Burn, "Fester" extends all debuff durations on target by 2 turns | Extends existing debuffs; uses Fester on most-debuffed target |
| Revenant Fencer | Knight | Ventus | M/H/M | Execute | "Spectral Riposte" counters melee, "Reaping Blow" bonus damage below 50% HP, "Ghostly Waltz" dodge next attack | Uses Riposte when threatened by melee; saves Reaping Blow for targets below 50% |
| Crypt Cantor | Magus | Umbra | M/M/M | Necro Support | "Raise Dead" revives fallen undead at 50% HP, "Dirge of Despair" AoE AtkDown | Top priority target -- kill first; always revives before attacking |
| Hollow Colossus | Knight | Umbra | VH/M/H | Elite | "Crushing Grasp" high physical+Stun, "Bone Storm" AoE physical+DefDown, "Undying Frame" revives once at 30% | Uses Crushing Grasp on healer; Bone Storm when 3+ players clustered |
| Pyre Revenant | Knight | Ignis | M/H/L | Self-Destructive DPS | "Soulfire Slash" Ignis single+Burn, "Immolation" AoE Ignis on death (guaranteed), "Rage of the Pyre" AtkUp when below 30% | Attacks aggressively, explodes on death for AoE Ignis; do not overkill near low-HP allies |
| Sanctified Husk | Magus | Lux | L/M/H | Anti-Healer | "Purging Light" Lux single+removes one buff from target, "Luminous Cage" prevents healing on one player 2 turns | Targets buffed players to strip buffs; uses Luminous Cage on player healer to shut down recovery |

---

### A7. Elite / Boss Enemies (10 bosses)

Each boss has multiple phases with distinct skill sets and AI behavior.

#### Boss 1: Archmagister Veradyne, the Curator of Ashes
- **Faction:** Magus Empire
- **Element:** Ignis/Umbra
- **Stats:** VH/H/M
- **Lore:** Head of the Magus Empire's war college, views battle as academic study.
- **Phase 1 -- Lecturer (100%-50% HP):**
  - "Lecture Hall" -- summons 2 Tome Sentinels
  - "Cinder Thesis" -- AoE Ignis medium damage
  - "Warding Mantle" -- self DefUp+regen
  - AI: Resummons if Sentinels die, alternates offense and defense.
- **Phase 2 -- Cinder Archivist (50%-0% HP):**
  - "Total Immolation" -- 2-turn charge, massive AoE Ignis (interruptible with Stun)
  - "Cinder Clones" -- summons 2 illusory copies (low HP, explode Ignis on death)
  - "Ash Storm" -- AoE Ignis+Burn+AtkDown
  - AI: Aggressive, always charges Total Immolation when available. Interrupt or suffer massive AoE.

#### Boss 2: Ironmarshal Drengan, the Coin-Blooded General
- **Faction:** Hired Knights
- **Element:** Terra
- **Stats:** VH/VH/H
- **Lore:** Legendary mercenary commander, fights for whoever pays most.
- **Phase 1 -- Commander (100%-60% HP):**
  - "Call Reinforcements" -- summons 2 Knight mercenaries
  - "Shield Wall" -- grants DefUp to self and all allies
  - "Warlord's Cleave" -- high physical AoE
  - AI: Maintains reinforcements, uses Shield Wall when allies present.
- **Phase 2 -- Unbreakable (60%-0% HP):**
  - "Tactical Momentum" -- each consecutive hit on the same target increases damage by 15% (stacks 3x)
  - "Executioner's Swing" -- massive single-target on lowest HP player
  - "Iron Resolve" -- immune to debuffs for 2 turns
  - Hidden mechanic: "Grudging Respect" -- if player party survives 3 turns in Phase 2 without anyone dying, Drengan's ATK drops 15%.
  - AI: Locks onto one target and escalates damage per hit. Spread threat or lose a party member fast.

#### Boss 3: The Hollow Pontifex, Voice of the Dead King
- **Faction:** Undead Legion
- **Element:** Umbra
- **Stats:** VH/H/M
- **Lore:** Vessel for the will of the ancient Dead King, speaks with two voices.
- **Phase 1 -- Mourning Sermon (100%-50% HP):**
  - "Congregation of Bones" -- summons 3 Shambling Conscripts
  - "Requiem" -- heals all undead allies 20% HP
  - "Death Knell" -- single Umbra+Poison
  - AI: Maintains undead army, heals before attacking.
- **Phase 2 -- Dead King Speaks (50%-0% HP):**
  - "Royal Decree" -- AoE Stun (2 turns)
  - "Undying Court" -- mass revive all dead undead at 50% HP
  - "Crown of Thorns" -- damage scales with number of dead allies (more dead = more powerful)
  - AI: Uses Royal Decree first, then Undying Court, then Crown of Thorns for massive burst.

#### Boss 4: Silkvein, the Gilded Betrayer
- **Faction:** Hired Knights (Assassin)
- **Element:** Umbra/Ventus
- **Stats:** H/VH/L
- **Lore:** Former noble turned assassin, fights with theatrical cruelty.
- **Phase 1 -- Performance (100%-40% HP):**
  - "Venom Waltz" -- single Ventus+Poison
  - "Coup de Grace" -- execute: instant kill on target below 15% HP
  - "Curtain Call" -- becomes untargetable for 1 turn
  - AI: Poisons first, then waits for HP thresholds to execute.
- **Phase 2 -- Finale (40%-0% HP):**
  - Vanishes for 1 full round (untargetable)
  - All existing Poisons on players intensify (double damage)
  - "Final Bow" -- on death, AoE Poison to entire party
  - AI: Hit-and-run pattern, maximizes Poison damage.

#### Boss 5: Moltenwing Kaelrath, the First Flame
- **Faction:** Dragons
- **Element:** Ignis
- **Stats:** VH/VH/H
- **Lore:** Eldest of the fire dragons, wreathed in living flame.
- **Phase 1 -- Inferno (100%-60% HP):**
  - "Flame Breath" -- AoE Ignis+Burn
  - "Molten Scales" -- passive counter: melee attackers take Ignis damage
  - "Wing Slam" -- high physical single
  - AI: Standard aggression, punishes melee.
- **Phase 2 -- Crumbling Titan (60%-25% HP):**
  - "Collapsing Wings" -- loses flight, gains +30% DEF but melee attacks now reach it
  - "Pyroclastic Storm" -- massive AoE Ignis
  - "Magma Eruption" -- creates burning terrain (Burn damage to all each turn for 3 turns)
  - AI: Establishes burning terrain first, then uses Pyroclastic Storm. Counter by using Aqua skills to douse terrain.
- **Phase 3 -- Last Ember (25%-0% HP):**
  - "Final Conflagration" -- 3-turn charge, party wipe (interruptible with enough burst damage)
  - "Death Throes" -- on death, AoE Ignis damage to all
  - AI: Charges Final Conflagration immediately. Must be burst down.

#### Boss 6: Crystallord Myranthas, the Living Archive
- **Faction:** Arcane Constructs
- **Element:** Lux/Terra
- **Stats:** VH/H/VH
- **Lore:** A sentient crystal formation that absorbs and adapts to magical knowledge.
- **Phase 1 -- Prism (100%-50% HP):**
  - "Adaptive Scan" -- analyzes party, next turn targets weakness
  - "Summon Facets" -- creates 2 Crystal Facets (support units)
  - "Prismatic Ray" -- high Lux single target
  - AI: Scans first, then exploits weaknesses.
- **Phase Transition:** Shatters into 4 elemental shards (Ignis/Aqua/Ventus/Terra). Kill all within 3 turns or Myranthas reforms at 60% HP.
- **Phase 2 -- Reconstructed (after shards destroyed):**
  - "Adaptive Plating" -- resists the last element used against it
  - "Crystal Regeneration" -- heals 5% HP per turn
  - "Archive Overload" -- AoE Lux+Terra, scales with number of skills used against it
  - AI: Forces element variety. Punishes repetition.

#### Boss 7: Grand Inquisitor Theron Ashcroft
- **Faction:** Magus Empire (Military)
- **Element:** Umbra
- **Stats:** VH/H/H
- **Lore:** Fanatical enforcer of the Magus Empire's doctrine of magical supremacy.
- **Phase 1 -- Inquisition (100%-50% HP):**
  - "Heretic's Brand" -- marks one player for +25% damage from all Magus sources
  - "Judgment Bolt" -- high Umbra single
  - "Unshakable Faith" -- immune to AtkDown and DefDown
  - AI: Always brands first, then focuses marked target.
- **Phase 2 -- Executioner (50%-0% HP):**
  - "Inquisition Tribunal" -- summons 2 Inquisitor Censors; while they live, Ashcroft gains damage immunity
  - "Final Judgment" -- instant kill on marked target below 25% HP
  - "Dark Augment Overload" -- massive AoE Umbra, but deals 10% self-damage
  - AI: Hides behind Censors while charging Final Judgment. Kill Censors to expose him, then burst before he re-summons. Self-damage creates a ticking clock.

#### Boss 8: Abysswing Seravoth, the Drowned Sovereign
- **Faction:** Dragons
- **Element:** Aqua/Umbra
- **Stats:** VH/H/H
- **Lore:** An ancient sea dragon corrupted by abyssal void energy.
- **Phase 1 -- Rising Tide (100%-60% HP):**
  - "Flood the Arena" -- terrain effect: non-Aqua damage reduced 30%
  - "Tidal Crush" -- high Aqua single+DefDown
  - "Abyssal Gaze" -- Umbra+AtkDown
  - AI: Establishes terrain advantage, then attacks methodically.
- **Phase Transition:** Submerges for 2 turns. Each turn, massive AoE tidal wave damage. Cannot be targeted.
- **Phase 2 -- Drowned Sovereign (after resurfacing):**
  - "Deathless Scales" -- passive 5% HP heal per turn
  - "Cursed Deluge" -- AoE Aqua+Burn+Poison
  - "Void Maelstrom" -- AoE Umbra+AtkDown+DefDown
  - AI: Attrition pattern, stacks debuffs while regenerating.

#### Boss 9: The Concordance, Collective Will
- **Faction:** Magus Empire (Supreme)
- **Element:** Cycles all 6 elements (Ignis/Aqua/Ventus/Terra/Lux/Umbra)
- **Stats:** VH/H/M
- **Lore:** A gestalt entity formed from the combined will of the Magus Empire's inner circle.
- **Phase 1 -- Rotation (100%-60% HP):**
  - Cycles through 5 elemental forms, 2 turns each
  - Each form uses that element's signature attack and has matching resistance
  - "Harmonic Shield" -- reduces damage from current element by 80%
  - AI: Predictable rotation. Exploit the element it is NOT currently using.
- **Phase 2 -- Dissonance (60%-0% HP):**
  - Splits into 3 aspects (Ignis/Aqua, Ventus/Terra, Lux/Umbra) with shared HP pool
  - Must be killed simultaneously (if one dies, others heal it back within 2 turns)
  - "Resonance Cascade" -- if all 3 aspects are alive, combined AoE every 3 turns
  - "Final Proof" -- at 10% HP, 4-turn countdown to party wipe
  - AI: Forces balanced damage across all 3 aspects.

#### Boss 10: The Unbound, Primordial Dragon of the Void
- **Faction:** Final Boss
- **Element:** Umbra/Lux
- **Stats:** VH(x2)/VH/H
- **Lore:** The original dragon, imprisoned since the world's creation, now breaking free.
- **Phase 1 -- Revelation (100%-70% HP):**
  - "Void Breath" -- massive AoE Umbra
  - "Eclipse" -- alternates immunity: immune to Lux odd turns, immune to Umbra even turns
  - "Primordial Presence" -- passive AtkDown aura on entire party
  - AI: Tests the party, forces element management.
- **Phase 2 -- Duality (70%-30% HP):**
  - Splits into Lux half and Umbra half
  - Halves heal each other if damage is unbalanced
  - "Solar Flare" (Lux half) -- AoE Lux+Burn
  - "Void Collapse" (Umbra half) -- AoE Umbra+DefDown
  - AI: Must damage both halves evenly to progress.
- **Phase 3 -- Unchained (30%-0% HP):**
  - Reforms as single entity with all skills available
  - "Annihilation Beam" -- highest single-target damage in game
  - "Reality Fracture" -- AoE all elements, random status effects
  - "The End" -- 3-turn countdown to guaranteed party wipe. Must be killed before it resolves.
  - AI: All-out assault. Pure DPS race.

### Implementation Notes -- Section A

**Current architecture:** Each enemy is a MonoBehaviour implementing `IBattleUnit` + `IEnemyAI`. Stats are serialized fields. Skills are looked up via `SkillAndItemData.GetEnemySkills(UnitType)`, keyed by UnitType (Alchemist/Magus/Knight) -- NOT per-enemy. AI logic lives in `IEnemyAI.DecideAction()`.

| Feature | Architecture Impact | Key Changes Needed |
| --- | --- | --- |
| Basic enemies (simple stats + existing effects) | **None** -- new MonoBehaviour per enemy | Create prefab with TestBattleUnit or custom IBattleUnit impl; set stats in Inspector |
| Per-enemy skill sets | **Extend SkillAndItemData** | Currently skills are per-UnitType, not per-enemy. Need per-entity skill list (add `List<SkillDefinition>` to IBattleUnit or a new ISkillHolder interface), or key enemy skills by string ID instead of UnitType |
| Unique AI behaviors (e.g. "targets highest ATK", "rotates debuffs") | **None** -- new IEnemyAI subclasses | One MonoBehaviour per AI pattern, or a configurable AI with behavior flags |
| Dual elements (e.g. Lux/Umbra on Binding Circle Ritualist) | **Extend IBattleUnit** | `ElementType Element` is singular. Need `ElementType[] Elements` or a secondary element field; update `GetElementMultiplier` to check multiple |
| New status effects (Regen, Slow, Silence, etc.) | **Extend BattleEnums + BattleController** | Add values to `StatusEffectType` enum; add tick/check logic in `TickStatusEffects()` and `BattleLoop` (e.g. Silence blocks skill use) |
| AoE skills | **Extend SkillDefinition + ExecuteSkillEffect** | `SkillDefinition` has no AoE flag or multi-target support. Need `IsAoE` bool and loop in `ExecuteSkillEffect` to hit all living targets |
| Multi-hit attacks (e.g. "Frenzy Strikes" 3-hit) | **Extend SkillDefinition + ExecuteSkillEffect** | Add `HitCount` field to `SkillDefinition`; loop damage application in `ExecuteSkillEffect` |
| Summoning (War Hound Handler, bosses) | **Restructure** | No mid-battle unit spawn system. Need `AddEnemy(IBattleUnit)` method on BattleController, update `_enemyUnits`/`_enemyAIs` at runtime, handle intent list for newly spawned units |
| Untargetable / stealth states | **Extend IBattleUnit or status system** | Need `IsTargetable` property or a new StatusEffectType; update target selection in `DeclareAllEnemyIntents`, `ResolveSkillTarget`, and UI |
| Attack interception / taunt / redirect | **Restructure enemy targeting** | `DeclareAllEnemyIntents` does not check for taunt/redirect. Need a post-declaration pass or a threat/taunt system in ResolveDefense |
| Boss multi-phase AI | **Extend IEnemyAI** | `DecideAction` has no phase awareness. AI impl can track phase internally (check own HP%), but phase transitions that change stats/skills need mutable stats on IBattleUnit or a phase callback |
| Delayed/charged attacks (Glyph Trap, Total Immolation) | **Restructure BattleLoop** | No concept of multi-turn charge-up. Need a "pending action" queue that resolves on future turns, or a charging StatusEffect |
| On-death effects (Martyr heal, Crumble AoE) | **Extend BattleController** | `OnUnitDied` event exists but nothing processes on-death triggers. Need an on-death handler or `IDeathEffect` interface |
| Terrain / area denial | **Restructure** | No terrain system exists. Need a BattleField state object with active terrain effects processed each turn |

---

## B. Enemy Tactics & AI Patterns

49 total tactical patterns across 4 categories.

---

### B1. Individual AI Patterns (17)

| # | Name | Difficulty | Description |
|---|------|------------|-------------|
| 1 | Berserker | Easy | Aggression increases as HP drops (0.3 at full to 1.0 at low). Gains AtkUp at 25% HP. |
| 2 | Tactician | Medium | Always targets element weakness. Uses AtkDown on strongest player first. |
| 3 | Bodyguard | Medium | Redirects attacks to self when any ally drops below 40% HP. |
| 4 | Sniper | Medium | Always targets lowest DEF unit. Ignores taunt. High damage, fragile. |
| 5 | Healer-Priest | Easy | Prioritizes healing allies. Only attacks when all allies are at full health. |
| 6 | Glass Cannon | Easy | Maximum aggression every turn. Uses strongest available skill. Very fragile. |
| 7 | Debuffer | Medium | Applies AtkDown first turn, DefDown second turn, then attacks debuffed targets. |
| 8 | Vampire | Medium | Uses lifesteal attacks exclusively. Targets highest HP unit for maximum drain. |
| 9 | Martyr | Hard | On death: AoE heal all allies 30% HP and grant AtkUp to all surviving allies. Kill last. |
| 10 | Mimic | Hard | Copies the last skill used by any player. Uses the copied skill on its next turn. |
| 11 | Coward | Easy | Attempts to flee at 30% HP. Drops bonus loot if killed before fleeing. |
| 12 | Avenger | Medium | Gains permanent AtkUp stack each time an ally dies. Kill all simultaneously to avoid snowball. |
| 13 | Ritualist | Hard | Charges for 3 turns (visible charge bar), then unleashes devastating AoE. Interrupt with Stun. |
| 14 | Swarm Drone | Easy | Weak individually. Gains +20% damage per additional swarm member alive. |
| 15 | Saboteur | Medium | Targets player resources: burns MP, depletes skill charges, destroys held items. |
| 16 | Elemental Shifter | Hard | Changes element every 2 turns to resist the last element the player used against it. |
| 17 | Commander | Hard | Passively boosts all ally stats while alive. Issues coordination commands that synchronize ally attacks. |

---

### B2. Team Compositions (12)

| # | Name | Difficulty | Composition & Gimmick |
|---|------|------------|----------------------|
| 1 | Tank + Healer Wall | Medium | Tank absorbs all damage while Healer keeps it alive. Must kill Healer first or Tank is effectively immortal. |
|   |  |  | **Example Encounter:** Ironvow Shieldbearer (Tank) + Field Surgeon (Healer) + Contract Archer (Ranged). Shieldbearer intercepts for Surgeon while Archer chips from behind -- burn down Surgeon before Shieldbearer's DefUp stacks become impenetrable. |
| 2 | Element Triangle | Medium | 3 enemies each covering one element of the cycle. Exploit the gap element none of them cover. |
|   |  |  | **Example Encounter:** Ruinscribe Igniteur (Ignis) + Frost Drake (Aqua) + Stormwing Fledgling x2 (Ventus). No Terra coverage -- Terra skills hit all three for weakness damage; the puzzle is choosing which element threat to eliminate first. |
| 3 | Alpha & Pack | Medium | Alpha wolf boosts pack. Kill Alpha to demoralize pack (-30% stats), or pick off pack to isolate Alpha. |
|   |  |  | **Example Encounter:** Riftclaw Bear (Alpha/Elite) + Frostfang Wolf x2 (Pack). Bear enrages if a wolf dies, but wolves' Pack Howl buffs the Bear -- kill the Bear first to demoralize, or kill both wolves simultaneously to avoid the enrage trigger. |
| 4 | Debuff Cascade | Hard | 3 enemies each apply different debuffs (AtkDown, DefDown, Poison). Combined effect renders player party nearly useless. Cure or kill fast. |
|   |  |  | **Example Encounter:** Inquisitor Censor (AtkDown) + Mind-Chain Psion (DefDown) + Hexweaver Adept (Poison+Burn). All three Magus Empire units layer debuffs each turn -- if you don't kill one within 2 turns, your party is debuffed into helplessness. |
| 5 | Resurrection Duo | Hard | Two enemies that revive each other on death. Must kill both in the same turn or within 1 turn of each other. |
|   |  |  | **Example Encounter:** Crypt Cantor (Raise Dead) + Grave Knight (Unyielding). Cantor revives the Knight, Knight survives one lethal hit via Unyielding -- you must pop Unyielding, then burst both down in the same turn. |
| 6 | Lockdown Squad | Hard | 3 enemies rotate Stun on party members, keeping one player permanently stunned. Disrupt rotation by killing one. |
|   |  |  | **Example Encounter:** Mind-Chain Psion (AoE Stun) + Stonehide Basilisk (Petrifying Gaze) + Gloomwing Moth (Duskpowder Stun). Three different CC sources rotate to keep at least one party member locked down every turn -- kill the fragile Moth first to break the rotation. |
| 7 | Element Rotation Squad | Expert | 4 enemies cycle elements in synchronized pattern. Small vulnerability window each rotation. Requires timing. |
|   |  |  | **Example Encounter:** Ruinscribe Igniteur (Ignis) + Tideweaver Initiate (Aqua) + Galvanic Sprite (Ventus) + Geomantic Sentry (Terra). Each covers the previous one's weakness -- the 1-turn window when an enemy just used its big skill is the only safe time to exploit that element. |
| 8 | Shield Wall Formation | Medium | Front enemy absorbs all physical damage; magic bypasses the shield entirely. Requires mage DPS or flanking skills to bypass. Kill shield to enable melee. |
|   |  |  | **Example Encounter:** Granite Warden (Wall) + Sparkcore Automaton (Burst DPS) + Voidheart Engine (AoE Caster). Warden's massive DEF absorbs all physical hits while the two casters unload -- use magic to bypass the Warden or destroy it to expose the fragile casters. |
| 9 | Sacrifice Engine | Hard | Minions die to power the boss. Boss gains +50% ATK per dead minion. Kill boss first, or manage minion kills carefully. |
|   |  |  | **Example Encounter:** Obsidian Colossus (Boss) + Clockwork Hound x3 (Minions). Each Hound that dies powers up the Colossus, but Hounds also revive once via Magnetic Recall -- focus the Colossus while controlling Hound damage without killing them. |
| 10 | Mirror Match | Medium | Enemies mirror player party composition (same classes). Counter by cross-targeting: your Knight attacks their Magus, etc. |
|   |  |  | **Example Encounter:** Nethervault Warden (Knight-type Tank) + Binding Circle Ritualist (Magus-type Support) + Blackpowder Sapper (Alchemist-type AoE). Mirrors a balanced party -- your Knight should rush their Ritualist, your Magus should blast the Sapper, your Alchemist should debuff the Warden. |
| 11 | Relay Team | Hard | Only 1 enemy active at a time. Retreats at 25% HP and tags in the next fresh enemy. Kill before retreat threshold. |
|   |  |  | **Example Encounter:** Silveredge Duelist (Finisher) -> Ironheart Berserker (Glass Cannon) -> Bounty Hunter (Assassin). Each tags out at 25% HP; the Duelist counters melee, the Berserker hits harder as it drops, the Bounty Hunter marks and executes -- burst each before they can retreat. |
| 12 | Lux-Umbra Duality Pair | Medium | Two enemies that buff each other (Lux buffs Umbra, Umbra buffs Lux). Avoid the trap of using their mutual weakness element. |
|   |  |  | **Example Encounter:** Mirrorplate Sentinel (Lux Reflector) + Wailing Wraith (Umbra DPS). Sentinel reflects Umbra magic aimed at it, Wraith phases through Lux attacks -- using either Lux or Umbra plays into one enemy's strength; use neutral or classical elements instead. |

---

### B3. Boss Mechanics (12)

| # | Name | Difficulty | Core Mechanic |
|---|------|------------|--------------|
| 1 | Enrage Timer | Medium | Boss gains permanent AtkUp every 3 turns. Gradual escalation -- counter with sustained DPS and debuffs to slow the ramp. Unlike Countdown Doomsday, the boss becomes harder over time rather than instantly lethal. |
| 2 | Minion Summoner | Hard | Spawns adds every 4 turns. If 4+ adds alive simultaneously, triggers Brood Fury (massive AoE). Manage adds. |
| 3 | Phase Shift | Expert | 3 distinct phases with element changes: starts Lux, shifts to Umbra at 60%, cycles all elements at 30%. |
| 4 | Damage Reflection Shield | Hard | Alternates between shield turns (reflects 50% damage back) and attack turns. Time your damage carefully. |
| 5 | Target Lock | Medium | Marks one player with guaranteed unblockable attack next turn. Marked player must defend/heal; others must burst. |
| 6 | Elemental Weakness Cycle | Hard | Resistant to all elements normally. One element deals 3x damage, cycling every 3 turns. Track the cycle. |
| 7 | Counterspell Stance | Expert | Alternates each turn: immune to skills or immune to basic attacks. Must alternate attack types. |
| 8 | Soul Link | Expert | Two bosses share a single HP pool. Must balance damage evenly between them or the pool regenerates. |
| 9 | Berserk Heal | Hard | Boss heals 15% HP per turn passively. Interrupt healing by dealing >20% HP in a single turn. |
| 10 | Puppet Master | Expert | Possesses one player each turn, forcing them to attack their own allies. Must cleanse or work around it. |
| 11 | Armor Break | Medium | Extremely high DEF. Hit the weak-point element to permanently reduce DEF by a large amount. Stacks. |
| 12 | Countdown Doomsday | Expert | 8-turn countdown to guaranteed party wipe. Unlike Enrage Timer, boss does NOT get stronger over time -- it acts normally until the instant kill. Counter: focus on burst combos and vulnerability windows rather than sustained DPS. |

---

### B4. Environmental Tactics (8)

| # | Name | Difficulty | Effect |
|---|------|------------|--------|
| 1 | Ambush Battle | Medium | Enemies act first (full round before players). No intents shown on turn 1. |
|   |  |  | **Suggested Enemy Lineup:** Veilstalker Spy (Assassin) + Shadowscale Raptor x2 (Ambusher). All three open from stealth for a devastating alpha strike. |
| 2 | Weather: Ignis Storm | Medium | Ignis damage +30%, Aqua damage -30%. 3 Burn damage per turn to all units. |
|   |  |  | **Suggested Enemy Lineup:** Cindermane Stalker (Ignis DPS) + Furnace Golem (Ignis Sustained) + Ashscale Wyvern (Ignis Aerial). Storm amplifies all their Ignis attacks while suppressing player Aqua counters. |
| 3 | Terrain: Narrow Bridge | Medium | Basic attacks can only target front-row enemy. Skills bypass this restriction. |
|   |  |  | **Suggested Enemy Lineup:** Ironvow Shieldbearer (front, Tank) + Bone Archer x2 (back, Ranged) + Crypt Cantor (back, Necro Support). Shieldbearer blocks the bridge while ranged units fire safely from behind. |
| 4 | Moonlight Arena | Hard | Alternates Lux boost (+30%) and Umbra boost (+30%) every 2 turns. |
|   |  |  | **Suggested Enemy Lineup:** Sanctified Husk (Lux) + Wailing Wraith (Umbra) + Resonance Amplifier (Lux aura). Each enemy surges in power on alternating phases, so there is no safe window. |
| 5 | Reinforcement Waves | Hard | 2 new enemies arrive every 3 turns, up to 8 total enemies on the field. |
|   |  |  | **Suggested Enemy Lineup:** Initial: Shambling Conscript x3 (Fodder). Waves: Bone Archer x2, then Blightwretch + Revenant Fencer, then Hollow Colossus (Elite). Escalating undead threat tests endurance. |
| 6 | Poison Swamp | Medium | All units (both sides) are permanently Poisoned. Race to kill enemies faster than Poison kills you. |
|   |  |  | **Suggested Enemy Lineup:** Vitriol Toad (DoT + counter-Poison) + Blightback Boar (Charger) + Venomspore Cluster x2 (Hazard). Enemies that thrive in toxic conditions while stacking even more Poison. |
| 7 | Arcane Null Zone | Hard | All skills cost double MP/charges. Lux and Umbra damage halved. |
|   |  |  | **Suggested Enemy Lineup:** Ironheart Berserker (physical, no MP) + Ironvow Shieldbearer (physical Tank) + Silveredge Duelist (physical Finisher). All-physical enemies unaffected by the Null Zone while your casters are crippled. |
| 8 | Cursed Battlefield | Expert | Random status effect applied to a random unit (either side) at the start of each turn. Chaotic. |
|   |  |  | **Suggested Enemy Lineup:** Hexweaver Adept (DoT Specialist) + Gloomwing Moth (Disruptor) + Galefeather Harpy (Silence/Healer). Enemies that layer additional status effects on top of the battlefield's random chaos. |

### Implementation Notes -- Section B

**Current architecture:** `IEnemyAI.DecideAction()` receives living player party and enemy party lists, returns a single `EnemyIntent` (target, unblockable flag, estimated damage, optional skill). `TestBattleUnit` is the only IEnemyAI impl -- it uses aggression-weighted random targeting. BattleController has no concept of team coordination, environmental modifiers, or multi-turn AI state.

| Feature | Architecture Impact | Key Changes Needed |
| --- | --- | --- |
| Individual AI patterns (Berserker, Tactician, Sniper, etc.) | **None** -- new IEnemyAI subclasses | Create one MonoBehaviour per pattern (or a parameterized base class). Each reads party/enemy state in `DecideAction` to make decisions. Most patterns (1-8, 11, 14) work within the existing signature. |
| Mimic (copies player skill) | **Extend IEnemyAI or add shared state** | `DecideAction` has no visibility into what the player just did. Need `BattleController` to expose last player action/skill, or pass it as a parameter. |
| Avenger / Commander (react to ally death) | **Extend** -- need death notification | AI needs to track ally death count. Can subscribe to `OnUnitDied` event, or BattleController passes death count. Commander's passive stat boost requires an aura system not yet present. |
| Team Compositions (Tank+Healer, Resurrection Duo, etc.) | **None for AI** -- new IEnemyAI subclasses | Team coordination is just each AI being aware of ally roles. The hard part is shared team state (e.g. "who is taunting"). Consider a shared `BattleTeamContext` object passed to `DecideAction`. |
| Resurrection Duo (revive each other) | **Extend BattleController** | No mid-battle revive for enemies. Need `Revive()` call path from enemy skills, plus logic in `ExecuteSkillEffect` to target dead allies. `EnemyIntent` targeting a dead unit is currently rejected. |
| Boss Mechanics (Enrage, Phase Shift, Soul Link, Puppet Master, etc.) | **Restructure for complex ones** | Simple ones (Enrage Timer, Target Lock) work as AI state + status effects. Complex ones require: Damage Reflection = new defense pass; Soul Link = shared HP pool not in IBattleUnit; Puppet Master = player control override in BattleLoop; Countdown Doomsday = turn-count tracker with auto-kill. |
| Environmental Tactics (Ambush, Weather, Terrain, Reinforcements) | **Restructure** | Ambush = reorder BattleLoop to skip first player phase. Weather = global damage modifier not yet supported. Terrain = row/position system absent. Reinforcements = mid-battle enemy spawn (same as summoning in A). |
| Reinforcement Waves | **Restructure** | Need `AddEnemy()` on BattleController + re-declaration of intents for new arrivals. Current intent system declares once per turn. |

---

## C. Dragon Encounters

Full specifications are in `Assets/Scripts/DragonEncounterDesign.md`. This section provides a summary.

---

### C1. Dragon Roster (15 dragons)

| # | Name | Title | Element | Role | Unique Mechanic |
|---|------|-------|---------|------|-----------------|
| 1 | Pyrevathan | Cinderthrone | Ignis | Glass Cannon / Area Pressure | Slag Stack system -- stacks amplify AoE damage; players clear stacks by dealing Aqua damage exceeding 20% of max HP per hit |
| 2 | Glacivorn | Stillwater Leviathan | Aqua | Tank / Attrition | Permafrost targets highest-DEF party member; Undertow cancels Block/Dodge stances, forcing defensive rotation |
| 3 | Zephyrax | Stormcaller Sovereign | Ventus | Glass Cannon / Speed Controller | Multi-intent system (2-3 attacks per turn), forces defensive allocation choices across party members |
| 4 | Terrathos | Living Mountain | Terra | Ultra-Tank / Siege Monster | Wind-up telegraph attacks in Phase 1 give extra prep turn; telegraph disappears in Phase 2, all attacks become unblockable |
| 5 | Umbraxis | Eclipse Wyrm | Umbra | Debuffer / Assassin | Untargetable Eclipse Veil turns force buff/heal rhythm; Phase 2 Shadow Duplicate mirrors attacks at 50% damage |
| 6 | Luxarion | Dawnbreaker Wyrm | Lux | Healer-Turned-Aggressor / Phase Inversion | Phase 2 inverts heal into self-damage (+50% ATK); Purging Light strips all party buffs |
| 7 | Venerath & Corraxis | Twinbound Serpents | Aqua/Ignis | Synergy Duo / Combo Attackers | Linked HP with retreat/enrage -- wounded twin retreats at 30% HP to heal, healthy twin enrages (+40% ATK, unblockable); must balance damage |
| 8 | Lithocron | Crystal Tyrant | Terra/Lux | Defensive Fortress / Reflect Tank | Crystal Refraction reflects 30-50% damage to attacker; spawns Crystal Shard adds that explode for Lux AoE on death |
| 9 | Nihildrake | Void Maw | Umbra/Aqua | Lifesteal Sustain / Entropy | 40-60% lifesteal on all attacks makes chip damage useless; Entropy Wave applies double debuff (AtkDown+DefDown); Reality Tear is 50% instant-kill (dodgeable) |
| 10 | Galebrand | Tempest Warden | Ventus/Terra | Control / AoE Debuffer | Weather rotation system cycles Gale (AtkDown), Dust Storm (DefDown), and Calm (double damage) every 2 turns; Phase 2 Superstorm combines all |
| 11 | Sanguinax | Bloodforged Drake | Ignis/Umbra | Berserker / Self-Destructive Attacker | Self-damage economy (3-6% HP per attack) with lifesteal and stacking AtkUp; Sanguine Nova at 20% HP is 4.0x AoE suicide bomb |
| 12 | Petravar | Fossilbound Ancient | Terra/Umbra | Summoner / Attrition Boss | Summons Fossil Minions that grant +5% DEF each; Phase 2 absorbs minions to heal 5% HP each and gain permanent DEF; must manage add count before transition |
| 13 | Prismathra | Chromatic Aberration | Cycling (Ignis/Ventus/Terra/Aqua) | Elemental Puzzle / Adaptive Threat | Fixed element cycle each turn with matching weakness; Phase 2 shifts element mid-turn requiring prediction; Chromatic Overload hits all 4 elements simultaneously |
| 14 | Morathul | Plaguebreath Sovereign | Aqua/Terra | DoT Specialist / Healing Denial | Miasma Field reduces all party healing by 50% (Phase 1) to 75% (Phase 2); self-heals 5% max HP per active status effect on party |
| 15 | Aeonvrax | The Eternal Pyre | Ignis/Lux/Umbra | Superboss / Ultimate Challenge | Triple-element, 3 phases, Primordial Authority cancels player turns every 3 turns in Phase 2, 10-turn enrage timer in Phase 3, all Phase 3 attacks unblockable |

---

### C2. Encounter Types (5)

| # | Type | Description |
|---|------|-------------|
| 1 | Solo Dragon Boss | Standard single-dragon fight. 3-member party vs 1 dragon. Full phase system, multi-intent declarations, unique mechanics tracked via UI counters. Higher stats than normal enemies. |
| 2 | Dragon + Handler | Dragon accompanied by a Magus Handler. Handler grants dragon +25% ATK/DEF, heals dragon 10% HP every 3 turns. Handler has "Guarded" status redirecting 50% of attacks to dragon while dragon above 50% HP. Kill Handler to disorient dragon (AtkDown 0.70x, basic attacks only for 2 turns). |
| 3 | Twin Dragons | Two complementary-element dragons with linked HP. If one drops below 30% while the other is above 60%, wounded twin retreats (heals 5% HP/turn, untargetable) and healthy twin enrages (+40% ATK, double intents, unblockable). Must balance damage evenly. |
| 4 | Siege Dragon | Protect a fortress Wall (separate HP bar) from dragon attacks. Dragon splits intents between party and Wall. Party can Fortify (restore 10% Wall HP, costs full turn) or intercept Wall-targeted attacks. 12-turn limit -- Wall falls automatically if dragon not killed in time. |
| 5 | Ancient Wyrm Superboss | Optional endgame. No items allowed (wyrm aura nullifies). Permanent death (no revives during battle). Stun immunity. Poison/Burn damage halved. 10-turn enrage timer in Phase 3. 3 phases with escalating mechanics. The ultimate challenge. |

---

### C3. Dragon Liberation Mechanic

#### Binding System
Dragons are controlled by the Magus Conclave through three components:
- **Subjugation Ritual** -- A synchronized incantation by a circle of Magus channelers that severs the dragon's connection to its elemental source, creating a one-hour vulnerability window for physical binding.
- **Corruption Crystal** -- An Umbra-infused crystal surgically embedded in the dragon (chest, skull, or shoulders). Creates a feedback loop: siphons the dragon's magic through Umbra corruption and feeds it back as a control signal overriding the dragon's will. The dragon remains conscious and aware but cannot disobey.
- **Command Lattice** -- A network of smaller crystals along the spine and wings, connected to the primary Corruption Crystal. Amplifies the control signal for fine-grained commands. Destroying the primary crystal severs the entire lattice.

#### Alternative Win Condition
Instead of killing the dragon, players can destroy the exposed **Corruption Crystal** during Phase 2:
- The crystal becomes targetable when the dragon enters Phase 2 (first HP threshold).
- **Crystal stats:** ~25% of dragon's max HP, 0 DEF, 0 ATK. It does not act or declare intents.
- The dragon prioritizes protecting the crystal: **50% of attacks aimed at the crystal are redirected to the dragon** (similar to the Handler Guard mechanic).
- The dragon occasionally attacks its own crystal (resisting the binding), dealing small self-damage to it.
- If the dragon's HP reaches 0 before the crystal is destroyed, the dragon dies (standard kill). You cannot free a dead dragon.
- Liberation requires more skill than killing: players must manage damage output precisely, switch targets, and survive without just bursting the dragon down.

#### Freed Dragon Summons
Each freed dragon becomes a once-per-battle Dragon Summon ability:
- Summoning **costs the entire party's turn** (all AP from all remaining party members for that round is consumed)
- Dragon performs a single powerful themed action (damage, heal, buff, or status effect)
- 1 use per battle, only one summon per battle
- **Cannot** be used in the same battle where the dragon was freed
- **Not available** in the Ancient Wyrm Superboss fight (Aeonvrax's aura suppresses summons)

#### Dragon Bond Tiers
Each freed dragon grants a permanent passive bonus. Cumulative tier bonuses activate at milestones:

| Bond Tier | Dragons Freed | Bonus |
|-----------|---------------|-------|
| Tier 1 | 1-3 | Each individual dragon's passive bonus is active |
| Tier 2 | 4-7 | All individual bonuses + Draconic Resonance (+5% max HP to all party members) |
| Tier 3 | 8-11 | Draconic Resonance +10% max HP + skill resource costs reduced by 1 |
| Tier 4 | 12-14 | Draconic Resonance +15% max HP + resource cost reduction + all elemental damage +10% |
| Tier 5 | 15 (all) | Dragonlord tier: all above + 5% crit chance + 10% dodge bonus + unlocks true ending path |

#### Example Freed Dragon Bonuses
- **Pyrevathan:** +15% Ignis damage, Burn effects last +1 turn. Summon "Slag Cascade" -- 2.5x Ignis AoE + Burn.
- **Glacivorn:** +20% Poison resistance, healing +10% effective. Summon "Abyssal Tide" -- heals all 40% max HP, cures Poison.
- **Umbraxis:** +10% dodge bonus, once-per-battle survive lethal hit at 1 HP. Summon "Shadow Step" -- party untargetable 1 enemy turn.
- **Prismathra:** +10% all elemental damage, +10% status chance. Summon "Prismatic Storm" -- 4-hit cycling all classical elements at 2.0x each.
- **Aeonvrax:** +10% all stats, +5% crit, +10% all elemental damage. Summon "Genesis Flame" -- 4.0x damage of target's weakness to all enemies.

### Implementation Notes -- Section C

**Current architecture:** Battles are 1-vs-N with a fixed unit list. No targetable sub-objects, alternate win conditions, external HP bars, multi-intent per enemy, or persistent post-battle unlocks.

| Feature | Architecture Impact | Key Changes Needed |
| --- | --- | --- |
| Solo Dragon Boss (multi-phase) | **None for basic; Extend for multi-intent** | Basic 2-phase = IEnemyAI checks own HP%. Multi-intent needs `DecideAction` returning `List<EnemyIntent>`. |
| Dragon + Handler (guarded redirect) | **Extend** | Attack redirect pass in `ExecuteEnemyAttacks` or `ResolveDefense`. |
| Twin Dragons (linked HP) | **Restructure** | Shared HP pool not in per-unit `CurrentHP`. Need sync wrapper. Retreat = new unit state. |
| Siege Dragon (Wall objective) | **Restructure** | Non-combatant HP entity, multi-intent splitting, alternate loss condition. |
| Ancient Wyrm (no items, perma-death) | **Extend** | Battle flags in `SelectItem`/revive; per-unit status immunity. |
| Liberation (corruption crystal) | **Restructure** | Sub-target system or crystal as IBattleUnit. Alternate win condition logic. |
| Freed Dragon Summons | **Restructure** | Unlock tracker, new `PlayerAction.Summon`, consume all party AP. |
| Dragon Bond Tiers | **Extend** -- outside battle | Meta-progression ScriptableObject. Bonuses in IBattleUnit at init. |

---

## D. Items, Loot & Crafting

---

### D1. Consumable Items (20)

Design targets (% of expected mid-game HP ~150-250):
- Basic: heal ~20-30% (~40-60 HP), Mid: ~50% (~80-120 HP), High: ~80% (~150 HP), Rare: 100%.
- Party heals should be roughly 60% of the single-target equivalent at the same tier.

| Name | Tier | Effect | Target | Special |
|------|------|--------|--------|---------|
| Herb Poultice | Basic | Heal 50 HP | Single | - |
| Healing Draught | Mid | Heal 120 HP | Single | - |
| Grand Elixir | High | Heal 200 HP | Single | - |
| Ether Shard | Basic | Restore 20 MP | Single | - |
| Ether Core | Mid | Restore 50 MP | Single | - |
| Voltaic Capsule | Basic | Restore 2 Charges | Single | - |
| Voltaic Battery | Mid | Restore 5 Charges | Single | - |
| Antidote | Basic | - | Single | Cure Poison |
| Burn Salve | Basic | - | Single | Cure Burn |
| Smelling Salts | Basic | - | Single | Cure Stun |
| Vigor Tonic | Mid | - | Single | Cure AtkDown |
| Ironbark Tea | Mid | - | Single | Cure DefDown |
| Phoenix Down | Rare | Revive at 50% HP | Single | Revive from KO |
| War Drum Powder | Mid | - | Single | AtkUp 3 turns |
| Ironwall Pill | Mid | - | Single | DefUp 3 turns |
| Field Ration | Basic | Heal 30 HP | All allies | - |
| Medic's Kit | High | Heal 90 HP | All allies | - |
| Panacea | High | - | Single | Cure all status effects |
| Full Restore | Rare | Heal max HP | Single | Cure all status effects |
| Sovereign Elixir | Rare | Heal max HP + full resource restore | Single | Cure all status effects |

---

### D2. Offensive Items (10)

Design note: Item damage is flat (ignores ATK/DEF), so it must stay below skill damage to
avoid replacing skills. Reference: early skills deal ~15-30 effective damage ((ATK-DEF)*mult).
Basic bombs should match a weak skill (~25), Mid should match a mid skill (~35), High AoE
should deal less per-target than a single-target skill (~30 per target).

| Name | Tier | Damage | Element | Target | Special |
|------|------|--------|---------|--------|---------|
| Ignis Bomb | Basic | 25 | Ignis | Single | - |
| Ventus Bomb | Basic | 25 | Ventus | Single | - |
| Terra Bomb | Basic | 25 | Terra | Single | - |
| Aqua Bomb | Basic | 25 | Aqua | Single | - |
| Lux Bomb | Mid | 40 | Lux | Single | - |
| Umbra Bomb | Mid | 40 | Umbra | Single | - |
| Havoc Grenade | High | 30 | None | All enemies | - |
| Poison Vial | Mid | 10 | Terra | Single | Inflict Poison 70% chance |
| Stun Grenade | Mid | 15 | Lux | Single | Inflict Stun 50% chance |
| Inferno Flask | High | 45 | Ignis | All enemies | Inflict Burn 40% chance |

---

### D3. Tactical Items (12)

Design note: Consolidated 4 element sigils into 1 generic sigil. Added aggro, dispel,
position-swap, status-immunity, and reflect items to give tactical depth beyond
what skills provide.

| Name | Tier | Effect | Target | Duration |
|------|------|--------|--------|----------|
| Smoke Bomb | Basic | Evasion +30% | All allies | 2 turns |
| Iron Aegis | Mid | DefUp +25% | All allies | 3 turns |
| Elemental Sigil | Mid | Shift element to chosen classical element (Ignis/Aqua/Ventus/Terra) | Single ally | 3 turns |
| Scan Lens | Basic | Reveal enemy stats and weakness | Single enemy | Instant |
| Escape Flare | Basic | Guaranteed flee from non-boss battles | All | Instant |
| Decoy Puppet | High | Redirect all single-target attacks to puppet | Field | 2 turns or 1 hit |
| Chrono Dust | Rare | Grant extra action this turn | Single ally | Instant |
| Taunt Banner | Mid | Force all enemies to target one ally for 1 turn | Single ally | 1 turn |
| Dispel Powder | Mid | Remove all buffs from target enemy | Single enemy | Instant |
| Swap Charm | Mid | Swap front-row and back-row position of two allies | Two allies | Instant |
| Null Tonic | High | Grant immunity to the next status effect received | Single ally | Until triggered |
| Mirror Shard | High | Reflect the next single-target skill back at caster | Single ally | Until triggered |

---

### D4. Enemy Drop Materials (15)

| Name | Source Faction | Drop Rate | Primary Use |
|------|---------------|-----------|-------------|
| Arcane Crystal | Magus Empire | 30% | Magic items, ether crafting |
| Dark Essence | Magus Empire | 15% | Umbra items, Panacea |
| Spell Scroll Fragment | Magus Empire | 40% | Skill books, bombs |
| Tempered Weapon Shard | Hired Knights | 35% | Weapon upgrades |
| Knight's Insignia | Hired Knights | 10% | Rare gear crafting |
| Dragon Scale | Dragons/Dragonkin | 20% | High-tier armor |
| Dragon Fang | Dragons/Dragonkin | 15% | Weapon upgrades |
| Elemental Gem | Dragons/Dragonkin | 12% | Sigils, elemental bombs |
| Rusted Gear | Arcane Constructs | 45% | Basic components |
| Construct Core | Arcane Constructs | 10% | Batteries, high-tier crafting |
| Soul Fragment | Undead Legion | 25% | Phoenix Down, Lux items |
| Ancient Bone | Undead Legion | 40% | Status cures, Terra bombs |
| Corrupted Ichor | Corrupted Creatures | 30% | Poison items, debuff items |
| Mutant Hide | Corrupted Creatures | 35% | Armor, Decoy Puppet |
| Void Shard | Corrupted Creatures | 8% | Chrono Dust, rare elixirs |

---

### D5. Crafting Recipes (10)

Design note: Every recipe output must be clearly better than just buying the items
separately. Rank 1 recipes yield x2 output to justify the rare-material cost. Void Shard
requirement reduced from x2 to x1 to respect its 8% drop rate (~12-13 kills per shard).

| Recipe | Materials Required | Output | Alchemy Rank |
|--------|--------------------|--------|-------------|
| Refined Ether | Arcane Crystal x2 + Ether Shard x1 | Ether Core x2 | Rank 1 |
| Charged Cell | Rusted Gear x3 + Construct Core x1 | Voltaic Battery x2 | Rank 1 |
| Elemental Munition | Elemental Gem x1 + Spell Scroll Fragment x2 | Lux Bomb x2 or Umbra Bomb x2 | Rank 2 |
| Purification Draught | Ancient Bone x2 + Dark Essence x1 + Herb Poultice x1 | Panacea x1 | Rank 2 |
| Sunbird Feather | Soul Fragment x3 + Arcane Crystal x1 | Phoenix Down x1 | Rank 3 |
| Commander's Ration | Tempered Weapon Shard x2 + Knight's Insignia x1 | War Drum Powder x3 + Ironwall Pill x1 | Rank 2 |
| Fortress Ward | Dragon Scale x2 + Knight's Insignia x1 + Rusted Gear x2 | Iron Aegis x2 | Rank 3 |
| Toxin Refinery | Corrupted Ichor x3 + Ancient Bone x1 | Poison Vial x3 + Inferno Flask x1 | Rank 2 |
| Shadow Mannequin | Mutant Hide x3 + Dark Essence x1 + Rusted Gear x2 | Decoy Puppet x1 | Rank 3 |
| Temporal Distillate | Void Shard x1 + Construct Core x1 + Soul Fragment x2 | Chrono Dust x1 | Rank 4 |

### Implementation Notes -- Section D

**Current architecture:** `ItemDefinition` supports flat HP heal, resource restore, flat damage with element, single status cure, and revive. Inventory is `List<ItemSlot>` from `GetStartingInventory()`. No AoE items, buff-applying items, crafting, drop tables, or materials.

| Feature | Architecture Impact | Key Changes Needed |
| --- | --- | --- |
| Basic consumables (HP/MP/cure/revive) | **None** -- add entries | Existing `ItemDefinition` fields cover these. Just add to inventory. |
| Cure-all items (Panacea, Full Restore) | **Extend ItemDefinition** | `CuresEffect` is single type. Add `CuresAll` bool. |
| Party-target items (Field Ration, Medic's Kit) | **Extend ItemDefinition + ExecuteItemEffect** | Add `IsPartyTarget` bool; loop over living allies. |
| AoE offensive items (Havoc Grenade) | **Extend ItemDefinition + ExecuteItemEffect** | Add `IsAoEEnemy` flag; loop over living enemies. |
| Status-inflicting items (Poison Vial) | **Extend ItemDefinition** | Add `AppliesEffect` fields mirroring SkillDefinition. |
| Tactical items (Smoke Bomb, Decoy, Chrono Dust) | **Extend significantly** | Buff application, field effects, AP manipulation -- each unique. |
| Enemy Drop Materials + Crafting | **New system** -- outside battle | Loot tables, material inventory, recipes. No BattleController changes. |
| Loot Tables on victory | **Extend end-of-battle** | Roll drops after `OnBattleWon`. Need enemy-to-loot mapping. |

---

## E. Experience & Progression

---

### E1. Experience Types (6)

Design note: Reduced from 8 to 6 types. Tactical XP merged into Combat XP (weakness
exploits and perfect blocks grant bonus Combat XP). Alchemical Knowledge merged into
Class Proficiency for Alchemist (crafting IS their class action). Each type must be
earned distinctly and unlock a distinct reward track.

| Name | Earned From | Unlocks | Priority |
|------|-------------|---------|----------|
| Combat XP | All battles (scaled by enemy level); bonus for weakness exploits, perfect blocks, status combos | Level-ups, stat growth, tactical bonuses | Core |
| Element Mastery | Dealing damage with element-typed skills (not items) | Element damage bonuses (+5/10/15%), advanced elemental spells | Core |
| Class Proficiency | Class-specific actions (Knight: blocking/taunting, Magus: casting, Alchemist: items/crafting) | Skill tree tier unlocks, class promotion, crafting rank (Alchemist) | Core |
| Skill Mastery | Repeated use of individual skills (per-skill counter) | Skill evolution (+, Evolved, Ultimate forms) | High |
| Bond XP | Support actions toward same partner across battles (healing, buffing, covering) | Dual attacks, cross-class skill unlocks | High |
| Boss Tokens | Boss kills (1 per unique boss first clear; repeatable in NG+) | Unique upgrades, rare recipes, cosmetic titles | Low |

---

### E2. Stat Growth (6 mechanics)

#### Level-Up Distributions
Per-class growth rates determine stat gains on level-up:
- **Knight:** HP-focused (+12 HP, +2 ATK, +4 DEF per level)
- **Magus:** ATK/MP-focused (+6 HP, +5 ATK, +1 DEF, +8 MP per level)
- **Alchemist:** Balanced (+8 HP, +3 ATK, +3 DEF, +4 MP, +2 Charges per level)

#### Class Promotion (at Level 15)
Each class can promote to one of two specializations. On promotion, the character keeps
all skills from their 3 base branches but gains access to a new 4th promotion branch
(4 skills + 1 Ultimate). The promotion branch replaces the class's base stat growth
curve with the promoted curve. Promotion is permanent -- choose carefully.

- **Knight:** Paladin (tank/support hybrid, gains healing aura skills) or Champion (offensive melee, gains armor-piercing skills)
- **Magus:** Archmage (pure magical power, gains multi-target burst) or Warlock (lifesteal/debuff specialist, gains curse stacking)
- **Alchemist:** Artisan (crafting/item mastery, gains enhanced item effects and auto-brew) or Sage (elemental versatility, gains element-combo skills)

#### Equipment
3 equipment slots per character:
- **Weapon** -- primarily affects ATK, may grant element affinity
- **Armor** -- primarily affects DEF and HP
- **Accessory** -- grants special effects (status immunity, element resistance, stat bonuses)

#### Dungeon Momentum
Temporary buffs accumulate across consecutive encounters within a dungeon:
- Each victory grants a small stacking buff (+2% ATK, +1% DEF per consecutive win)
- Resting at a save point resets momentum but fully heals the party
- Creates risk/reward decision: push forward with buffs or rest and lose momentum

#### Stat Soft Caps
- Diminishing returns activate past 80% of maximum stat value
- Hard cap at maximum value prevents over-stacking
- Encourages diverse stat builds rather than single-stat focus

#### Attribute Points
- 3 points per level, freely distributed across: HP, ATK, DEF, Speed
- Respec available at camp for a material cost

---

### E3. Skill Trees (3 classes x 3 branches)

Each class has 3 skill branches. Each branch contains 4 skills plus 1 Ultimate at the end.

#### Alchemist Branches
- **Pyrotechnics** -- Ignis-focused DPS: fire bombs, burn stacking, AoE explosions. Identity: sustained AoE pressure.
  - Ignis Toss (Ignis single 1.2x), Volatile Mix (Ignis single 1.5x+Burn), Firestorm Flask (Ignis AoE 1.0x+Burn), Napalm Surge (Ignis AoE 1.3x, Burn extends +1 turn). **Ultimate:** Inferno Cascade (Ignis AoE 2.0x+Burn all, Burn damage doubled for 3 turns).
- **Cryogenics** -- Aqua-focused crowd control: freezing, slow, shatter combos. Identity: enemy lockdown. (Renamed from Hydromancy; healing mists overlapped with Mending Salve and Transmutation support.)
  - Frost Vial (Aqua single 1.0x+Slow), Cryo Bomb (Aqua AoE 0.8x+Slow), Deep Freeze (Aqua single 1.5x+Stun if Slowed), Shatter (2.0x physical to Stunned target, ignores DEF). **Ultimate:** Absolute Zero (Aqua AoE 1.5x, Stun all Slowed enemies 2 turns).
- **Transmutation** -- Buff/utility: stat transmutation, item enhancement, resource conversion. Identity: party enabler.
  - Mending Salve (heal single 40 HP), Fortify Brew (single DefUp 3 turns), Catalytic Boost (single AtkUp+Regen 3 turns), Essence Swap (convert 30 MP into 50 HP or vice versa). **Ultimate:** Philosopher's Draft (all allies: heal 30%, cure all debuffs, AtkUp+DefUp 2 turns).

#### Magus Branches
- **Radiance** -- Lux-focused burst DPS: light beams, holy damage, anti-undead. Identity: single-target burst.
  - Lux Bolt (Lux single 1.3x), Radiant Burst (Lux single 1.8x), Holy Lance (Lux single 2.2x, +50% vs Undead), Purifying Beam (Lux single 1.5x+removes 1 buff from target). **Ultimate:** Judgement Ray (Lux single 3.0x, ignores 50% DEF, +100% vs Undead).
- **Shadow Weaving** -- Umbra-focused lifesteal/debuff: life drain, curse stacking, fear. Identity: self-sustain DPS.
  - Shadow Bolt (Umbra single 1.2x), Life Siphon (Umbra single 1.0x+heal 50% of damage dealt), Creeping Dread (Umbra single 1.0x+Fear 2 turns), Curse Stack (Umbra single 0.8x, +0.3x per debuff on target). **Ultimate:** Abyssal Ruin (Umbra AoE 2.0x+lifesteal 30%, Fear all 1 turn).
- **Arcane Warding** -- Defensive/utility: magic shields, reflect, dispel, mana restoration. Identity: magic tank.
  - Mana Shield (single, absorb next 60 damage), Reflect Ward (single, Reflect 1 magic attack), Dispel (remove all buffs from one enemy), Arcane Restoration (restore 40 MP to one ally). **Ultimate:** Prismatic Aegis (all allies: Reflect 1 attack + Protect + Shell 3 turns).

#### Knight Branches
- **Vanguard** -- Offensive melee: charge attacks, combo strikes, armor penetration. Identity: single-target burst.
  - Power Strike (physical single 1.5x), Armor Rend (physical single 1.2x+DefDown 3 turns), Rush Combo (physical single 3-hit 0.7x each), Piercing Thrust (physical single 2.0x, ignores 50% DEF). **Ultimate:** Execution Cleave (physical single 3.5x, +50% damage below 30% HP).
- **Sentinel** -- Tank/counter: taunt, counter-attack, damage reduction, shield wall. Identity: party protector.
  - Taunt (force all enemies to target self 1 turn), Counter Stance (counter next melee for 1.0x physical), Iron Guard (self DefUp +40% 2 turns), Cover (intercept all attacks on one ally 1 turn). **Ultimate:** Unbreakable Wall (self: immune to damage 1 turn, counter all attacks for 1.5x).
- **Warlord** -- Party buff/command: war cries (AtkUp/DefUp), formation commands, morale passives. Identity: force multiplier.
  - Battle Cry (all allies AtkUp 2 turns), Rally Guard (all allies DefUp 2 turns), Inspiring Shout (all allies cure AtkDown+DefDown), Command Strike (single ally takes a bonus attack at 0.8x). **Ultimate:** Supreme Command (all allies: AtkUp+DefUp+Regen 3 turns, +1 AP this turn).

#### Cross-Class Skills
Unlocked via Bond XP Level 4 between two characters. Examples:
- Knight + Magus: "Spellblade" -- physical attack with element enchantment
- Magus + Alchemist: "Catalytic Surge" -- AoE heal + AoE damage in one action
- Knight + Alchemist: "Fortified Elixir" -- item use that also grants DefUp

#### Skill Evolution System
Skills evolve through repeated use. Thresholds assume ~2 uses per battle and ~50 battles
per playthrough, so Ultimate should be reachable late-game for a focused skill but not
for every skill.

| Stage | Mastery Required | Suffix | Effect |
|-------|-----------------|--------|--------|
| Base | 0 | (none) | Standard skill |
| Enhanced | 10 uses | + | +20% damage/healing, minor bonus effect |
| Evolved | 25 uses | (new name) | Significantly enhanced, new secondary effect |
| Ultimate | 50 uses | (unique name) | Maximum power, unique mechanic added |

Example evolution chains (one per class):
- Alchemist: Ignis Toss -> Ignis Toss+ -> Ignis Barrage -> Inferno Cascade
- Magus: Lux Bolt -> Lux Bolt+ -> Radiant Lance -> Judgement Ray
- Knight: Power Strike -> Power Strike+ -> Rending Blow -> Execution Cleave

---

### E4. Meta-Progression (5 core systems + 3 deferred)

Design note: Reduced active scope from 8 to 5 systems. Party Composition Bonuses moved to
F3.4 (Formation Bonuses) to avoid duplication. NG+, Camp Conversations, and World Compendium
are explicitly deferred to post-launch to avoid scope creep. The 5 core systems below are
sufficient for a full progression loop.

| Name | Description | Complexity | Priority |
|------|-------------|------------|----------|
| Bestiary Completion | Kill enemies to reveal data (HP, weakness, drops). Milestone bonuses at 25%, 50%, 75%, 100% completion. | Small | High |
| Difficulty Tiers | 4 levels: Apprentice (0.7x), Journeyman (1.0x), Master (1.3x), Grandmaster (1.6x). Affects enemy stats and reward multipliers. | Small | High |
| Alchemy Crafting | Combine loot materials into consumables and equipment. Quality scales with Alchemist Class Proficiency rank. Higher ranks unlock more recipes. | Large | High |
| Achievement System | One-time accomplishments (first boss kill, no-death run, etc.) with unique rewards (titles, accessories, items). | Medium | High |
| Dragon Bond Milestones | See C3. Cumulative stat bonuses for freed dragons. Integrates with Bestiary as a sub-tracker. | Small | High |

**Bestiary Milestone Rewards:**
| Completion | Reward |
| --- | --- |
| 25% (17 entries) | Scan Lens shows enemy HP numbers without using an item |
| 50% (33 entries) | +5% Combat XP from all battles |
| 75% (49 entries) | Scan Lens also reveals drop tables |
| 100% (65 entries) | Accessory "Scholar's Monocle" (+10% XP, auto-scan all enemies) |

**Example Achievements:**
| Achievement | Condition | Reward |
| --- | --- | --- |
| First Blood | Win your first battle | Herb Poultice x5 |
| Elemental Scholar | Exploit a weakness 50 times | Elemental Gem x3 |
| Flawless Victor | S-rank a boss fight | Accessory "Victor's Crest" (+5% ATK) |
| Liberator | Free your first dragon | Dragon Scale x2 |
| Undying | Complete a dungeon without any KOs | Phoenix Down x2 |
| Grandmaster's Trial | Clear any boss on Grandmaster difficulty | Title "Grandmaster" + Sovereign Elixir x1 |

**Deferred to post-launch:**
| Name | Description | Complexity | Priority |
|------|-------------|------------|----------|
| New Game+ | Restart with level/skill carryover. Enemies gain +50% stats per NG+ cycle. Exclusive NG+ content and bosses. | Large | Post-Launch |
| Camp Conversations | Bond-building dialogue scenes between dungeons. Increase Bond XP and unlock lore. Affect story outcomes. | Medium | Post-Launch |
| World Compendium | Unified collection tracker: Bestiary, Items, Lore, Maps. Sub-compendium milestones grant bonus rewards. | Medium | Post-Launch |

---

### E5. Battle Rewards Structure (6 mechanics)

| Name | Description | Priority |
|------|-------------|----------|
| Victory Screen | Battle grade (S/A/B/C/D) based on performance. XP bars with level-up fanfare. Loot display with rarity highlights. | Core |
| Bonus Conditions | 3-5 optional objectives per battle (e.g., no deaths, clear in under 5 turns, use 3+ elements, exploit weakness). Bonus XP and loot for completion. | Core |
| Victory Streak | Consecutive wins increase reward multiplier: 1.0x base, up to 4.0x at streak 20. Resets on defeat or retreat. | High |
| Retreat Mechanics | 75% base flee chance (0% in boss fights). On retreat: lose streak and partial gold, keep partial Combat XP. Escape Flare guarantees success. | High |
| Loot Tables | Per-enemy drop tables with rarity tiers (Common/Uncommon/Rare/Epic). Drop rates modified by battle grade, victory streak, and difficulty tier. | Core |
| Performance Metrics | Track per-battle stats: total damage dealt, healing done, dodges, status effects applied/cured, weakness exploits. Feed into battle grade calculation. | Core |

### Implementation Notes -- Section E

**Current architecture:** `OnBattleWon`/`OnBattleLost` fire but do no post-battle processing. No XP, rewards, leveling, stat growth, skill trees, equipment, or save data. IBattleUnit stats are fixed serialized fields. All progression is a new system built on top.

| Feature | Architecture Impact | Key Changes Needed |
| --- | --- | --- |
| Combat XP + Level-ups | **New system** -- outside battle | XP tracker, level-up curves per class, post-battle grant on `OnBattleWon`. |
| Element/Class/Skill Mastery | **Extend** -- event subscribers | Subscribe to `OnAttackExecuted`, `OnPlayerActionSelected` to count uses. New tracker classes. |
| Bond XP | **Extend** -- track pair interactions | Subscribe to heal/buff events; track which units support which. |
| Boss Tokens | **None** -- post-battle only | Grant on first boss kill. Needs boss ID on enemies + save data. |
| Stat Growth + Equipment | **Extend IBattleUnit** | Stats are currently fixed. Need mutable base stats, level scaling, equipment modifier layer. |
| Class Promotion | **Extend IBattleUnit + skills** | Promotion flag or new UnitType. 4th skill branch. No BattleController changes. |
| Skill Trees | **New system** -- outside battle | Tree data, unlock conditions, UI. Feeds into available SkillDefinitions. |
| Skill Evolution | **Extend SkillDefinition** | Per-skill use counter + replacement logic at thresholds. |
| Victory Screen + Grade | **Extend end-of-battle** | Performance metrics = subscribe to events. Grade formula + new UI. |
| Victory Streak | **Extend** -- persistent counter | Increment on win, reset on loss/flee. Outside BattleController. |
| Retreat Mechanics | **Extend BattleController** | Flee action (F1.15). New `OnBattleFled` event with partial rewards. |
| Loot Tables | **New system** | Per-enemy drop data, roll on victory, material inventory. |
| Difficulty Tiers | **Extend init** | Multiply enemy stats by scalar in `InitBattle`. Global setting. |

---

## F. Battle System Extensions

---

### F1. Combat Mechanic Extensions (16)

| # | Name | Complexity | Priority | Description |
|---|------|-----------|----------|-------------|
| 1 | Elemental Weakness/Resistance | Medium | Core | Full 1.5x weakness / 0.5x resistance damage matrix beyond the base element cycle. Each enemy and player has defined weaknesses and resistances. |
| 2 | Counter-Attack | Medium | High | Trigger automatic counter-attack on successful block or dodge. Knight Sentinel branch specializes in this. |
| 3 | Combo System | Medium | High | Sequential attacks by different party members in the same turn grant +10% damage per chain hit. Resets each turn. Requires per-turn chain counter, damage accumulator, and UI combo display. (Upgraded from Small -- no existing chain tracking in BattleController.) |
| 4 | Aggro/Threat System | Medium | Core | Knight Taunt skill draws enemy attacks. Internal threat table determines AI targeting. High threat = more likely to be targeted. Healers generate threat from healing. |
| 5 | Limit Break | Large | High | Charge gauge fills from damage taken (and minor fill from damage dealt). When full, unleash a powerful character-specific special attack. Resets after use. |
| 6 | Formation System | Large | Medium | Front row and back row. Front row takes more damage but deals more melee damage. Back row takes less damage, ranged/magic unaffected. Some skills can only target specific rows. |
| 7 | Terrain Effects | Medium | Low | Battle arena has environmental modifiers: lava fields (Burn damage), ice floors (Slow chance), wind tunnels (Ventus boost), etc. |
| 8 | Weather System | Medium | Low | Dynamic weather during battle affects element damage. Weather can change mid-battle via skills or environmental triggers. |
| 9 | Weapon Triangle | Small | Medium | Type advantage: Melee > Ranged > Magic > Melee. 1.2x damage when advantaged, 0.8x when disadvantaged. |
| 10 | Chain Attacks | Medium | Medium | When a party member hits an enemy's elemental weakness, other party members with that element equipped can follow up with a bonus attack. |
| 11 | Overkill Bonus | Small | Low | Damage dealt past 0 HP counts as overkill. Overkill amount translates to bonus XP and improved drop rates. |
| 12 | Stance System | Large | Medium | Characters adopt persistent stances: Offensive (+25% ATK, -15% DEF), Defensive (-15% ATK, +25% DEF), Balanced (no modifier). Costs 1 AP to switch. **Integration note:** Must coexist with the existing per-turn Block/Dodge `DefensiveStance` system. Stances here are persistent across turns; Block/Dodge remain single-turn commitments. |
| 13 | Reaction/Interrupt | Large | Low | React to enemy actions mid-turn. Counter-spells cancel enemy magic, intercepts redirect attacks. Costs resources and requires specific skills. |
| 14 | Speed/Initiative System | Medium | Core | Add a Speed stat to IBattleUnit. Turn order determined by Speed (highest acts first) instead of fixed list index. **Prerequisite for F4.2 (Battle Timeline).** Currently absent from IBattleUnit and BattleController. |
| 15 | Flee/Retreat Mechanic | Small | High | Add a Flee player action to `PlayerAction` enum. 75% base success in non-boss encounters, 0% in boss fights. Escape Flare item guarantees success. On flee: lose streak, keep partial XP. Cross-ref: E5 Retreat Mechanics. |
| 16 | Multi-Hit Attacks | Small | High | Support skills and basic attacks that hit multiple times (e.g., "Frenzy Strikes" 3-hit random). Requires hit-count field on SkillDefinition and a damage loop in ExecuteSkillEffect. Referenced throughout enemy designs (A2, A6) but not yet specified as a mechanic. |

---

### F2. New Status Effects (14)

| Name | Type | Effect | Duration |
|------|------|--------|----------|
| Regen | Buff | Heal 5% max HP per turn | 3 turns |
| Haste | Buff | +1 AP per turn | 3 turns |
| Slow | Debuff | -1 AP per turn (at 2 AP baseline, this halves effectiveness) | 2 turns |
| Blind | Debuff | 40% miss chance on basic attacks | 2 turns |
| Silence | Debuff | Cannot use skills (basic attack and items only) | 2 turns |
| Berserk | Buff/Debuff | +50% ATK, cannot use skills or items, auto-attacks random enemy | 3 turns |
| Protect | Buff | Physical damage received reduced by 30% | 3 turns |
| Shell | Buff | Magic damage received reduced by 30% | 3 turns |
| Reflect | Buff | Reflects the next magic attack back at its caster | Until triggered (1 use) or 5 turns max |
| Doom | Debuff | Unit dies instantly when countdown reaches 0 | 3-5 turns (varies) |
| Petrify | CC | Cannot act, takes 0 damage, cured only by specific items or skills. Auto-cures after 5 turns to prevent stalemate if all party members are petrified. | Until cured or 5 turns max |
| Charm | CC | Attacks allies instead of enemies, cannot be controlled | 2 turns |
| Fear | CC | 40% chance to skip action each turn, -20% ATK, cannot use Block | 2 turns |
| Sap | Debuff | Drain 5% max resource (MP or Charges) per turn. No effect on resourceless units (Knight). | 4 turns |

**CC Priority Rules:** When multiple CC effects are active on the same unit, resolve in this order: Petrify > Charm > Berserk > Fear > Stun > Silence. Higher-priority CC overrides lower-priority CC behavior (e.g., a Petrified unit cannot also act as if Charmed). Stun and Silence are suppressed (not removed) while a higher-priority CC is active.

---

### F3. Party System Extensions (6)

| # | Name | Complexity | Priority | Description |
|---|------|-----------|----------|-------------|
| 1 | Party Swap Mid-Battle | Large | High | Swap an active party member with a reserve member. Costs 2 AP. Swapped-in member acts next turn. Up to 2 swaps per battle. |
| 2 | Summon/Companion System | XLarge | Low | Summon elemental companions that persist for 3 turns and auto-act each turn. One summon active at a time. Costs significant MP. **Note:** Overlaps with C3 Dragon Liberation (freed dragons as once-per-battle summons). Consider implementing C3 first as a simpler summon framework, then generalizing to full companion system later. |
| 3 | Dual/Team Attacks | XLarge | Medium | Bond-level unlocked combined attacks between two party members. Costs both members' turns. Extremely powerful with unique animations. |
| 4 | Formation Bonuses | Small | Medium | Passive bonuses for specific party compositions: all same class (+15% class stat), all different (+5% all stats), element coverage bonus. |
| 5 | Reserve Party Passives | Small | Low | Benched party members contribute minor passive effects: reserve Knight gives +3% DEF, reserve Magus gives +3% ATK, etc. |
| 6 | Guest/NPC Party Members | Medium | Medium | AI-controlled story guest characters join the party for specific battles. Have their own skills and AI patterns. Cannot be directly controlled. |

---

### F4. UI/UX Extensions (11)

| # | Name | Complexity | Priority | Description |
|---|------|-----------|----------|-------------|
| 1 | Damage Number Popups | Small | Core | Color-coded floating damage numbers: white (physical), element color (elemental), green (healing), red (critical), gray (resisted). |
| 2 | Battle Timeline | Large | High | Visual turn order display showing upcoming turns based on speed-based initiative. Shows when buffs/debuffs expire. **Dependency:** Requires F1.14 (Speed/Initiative System) to be implemented first. |
| 3 | Enemy Scan/Bestiary | Large | High | Scan enemies to reveal HP, weaknesses, resistances, and drop tables. Data persists in bestiary. Unscanned enemies show ??? for unknown stats. |
| 4 | Battle Speed Controls | Medium | Medium | Toggle between 1x, 2x, and 4x battle speed. Affects animations and delays but not gameplay logic. |
| 5 | Victory Screen | Medium | Medium | Post-battle screen showing: battle grade, XP gained (with bars), loot acquired, bonus conditions met, streak counter. |
| 6 | Skill/Item Hotbar | Small | Low | Quick-access bar for up to 4 frequently used skills or items. Accessible via number keys. |
| 7 | Dynamic Camera | Large | Low | Cinematic camera angles during attacks, skills, and critical hits. Zoom on impactful moments. |
| 8 | Battle Transitions | Medium | Medium | Screen wipe/fade effects when entering and exiting battles. Different transitions for random encounters vs. boss fights. |
| 9 | Enemy HP Bars | Small | Core | Display HP bars above or beside enemy units. Show numerical HP after scanning (F4.3). Without scan, show bar only (no numbers). Essential for player decision-making. |
| 10 | Status Effect Icons | Small | Core | Display active status effect icons on unit portraits/frames with remaining-turn counters. Both player and enemy units. Group buff icons separately from debuff/CC icons. |
| 11 | Tooltip/Info System | Small | High | Hover or long-press on skills, items, status effects, and elements to show description popup. Include damage formula preview for skills, element matchup for attacks, and remaining duration for effects. |

---

### F5. Quality of Life (8)

| # | Name | Complexity | Priority | Description |
|---|------|-----------|----------|-------------|
| 1 | Auto-Battle | Medium | High | AI takes over player actions. Configurable presets: Aggressive (all-out attack), Healer (prioritize healing), Balanced (mixed), Conserve (minimal resource use). |
| 2 | Repeat Last Action | Small | High | One-button shortcut to repeat the previous action and target. Speeds up grinding. |
| 3 | Battle Log Scrollback | Small | Medium | Full scrollable battle history with color-coded entries (damage in red, healing in green, status in yellow, system in white). |
| 4 | Undo Last Action | Large | Medium | Undo the last action before confirming turn end. Limited to 1 undo per turn. Cannot undo after enemy actions. **Implementation note:** Current BattleController applies damage/effects immediately on action selection. Undo requires either a full state snapshot before each action (memento pattern) or a deferred-resolution model. |
| 5 | Quick-Use Favorites | Small | Low | Mark skills and items as favorites. Favorites are sorted to the top of selection lists. |
| 6 | Skip Animations | Small | High | Three animation modes: Full (all animations), Brief (shortened animations), Skip (instant resolution with damage numbers only). |
| 7 | Difficulty Settings | Medium | Medium | 4 difficulty tiers adjustable at any time: Apprentice (0.7x enemy stats, 1.3x rewards), Journeyman (1.0x/1.0x), Master (1.3x/1.3x), Grandmaster (1.6x/1.6x). Cross-ref: Same system as E4 Difficulty Tiers -- implement once, referenced from both sections. |
| 8 | Retry on Game Over | Medium | Core | On party wipe: Restart Battle (same setup), Lower Difficulty (reduce one tier and restart), Return to Title (main menu). No progress lost except current battle. |

### Implementation Notes -- Section F

**Current architecture:** Fixed turn loop: EnemyIntent -> PlayerTurn (per-member, 2 AP) -> EnemyTurn (execute intents) -> TurnEnd (tick status). Damage = `(ATK - DEF) * mult * element`. Status effects: Poison, Burn, Stun, AtkUp/Down, DefUp/Down. No Speed stat, no AoE, no multi-hit, no combos, no formations, no flee. `PlayerAction` has Attack/Skill/Item/Block/Dodge.

**F1. Combat Mechanic Extensions:**

| Feature | Architecture Impact | Key Changes Needed |
| --- | --- | --- |
| F1.1 Weakness/Resistance | **Extend IBattleUnit** | Add weakness/resistance lists. Update `GetElementMultiplier` to check per-unit. |
| F1.2 Counter-Attack | **Extend ResolveDefense** | On block/dodge success, fire counter damage. Add counter flag to IBattleUnit. |
| F1.3 Combo System | **Extend BattleController** | `_comboCounter` int, +10%/chain in damage calc. Reset at turn start. |
| F1.4 Aggro/Threat | **Extend BattleController + IEnemyAI** | Threat table dictionary. Update on damage/heal. Expose to AI. |
| F1.5 Limit Break | **Extend IBattleUnit** | `LimitGauge` float, charge on damage taken, new PlayerAction. |
| F1.6 Formations | **Restructure** | Row position, damage modifiers, targeting rules, skill range. Significant. |
| F1.9 Weapon Triangle | **Extend damage calc** | `AttackType` enum on IBattleUnit. 1.2x/0.8x in `CalculateRawDamage`. Small. |
| F1.12 Stance System | **Restructure DefensiveStance** | Persistent `CombatStance` separate from per-turn Block/Dodge. |
| F1.14 Speed/Initiative | **Extend IBattleUnit + Restructure BattleLoop** | `Speed` stat. Sort turn order. Changes player/enemy phase loops. |
| F1.15 Flee/Retreat | **Extend BattleController** | New `PlayerAction.Flee`, `SelectFlee()`, success roll. Small. |
| F1.16 Multi-Hit | **Extend SkillDefinition** | `HitCount` field, loop in `ExecuteSkillEffect`. Small. |
| F1.7/F1.8 Terrain/Weather | **Restructure** | New BattleField state with active modifiers in damage calc. |
| F1.10 Chain Attacks | **Extend** | Follow-up on weakness hit. Post-attack hook needed. |
| F1.11 Overkill Bonus | **Extend end-of-battle** | Track damage past 0 HP per enemy. Feed excess into XP/drop bonus. Small. |
| F1.13 Reaction/Interrupt | **Restructure BattleLoop** | Mid-turn interrupt windows break sequential execution. |

**F2. New Status Effects:**

| Feature | Architecture Impact | Key Changes Needed |
| --- | --- | --- |
| Regen, Protect, Shell | **Extend TickStatusEffects + enum** | Regen = heal tick. Protect/Shell = damage reduction check. |
| Haste, Slow | **Extend AP system** | Modify per-member AP based on status before player turn. |
| Silence | **Extend ValidateInput** | Block Skill action if Silenced. Block enemy skills too. |
| Blind | **Extend damage calc** | 40% miss roll on basic attacks. |
| Reflect | **Extend ExecuteSkillEffect** | Check Reflect, redirect magic to caster. |
| Doom | **Extend TickStatusEffects** | Instant kill at countdown 0. |
| Petrify/Charm/Fear/Berserk | **Restructure player input** | Override player control. Each needs special BattleLoop handling. |

**F3. Party System Extensions:**

| Feature | Architecture Impact | Key Changes Needed |
| --- | --- | --- |
| F3.1 Party Swap | **Restructure** | Reserve list, swap action (2 AP), modify `_playerParty` mid-battle. |
| F3.2 Summons | **Restructure** | Add/remove summoned units, auto-AI for summon turns. |
| F3.3 Dual Attacks | **Extend** | Bond data, combined skills, two-member turn consumption. |
| F3.4 Formation Bonuses | **None** -- init only | Check composition in `InitBattle`, apply passive modifiers. |
| F3.5 Reserve Passives | **None** -- init only | Read reserve list at battle start, apply flat stat bonuses. |
| F3.6 Guest NPCs | **Extend BattleLoop** | AI-controlled party member, skip manual input for guest. |

**F4. UI/UX + F5. QoL (mostly UI-layer, low battle-engine impact):**

| Feature | Architecture Impact | Key Changes Needed |
| --- | --- | --- |
| F4.1 Damage Popups, F4.9 HP Bars, F4.10 Status Icons | **None** -- UI only | Subscribe to existing events. |
| F4.2 Battle Timeline | **Depends on F1.14** | UI reads speed-sorted order. |
| F4.3 Scan/Bestiary | **Extend** | Scan action + persistent data store. |
| F5.1 Auto-Battle | **Extend** | AI wrapper calling existing input methods. |
| F5.2 Repeat Action | **Extend** | Cache last action+target, re-invoke. |
| F5.4 Undo | **Restructure** | State snapshot before each action (memento). Most invasive QoL. |
| F5.6 Skip Animations | **None** -- timing only | Zero out WaitForSeconds delays. |
| F5.8 Retry | **Extend end-of-battle** | Cache init params, re-call `InitBattle` on loss. |

**Architecture Impact Summary -- Section F:**

| Sub-Section | None (UI/data only) | Extend (additive) | Restructure (invasive) | Highest-Risk Item |
| --- | --- | --- | --- | --- |
| F1. Combat Mechanics | F1.9, F1.11 | F1.1, F1.2, F1.3, F1.4, F1.5, F1.10, F1.15, F1.16 | F1.6, F1.7/8, F1.12, F1.13, F1.14 | F1.14 Speed/Initiative (changes BattleLoop turn order) |
| F2. Status Effects | -- | Regen/Protect/Shell/Silence/Blind/Doom/Haste/Slow/Reflect | Petrify/Charm/Fear/Berserk (override player input) | Charm/Berserk (player control override) |
| F3. Party System | F3.4 | F3.3, F3.5, F3.6 | F3.1, F3.2 | F3.1 Party Swap (mutates live party list) |
| F4. UI/UX | F4.1, F4.9, F4.10, F4.6, F4.7, F4.8 | F4.3, F4.4, F4.11 | F4.2 (depends on F1.14) | F4.2 Battle Timeline (needs Speed system first) |
| F5. QoL | F5.6 | F5.1, F5.2, F5.5, F5.7, F5.8 | F5.4 | F5.4 Undo (memento pattern on entire battle state) |

---

## Implementation Priority Roadmap

### Phase 1 -- Core Foundation
These systems form the essential backbone of the battle system. Implement first.

- **F1.1** Elemental Weakness/Resistance system
- **F1.4** Aggro/Threat system
- **F1.14** Speed/Initiative System (prerequisite for F4.2 Battle Timeline)
- **F1.15** Flee/Retreat Mechanic
- **F2** New Status Effects -- at least Regen, Haste, Slow, Silence, Protect, Shell (needed by enemy designs throughout A1-A7)
- **F4.1** Damage Number Popups
- **F4.9** Enemy HP Bars
- **F4.10** Status Effect Icons
- **F5.8** Retry on Game Over
- **F4.5** Victory Screen + Battle Rewards (E5)
- **E5.5** Loot Tables
- **E5.6** Performance Metrics

### Phase 2 -- High Impact
These features add significant depth and player satisfaction. Implement after core is stable.

- **F1.2** Counter-Attack
- **F1.3** Combo System
- **F1.5** Limit Break
- **F1.16** Multi-Hit Attacks
- **F2** Remaining Status Effects (Reflect, Doom, Petrify, Charm, Fear, Sap, Blind, Berserk)
- **F3.1** Party Swap Mid-Battle
- **F4.2** Battle Timeline (Turn Order Display) -- depends on F1.14 from Phase 1
- **F4.3** Enemy Scan/Bestiary
- **F4.11** Tooltip/Info System
- **F5.1** Auto-Battle
- **F5.2** Repeat Last Action
- **F5.6** Skip Animations
- **B1** Enemy AI Patterns (implement as IEnemyAI subclasses)
- **E1/E2** Experience Types + Stat Growth (prerequisite for E3 in Phase 3)

### Phase 3 -- Depth & Polish
These features add strategic depth and long-term engagement. Implement once Phase 2 is polished.

- **F1.6** Formation System
- **F1.9** Weapon Triangle
- **F1.10** Chain Attacks
- **F1.12** Stance System (requires refactoring existing DefensiveStance system)
- **F3.3** Dual/Team Attacks (Bond system)
- **F3.4** Formation Bonuses
- **F3.6** Guest/NPC Party Members
- **F4.4** Battle Speed Controls
- **F4.8** Battle Transitions
- **F5.3** Battle Log Scrollback
- **F5.4** Undo Last Action
- **F5.7** Difficulty Settings (same system as E4 Difficulty Tiers)
- **E3** Skill Trees + Progression Systems (depends on E1/E2 from Phase 2)
- **C** Dragon Encounters + Liberation Mechanic

### Phase 4 -- Nice-to-Have
These features are enhancements that can be added last or deferred to post-launch.

- **F1.7** Terrain Effects
- **F1.8** Weather System
- **F1.11** Overkill Bonus
- **F1.13** Reaction/Interrupt
- **F3.2** Summon/Companion System (consider C3 Dragon Summons as initial framework)
- **F3.5** Reserve Party Passives
- **F4.6** Skill/Item Hotbar
- **F4.7** Dynamic Camera
- **F5.5** Quick-Use Favorites
- **E4.6** New Game+
- **E4.3** Alchemy Crafting (full system)
- **E4.1** Bestiary Completion rewards
- **E4.4** Achievement System

---

*End of Battle System Extensions document.*
