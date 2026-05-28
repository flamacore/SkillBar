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
	public const int DragHandleSize = 18;

	private static bool _prevMouseLeft;
	private static bool _prevMouseRight;
	private static bool _prevMouseMiddle;

	public static float GetBarWidth()
	{
		return DragHandleSize + SlotPadding + SkillBar.SlotCount * (SlotSize + SlotPadding) - SlotPadding;
	}

	public static float GetBarHeight()
	{
		return SlotSize;
	}

	public static Point MousePoint => new(Main.mouseX, Main.mouseY);

	public static Rectangle GetDragHandle(SkillBarPlayer sb)
	{
		sb.EnsureDefaultPosition();
		int y = (int)(sb.BarScreenPosition.Y + (SlotSize - DragHandleSize) * 0.5f);
		return new Rectangle((int)sb.BarScreenPosition.X, y, DragHandleSize, DragHandleSize);
	}

	public static Rectangle GetSlotRect(SkillBarPlayer sb, int slot)
	{
		sb.EnsureDefaultPosition();
		float x = sb.BarScreenPosition.X + DragHandleSize + SlotPadding + slot * (SlotSize + SlotPadding);
		float y = sb.BarScreenPosition.Y;
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

		Texture2D panel = TextureAssets.InventoryBack.Value;
		Rectangle dragHandle = GetDragHandle(sb);

		spriteBatch.Draw(panel, dragHandle.TopLeft(), new Rectangle(0, 0, 52, 52), Color.White * 0.9f, 0f, Vector2.Zero, DragHandleSize / 52f, SpriteEffects.None, 0f);
		DrawDragHandleIcon(spriteBatch, dragHandle);

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

		if (!IsMouseOverBar(sb) && !sb.DraggingBar)
			return;

		Rectangle dragHandle = GetDragHandle(sb);
		Point mouse = MousePoint;
		bool shiftHeld = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);

		if (mouseLeft && dragHandle.Contains(mouse)) {
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

		if (shiftHeld && leftReleased && slotHasItem && Main.mouseItem.IsAir) {
			int emptySlot = sb.FirstEmptySlotIndex();
			if (emptySlot >= 0 && emptySlot != hoveredSlot) {
				sb.Slots[emptySlot] = sb.Slots[hoveredSlot].Clone();
				sb.Slots[emptySlot].stack = 1;
				SoundEngine.PlaySound(SoundID.Grab);
				Main.NewText(Language.GetTextValue("Mods.SkillBar.Assigned", sb.Slots[emptySlot].Name), Color.LightGreen);
				return;
			}
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

		if (SkillBarBlockedItems.IsBlocked(source)) {
			SoundEngine.PlaySound(SoundID.MenuTick);
			Main.NewText(Language.GetTextValue("Mods.SkillBar.BlockedItem"), Color.OrangeRed);
			return false;
		}

		if (!SkillBarItemUse.IsEligibleForSkillBar(source)) {
			SoundEngine.PlaySound(SoundID.MenuTick);
			Main.NewText(Language.GetTextValue("Mods.SkillBar.NotUsable"), Color.OrangeRed);
			return false;
		}

		// Bookmarks are independent per slot — duplicates are allowed.
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

		sb.Slots[slot].TurnToAir();
		SoundEngine.PlaySound(SoundID.Grab);
	}

	private static void DrawSlotItem(SpriteBatch spriteBatch, Item item, Rectangle slotRect)
	{
		Main.instance.LoadItem(item.type);
		Texture2D texture = TextureAssets.Item[item.type].Value;
		Main.GetItemDrawFrame(item.type, out _, out Rectangle frame);

		float maxDim = System.Math.Max(frame.Width, frame.Height);
		float scale = maxDim > 0f ? (SlotSize * 0.85f) / maxDim : 1f;
		scale = System.Math.Min(scale, 1f);

		Vector2 center = slotRect.Center.ToVector2();
		spriteBatch.Draw(texture, center, frame, Color.White, 0f, frame.Size() * 0.5f, scale, SpriteEffects.None, 0f);
	}

	private static void DrawDragHandleIcon(SpriteBatch spriteBatch, Rectangle handleRect)
	{
		// Open-hand cursor texture (vanilla index 2).
		Texture2D hand = TextureAssets.Cursors[2].Value;
		float maxDim = System.Math.Max(hand.Width, hand.Height);
		float scale = maxDim > 0f ? (DragHandleSize - 6f) / maxDim : 1f;
		Vector2 center = handleRect.Center.ToVector2();
		spriteBatch.Draw(hand, center, null, Color.White * 0.95f, 0f, hand.Size() * 0.5f, scale, SpriteEffects.None, 0f);
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
