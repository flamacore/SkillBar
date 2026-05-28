using Terraria.ModLoader;

namespace SkillBar;

public class SkillBarKeybindSystem : ModSystem
{
	public override void Load()
	{
		SkillBarKeybinds.Load(Mod);
	}

	public override void Unload()
	{
		SkillBarKeybinds.Unload();
	}
}
