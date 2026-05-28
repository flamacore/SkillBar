using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SkillBar;

/// <summary>
/// Places tiles and walls at the cursor (rope, platforms, blocks, etc.).
/// </summary>
public static class SkillBarPlacement
{
	public static bool IsPlacementItem(Item item)
	{
		return item.createTile >= 0 || item.createWall > 0;
	}

	public static bool PlaceTowardCursor(Player player, Item item, Vector2 cursorWorld, bool consumeResources)
	{
		Point tile = ResolveTargetTile(player, cursorWorld);
		if (!IsValidReachTile(player, tile.X, tile.Y)) {
			if (player.whoAmI == Main.myPlayer)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.OutOfRange"), Color.OrangeRed);
			return false;
		}

		int oldTargetX = Player.tileTargetX;
		int oldTargetY = Player.tileTargetY;
		bool oldControl = player.controlUseItem;

		Player.tileTargetX = tile.X;
		Player.tileTargetY = tile.Y;
		player.controlUseItem = true;

		bool placed = item.createTile >= 0
			? TryPlaceTile(player, item, tile.X, tile.Y)
			: TryPlaceWall(player, item, tile.X, tile.Y);

		player.controlUseItem = oldControl;
		Player.tileTargetX = oldTargetX;
		Player.tileTargetY = oldTargetY;

		if (placed && consumeResources)
			SkillBarItemUse.ConsumeFromInventory(player, item);

		if (!placed && player.whoAmI == Main.myPlayer)
			Main.NewText(Language.GetTextValue("Mods.SkillBar.CannotPlace"), Color.OrangeRed);

		return placed;
	}

	private static bool TryPlaceTile(Player player, Item item, int i, int j)
	{
		int type = item.createTile;
		if (!TileLoader.CanPlace(i, j, type))
			return false;

		if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == type)
			return true;

		WorldGen.PlaceTile(i, j, type, false, false, -1, item.placeStyle);

		if (!Main.tile[i, j].HasTile || Main.tile[i, j].TileType != type)
			return false;

		if (Main.netMode == NetmodeID.MultiplayerClient)
			NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);

		TileLoader.PlaceInWorld(i, j, item);
		return true;
	}

	private static bool TryPlaceWall(Player player, Item item, int i, int j)
	{
		int wallType = item.createWall;
		if (Main.tile[i, j].WallType == wallType)
			return true;

		WorldGen.PlaceWall(i, j, wallType, false);

		if (Main.tile[i, j].WallType != wallType)
			return false;

		if (Main.netMode == NetmodeID.MultiplayerClient)
			NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);

		WallLoader.PlaceInWorld(i, j, item);
		return true;
	}

	internal static Point ResolveTargetTile(Player player, Vector2 cursorWorld)
	{
		Point target = ClampTileToWorld(cursorWorld.ToTileCoordinates());

		if (IsValidReachTile(player, target.X, target.Y))
			return target;

		// Step toward the cursor until we hit max placement reach.
		Vector2 start = player.Center;
		Vector2 dir = cursorWorld - start;
		if (dir.LengthSquared() < 1f)
			return ClampTileToWorld(player.Center.ToTileCoordinates());

		dir.Normalize();
		float maxReach = (Player.tileRangeX + Player.tileRangeY) * 8f + 16f;
		Point best = ClampTileToWorld(player.Center.ToTileCoordinates());

		for (float dist = 16f; dist <= maxReach; dist += 16f) {
			Point step = ClampTileToWorld((start + dir * dist).ToTileCoordinates());
			if (!IsValidReachTile(player, step.X, step.Y))
				break;

			best = step;
		}

		return best;
	}

	internal static bool IsValidReachTile(Player player, int tileX, int tileY)
	{
		return SkillBarReach.IsWithinTileReach(player, tileX, tileY);
	}

	private static Point ClampTileToWorld(Point tile)
	{
		int x = Utils.Clamp(tile.X, 0, Main.maxTilesX - 1);
		int y = Utils.Clamp(tile.Y, 0, Main.maxTilesY - 1);
		return new Point(x, y);
	}
}
