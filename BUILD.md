# Build Instructions

## Play in Editor
1. Open `CandyCrush` in Unity 6000.3.10f1
2. Run **CandyCrush > Setup Project** from the menu (creates level assets and folders)
3. Open `Assets/Scenes/Menu.unity` and press Play

## WebGL (itch.io)
1. File > Build Settings
2. Select WebGL, Switch Platform
3. Player Settings: Compression = Disabled (for faster loads)
4. Build to `Builds/WebGL`

## Android APK (Google Play internal test)
1. File > Build Settings > Android > Switch Platform
2. Set Package Name in Player Settings
3. Build APK or App Bundle to `Builds/Android`

## Smoke test checklist
- Menu loads, lives/gold display
- Level select shows 15 levels
- Match-3 swap, cascade, special candies
- Win at target score, lose at 0 moves
- Pause exit costs 1 life
- Daily check-in grants reward once per day
- Save persists after restart

## Backlog (post-launch)
Chocolate, Ice obstacles, leaderboard, IAP, Firebase auth
