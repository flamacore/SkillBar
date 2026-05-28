using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace SkillBar;

public class SkillBarSystem : ModSystem
{
	// Runs during UI update so inventory being open does not block skill-bar clicks or keybinds.
	public override void UpdateUI(GameTime gameTime)
	{
		if (Main.gameMenu || Main.dedServ)
			return;

		Player local = Main.LocalPlayer;
		if (local == null || !local.active)
			return;

		SkillBarUI.HandleInput(local.GetModPlayer<SkillBarPlayer>());
	}

	public override void PostDrawInterface(SpriteBatch spriteBatch)
	{
		if (Main.gameMenu || Main.dedServ)
			return;

		Player local = Main.LocalPlayer;
		if (local == null || !local.active)
			return;

		SkillBarUI.Draw(spriteBatch, local.GetModPlayer<SkillBarPlayer>());
	}
}
