using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace SkillBar;

/// <summary>
/// Shift+left-click an inventory item to bind it to the skill slot under the cursor (or first empty slot).
/// </summary>
public class SkillBarInventoryHook : ModPlayer
{
	public override bool HoverSlot(Item[] inventory, int context, int slot)
	{
		if (Player.whoAmI != Main.myPlayer)
			return false;

		bool shift = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
		if (!shift || !PlayerInput.Triggers.JustPressed.MouseLeft)
			return false;

		Item item = inventory[slot];
		if (item.IsAir)
			return false;

		SkillBarPlayer sb = Player.GetModPlayer<SkillBarPlayer>();
		int targetSlot = SkillBarUI.GetHoveredSlot(sb);
		if (targetSlot < 0)
			targetSlot = sb.FirstEmptySlotIndex();

		if (targetSlot < 0)
			return false;

		if (SkillBarUI.TryAssignToSlot(sb, targetSlot, item))
			return true;

		return false;
	}
}
