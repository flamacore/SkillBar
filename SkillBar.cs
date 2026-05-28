using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace SkillBar;

public class SkillBar : Mod
{
	public const int SlotCount = 10;

	public enum MessageType : byte
	{
		RequestUseSlot
	}

	public override void HandlePacket(BinaryReader reader, int whoAmI)
	{
		MessageType type = (MessageType)reader.ReadByte();
		if (type == MessageType.RequestUseSlot) {
			int slot = reader.ReadByte();
			Vector2 cursorWorld = reader.ReadVector2();
			Player player = Main.player[whoAmI];
			var sb = player.GetModPlayer<SkillBarPlayer>();
			sb.UseSlot(slot, cursorWorld, fromNetwork: true);
		}
	}
}
