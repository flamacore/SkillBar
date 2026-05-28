# Skill Bar

**A second hotbar for Terraria — bookmark your gear, press a key, use it at the cursor.**

Skill Bar adds a draggable 10-slot bar for items you want quick access to without cluttering your main hotbar. Slots are **bookmarks only**: items stay in your inventory. Press a bound key (or click a slot) to use that item toward your cursor, the same way you would from the normal hotbar — spells, tools, potions, mirrors, boss summons, and more.

Works in single player and multiplayer. Client-side UI and keybinds; uses sync-friendly item logic.

---

## Why use it?

- Keep **weapons, spells, and summons** on dedicated keys while your hotbar holds building blocks and tools.
- **Mine, place, and fight** from the skill bar without swapping your selected hotbar slot.
- **Potions and food** on one key — consumes from inventory and clears the slot when you run out.
- **Magic Mirror, Cell Phone, boss spawners**, and similar items work through vanilla use logic.
- **Same item in multiple slots** — e.g. two different keys for the same wand, or duplicate bookmarks via Shift+click.

---

## Quick start

1. Enable **Skill Bar** in the mod list and join a world.
2. On first load you’ll see a hint: default slot 1 is **Z**.
3. **Assign items:**
   - **Left-click** a skill slot while holding an item (or hovering one in inventory), or  
   - **Shift + left-click** an inventory item (assigns to the slot under your cursor, or the first empty slot).
4. **Use items:**
   - Press the key for that slot (**Z, X, C, V, B, N, M**, comma, period, slash by default), or  
   - **Middle-click** a filled slot, or  
   - **Alt + left-click** a filled slot.
5. **Move the bar:** drag the small **hand square** on the left of the bar.

Your bar position and slot bookmarks are saved per character.

---

## Default keys

| Slot | Default key |
|------|-------------|
| 1 | Z |
| 2 | X |
| 3 | C |
| 4 | V |
| 5 | B |
| 6 | N |
| 7 | M |
| 8 | , (comma) |
| 9 | . (period) |
| 10 | / (slash) |

Rebind any slot under **Settings → Mod Configuration → Skill Bar**, or **Settings → Key Bindings → Skill Bar** (menu keybinds).

---

## What you can put on the bar

| Category | Examples | Notes |
|----------|----------|--------|
| **Weapons & magic** | Wands, bows, guns, staffs | Uses vanilla shoot logic (Meteor Staff rain, Daedalus Stormbow, Terra Blade, mod weapons). |
| **Summons & sentries** | Imp Staff, Queen Spider Staff | Minion and sentry items supported. |
| **Tools** | Pickaxe, axe, hammer | Aims at the block under your cursor; respects range and cooldown. |
| **Placement** | Blocks, platforms, rope, walls | Places at the cursor within normal reach. |
| **Consumables** | Potions, food, buff drinks | Uses one from inventory; slot clears when none left. |
| **Usable items** | Magic Mirror, Cell Phone, boss summons | Full use animation where needed; consumable summons use one from stack. |
| **Channel weapons** | Flamethrower, drills | **Hold** the skill key while spraying (like holding mouse on the hotbar). |

Items must stay in your **inventory** (or hotbar) to use. The bar never deletes or duplicates real items — only bookmarks.

---

## Tips

- **Shift + left-click** a filled skill slot to **copy** that bookmark to the next empty slot.
- **Right-click** a skill slot to **clear** the bookmark (does not destroy inventory items).
- If your cursor is on the bar when you press a key, aim defaults **forward from the player** so you don’t shoot into the UI.
- **Channel weapons** need the key held, not a single tap.
- **Potion sickness** and normal use cooldowns apply.

---

## Mod configuration

Open **Settings → Mod Configuration → Skill Bar**:

| Option | Description |
|--------|-------------|
| Enable skill bar | Turn the bar and all skill input on or off. |
| Show bound keys on slots | Display key labels on each slot. |
| Slot 1–10 keys | Keyboard binding per slot (client-side). |
| Middle-click slot to use | Use a skill by middle-clicking its slot. |
| Alt + left-click slot to use | Alternative mouse use binding. |
| Allow use while inventory open | Skills work with inventory open (default: on). |
| Allow use while chat open | Skills work while typing in chat (default: off). |
| Debug log all skill input | Chat messages when slots fire (for troubleshooting). |
| Audit weapons on load | Writes a debug item list to your tModLoader folder (off by default). |

---

## Multiplayer

- Skill Bar UI and keybinds are **client-side** — each player configures their own bar.
- Item use runs on your client with normal Terraria/tModLoader behavior; other players see the results (projectiles, mining, buffs, etc.) as usual.
- Bookmarks and bar position are saved on **your character**.

---

## Compatibility

- Built for **tModLoader** (tested on recent 2025–2026 builds).
- Should work alongside other UI and hotbar mods; if both mods bind the same key, resolve conflicts in tModLoader key settings.
- Modded weapons and consumables are supported via vanilla `ItemCheck` / `UseItem` paths where possible.

If something doesn’t behave like the normal hotbar, report the **item name** and what happens — especially for modded boss items or unusual use styles.

---

## Building from source

Requires the tModLoader mod development environment.

```bash
git clone https://github.com/flamacore/SkillBar.git
# Open the folder as a tModLoader mod source, or:
dotnet build SkillBar.csproj
```

Close the game (or disable the mod) before building to avoid file locks on `SkillBar.tmod`.

---

## Credits

- **Author:** Chao  
- **Repository:** [github.com/flamacore/SkillBar](https://github.com/flamacore/SkillBar)

---

## License

All rights reserved by the author unless a license file is added to the repository.

---

<details>
<summary><strong>Short copy for Steam / Workshop / mod browser</strong></summary>

**Skill Bar** — A draggable 10-slot skill bar for tModLoader. Bookmark items from your inventory and use them with dedicated keys or mouse clicks, aimed at your cursor. Supports weapons, spells, summons, tools, placement, potions, Magic Mirror, boss summons, and more — without replacing your main hotbar selection.

**Controls:** Assign with left-click or Shift+click from inventory. Use with Z–M and punctuation keys (configurable), middle-click, or Alt+click. Drag the hand icon to move the bar.

**Features:** Vanilla-accurate shooting and tool use, consumables consume from inventory, empty potion slots auto-clear, per-character save, client-side config, multiplayer-friendly.

</details>
