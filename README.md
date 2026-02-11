# PopAndStack

PopAndStack is a hyper-casual merge game prototype for Android. Tap to drop balls, merge matching colors, and avoid hitting the game-over line while gravity shifts every few seconds.

## Quick Start (Unity)

1. Open the project in Unity 2022.3 LTS or newer.
2. Open the `Main` scene from `Assets/Scenes`.
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
- Interstitial: show after a game over every few runs.

## Ads Integration (AdMob)

This project includes a placeholder ads wrapper. To enable real ads:

1. Import Google Mobile Ads SDK for Unity.
2. Replace placeholder IDs with your AdMob App ID and Ad Unit IDs.
3. Wire up rewarded callbacks to grant a second chance.

Placeholders live in `Assets/Scripts/Ads/AdsService.cs`.

## Play Store Prep

Use the text placeholders in `store/` to build your listing.

## Notes

- All visuals are generated at runtime as placeholders. Replace them with your own art later.
- Audio uses generated tones as placeholders. Replace with real SFX before release.
