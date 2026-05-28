using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SkillBar;

/// <summary>
/// Skill items act on inventory bookmarks — the selected hotbar item is never replaced.
/// </summary>
public static class SkillBarItemUse
{
	public static bool IsEligibleForSkillBar(Item item)
	{
		if (item == null || item.IsAir)
			return false;

		return item.useStyle > ItemUseStyleID.None
			|| item.shoot > ProjectileID.None
			|| item.createTile > -1
			|| item.consumable
			|| item.channel
			|| SkillBarToolUse.IsMiningTool(item);
	}

	public static bool TryUseMiningTool(Player player, Item bookmark, Vector2 cursorWorld)
	{
		if (player.dead || player.noItems || player.CCed)
			return false;

		Item tool = FindInventoryItem(player, bookmark.type);
		if (tool == null)
			return false;

		AimPlayer(player, cursorWorld);

		if (!SkillBarToolUse.UseTowardCursor(player, bookmark, cursorWorld))
			return false;

		ApplyUseCooldown(player, tool);
		return true;
	}

	public static bool TryUsePlacement(Player player, Item bookmark, Vector2 cursorWorld)
	{
		if (player.dead || player.noItems || player.CCed)
			return false;

		Item item = FindInventoryItem(player, bookmark.type);
		if (item == null)
			return false;

		AimPlayer(player, cursorWorld);

		if (!SkillBarPlacement.PlaceTowardCursor(player, item, cursorWorld, consumeResources: true))
			return false;

		ApplyUseCooldown(player, item);
		return true;
	}

	public static bool TryUseWeapon(Player player, Item bookmark, Vector2 cursorWorld, int slot, SkillBarPlayer sb)
	{
		if (player.dead || player.noItems || player.CCed)
			return false;

		Item item = FindInventoryItem(player, bookmark.type);
		if (item == null)
			return false;

		if (item.mana > 0 && !player.CheckMana(item, -1, pay: false)) {
			if (player.whoAmI == Main.myPlayer)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.NoMana"), Color.OrangeRed);
			return false;
		}

		AimPlayer(player, cursorWorld);

		SkillBarWeaponKind kind = SkillBarWeaponKindClassifier.Classify(item);
		bool used;

		switch (kind) {
			case SkillBarWeaponKind.Channel:
				used = SkillBarChannelUse.TryStart(sb, player, bookmark, slot, cursorWorld);
				break;

			case SkillBarWeaponKind.MeleeSwing:
				used = SkillBarMeleeUse.TrySwing(player, item);
				break;

			case SkillBarWeaponKind.VanillaShoot:
			case SkillBarWeaponKind.Summon:
				used = WithTemporaryHeldItem(player, item, held => SkillBarVanillaShoot.TryShoot(player, held));
				if (!used && kind == SkillBarWeaponKind.Summon)
					used = WithTemporaryHeldItem(player, item, held => ItemLoader.UseItem(held, player) == true);
				break;

			default:
				used = WithTemporaryHeldItem(player, item, held => ItemLoader.UseItem(held, player) == true);
				break;
		}

		if (!used)
			return false;

		if (!item.channel && item.mana > 0)
			player.CheckMana(item, -1, pay: true);

		if (!item.channel)
			ApplyUseCooldown(player, item);

		return true;
	}

	public static void AimPlayer(Player player, Vector2 cursorWorld)
	{
		Vector2 toCursor = cursorWorld - player.MountedCenter;
		if (toCursor.LengthSquared() < 0.0001f)
			toCursor = Vector2.UnitX * player.direction;
		else
			toCursor.Normalize();

		player.direction = toCursor.X >= 0f ? 1 : -1;
		player.itemRotation = toCursor.ToRotation() + (player.direction < 0 ? MathHelper.Pi : 0f);
	}

	public static void ApplyUseCooldown(Player player, Item item)
	{
		int delay = System.Math.Max(item.useAnimation, item.useTime);
		if (delay <= 0)
			delay = 7;

		player.ApplyItemTime(item);
		player.itemAnimation = System.Math.Max(player.itemAnimation, delay);
		player.itemTime = System.Math.Max(player.itemTime, delay);
	}

	internal static Item FindInventoryItem(Player player, int itemType)
	{
		for (int i = 0; i < player.inventory.Length; i++) {
			Item inv = player.inventory[i];
			if (inv.type == itemType && inv.stack > 0)
				return inv;
		}

		return null;
	}

	/// <summary>
	/// Briefly holds the inventory tool on a background hotbar slot (never the selected slot) so PickTile works.
	/// </summary>
	internal static bool WithTemporaryHeldItem(Player player, Item tool, System.Func<Item, bool> action)
	{
		int holdSlot = FindBackgroundHotbarSlot(player);
		int prevSelected = player.selectedItem;
		Item backupHold = player.inventory[holdSlot].Clone();

		try {
			player.inventory[holdSlot] = tool.Clone();
			player.inventory[holdSlot].stack = 1;
			player.selectedItem = holdSlot;
			return action(player.inventory[holdSlot]);
		}
		finally {
			player.inventory[holdSlot] = backupHold.Clone();
			player.selectedItem = prevSelected;
		}
	}

	internal static int FindBackgroundHotbarSlot(Player player)
	{
		const int hotbarSlots = 10;
		for (int i = hotbarSlots - 1; i >= 0; i--) {
			if (i != player.selectedItem)
				return i;
		}

		return (player.selectedItem + 1) % hotbarSlots;
	}

	public static bool PlayerHasItem(Player player, int itemType)
	{
		return FindInventoryItem(player, itemType) != null;
	}

	public static void ConsumeFromInventory(Player player, Item template)
	{
		Item inv = FindInventoryItem(player, template.type);
		if (inv == null)
			return;

		inv.stack--;
		if (inv.stack <= 0)
			inv.TurnToAir();
	}

	public static bool HasResource(Player player, Item template)
	{
		Item inv = FindInventoryItem(player, template.type);
		if (inv == null)
			return false;

		if (template.mana > 0 && player.statMana < template.mana)
			return false;

		if (SkillBarPlacement.IsPlacementItem(template))
			return true;

		if (SkillBarToolUse.IsMiningTool(template))
			return true;

		if (SkillBarConsumableUse.IsConsumable(template))
			return inv.stack > 0;

		if (SkillBarUsableItemUse.IsUsableItem(template))
			return inv.stack > 0;

		if (template.shoot > ProjectileID.None) {
			int projectileType = template.shoot;
			float shootSpeed = template.shootSpeed;
			int damage = player.GetWeaponDamage(inv);
			float knockback = player.GetWeaponKnockback(inv, inv.knockBack);
			int usedAmmoItemId = 0;
			player.PickAmmo(inv, out projectileType, out shootSpeed, out damage, out knockback, out usedAmmoItemId, dontConsume: true);

			if (template.useAmmo != AmmoID.None && usedAmmoItemId <= ItemID.None)
				return false;

			return projectileType > ProjectileID.None;
		}

		return true;
	}
}
