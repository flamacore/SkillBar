using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace SkillBar;

/// <summary>
/// Scans every loaded item once (debug config) and writes unusual combat patterns to a log file.
/// </summary>
public class SkillBarWeaponAudit : ModSystem
{
	public override void OnModLoad()
	{
		if (!SkillBarConfig.Instance.AuditWeaponsOnLoad)
			return;

		RunAudit();
	}

	private static void RunAudit()
	{
		var buckets = new Dictionary<string, List<int>> {
			["channel_shoot"] = new(),
			["channel_no_shoot"] = new(),
			["melee_shoot"] = new(),
			["shoot_non_shoot_style"] = new(),
			["shoot_style_no_shoot"] = new(),
			["summon_staff"] = new(),
			["multi_use_limit"] = new(),
			["no_usestyle_has_shoot"] = new(),
		};

		for (int type = 0; type < ItemLoader.ItemCount; type++) {
			Item item = new Item();
			item.SetDefaults(type);
			if (item.IsAir || !SkillBarItemUse.IsEligibleForSkillBar(item))
				continue;

			if (item.channel && item.shoot > 0)
				buckets["channel_shoot"].Add(type);
			else if (item.channel)
				buckets["channel_no_shoot"].Add(type);

			if (item.CountsAsClass(DamageClass.Melee) && item.shoot > 0)
				buckets["melee_shoot"].Add(type);

			if (item.shoot > 0 && item.useStyle != ItemUseStyleID.Shoot && item.useStyle != ItemUseStyleID.Swing)
				buckets["shoot_non_shoot_style"].Add(type);

			if (item.useStyle == ItemUseStyleID.Shoot && item.shoot <= 0)
				buckets["shoot_style_no_shoot"].Add(type);

			if (item.shoot > 0 && ProjectileID.Sets.MinionTargettingFeature[item.shoot])
				buckets["summon_staff"].Add(type);

			if (item.useLimitPerAnimation != null)
				buckets["multi_use_limit"].Add(type);

			if (item.useStyle <= ItemUseStyleID.None && item.shoot > 0)
				buckets["no_usestyle_has_shoot"].Add(type);
		}

		var sb = new StringBuilder();
		sb.AppendLine("Item Bar weapon audit — unusual vanilla/mod item patterns");
		sb.AppendLine($"Items scanned: {ItemLoader.ItemCount}");
		sb.AppendLine("Item bar uses ItemCheck_Shoot / ItemCheck / channel hold for these categories.");
		sb.AppendLine();

		foreach (var pair in buckets.OrderBy(p => p.Key)) {
			sb.AppendLine($"## {pair.Key} ({pair.Value.Count})");
			foreach (int type in pair.Value.OrderBy(t => t))
				sb.AppendLine($"  {type}\t{Lang.GetItemNameValue(type)}");
			sb.AppendLine();
		}

		string path = Path.Combine(Main.SavePath, "ItemBar_weapon_audit.txt");
		File.WriteAllText(path, sb.ToString());
		ModContent.GetInstance<SkillBar>().Logger.Info($"Item Bar weapon audit written to {path}");
	}
}
