using DungeonmansModKit;

namespace DungeonmansPay2Lose;

// The mod's switch, shown in ModKit's Mod Settings menu and saved in Mods\Pay2Lose\config.json.
internal static class Config
{
	public const string Legacy = "p2l_legacy";

	private static ModSettings settings;

	public static void Register(ModSettings s)
	{
		settings = s;
		s.AddToggle(Legacy, "Pay 2 Lose Metaprogression (no leaderboards)",
			"Beating the game with Pay 2 Lose gives later heroes +10% XP per new Brutality tier. While on, no score is sent to the Steam leaderboards.",
			group: "Pay 2 Lose");
	}

	public static bool On(string key) => settings?.On(key) ?? true;
}
