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
	}
}

internal static class Mod
{
	public static HarmonyMethod Hook(Type type, string name) => Patching.Hook(type, name);

	public static MethodInfo Target(Type type, string name) => Patching.Method(type, name);
}
