using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SkillBar;

/// <summary>
/// Pickaxes, axes, and hammers — one swing at the cursor without changing the hotbar selection.
/// </summary>
public static class SkillBarToolUse
{
	public static bool IsMiningTool(Item item)
	{
		return item.pick > 0 || item.axe > 0 || item.hammer > 0;
	}

	public static bool UseTowardCursor(Player player, Item bookmark, Vector2 cursorWorld)
	{
		Item tool = SkillBarItemUse.FindInventoryItem(player, bookmark.type);
		if (tool == null || tool.IsAir)
			return false;

		if (!SkillBarTargeting.TryGetToolTarget(player, cursorWorld, out Point tile)) {
			if (player.whoAmI == Main.myPlayer)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.NothingToMine"), Color.OrangeRed);
			return false;
		}

		Tile target = Main.tile[tile.X, tile.Y];
		int oldTargetX = Player.tileTargetX;
		int oldTargetY = Player.tileTargetY;

		Player.tileTargetX = tile.X;
		Player.tileTargetY = tile.Y;

		bool hit = false;

		try {
			if (tool.hammer > 0 && target.WallType > WallID.None) {
				player.PickWall(tile.X, tile.Y, tool.hammer);
				hit = true;
			}
			else if (tool.pick > 0 && target.HasTile && CanPickTile(tool, tile.X, tile.Y)) {
				player.PickTile(tile.X, tile.Y, tool.pick);
				hit = true;
			}
			else if (tool.axe > 0 && target.HasTile) {
				player.PickTile(tile.X, tile.Y, tool.axe);
				hit = true;
			}
			else if (tool.hammer > 0 && target.HasTile) {
				player.PickTile(tile.X, tile.Y, tool.hammer);
				hit = true;
			}
		}
		finally {
			Player.tileTargetX = oldTargetX;
			Player.tileTargetY = oldTargetY;
		}

		if (SkillBarConfig.Instance.LogKeyPresses && player.whoAmI == Main.myPlayer) {
			if (hit)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.UseOk", tile.X, tile.Y), Color.LimeGreen);
			else
				Main.NewText(Language.GetTextValue("Mods.SkillBar.UseDebugFail", $"{tile.X},{tile.Y} pick={tool.pick}"), Color.OrangeRed);
		}

		if (!hit && player.whoAmI == Main.myPlayer)
			Main.NewText(Language.GetTextValue("Mods.SkillBar.NothingToMine"), Color.OrangeRed);

		return hit;
	}

	private static bool CanPickTile(Item item, int x, int y)
	{
		if (!WorldGen.InWorld(x, y, 1))
			return false;

		Tile tile = Main.tile[x, y];
		if (!tile.HasTile)
			return false;

		int damage = 0;
		TileLoader.PickPowerCheck(tile, item.pick, ref damage);
		return damage > 0;
	}
}
