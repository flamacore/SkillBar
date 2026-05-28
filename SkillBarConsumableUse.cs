using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SkillBar;

/// <summary>
/// Potions, food, and other stackable consumables — uses vanilla effects and removes one from inventory.
/// </summary>
public static class SkillBarConsumableUse
{
	public static bool IsConsumable(Item item)
	{
		if (item == null || item.IsAir)
			return false;

		if (item.consumable)
			return true;

		if (item.healLife > 0 || item.healMana > 0)
			return true;

		if (item.buffType > 0 && item.buffTime > 0)
			return true;

		return item.useStyle == ItemUseStyleID.DrinkLong
			|| item.useStyle == ItemUseStyleID.EatFood
			|| item.useStyle == ItemUseStyleID.DrinkLiquid;
	}

	public static bool TryUse(Player player, Item bookmark, int slot, SkillBarPlayer sb)
	{
		if (player.dead || player.noItems || player.CCed)
			return false;

		Item inv = SkillBarItemUse.FindInventoryItem(player, bookmark.type);
		if (inv == null) {
			ClearBookmarkIfDepleted(sb, slot, bookmark);
			return false;
		}

		if (inv.potion && player.potionDelay > 0)
			return false;

		if (SkillBarSustainedUse.RequiresSustainedUse(inv))
			return SkillBarSustainedUse.TryStart(sb, player, inv, slot, consumeOnComplete: true);

		bool used = SkillBarItemUse.WithTemporaryHeldItem(player, inv, held => TryUseHeldConsumable(player, held));

		if (!used)
			return false;

		SkillBarItemUse.ConsumeFromInventory(player, bookmark);
		SkillBarItemUse.ApplyUseCooldown(player, inv);
		ClearBookmarkIfDepleted(sb, slot, bookmark);
		return true;
	}

	private static bool TryUseHeldConsumable(Player player, Item held)
	{
		if (ItemLoader.UseItem(held, player) == true)
			return true;

		if (held.useStyle != ItemUseStyleID.EatFood
			&& held.useStyle != ItemUseStyleID.DrinkLong
			&& held.useStyle != ItemUseStyleID.DrinkLiquid) {
			return false;
		}

		bool oldUse = player.controlUseItem;
		player.controlUseItem = true;

		if (player.itemAnimation <= 0) {
			player.itemAnimation = held.useAnimation;
			player.itemAnimationMax = held.useAnimation;
			player.itemTime = 0;
		}

		player.ItemCheck();
		player.controlUseItem = oldUse;

		return player.itemTime > 0 || player.itemAnimation > 0;
	}

	public static void ClearBookmarkIfDepleted(SkillBarPlayer sb, int slot, Item bookmark)
	{
		if (bookmark.IsAir)
			return;

		if (SkillBarItemUse.FindInventoryItem(sb.Player, bookmark.type) != null)
			return;

		string name = bookmark.Name;
		sb.Slots[slot].TurnToAir();

		if (sb.Player.whoAmI == Main.myPlayer)
			Main.NewText(Language.GetTextValue("Mods.SkillBar.SlotCleared", name), Color.Gray);
	}

	public static void PruneDepletedBookmarks(SkillBarPlayer sb)
	{
		for (int i = 0; i < SkillBar.SlotCount; i++) {
			Item bookmark = sb.Slots[i];
			if (!bookmark.IsAir && !IsConsumable(bookmark))
				continue;

			if (SkillBarItemUse.FindInventoryItem(sb.Player, bookmark.type) == null)
				sb.Slots[i].TurnToAir();
		}
	}
}
