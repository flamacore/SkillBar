using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace SkillBar;

public static class SkillBarKeybinds
{
	public static ModKeybind[] SlotKeys { get; private set; } = null!;

	internal static void Load(Mod mod)
	{
		SlotKeys = new ModKeybind[SkillBar.SlotCount];
		Keys[] defaults = SkillBarConfig.DefaultSlotKeys;
		for (int i = 0; i < SkillBar.SlotCount; i++) {
			SlotKeys[i] = KeybindLoader.RegisterKeybind(mod, $"SkillSlot{i + 1}", defaults[i]);
		}
	}

	internal static void Unload()
	{
		SlotKeys = null!;
	}

	public static string GetDisplayName(int slot)
	{
		if (slot < 0 || slot >= SkillBar.SlotCount)
			return "?";

		if (SlotKeys != null) {
			var assigned = SlotKeys[slot].GetAssignedKeys(InputMode.Keyboard);
			if (assigned != null && assigned.Count > 0)
				return assigned[0];
		}

		return SkillBarConfig.Instance.GetSlotKey(slot).ToString();
	}
}
