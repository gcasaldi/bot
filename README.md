# PopAndStack

PopAndStack is a hyper-casual merge game prototype for Android. Tap to drop balls, merge matching colors, and avoid hitting the game-over line while gravity shifts every few seconds.

## Quick Start (Unity)

1. Open the project in Unity 2022.3 LTS or newer.
2. Create a new empty scene and save it as `Main`.
3. Press Play. The game auto-spawns the manager and builds the scene at runtime.

## Controls

- Tap / click to drop a ball.
- When the stack touches the top line, game over.
- Tap after game over to restart.
- Gravity rotates in phases (down, left, up, right).
- Merges within a short window build a score combo.

## Android Settings

- Set Package Name to `com.giuliacasaldi.popandstack` in Player Settings.
- Target Android and build an APK or AAB.

## Monetization Hook (not implemented)

- Rewarded ad: offer a second chance by removing the top ball.

## Notes

- All visuals are generated at runtime as placeholders. Replace them with your own art later.
- Audio uses generated tones as placeholders. Replace with real SFX before release.
