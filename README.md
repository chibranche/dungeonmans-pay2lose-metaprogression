# Dungeonmans Pay 2 Lose Metaprogression

A Dungeonmans mod (Steam, .NET 8) that turns the Pay 2 Lose challenge settings into Academy progression. Requires
[Dungeonmans ModKit](https://github.com/chibranche/dungeonmans-mod-toolkit) (included in the release zip). It changes
the game through Harmony patches at runtime; `Dungeonmans.dll` is never modified.

## What it does

In the base game, Pay 2 Lose only gives a Brutality score for the Steam leaderboards. With this mod:

- **Beat the game** (kill the Final Foom) with a Pay 2 Lose hero, and this hero (from the winning kill on) and every later hero of this Academy gain
  **+10% XP** for each Brutality tier reached for the first time.
- The tiers follow the game's own Brutality comments: any setting, 1,000, 10,000, 50,000, 100,000, 500,000,
  1 million, 5 million and 100 million, so **+90% at most**.
- A clear at the same or a lower Brutality adds nothing. Your best tier is saved with the world.
- Each new hero's log shows the current bonus. The kill shows how many tiers you gained.
- **No score is sent to the Steam leaderboards** while the mod is on, since the boosted XP would make the scores
  unfair.

Switch it off in **Mod Settings** under "Dungeonmans Pay 2 Lose Metaprogression"; the bonus and the leaderboard
block both stop. The tiers you earned stay saved.

## Install

Download `DungeonmansPay2Lose-<version>-with-ModKit.zip` from the Releases page, extract it into the Dungeonmans game
folder (Steam: right-click the game > Manage > Browse local files) and double-click `ModKit-Install.bat`.
Run it again after a Steam update. Details: `ModKit-README.txt` in the zip. Extracting it over other ModKit mods is
fine: they all carry the same ModKit.

Log: `<game>\ModKit\modkit.log`, where the mod is listed as `[pay2lose] ok` or `[pay2lose] FAIL`.

## Building from source

Needs the .NET 8 SDK, the game, and [Dungeonmans ModKit](https://github.com/chibranche/dungeonmans-mod-toolkit)
installed in the game folder (the project references `<game>\ModKit\DungeonmansModKit.dll`).

```powershell
.\install.ps1 [-GameDir <path>]   # build and copy into <game>\Mods\Pay2Lose
.\build-release.ps1               # dist\DungeonmansPay2Lose-<v>-with-ModKit.zip (uses the toolkit's release zip)
```

## License

MIT, see [LICENSE](LICENSE).
