using Terraria;

namespace SkillBar;

/// <summary>
/// Melee swings (Terra Blade, spears, etc.) through vanilla ItemCheck while briefly holding the weapon.
/// </summary>
internal static class SkillBarMeleeUse
{
	public static bool TrySwing(Player player, Item item)
	{
		return SkillBarItemUse.WithTemporaryHeldItem(player, item, held => {
			bool oldUse = player.controlUseItem;
			player.controlUseItem = true;

			if (player.itemAnimation <= 0) {
				player.itemAnimation = held.useAnimation;
				player.itemAnimationMax = held.useAnimation;
				player.itemTime = 0;
			}

			player.ItemCheck();
			player.controlUseItem = oldUse;

			return player.itemAnimation > 0 || player.itemTime > 0;
		});
	}
}
