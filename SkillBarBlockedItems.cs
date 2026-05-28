using Terraria;
using Terraria.ID;

namespace SkillBar;

/// <summary>
/// Yoyos and flails/maces spawn persistent projectiles — not compatible with skill-bar one-shot use.
/// </summary>
public static class SkillBarBlockedItems
{
	public static bool IsBlocked(Item item)
	{
		if (item == null || item.IsAir)
			return false;

		if (ItemID.Sets.Yoyo[item.type])
			return true;

		if (item.shoot <= ProjectileID.None)
			return false;

		return UsesBlockedProjectileAi(item.shoot);
	}

	private static bool UsesBlockedProjectileAi(int projectileType)
	{
		Projectile probe = new Projectile();
		probe.SetDefaults(projectileType);

		return probe.aiStyle == ProjAIStyleID.Yoyo
			|| probe.aiStyle == ProjAIStyleID.Flail;
	}
}
