using Microsoft.Xna.Framework.Input;
using Terraria;

namespace SkillBar;

/// <summary>
/// Keyboard edge detection. Call WasJustPressed BEFORE Sync each frame.
/// </summary>
internal static class SkillBarKeyState
{
	private static readonly bool[] Previous = new bool[256];

	public static bool WasJustPressed(Keys key)
	{
		int index = (int)key;
		if (index < 0 || index >= Previous.Length)
			return false;

		bool current = IsDown(key);
		return current && !Previous[index];
	}

	public static void Sync()
	{
		for (int i = 0; i < Previous.Length; i++)
			Previous[i] = IsDown((Keys)i);
	}

	private static bool IsDown(Keys key)
	{
		// Prefer XNA keyboard (always updated); fall back to Terraria's buffer.
		if (Keyboard.GetState().IsKeyDown(key))
			return true;

		return Main.keyState.IsKeyDown(key);
	}
}
