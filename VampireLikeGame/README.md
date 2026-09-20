# VampireLikeGame

A Vampire Survivors–style auto-battler prototype for Unity: move around an arena while your equipped
weapons auto-attack the nearest enemies, collect XP gems, and pick an upgrade every time you level up.

## Requirements

- Unity **2022.3 LTS** (any recent 2022.3.x patch works; other 2021/2023 versions will likely open fine
  too — Unity will just offer to upgrade the project).
- No paid assets or extra packages needed. Uses the built-in render pipeline, legacy `Input` manager,
  2D physics, and `UnityEngine.UI` (uGUI).

## Getting started

1. Open **Unity Hub → Add → select the `VampireLikeGame` folder** (the one containing `Assets/`,
   `Packages/`, `ProjectSettings/`).
2. Let Unity import the project (first import can take a minute or two).
3. In the menu bar, run **VampireLike → Build Demo Scene**.
   This is a one-click editor tool (`Assets/Editor/SceneBootstrapper.cs`) that generates everything
   needed to press Play immediately:
   - Placeholder sprites (colored squares/circles) under `Assets/Sprites/`
   - Prefabs (`Enemy`, `ToughEnemy`, `Projectile`, `ExperienceGem`) under `Assets/Prefabs/`
   - Data assets (enemy stats, weapons, upgrade cards) under `Assets/ScriptableObjects/`
   - A fully wired scene, `Assets/Scenes/MainGame.unity`, with the player, camera, spawner, and UI
     already connected, and registered as the game's build scene.
4. Open `Assets/Scenes/MainGame.unity` if it isn't already open, and press **Play**.
5. Re-running **VampireLike → Build Demo Scene** is safe — it overwrites the generated assets/scene
   instead of duplicating them, so you can re-run it after pulling script changes.

You never need to hand-wire GameObjects in the Inspector to get a working build; the bootstrapper does
that. It exists specifically because scenes/prefabs are fragile to hand-author outside the Editor —
regenerate them anytime with that one menu command instead of editing `.unity`/`.prefab` files directly.

## Controls

- **WASD / Arrow keys** — move
- Weapons fire automatically at the nearest enemy/enemies — there is no attack button
- On level-up, the game pauses and shows 3 upgrade cards — click one to continue

## Project structure

```
Assets/
  Scripts/
    Core/       GameManager (run timer/state), ObjectPool, CameraFollow, IDamageable
    Player/     PlayerController (movement), PlayerStats, PlayerHealth, PlayerLeveling, Player (hub)
    Enemies/    EnemyData (SO), EnemyController (chase + contact damage), EnemySpawner (wave data)
    Weapons/    WeaponData (SO), WeaponBase, ProjectileWeapon, AreaWeapon, Projectile, WeaponManager
    Items/      ExperienceGem
    Upgrades/   UpgradeData (SO), UpgradeSystem (picks/applies level-up cards)
    UI/         HUDController, LevelUpUI, UpgradeCardUI, GameOverUI
  Editor/
    SceneBootstrapper.cs   "VampireLike/Build Demo Scene" menu command described above
  ScriptableObjects/       Generated data assets (enemies/weapons/upgrades)
  Prefabs/                 Generated prefabs
  Sprites/                 Generated placeholder art
  Scenes/MainGame.unity    Generated scene
```

### Design notes / how to extend

- **Everything is data-driven.** New enemies are `EnemyData` assets, new weapons are `WeaponData`
  assets, new level-up cards are `UpgradeData` assets. You rarely need to touch code to add content —
  create the asset (`Assets → Create → VampireLike → ...`), fill in the fields, and (for upgrades)
  add it to `UpgradeSystem.allUpgrades` on the `GameSystems` object, or just re-run the bootstrapper's
  `t:UpgradeData` search picks up every `UpgradeData` asset in the project automatically.
- **Pooling.** Enemies, projectiles, and XP gems all spawn/despawn through `ObjectPool` instead of
  `Instantiate`/`Destroy`, so the game stays smooth once dozens of enemies are alive — the same pattern
  the genre needs at scale.
- **Weapons** are plain components (`ProjectileWeapon`, `AreaWeapon`) added at runtime by
  `WeaponManager` based on `WeaponData.weaponType`. Add a third `WeaponType` (e.g. an orbiting weapon)
  by subclassing `WeaponBase` and extending the `switch` in `WeaponManager.AddWeapon`.
- **Stats** (`PlayerStats`) are a single source of truth for damage/cooldown/area/etc. multipliers —
  upgrade cards call `ApplyFlatBonus`/`ApplyMultiplierBonus` rather than poking individual systems.
- **Difficulty** is entirely defined by the `EnemySpawner.waves` list (start time, enemy type, spawn
  interval/count) — tune pacing there without touching spawner code.

### Known limitations (by design, for a prototype)

- Placeholder art only (flat-colored squares/circles) — swap sprites on the prefabs/`EnemyData`/
  `WeaponData` once you have real art.
- No audio, no main menu, no pause menu beyond the level-up/game-over screens.
- Single arena, no camera bounds — enemies spawn in a ring around the player indefinitely.
