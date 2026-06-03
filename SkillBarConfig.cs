using System.ComponentModel;
using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SkillBar;

public class SkillBarConfig : ModConfig
{
	public static SkillBarConfig Instance => ModContent.GetInstance<SkillBarConfig>();

	public static readonly Keys[] DefaultSlotKeys =
	{
		Keys.Z, Keys.X, Keys.C, Keys.V, Keys.B, Keys.N, Keys.M,
		Keys.OemComma, Keys.OemPeriod, Keys.OemQuestion
	};

	public override ConfigScope Mode => ConfigScope.ClientSide;

	[Header("GeneralHeader")]
	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.Enabled.Label")]
	[TooltipKey("$Mods.SkillBar.Configs.SkillBarConfig.Enabled.Tooltip")]
	[DefaultValue(true)]
	public bool Enabled { get; set; } = true;

	[Header("AppearanceHeader")]
	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.BarOpacity.Label")]
	[TooltipKey("$Mods.SkillBar.Configs.SkillBarConfig.BarOpacity.Tooltip")]
	[Range(0f, 1f)]
	[Increment(0.05f)]
	[Slider]
	[DefaultValue(1f)]
	public float BarOpacity { get; set; } = 1f;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.ShowCooldownOverlay.Label")]
	[TooltipKey("$Mods.SkillBar.Configs.SkillBarConfig.ShowCooldownOverlay.Tooltip")]
	[DefaultValue(false)]
	public bool ShowCooldownOverlay { get; set; } = false;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.ShowCooldownChatMessages.Label")]
	[TooltipKey("$Mods.SkillBar.Configs.SkillBarConfig.ShowCooldownChatMessages.Tooltip")]
	[DefaultValue(true)]
	public bool ShowCooldownChatMessages { get; set; } = true;

	[Header("KeybindsHeader")]
	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.ShowKeyLabels.Label")]
	[TooltipKey("$Mods.SkillBar.Configs.SkillBarConfig.ShowKeyLabels.Tooltip")]
	[DefaultValue(true)]
	public bool ShowKeyLabels { get; set; } = true;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.Slot1Key.Label")]
	public Keys Slot1Key = Keys.Z;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.Slot2Key.Label")]
	public Keys Slot2Key = Keys.X;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.Slot3Key.Label")]
	public Keys Slot3Key = Keys.C;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.Slot4Key.Label")]
	public Keys Slot4Key = Keys.V;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.Slot5Key.Label")]
	public Keys Slot5Key = Keys.B;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.Slot6Key.Label")]
	public Keys Slot6Key = Keys.N;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.Slot7Key.Label")]
	public Keys Slot7Key = Keys.M;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.Slot8Key.Label")]
	public Keys Slot8Key = Keys.OemComma;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.Slot9Key.Label")]
	public Keys Slot9Key = Keys.OemPeriod;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.Slot10Key.Label")]
	public Keys Slot10Key = Keys.OemQuestion;

	[Header("InputHeader")]
	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.AllowWithInventoryOpen.Label")]
	[TooltipKey("$Mods.SkillBar.Configs.SkillBarConfig.AllowWithInventoryOpen.Tooltip")]
	[DefaultValue(true)]
	public bool AllowWithInventoryOpen { get; set; } = true;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.AllowWhileChatOpen.Label")]
	[TooltipKey("$Mods.SkillBar.Configs.SkillBarConfig.AllowWhileChatOpen.Tooltip")]
	[DefaultValue(false)]
	public bool AllowWhileChatOpen { get; set; } = false;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.LogKeyPresses.Label")]
	[TooltipKey("$Mods.SkillBar.Configs.SkillBarConfig.LogKeyPresses.Tooltip")]
	[DefaultValue(false)]
	public bool LogKeyPresses { get; set; } = false;

	[LabelKey("$Mods.SkillBar.Configs.SkillBarConfig.AuditWeaponsOnLoad.Label")]
	[TooltipKey("$Mods.SkillBar.Configs.SkillBarConfig.AuditWeaponsOnLoad.Tooltip")]
	[DefaultValue(false)]
	public bool AuditWeaponsOnLoad { get; set; } = false;

	public Keys GetSlotKey(int slot)
	{
		return slot switch {
			0 => Slot1Key,
			1 => Slot2Key,
			2 => Slot3Key,
			3 => Slot4Key,
			4 => Slot5Key,
			5 => Slot6Key,
			6 => Slot7Key,
			7 => Slot8Key,
			8 => Slot9Key,
			9 => Slot10Key,
			_ => Keys.None,
		};
	}
}
