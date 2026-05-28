using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SkillBar;

/// <summary>
/// Last-resort shoot path when ItemCheck_Shoot is unavailable (should not happen on supported tML builds).
/// </summary>
internal static class SkillBarShootFallback
{
	public static bool TryDirectShoot(Player player, Item item, Vector2 cursorWorld)
	{
		if (item.shoot <= ProjectileID.None)
			return false;

		int projectileType = item.shoot;
		float shootSpeed = item.shootSpeed;
		int damage = player.GetWeaponDamage(item);
		float knockback = player.GetWeaponKnockback(item, item.knockBack);
		int usedAmmoItemId = 0;

		player.PickAmmo(item, out projectileType, out shootSpeed, out damage, out knockback, out usedAmmoItemId, dontConsume: false);

		if (projectileType <= ProjectileID.None)
			return false;

		Vector2 position = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
		Vector2 velocity = cursorWorld - position;
		if (velocity.LengthSquared() < 1f)
			velocity = new Vector2(player.direction, 0f);
		velocity.Normalize();
		velocity *= shootSpeed;

		ItemLoader.ModifyShootStats(item, player, ref position, ref velocity, ref projectileType, ref damage, ref knockback);

		if (!CombinedHooks.Shoot(player, item, new EntitySource_ItemUse_WithAmmo(player, item, usedAmmoItemId), position, velocity, projectileType, damage, knockback))
			return true;

		int index = Projectile.NewProjectile(new EntitySource_ItemUse_WithAmmo(player, item, usedAmmoItemId), position, velocity, projectileType, damage, knockback, player.whoAmI);
		return index >= 0 && index < Main.maxProjectiles;
	}
}
