using System;
using DungeonMans;
using DungeonmansModKit;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace DungeonmansPay2Lose;

// The "P2L" button in ModKit's top bar opens this window: the current XP bonus, the best win, the 9 tiers and what a
// win with the current hero would add. The game's own Pay 2 Lose menu also gets one line on its Summary page.
internal static class LegacyWindow
{
	public static bool IsOpen;

	// The game's comment for each tier (dmPay2Lose.GetTauntFromBrutalityScore), shown when you hover a row.
	private static readonly string[] Comments =
	{
		"This is not enough Brutalities.",
		"Not bad, well on your way to becoming Triger chow.",
		"Winning with this score would be almost admirable.",
		"A mighty challenge! Probably too much for you, though.",
		"You're in over your head, but you knew that anyway.",
		"It's hopeless, just make it as far as you can before you bleed out.",
		"You won't win, but maybe you'll almost-win better than all your friends?",
		"See you at the character creation screen in five minutes, champ.",
		"Yeah alright, whatever, numbers are meaningless. Math is hard.",
	};

	public static void Register()
	{
		Ui.AddModal(() => IsOpen);
		Events.HeroChanged += (g, hero) => IsOpen = false;
		Events.Input += g =>
		{
			if (!IsOpen)
			{
				return false;
			}
			if (g.theInput.ExecutedActions.Contains(ActionButton.Close))
			{
				Close();
			}
			g.theInput.ExecutedActions.Clear();
			return true;
		};
		Events.DrawOverlay += g =>
		{
			if (g.bPay2LoseMenuMode)
			{
				DrawMenuLine(g);
				return;
			}
			if (g.dungeonMans == null || g.bShowMainMenu)
			{
				return;
			}
			if (!IsOpen && !Ui.IsBusy(g))
			{
				TopBar.Add("pay2lose.legacy", 64, (r, clickable) =>
				{
					g.theRenderer.BeginSpriteBatch();
					Ui.HangingButton("p2l_legacy_open", r, "P2L", clickable ? Open : (Action)null,
						tooltipText: "Pay 2 Lose legacy\nYour XP bonus and the Brutality tiers\nDrag to move", stackLevel: 6);
					g.theRenderer.EndSpriteBatch();
				}, priority: 50);
			}
			if (IsOpen)
			{
				Draw(g);
			}
		};
	}

	public static void Open()
	{
		IsOpen = true;
		dmAudio.PlayUIFX("open_dialog");
	}

	public static void Close()
	{
		IsOpen = false;
		dmAudio.PlayUIFX("ui_back_02");
	}

	private static string Number(long n) => n.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);

	private static void Draw(dmGame g)
	{
		g.theRenderer.BeginSpriteBatch();
		Rectangle c = Ui.ModalWindow("p2l_legacy", 22, 17, "Pay 2 Lose legacy", Close);
		int x = c.X + 8, y = c.Y + 4;
		bool on = Config.On(Config.Legacy);
		int tiers = Legacy.TiersReached;

		// Current bonus and best win.
		if (tiers > 0)
		{
			Ui.Text("Every hero of this Academy gets +" + Legacy.BonusPercent + "% XP.", x, y, Palette.Accent, UiFont.Medium);
		}
		else
		{
			Ui.Text("No bonus yet.", x, y, Palette.Accent, UiFont.Medium);
		}
		int best = Legacy.BestBrutality;
		Ui.Text(best > 0 ? "Best win: Brutality " + Number(best) + "." : "Beat the game with Pay 2 Lose settings on to earn a bonus.", x, y + 32, Palette.Text);

		// The current hero.
		string hero;
		Color heroColor = Palette.TextDim;
		if (!dmPay2Lose.IsActiveForThisHero())
		{
			hero = "This hero has no Pay 2 Lose settings, so a win adds nothing.";
		}
		else
		{
			long b = Legacy.Brutality();
			int gain = Math.Max(0, Legacy.TierFor(b) - tiers) * Legacy.PercentPerTier;
			hero = "This hero: Brutality " + Number(b) + ". " + (gain > 0 ? "Beating the game adds +" + gain + "% XP." : "Beating the game adds nothing new.");
			heroColor = gain > 0 ? Palette.Good : Palette.TextDim;
		}
		Ui.Text(hero, x, y + 56, heroColor);

		// The tiers.
		int top = y + 96;
		Ui.Text("Tier", x + 8, top, Palette.TextMuted);
		Ui.Text("Win at Brutality", x + 70, top, Palette.TextMuted);
		Ui.Text("Bonus", x + 290, top, Palette.TextMuted);
		int heroTier = dmPay2Lose.IsActiveForThisHero() ? Legacy.TierFor(Legacy.Brutality()) : 0;
		const int rowH = 30;
		for (int i = 0; i < Legacy.Tiers.Length; i++)
		{
			var row = new Rectangle(c.X, top + 26 + i * rowH, c.Width, rowH - 2);
			bool reached = i < tiers;
			bool next = !reached && i < heroTier;
			Ui.Fill(row, reached ? Palette.PanelSelected : i % 2 == 0 ? Palette.PanelRaised : Palette.Panel);
			int ty = row.Y + 5;
			Ui.Text((i + 1).ToString(), x + 14, ty, reached ? Palette.Accent : Palette.Text);
			Ui.Text(i == 0 ? "Any setting" : Number(Legacy.Tiers[i]) + " or more", x + 70, ty, reached ? Palette.Accent : Palette.Text);
			Ui.Text("+" + (i + 1) * Legacy.PercentPerTier + "% XP", x + 290, ty, reached ? Palette.Accent : Palette.Text);
			string status = reached ? "Reached" : next ? "This hero can reach it" : "";
			Ui.TextRight(status, row.Right - 12, ty, reached ? Palette.Good : Palette.Info);
			if (row.Contains(Ui.Mouse))
			{
				Ui.Tooltip("The game says:\n\"" + Comments[i] + "\"");
			}
		}

		// Footer.
		int fy = top + 26 + Legacy.Tiers.Length * rowH + 10;
		if (on)
		{
			Ui.Text("While this mod is on, no score is sent to the Steam leaderboards.", x, fy, Palette.Warn);
		}
		else
		{
			Ui.Text("The mod is off in Mod Settings: no XP bonus, and scores are sent as usual.", x, fy, Palette.Warn);
		}
		Ui.Text("Bonuses add up: each tier counts once. Your tiers are saved with this world.", x, fy + 24, Palette.TextDim);
		g.theRenderer.EndSpriteBatch();
	}

	// One line at the bottom of the game's Pay 2 Lose menu, on its Summary page.
	private static void DrawMenuLine(dmGame g)
	{
		try
		{
			if (!Config.On(Config.Legacy) || Traverse.Create(typeof(dmPay2Lose)).Field("strCurrentSection").GetValue<string>() != "summary")
			{
				return;
			}
			Rectangle area = Traverse.Create(typeof(dmPay2Lose)).Field("rectMainDrawArea").GetValue<Rectangle>();
			Vector2 left = Traverse.Create(typeof(dmPay2Lose)).Field("vOptionsAreaTopLeft").GetValue<Vector2>();
			int tier = Legacy.TierFor(dmPay2Lose.GetCurrentBrutality());
			string text = tier == 0
				? "Pay 2 Lose legacy: pick settings, then beat the game to earn an XP bonus."
				: "Pay 2 Lose legacy: a win at this Brutality reaches tier " + tier + " of 9 (+" + tier * Legacy.PercentPerTier + "% XP).";
			if (tier > 0 && g.dungeonMans != null && !g.bShowMainMenu)
			{
				int gain = Math.Max(0, tier - Legacy.TiersReached) * Legacy.PercentPerTier;
				text += gain > 0 ? " That is +" + gain + "% more than now." : " You already have it.";
			}
			g.theRenderer.BeginSpriteBatch();
			Ui.Text(text, (int)left.X, area.Bottom - 50, Color.White, UiFont.Medium);
			g.theRenderer.EndSpriteBatch();
		}
		catch
		{
			// The game's menu changed: just don't show the line.
		}
	}
}
