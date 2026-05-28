using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SkillBar;

/// <summary>
/// Multi-frame item use (Magic Mirror, HoldUp boss spawners) — keeps the item on a background hotbar slot until the use animation finishes.
/// </summary>
internal static class SkillBarSustainedUse
{
	public static bool RequiresSustainedUse(Item item)
	{
		if (item.useStyle == ItemUseStyleID.HoldUp)
			return true;

		// Some mod boss items use a long drink-style animation without being flagged consumable.
		return (item.useStyle == ItemUseStyleID.DrinkLong || item.useStyle == ItemUseStyleID.EatFood)
			&& item.useAnimation >= 15;
	}

	public static bool TryStart(SkillBarPlayer sb, Player player, Item inv, int slot, bool consumeOnComplete)
	{
		if (sb.IsSustainedUseActive || sb.IsChanneling)
			return false;

		sb.BeginSustainedUse(slot, inv, consumeOnComplete);
		return Tick(sb, player, firstPulse: true);
	}

	public static bool Tick(SkillBarPlayer sb, Player player, bool firstPulse = false)
	{
		if (!sb.IsSustainedUseActive)
			return false;

		int slot = sb.SustainedUseSlot;
		Item template = sb.Slots[slot];
		Item inv = SkillBarItemUse.FindInventoryItem(player, template.type);

		if (inv == null || inv.IsAir) {
			sb.EndSustainedUse(player);
			if (SkillBarConsumableUse.IsConsumable(template))
				SkillBarConsumableUse.ClearBookmarkIfDepleted(sb, slot, template);
			return false;
		}

		// selectedItem stays on the background hold slot for the whole use (see BeginSustainedUse).
		bool oldUse = player.controlUseItem;

		sb.RefreshItemBarSuppress(sb.SustainedUseHoldSlot);

		try {
			if (firstPulse) {
				// One press — not a held mouse button. Holding controlUseItem would restart every cycle (Magic Mirror loop).
				player.controlUseItem = true;
				player.ItemCheck();
				sb.SustainedUseAnimationActive = player.itemAnimation > 0 || player.itemTime > 0;
			}
			else if (sb.SustainedUseAnimationActive) {
				player.controlUseItem = false;
				player.ItemCheck();
			}
		}
		finally {
			player.controlUseItem = oldUse;
		}

		if (!sb.SustainedUseAnimationActive) {
			sb.EndSustainedUse(player);
			return false;
		}

		if (player.itemAnimation > 0 || player.itemTime > 0)
			return true;

		sb.EndSustainedUse(player);

		if (sb.SustainedUseConsumeOnComplete)
			SkillBarItemUse.ConsumeFromInventory(player, template);

		SkillBarItemUse.FinishItemBarUse(player, inv);

		if (SkillBarConsumableUse.IsConsumable(template))
			SkillBarConsumableUse.ClearBookmarkIfDepleted(sb, slot, template);

		return true;
	}
}
