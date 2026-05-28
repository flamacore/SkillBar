using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SkillBar;

public class SkillBarPlayer : ModPlayer
{
	public Item[] Slots { get; private set; } = null!;

	public Vector2 BarScreenPosition;
	public bool PositionInitialized;
	public bool DraggingBar;
	public Vector2 DragMouseOffset;

	private KeyboardState _prevKeyboard;
	private ulong _lastTriggerFrame = ulong.MaxValue;
	private int _lastTriggerSlot = -1;

	// One skill use per frame, executed in PostUpdate (after vanilla input).
	private int _queuedSlot = -1;
	private Vector2 _queuedAim;
	private readonly int[] _slotCooldown = new int[SkillBar.SlotCount];

	public int ChannelSlot = -1;
	public int ChannelHoldSlot = -1;
	public Item ChannelBackup = new Item();

	public bool IsChanneling => ChannelSlot >= 0;

	private static bool _shownKeybindHint;
	private static bool _loggedInputReady;

	public override void Initialize()
	{
		Slots = new Item[SkillBar.SlotCount];
		for (int i = 0; i < SkillBar.SlotCount; i++)
			Slots[i] = new Item();
	}

	public override void OnEnterWorld()
	{
		if (Player.whoAmI != Main.myPlayer)
			return;

		if (!_shownKeybindHint) {
			_shownKeybindHint = true;
			Main.NewText(Language.GetTextValue("Mods.SkillBar.KeybindHelp"), Color.LightSteelBlue);
		}

		if (SkillBarConfig.Instance.LogKeyPresses && !_loggedInputReady) {
			_loggedInputReady = true;
			Main.NewText(Language.GetTextValue("Mods.SkillBar.InputReady", SkillBarKeybinds.GetDisplayName(0)), Color.Cyan);
		}
	}

	public int FirstEmptySlotIndex()
	{
		for (int i = 0; i < SkillBar.SlotCount; i++) {
			if (Slots[i].IsAir)
				return i;
		}
		return -1;
	}

	public void EnsureDefaultPosition()
	{
		if (PositionInitialized)
			return;

		float width = SkillBarUI.GetBarWidth();
		BarScreenPosition = new Vector2(Main.screenWidth * 0.5f - width * 0.5f, Main.screenHeight - 78f);
		PositionInitialized = true;
	}

	public override void PreUpdate()
	{
		if (Player.whoAmI != Main.myPlayer)
			return;

		PollConfigKeys();
	}

	public override void PostUpdate()
	{
		if (Player.whoAmI != Main.myPlayer)
			return;

		for (int i = 0; i < _slotCooldown.Length; i++) {
			if (_slotCooldown[i] > 0)
				_slotCooldown[i]--;
		}

		FlushQueuedUse();
		UpdateChanneling();
	}

	private void UpdateChanneling()
	{
		if (!IsChanneling)
			return;

		int slot = ChannelSlot;
		Item template = Slots[slot];
		Vector2 aim = SkillBarAim.GetUsePosition(Player, this);

		if (!SkillBarChannelUse.Tick(this, Player, aim) && !template.IsAir)
			SetSlotCooldown(slot, template);
	}

	public void BeginChannel(int slot, Item item)
	{
		if (IsChanneling)
			EndChannel(Player);

		ChannelSlot = slot;
		ChannelHoldSlot = SkillBarItemUse.FindBackgroundHotbarSlot(Player);
		ChannelBackup = Player.inventory[ChannelHoldSlot].Clone();
		Player.inventory[ChannelHoldSlot] = item.Clone();
		Player.inventory[ChannelHoldSlot].stack = 1;
	}

	public void EndChannel(Player player)
	{
		if (ChannelHoldSlot < 0)
			return;

		player.inventory[ChannelHoldSlot] = ChannelBackup.Clone();
		player.controlUseItem = false;
		ChannelHoldSlot = -1;
		ChannelSlot = -1;
	}

	public bool IsSlotOnCooldown(int slot)
	{
		return slot >= 0 && slot < _slotCooldown.Length && _slotCooldown[slot] > 0;
	}

	public void SetSlotCooldown(int slot, Item item)
	{
		if (slot < 0 || slot >= _slotCooldown.Length)
			return;

		int delay = System.Math.Max(item.useAnimation, item.useTime);
		if (delay <= 0)
			delay = 7;

		_slotCooldown[slot] = delay;
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (Player.whoAmI != Main.myPlayer)
			return;

		if (!CanProcessSkillInput())
			return;

		for (int i = 0; i < SkillBar.SlotCount; i++) {
			if (SkillBarKeybinds.SlotKeys?[i]?.JustPressed == true)
				TriggerSlot(i, "menu");
		}
	}

	/// <summary>Config keys only — menu keybinds are handled in ProcessTriggers to avoid double fire.</summary>
	private void PollConfigKeys()
	{
		KeyboardState current = Keyboard.GetState();

		if (!CanProcessSkillInput()) {
			_prevKeyboard = current;
			SkillBarKeyState.Sync();
			return;
		}

		SkillBarConfig cfg = SkillBarConfig.Instance;

		for (int i = 0; i < SkillBar.SlotCount; i++) {
			Keys key = cfg.GetSlotKey(i);
			bool configKey = current.IsKeyDown(key) && !_prevKeyboard.IsKeyDown(key);
			bool legacyKey = SkillBarKeyState.WasJustPressed(key);

			if (configKey || legacyKey)
				TriggerSlot(i, "keyboard");
		}

		_prevKeyboard = current;
		SkillBarKeyState.Sync();
	}

	public void TriggerSlot(int slot, string source)
	{
		if (_lastTriggerSlot == slot && _lastTriggerFrame == Main.GameUpdateCount)
			return;

		_lastTriggerFrame = Main.GameUpdateCount;
		_lastTriggerSlot = slot;

		if (SkillBarConfig.Instance.LogKeyPresses) {
			Main.NewText(Language.GetTextValue("Mods.SkillBar.SlotPressed", slot + 1, SkillBarKeybinds.GetDisplayName(slot), source), Color.Cyan);
		}

		try {
			OnSkillSlotTriggered(slot);
		}
		catch (System.Exception ex) {
			Mod.Logger.Error($"Skill bar slot {slot + 1} failed", ex);
			if (Player.whoAmI == Main.myPlayer)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.UseCrashed", ex.Message), Color.OrangeRed);
		}
	}

	public static bool CanProcessSkillInput()
	{
		if (Main.gameMenu || Main.dedServ)
			return false;

		if (PlayerInput.WritingText)
			return false;

		SkillBarConfig cfg = SkillBarConfig.Instance;
		if (cfg == null || !cfg.Enabled)
			return false;

		if (!cfg.AllowWhileChatOpen && Main.drawingPlayerChat)
			return false;

		if (!cfg.AllowWithInventoryOpen && Main.playerInventory)
			return false;

		return true;
	}

	private void OnSkillSlotTriggered(int slot)
	{
		if (Slots[slot].IsAir) {
			if (!Main.mouseItem.IsAir || !Main.HoverItem.IsAir)
				SkillBarUI.TryAssignToSlot(this, slot);
			else if (Player.whoAmI == Main.myPlayer)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.EmptySlot"), Color.Gray);
			return;
		}

		QueueUseSlot(slot);
	}

	private void QueueUseSlot(int slot)
	{
		if (slot < 0 || slot >= SkillBar.SlotCount)
			return;

		if (_queuedSlot >= 0)
			return;

		_queuedSlot = slot;
		_queuedAim = SkillBarAim.GetUsePosition(Player, this);

		if (Main.netMode == NetmodeID.MultiplayerClient) {
			ModPacket packet = ModContent.GetInstance<SkillBar>().GetPacket();
			packet.Write((byte)SkillBar.MessageType.RequestUseSlot);
			packet.Write((byte)slot);
			packet.WriteVector2(_queuedAim);
			packet.Send();
		}
	}

	private void FlushQueuedUse()
	{
		if (_queuedSlot < 0)
			return;

		int slot = _queuedSlot;
		Vector2 aim = _queuedAim;
		_queuedSlot = -1;

		UseSlot(slot, aim, fromNetwork: false);
	}

	public void UseSlot(int slot, Vector2 cursorWorld, bool fromNetwork)
	{
		if (slot < 0 || slot >= SkillBar.SlotCount)
			return;

		if (IsChanneling && slot != ChannelSlot)
			return;

		Item template = Slots[slot];
		if (template.IsAir) {
			if (Player.whoAmI == Main.myPlayer)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.EmptySlot"), Color.Gray);
			return;
		}

		if (!SkillBarItemUse.IsEligibleForSkillBar(template)) {
			if (Player.whoAmI == Main.myPlayer)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.NotUsable"), Color.OrangeRed);
			return;
		}

		Item inventoryItem = SkillBarItemUse.FindInventoryItem(Player, template.type);
		Item useItem = (inventoryItem ?? template).Clone();
		useItem.stack = 1;

		if (!SkillBarItemUse.HasResource(Player, useItem)) {
			if (Player.whoAmI == Main.myPlayer)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.NoResources"), Color.OrangeRed);
			return;
		}

		if (IsSlotOnCooldown(slot)) {
			if (Player.whoAmI == Main.myPlayer)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.OnCooldown"), Color.Gray);
			return;
		}

		bool isMiningTool = SkillBarToolUse.IsMiningTool(useItem);
		bool isChannel = SkillBarChannelUse.WantsChannel(useItem);
		if (!isMiningTool && !isChannel && (Player.itemAnimation > 0 || Player.itemTime > 0)) {
			if (Player.whoAmI == Main.myPlayer)
				Main.NewText(Language.GetTextValue("Mods.SkillBar.OnCooldown"), Color.Gray);
			return;
		}

		bool used;

		if (isMiningTool)
			used = SkillBarItemUse.TryUseMiningTool(Player, useItem, cursorWorld);
		else if (SkillBarPlacement.IsPlacementItem(useItem))
			used = SkillBarItemUse.TryUsePlacement(Player, useItem, cursorWorld);
		else
			used = SkillBarItemUse.TryUseWeapon(Player, useItem, cursorWorld, slot, this);

		if (used && !isChannel)
			SetSlotCooldown(slot, useItem);

		if (!used && Player.whoAmI == Main.myPlayer)
			Main.NewText(Language.GetTextValue("Mods.SkillBar.UseFailed"), Color.OrangeRed);

		if (SkillBarConfig.Instance.LogKeyPresses && Player.whoAmI == Main.myPlayer) {
			Main.NewText(
				used
					? Language.GetTextValue("Mods.SkillBar.UseOk", Player.tileTargetX, Player.tileTargetY)
					: Language.GetTextValue("Mods.SkillBar.UseDebugFail", useItem.Name),
				used ? Color.LimeGreen : Color.OrangeRed);
		}
	}

	public override void SaveData(TagCompound tag)
	{
		tag["barX"] = BarScreenPosition.X;
		tag["barY"] = BarScreenPosition.Y;
		tag["positionInit"] = PositionInitialized;

		var slotTags = new List<TagCompound>();
		for (int i = 0; i < SkillBar.SlotCount; i++)
			slotTags.Add(ItemIO.Save(Slots[i]));
		tag["slots"] = slotTags;
	}

	public override void LoadData(TagCompound tag)
	{
		BarScreenPosition = new Vector2(tag.GetFloat("barX"), tag.GetFloat("barY"));
		PositionInitialized = tag.GetBool("positionInit");

		if (tag.ContainsKey("slots")) {
			IList<TagCompound> slotTags = tag.GetList<TagCompound>("slots");
			for (int i = 0; i < SkillBar.SlotCount && i < slotTags.Count; i++)
				Slots[i] = ItemIO.Load(slotTags[i]);
		}
	}
}
