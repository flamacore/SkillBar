using Terraria;
using Terraria.ID;

namespace SkillBar;

/// <summary>
/// How a bookmarked item should be driven — vanilla ItemCheck paths cover almost all combat items.
/// </summary>
internal enum SkillBarWeaponKind
{
	/// <summary>Flamethrowers, drills, etc. — hold skill key while channeling.</summary>
	Channel,

	/// <summary>Guns, bows, staffs, Terra Blade beam, etc. — ItemCheck_Shoot.</summary>
	VanillaShoot,

	/// <summary>Potions, mounts, hooks — ItemLoader.UseItem while briefly held.</summary>
	VanillaUseItem,

	/// <summary>Summon scepters, minion staffs — often UseItem + shoot.</summary>
	Summon,

	/// <summary>Shortswords / spears — melee ItemCheck swing.</summary>
	MeleeSwing,
}

internal static class SkillBarWeaponKindClassifier
{
	public static SkillBarWeaponKind Classify(Item item)
	{
		if (item.channel)
			return SkillBarWeaponKind.Channel;

		if (item.shoot > ProjectileID.None && ProjectileID.Sets.MinionTargettingFeature[item.shoot])
			return SkillBarWeaponKind.Summon;

		if (item.shoot > ProjectileID.None)
			return SkillBarWeaponKind.VanillaShoot;

		if (item.useStyle == ItemUseStyleID.Swing || item.useStyle == ItemUseStyleID.Rapier)
			return SkillBarWeaponKind.MeleeSwing;

		return SkillBarWeaponKind.VanillaUseItem;
	}
}
