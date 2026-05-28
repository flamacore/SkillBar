using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace SkillBar;

/// <summary>
/// Tile reach checks without calling Player.InInteractionRange (throws on some tML builds).
/// </summary>
internal static class SkillBarReach
{
	public static bool IsWithinTileReach(Player player, int tileX, int tileY)
	{
		if (tileX < 0 || tileY < 0 || tileX >= Main.maxTilesX || tileY >= Main.maxTilesY)
			return false;

		Vector2 tileCenter = new Vector2(tileX * 16 + 8, tileY * 16 + 8);
		float maxReach = Math.Max(Player.tileRangeX, Player.tileRangeY) * 16f;
		return Vector2.Distance(player.Center, tileCenter) <= maxReach;
	}
}
