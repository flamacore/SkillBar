using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SkillBar;

/// <summary>
/// Pickaxes and axes need a brief background hotbar hold for vanilla PickTile / pick-power checks.
/// Hammers keep the direct path (works without swapping).
/// </summary>
public static class SkillBarToolUse
{
	public static bool IsMiningTool(Item item)
	{
		return item.pick > 0 || item.axe > 0 || item.hammer > 0;
	}

	public static bool UseTowardCursor(Player player, Item bookmark, Vector2 cursorWorld)
	{
		Item tool = SkillBarItemUse.FindInventoryItem(player, bookmark.type) ?? bookmark;

		if (tool.pick > 0 || tool.axe > 0)
			return UsePickOrAxeTowardCursor(player, tool, cursorWorld);

		return UseHammerTowardCursor(player, tool, cursorWorld);
	}

	private static bool UsePickOrAxeTowardCursor(Player player, Item tool, Vector2 cursorWorld)
	{
		return SkillBarItemUse.WithTemporaryHeldItem(player, tool, held => {
			if (!SkillBarTargeting.TryGetToolTarget(player, cursorWorld, out Point tile)) {
				if (player.whoAmI == Main.myPlayer)
					Main.NewText(Language.GetTextValue("Mods.SkillBar.NothingToMine"), Color.OrangeRed);
				return false;
			}

			if (!SkillBarReach.IsWithinTileReach(player, tile.X, tile.Y)) {
				if (player.whoAmI == Main.myPlayer)
					Main.NewText(Language.GetTextValue("Mods.SkillBar.OutOfRange"), Color.OrangeRed);
				return false;
			}

			Tile target = Main.tile[tile.X, tile.Y];
			int oldTargetX = Player.tileTargetX;
			int oldTargetY = Player.tileTargetY;

			Player.tileTargetX = tile.X;
			Player.tileTargetY = tile.Y;

			bool hit = false;
			string failReason = "no target";

			try {
				if (held.pick > 0 && target.HasTile) {
					if (player.HasEnoughPickPowerToHurtTile(tile.X, tile.Y)) {
						player.PickTile(tile.X, tile.Y, held.pick);
						hit = true;
					}
					else {
						failReason = "pick power";
					}
				}
				else if (held.axe > 0 && target.HasTile) {
					player.PickTile(tile.X, tile.Y, held.axe);
					hit = true;
				}
				else if (!target.HasTile) {
					failReason = "no tile";
				}
			}
			finally {
				Player.tileTargetX = oldTargetX;
				Player.tileTargetY = oldTargetY;
			}

			LogToolResult(player, tile, hit, failReason);
			return hit;
		});
	}

	private static bool UseHammerTowardCursor(Player player, Item tool, Vector2 cursorWorld)
	{
		if (!SkillBarTargeting.TryGetToolTarget(player, cursorWorld, out Point tile)) {
			if (player.whoAmI == Main.myPlayer)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.NothingToMine"), Color.OrangeRed);
			return false;
		}

		if (!SkillBarReach.IsWithinTileReach(player, tile.X, tile.Y)) {
			if (player.whoAmI == Main.myPlayer)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.OutOfRange"), Color.OrangeRed);
			return false;
		}

		Tile target = Main.tile[tile.X, tile.Y];
		int oldTargetX = Player.tileTargetX;
		int oldTargetY = Player.tileTargetY;

		Player.tileTargetX = tile.X;
		Player.tileTargetY = tile.Y;

		bool hit = false;
		string failReason = "no target";

		try {
			if (tool.hammer > 0 && target.WallType > WallID.None) {
				player.PickWall(tile.X, tile.Y, tool.hammer);
				hit = true;
			}
			else if (tool.hammer > 0 && target.HasTile) {
				player.PickTile(tile.X, tile.Y, tool.hammer);
				hit = true;
			}
			else if (!target.HasTile && target.WallType <= WallID.None) {
				failReason = "no tile";
			}
		}
		finally {
			Player.tileTargetX = oldTargetX;
			Player.tileTargetY = oldTargetY;
		}

		LogToolResult(player, tile, hit, failReason);
		return hit;
	}

	private static void LogToolResult(Player player, Point tile, bool hit, string failReason)
	{
		if (SkillBarConfig.Instance.LogKeyPresses && player.whoAmI == Main.myPlayer) {
			if (hit)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.UseOk", tile.X, tile.Y), Color.LimeGreen);
			else
				Main.NewText(Language.GetTextValue("Mods.SkillBar.UseDebugFail", failReason), Color.OrangeRed);
		}

		if (!hit && player.whoAmI == Main.myPlayer)
			Main.NewText(Language.GetTextValue("Mods.SkillBar.NothingToMine"), Color.OrangeRed);
	}
}
