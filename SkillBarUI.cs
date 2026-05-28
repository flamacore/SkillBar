using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SkillBar;

public static class SkillBarUI
{
	public const int SlotSize = 44;
	public const int SlotPadding = 4;
	public const int DragHandleHeight = 14;

	private static bool _prevMouseLeft;
	private static bool _prevMouseRight;
	private static bool _prevMouseMiddle;

	public static float GetBarWidth()
	{
		return SkillBar.SlotCount * (SlotSize + SlotPadding) - SlotPadding;
	}

	public static float GetBarHeight()
	{
		return DragHandleHeight + SlotSize + 4;
	}

	public static Point MousePoint => new(Main.mouseX, Main.mouseY);

	public static Rectangle GetDragHandle(SkillBarPlayer sb)
	{
		sb.EnsureDefaultPosition();
		int width = (int)GetBarWidth();
		return new Rectangle(
			(int)sb.BarScreenPosition.X,
			(int)sb.BarScreenPosition.Y,
			width,
			DragHandleHeight);
	}

	public static Rectangle GetSlotRect(SkillBarPlayer sb, int slot)
	{
		sb.EnsureDefaultPosition();
		float x = sb.BarScreenPosition.X + slot * (SlotSize + SlotPadding);
		float y = sb.BarScreenPosition.Y + DragHandleHeight + 2;
		return new Rectangle((int)x, (int)y, SlotSize, SlotSize);
	}

	public static int GetHoveredSlot(SkillBarPlayer sb)
	{
		Point mouse = MousePoint;
		for (int i = 0; i < SkillBar.SlotCount; i++) {
			if (GetSlotRect(sb, i).Contains(mouse))
				return i;
		}
		return -1;
	}

	public static bool IsMouseOverBar(SkillBarPlayer sb)
	{
		Point mouse = MousePoint;
		return GetDragHandle(sb).Contains(mouse) || GetHoveredSlot(sb) >= 0;
	}

	public static void Draw(SpriteBatch spriteBatch, SkillBarPlayer sb)
	{
		if (Main.gameMenu || Main.dedServ)
			return;

		if (!SkillBarConfig.Instance.Enabled)
			return;

		sb.EnsureDefaultPosition();
		ClampToScreen(sb);

		Rectangle dragHandle = GetDragHandle(sb);
		Texture2D panel = TextureAssets.InventoryBack.Value;

		spriteBatch.Draw(panel, dragHandle.TopLeft(), new Rectangle(0, 0, 52, 52), Color.White * 0.85f, 0f, Vector2.Zero, new Vector2(dragHandle.Width / 52f, DragHandleHeight / 52f), SpriteEffects.None, 0f);

		for (int i = 0; i < SkillBar.SlotCount; i++) {
			Rectangle slotRect = GetSlotRect(sb, i);
			spriteBatch.Draw(panel, slotRect.TopLeft(), Color.White);

			Item item = sb.Slots[i];
			if (!item.IsAir)
				DrawSlotItem(spriteBatch, item, slotRect);

			if (SkillBarConfig.Instance.ShowKeyLabels) {
				string keyLabel = SkillBarKeybinds.GetDisplayName(i);
				Utils.DrawBorderString(spriteBatch, keyLabel, slotRect.TopLeft() + new Vector2(2f, 2f), Color.Gray * 0.9f, scale: 0.55f);
			}
		}

		int hovered = GetHoveredSlot(sb);
		if (hovered >= 0 && !sb.Slots[hovered].IsAir) {
			Main.hoverItemName = sb.Slots[hovered].Name;
			Main.HoverItem = sb.Slots[hovered].Clone();
		}

		if (IsMouseOverBar(sb))
			Main.LocalPlayer.mouseInterface = true;

		if (dragHandle.Contains(MousePoint))
			Main.instance.MouseText(Language.GetTextValue("Mods.SkillBar.DragHint"));
	}

	public static void HandleInput(SkillBarPlayer sb)
	{
		if (Main.gameMenu || Main.dedServ)
			return;

		if (!SkillBarConfig.Instance.Enabled)
			return;

		if (PlayerInput.LockGamepadTileUseButton)
			return;

		sb.EnsureDefaultPosition();
		ClampToScreen(sb);

		bool mouseLeft = Main.mouseLeft;
		bool mouseRight = Main.mouseRight;
		bool mouseMiddle = Main.mouseMiddle;
		bool leftReleased = _prevMouseLeft && !mouseLeft;
		bool rightReleased = _prevMouseRight && !mouseRight;
		bool middlePressed = mouseMiddle && !_prevMouseMiddle;
		_prevMouseLeft = mouseLeft;
		_prevMouseRight = mouseRight;
		_prevMouseMiddle = mouseMiddle;

		// Only handle bar input when the cursor is over the bar (works even with inventory open).
		if (!IsMouseOverBar(sb) && !sb.DraggingBar)
			return;

		Rectangle dragHandle = GetDragHandle(sb);
		Point mouse = MousePoint;

		if (mouseLeft && dragHandle.Contains(mouse) && !IsMouseOverSlot(sb, mouse)) {
			if (!sb.DraggingBar) {
				sb.DraggingBar = true;
				sb.DragMouseOffset = mouse.ToVector2() - sb.BarScreenPosition;
			}
		}

		if (sb.DraggingBar) {
			if (mouseLeft) {
				sb.BarScreenPosition = mouse.ToVector2() - sb.DragMouseOffset;
				sb.PositionInitialized = true;
				ClampToScreen(sb);
				Main.LocalPlayer.mouseInterface = true;
			}
			else {
				sb.DraggingBar = false;
			}
			return;
		}

		int hoveredSlot = GetHoveredSlot(sb);
		if (hoveredSlot < 0)
			return;

		Main.LocalPlayer.mouseInterface = true;

		bool altHeld = Main.keyState.IsKeyDown(Keys.LeftAlt) || Main.keyState.IsKeyDown(Keys.RightAlt);
		bool slotHasItem = !sb.Slots[hoveredSlot].IsAir;

		if (SkillBarConfig.Instance.UseMiddleClick && middlePressed && slotHasItem && Main.mouseItem.IsAir) {
			sb.TriggerSlot(hoveredSlot, "mouse");
			return;
		}

		if (SkillBarConfig.Instance.UseAltClick && altHeld && leftReleased && slotHasItem && Main.mouseItem.IsAir) {
			sb.TriggerSlot(hoveredSlot, "mouse");
			return;
		}

		if (leftReleased && !altHeld)
			TryAssignToSlot(sb, hoveredSlot);

		if (rightReleased)
			TryRemoveFromSlot(sb, hoveredSlot);
	}

	public static bool TryAssignToSlot(SkillBarPlayer sb, int slot, Item explicitItem = null)
	{
		Item source = explicitItem != null && !explicitItem.IsAir ? explicitItem : Main.mouseItem;
		bool fromMouse = explicitItem == null && !Main.mouseItem.IsAir;

		if (source.IsAir)
			source = Main.HoverItem;

		if (source.IsAir)
			return false;

		if (!SkillBarItemUse.IsEligibleForSkillBar(source)) {
			SoundEngine.PlaySound(SoundID.MenuTick);
			Main.NewText(Language.GetTextValue("Mods.SkillBar.NotUsable"), Color.OrangeRed);
			return false;
		}

		// Skill slots are bookmarks only — never delete inventory items.
		sb.Slots[slot] = source.Clone();
		sb.Slots[slot].stack = 1;

		if (fromMouse && !Main.mouseItem.IsAir) {
			Item restore = Main.mouseItem.Clone();
			Main.mouseItem.TurnToAir();
			ReturnItemToInventory(Main.LocalPlayer, restore);
		}

		SoundEngine.PlaySound(SoundID.Grab);
		Main.NewText(Language.GetTextValue("Mods.SkillBar.Assigned", sb.Slots[slot].Name), Color.LightGreen);
		return true;
	}

	private static void TryRemoveFromSlot(SkillBarPlayer sb, int slot)
	{
		Item slotItem = sb.Slots[slot];
		if (slotItem.IsAir)
			return;

		// Clearing a bookmark does not create or delete real items.
		sb.Slots[slot].TurnToAir();
		SoundEngine.PlaySound(SoundID.Grab);
	}

	private static bool IsMouseOverSlot(SkillBarPlayer sb, Point mouse)
	{
		for (int i = 0; i < SkillBar.SlotCount; i++) {
			if (GetSlotRect(sb, i).Contains(mouse))
				return true;
		}
		return false;
	}

	private static void DrawSlotItem(SpriteBatch spriteBatch, Item item, Rectangle slotRect)
	{
		Main.instance.LoadItem(item.type);
		Main.GetItemDrawFrame(item.type, out Texture2D texture, out Rectangle frame);
		float scale = 1f;
		if (item.width > 0 && item.height > 0)
			scale = MathHelper.Min(SlotSize / (float)item.width, SlotSize / (float)item.height) * 0.9f;

		Vector2 center = slotRect.Center.ToVector2();
		spriteBatch.Draw(texture, center, frame, Color.White, 0f, frame.Size() * 0.5f, scale, SpriteEffects.None, 0f);
	}

	private static void ReturnItemToInventory(Player player, Item item)
	{
		if (item.IsAir)
			return;

		for (int i = 0; i < player.inventory.Length; i++) {
			if (!player.inventory[i].IsAir)
				continue;

			player.inventory[i] = item.Clone();
			return;
		}

		player.QuickSpawnItem(player.GetSource_Misc("SkillBar"), item);
	}

	private static void ClampToScreen(SkillBarPlayer sb)
	{
		float width = GetBarWidth();
		float height = GetBarHeight();
		sb.BarScreenPosition.X = MathHelper.Clamp(sb.BarScreenPosition.X, 0f, Main.screenWidth - width);
		sb.BarScreenPosition.Y = MathHelper.Clamp(sb.BarScreenPosition.Y, 0f, Main.screenHeight - height);
	}
}
