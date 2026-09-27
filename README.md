# Star Sweepers

A two player game about cleaning up space. You share a ship, vacuum up the trash drifting by, and sort it into the right filter while dodging hazards.

- Play: [itch.io](https://daniel-narvaez.itch.io/star-sweepers)
- Made: July to August 2023 for the DEV EXP game jam
- Team: [@mfclinton](https://github.com/mfclinton) (programming), [@daniel-narvaez](https://github.com/daniel-narvaez) (2D art and game design), [@MrAozora](https://github.com/MrAozora) (music)
- Engine: Unity, C#

This is an export of a private repo with only the code we wrote. Art, audio, the Unity project files, and third party plugins aren't included. The history is squashed into one commit.

## What I built

- Players walk around inside the ship while it flies and turns. Anyone standing in it moves and spins with the ship and stays upright, so the crew can keep working while the pilot steers.
- The ship has three stations, the pilot seat, the vacuum, and the filter, and any player can hop into whichever one is free. The vacuum nozzle swings on a physics hose, sucks trash in or blows it away, and drags whoever's running it along.
- Caught trash travels down a pipe and gets scored against the filter that's set when it arrives, not when it was caught, so the filter player has to time their switches.
- Hits drain the shield before the hull, with a short grace period after each one. Trash and hazards come from object pools and spawn as the ship makes progress, not while it sits still, and anything that drifts out of bounds goes back into its pool.
- Each player joins on their own controller, picks a name and character, and readies up. Either player can drive the shared menus, and at the end your team enters its name with an arcade style letter picker for the PlayFab leaderboard.
