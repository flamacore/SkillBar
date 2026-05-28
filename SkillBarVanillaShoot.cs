using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SkillBar;

/// <summary>
/// Fires weapons through vanilla ItemCheck_Shoot so per-item logic (Meteor Staff, Stormbow, Terra Blade, etc.) runs unchanged.
/// </summary>
internal static class SkillBarVanillaShoot
{
	private static MethodInfo _itemCheckShoot;
	private static bool _loggedMissingMethod;

	static SkillBarVanillaShoot()
	{
		_itemCheckShoot = typeof(Player).GetMethod(
			"ItemCheck_Shoot",
			BindingFlags.Instance | BindingFlags.NonPublic,
			null,
			new[] { typeof(int), typeof(Item), typeof(int) },
			null);
	}

	public static bool TryShoot(Player player, Item item)
	{
		if (player.whoAmI != Main.myPlayer || item == null || item.IsAir || item.shoot <= ProjectileID.None)
			return false;

		if (_itemCheckShoot == null) {
			if (!_loggedMissingMethod) {
				_loggedMissingMethod = true;
				ModContent.GetInstance<SkillBar>().Logger.Warn("ItemCheck_Shoot not found; using simplified projectile spawn fallback.");
			}

			return SkillBarShootFallback.TryDirectShoot(player, item, Main.MouseWorld);
		}

		if (!CombinedHooks.CanShoot(player, item))
			return false;

		int before = CountOwnedProjectiles(player);
		int weaponDamage = player.GetWeaponDamage(item);

		_itemCheckShoot.Invoke(player, new object[] { player.whoAmI, item, weaponDamage });

		// Some weapons apply item time without spawning yet; others spawn multiple projectiles.
		return CountOwnedProjectiles(player) > before
			|| player.itemTime > 0
			|| player.itemAnimation > 0;
	}

	private static int CountOwnedProjectiles(Player player)
	{
		int count = 0;
		for (int i = 0; i < Main.maxProjectiles; i++) {
			Projectile p = Main.projectile[i];
			if (p.active && p.owner == player.whoAmI)
				count++;
		}

		return count;
	}
}
