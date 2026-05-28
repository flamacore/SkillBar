using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SkillBar;

/// <summary>
/// Channel weapons (flamethrower, beam drills, etc.) need held controlUseItem across frames while the skill key is held.
/// </summary>
internal static class SkillBarChannelUse
{
	public static bool WantsChannel(Item item) => item.channel;

	public static bool TryStart(SkillBarPlayer sb, Player player, Item bookmark, int slot, Vector2 cursorWorld)
	{
		Item item = SkillBarItemUse.FindInventoryItem(player, bookmark.type);
		if (item == null)
			return false;

		if (item.mana > 0 && !player.CheckMana(item, -1, pay: false))
			return false;

		sb.BeginChannel(slot, item);
		return Tick(sb, player, cursorWorld, firstPulse: true);
	}

	public static bool Tick(SkillBarPlayer sb, Player player, Vector2 cursorWorld, bool firstPulse = false)
	{
		if (!sb.IsChanneling)
			return false;

		if (player.dead || player.noItems || player.CCed) {
			sb.EndChannel(player);
			return false;
		}

		Item template = sb.Slots[sb.ChannelSlot];
		Item item = SkillBarItemUse.FindInventoryItem(player, template.type);
		if (item == null || item.IsAir) {
			sb.EndChannel(player);
			return false;
		}

		if (!firstPulse && !IsChannelInputHeld(sb)) {
			sb.EndChannel(player);
			return false;
		}

		SkillBarItemUse.AimPlayer(player, cursorWorld);

		int prevSelected = player.selectedItem;
		bool oldUse = player.controlUseItem;

		try {
			player.selectedItem = sb.ChannelHoldSlot;
			player.controlUseItem = true;

			if (player.itemAnimation <= 0) {
				player.itemAnimation = item.useAnimation;
				player.itemAnimationMax = item.useAnimation;
				player.itemTime = 0;
			}

			player.ItemCheck();
		}
		finally {
			player.selectedItem = prevSelected;
			player.controlUseItem = oldUse;
		}

		if (item.mana > 0)
			player.CheckMana(item, -1, pay: true);

		return true;
	}

	public static bool IsChannelInputHeld(SkillBarPlayer sb)
	{
		if (sb.ChannelSlot < 0)
			return false;

		Keys configKey = SkillBarConfig.Instance.GetSlotKey(sb.ChannelSlot);
		KeyboardState keys = Keyboard.GetState();
		if (keys.IsKeyDown(configKey))
			return true;

		if (SkillBarKeybinds.SlotKeys?[sb.ChannelSlot]?.Current == true)
			return true;

		return false;
	}
}
