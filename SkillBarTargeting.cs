using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace SkillBar;

/// <summary>
/// Resolves which tile to hit — prefers the same tile vanilla highlights under the mouse.
/// </summary>
internal static class SkillBarTargeting
{
	public static bool TryGetToolTarget(Player player, Vector2 cursorWorld, out Point tile)
	{
		// Vanilla updates these from the cursor every frame.
		if (TryTile(player, Player.tileTargetX, Player.tileTargetY, out tile))
			return true;

		Point mouseTile = cursorWorld.ToTileCoordinates();
		if (TryTile(player, mouseTile.X, mouseTile.Y, out tile))
			return true;

		// Search neighbors when the cursor is on the edge between tiles.
		for (int r = 1; r <= 2; r++) {
			for (int dy = -r; dy <= r; dy++) {
				for (int dx = -r; dx <= r; dx++) {
					if (dx == 0 && dy == 0)
						continue;

					if (TryTile(player, mouseTile.X + dx, mouseTile.Y + dy, out tile))
						return true;
				}
			}
		}

		// Last resort: step along aim toward the cursor.
		tile = SkillBarPlacement.ResolveTargetTile(player, cursorWorld);
		return TryTile(player, tile.X, tile.Y, out tile);
	}

	private static bool TryTile(Player player, int x, int y, out Point tile)
	{
		tile = default;

		if (!WorldGen.InWorld(x, y, 0))
			return false;

		if (!SkillBarReach.IsWithinTileReach(player, x, y))
			return false;

		Tile t = Main.tile[x, y];
		if (!t.HasTile && t.WallType <= WallID.None)
			return false;

		tile = new Point(x, y);
		return true;
	}
}
