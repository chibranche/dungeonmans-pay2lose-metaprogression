using System;
using System.Reflection;
using DungeonMans;
using DungeonmansModKit;
using HarmonyLib;

namespace DungeonmansPay2Lose;

// Pay 2 Lose legacy: beating the game (killing the Final Foom) with a Pay 2 Lose hero gives every later hero of this
// Academy +10% XP for each Brutality tier reached for the first time. Tiers are the game's own Brutality comments:
// any setting, 1,000, 10,000, 50,000, 100,000, 500,000, 1 million, 5 million, 100 million (+90% at most).
// Replaying at the same Brutality pays nothing. Saved with the world (overworld script variables; the "qol_" names
// are kept from when this was part of the QoL mod, so earlier progress carries over).
// While this is on, no score is sent to the Steam leaderboards (boosted XP would skew them).
internal static class Legacy
{
	private const string MapName = "overworld";
	private const string TierVar = "qol_p2l_tiers";
	private const string BestVar = "qol_p2l_best";
	public const int PercentPerTier = 10;
	internal static readonly long[] Tiers = { 1, 1_000, 10_000, 50_000, 100_000, 500_000, 1_000_000, 5_000_000, 100_000_000 };

	private static readonly MethodInfo TotalBrutality = AccessTools.Method(typeof(dmPay2Lose), "TotalBrutality_All");

	public static int TiersReached
	{
		get => Math.Clamp(dmGazetteer.GetMapScriptVar_Int(TierVar, MapName), 0, Tiers.Length);
		private set => dmGazetteer.SetMapScriptVar_Int(TierVar, value, MapName);
	}

	public static int BonusPercent => TiersReached * PercentPerTier;

	// Highest Brutality this Academy has beaten the game with (0 if never).
	public static int BestBrutality => Math.Max(0, dmGazetteer.GetMapScriptVar_Int(BestVar, MapName));

	// Screenshot example only (DevCapture sessions never save): sets the tiers and best win, returns the old ones.
	internal static (int tiers, int best) DevSet(int tiers, int best)
	{
		var old = (TiersReached, BestBrutality);
		TiersReached = tiers;
		dmGazetteer.SetMapScriptVar_Int(BestVar, best, MapName);
		return old;
	}

	// How many tiers a win at this Brutality reaches.
	public static int TierFor(long brutality)
	{
		int reached = 0;
		while (reached < Tiers.Length && brutality >= Tiers[reached])
		{
			reached++;
		}
		return reached;
	}

	public static void Register(Harmony h)
	{
		h.Patch(Mod.Target(typeof(dmHero), nameof(dmHero.AwardXP)), prefix: Mod.Hook(typeof(Legacy), nameof(AwardXPPrefix)));
		h.Patch(Mod.Target(typeof(dmFinalFoom), nameof(dmFinalFoom.DropTreasureAndAwardXP)), prefix: Mod.Hook(typeof(Legacy), nameof(FoomDiedPrefix)));
		HarmonyMethod block = Mod.Hook(typeof(Legacy), nameof(UploadPrefix));
		h.Patch(AccessTools.Method(typeof(SteamLeaderboards), nameof(SteamLeaderboards.UploadScore), new[] { typeof(Leaderboard), typeof(int) })
			?? throw new MissingMethodException("SteamLeaderboards", "UploadScore(Leaderboard, int)"), prefix: block);
		h.Patch(AccessTools.Method(typeof(SteamLeaderboards), nameof(SteamLeaderboards.UploadScore), new[] { typeof(Leaderboard), typeof(int), typeof(int[]) })
			?? throw new MissingMethodException("SteamLeaderboards", "UploadScore(Leaderboard, int, int[])"), prefix: block);
		// Shown once the hero is in its first area: while a save loads, the world (where the bonus is saved) isn't ready.
		bool announce = false;
		Events.HeroChanged += (g, hero) => announce = true;
		Events.LevelEntered += (g, level) =>
		{
			if (!announce)
			{
				return;
			}
			announce = false;
			if (Config.On(Config.Legacy) && BonusPercent > 0)
			{
				g.AddStringToLog("^3Pay 2 Lose legacy:^7 +" + BonusPercent + "% XP for this hero.");
			}
		};
	}

	private static void AwardXPPrefix(dmHero __instance, ref int awardAmount)
	{
		try
		{
			if (awardAmount > 0 && Config.On(Config.Legacy) && __instance == dmUtilities.theGame?.dungeonMans)
			{
				int bonus = BonusPercent;
				if (bonus > 0)
				{
					awardAmount += awardAmount * bonus / 100;
				}
			}
		}
		catch (Exception e)
		{
			Log.Write("XP bonus skipped: " + e.Message); // never break the game's own XP award
		}
	}

	private static bool UploadPrefix() => !Config.On(Config.Legacy);

	private static void FoomDiedPrefix()
	{
		try
		{
			RecordWin();
		}
		catch (Exception e)
		{
			Log.Write("Could not record the win: " + e); // the Final Foom's death must go on regardless
		}
	}

	private static void RecordWin()
	{
		if (!Config.On(Config.Legacy) || !dmPay2Lose.IsActiveForThisHero())
		{
			return;
		}
		dmGame g = dmUtilities.theGame;
		long brutality = Brutality();
		int reached = TierFor(brutality);
		int before = TiersReached;
		if (brutality > dmGazetteer.GetMapScriptVar_Int(BestVar, MapName))
		{
			dmGazetteer.SetMapScriptVar_Int(BestVar, (int)Math.Min(int.MaxValue, brutality), MapName);
		}
		Log.Write("Pay 2 Lose legacy: game beaten at Brutality " + brutality + ", tier " + reached + " (had " + before + ")");
		if (reached <= before)
		{
			g.AddStringToLog("^3Pay 2 Lose legacy:^7 Brutality " + brutality + ", no new tier (still +" + BonusPercent + "% XP).");
			return;
		}
		TiersReached = reached;
		string msg = "+" + (reached - before) * PercentPerTier + "% XP for future heroes (now +" + BonusPercent + "%)";
		g.AddStringToLog("^3Pay 2 Lose legacy:^7 Brutality " + brutality + ". " + msg + ".");
		Ui.Toast("Pay 2 Lose legacy: " + msg, Palette.Good);
	}

	// The game's own score, recomputed from the settings in case it was not refreshed after loading the save.
	internal static long Brutality()
	{
		long b = dmPay2Lose.GetCurrentBrutality();
		try
		{
			if (TotalBrutality != null)
			{
				b = Math.Max(b, (int)TotalBrutality.Invoke(null, null));
			}
		}
		catch (Exception e)
		{
			Log.Write("Pay 2 Lose legacy: could not recompute Brutality: " + e.Message);
		}
		return b;
	}
}
