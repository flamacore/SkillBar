using Microsoft.Xna.Framework;
using Terraria;

namespace SkillBar;

public static class SkillBarAim
{
	/// <summary>
	/// World position to use for skills. When the cursor is on the skill bar UI, aim forward from the player instead.
	/// </summary>
	public static Vector2 GetUsePosition(Player player, SkillBarPlayer sb)
	{
		if (!SkillBarUI.IsMouseOverBar(sb))
			return ClampToWorld(Main.MouseWorld);

		Vector2 toMouse = Main.MouseWorld - player.Center;
		if (toMouse.LengthSquared() < 48f * 48f) {
			// Cursor is on/near the bar at the bottom — use player's facing and slight upward angle.
			toMouse = new Vector2(player.direction * 120f, -40f);
		}

		toMouse.Normalize();
		float reach = (Player.tileRangeX + Player.tileRangeY) * 8f + 32f;
		Vector2 aim = player.Center + toMouse * reach;
		return ClampToWorld(aim);
	}

	private static Vector2 ClampToWorld(Vector2 worldPos)
	{
		float maxX = Main.maxTilesX * 16f - 16f;
		float maxY = Main.maxTilesY * 16f - 16f;
		return new Vector2(
			MathHelper.Clamp(worldPos.X, 16f, maxX),
			MathHelper.Clamp(worldPos.Y, 16f, maxY));
	}
}
