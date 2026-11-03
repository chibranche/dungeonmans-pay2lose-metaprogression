using System;
using System.Reflection;
using DungeonmansModKit;
using HarmonyLib;

namespace DungeonmansPay2Lose;

// Forwards to ModKit's shared log (ModKit\modkit.log), prefixed "[pay2lose]".
internal static class Log
{
	public static void Write(string msg) => Pay2LoseMod.Ctx?.Log.Write(msg);
}

// ModKit entry point.
public sealed class Pay2LoseMod : IMod
{
	internal static ModContext Ctx;

	public void Load(ModContext ctx)
	{
		Ctx = ctx;
		Config.Register(ctx.Settings);
		ctx.Patch("Pay 2 Lose legacy", Legacy.Register);
		LegacyWindow.Register();
		// Screenshot scenes (developer tool, inactive for players).
		DevCapture.Scene("pay2lose_window", LegacyWindow.Open, () => LegacyWindow.IsOpen = false);
		(int, int) saved = (0, 0);
		DevCapture.Scene("pay2lose_window_example", () => { saved = Legacy.DevSet(3, 12_345); LegacyWindow.Open(); },
			() => { LegacyWindow.IsOpen = false; Legacy.DevSet(saved.Item1, saved.Item2); });
		DevCapture.Scene("pay2lose_menu", DungeonMans.dmPay2Lose.OpenMenu, DungeonMans.dmPay2Lose.CloseMenu);
		DevCapture.Scene("pay2lose_docs", () => Ui.OpenModDocs("pay2lose"), Ui.CloseModSettings);
	}
}

internal static class Mod
{
	public static HarmonyMethod Hook(Type type, string name) => Patching.Hook(type, name);

	public static MethodInfo Target(Type type, string name) => Patching.Method(type, name);
}
