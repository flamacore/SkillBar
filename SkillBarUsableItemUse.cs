using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SkillBar;

/// <summary>
/// Reusable items that are not consumed (Magic Mirror, Cell Phone) and special one-shot tools (mod boss spawners).
/// </summary>
public static class SkillBarUsableItemUse
{
	public static bool IsUsableItem(Item item)
	{
		if (item == null || item.IsAir)
			return false;

		if (SkillBarConsumableUse.IsConsumable(item))
			return false;

		if (SkillBarToolUse.IsMiningTool(item))
			return false;

		if (SkillBarPlacement.IsPlacementItem(item))
			return false;

		if (item.channel)
			return false;

		// Guns, swords, staffs — handled by weapon paths.
		if (item.shoot > ProjectileID.None)
			return false;

		if (item.useStyle == ItemUseStyleID.Swing || item.useStyle == ItemUseStyleID.Rapier)
			return false;

		return item.useStyle > ItemUseStyleID.None;
	}

	public static bool TryUse(Player player, Item bookmark, int slot, SkillBarPlayer sb)
	{
		if (player.dead || player.noItems || player.CCed)
			return false;

		Item inv = SkillBarItemUse.FindInventoryItem(player, bookmark.type);
		if (inv == null)
			return false;

		if (SkillBarSustainedUse.RequiresSustainedUse(inv))
			return SkillBarSustainedUse.TryStart(sb, player, inv, slot, consumeOnComplete: ShouldConsumeAfterUse(inv));

		bool used = SkillBarItemUse.WithTemporaryHeldItem(player, inv, held => TryInstantUse(player, held));

		if (!used)
			return false;

		if (ShouldConsumeAfterUse(inv))
			SkillBarItemUse.ConsumeFromInventory(player, bookmark);

		SkillBarItemUse.ApplyUseCooldown(player, inv);

		if (SkillBarConsumableUse.IsConsumable(bookmark))
			SkillBarConsumableUse.ClearBookmarkIfDepleted(sb, slot, bookmark);

		return true;
	}

	private static bool TryInstantUse(Player player, Item held)
	{
		if (ItemLoader.UseItem(held, player) == true)
			return true;

		if (held.useStyle <= ItemUseStyleID.None)
			return false;

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

	/// <summary>Consumable flag or vanilla-style summoning / throwable consumable.</summary>
	private static bool ShouldConsumeAfterUse(Item item)
	{
		if (item.consumable)
			return true;

		return item.makeNPC > 0 || item.bait > 0;
	}
}
