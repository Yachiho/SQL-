using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VampireLike.EditorTools
{
    /// <summary>
    /// One-click scene assembly so the project is playable right after cloning,
    /// without hand-authoring fragile .unity/.prefab YAML. Run via the
    /// "VampireLike/Build Demo Scene" menu. Safe to re-run: it overwrites the
    /// generated assets/scene each time rather than duplicating them.
    /// </summary>
    public static class SceneBootstrapper
    {
        private const string SpritesDir = "Assets/Sprites";
        private const string PrefabsDir = "Assets/Prefabs";
        private const string EnemiesDir = "Assets/ScriptableObjects/Enemies";
        private const string WeaponsDir = "Assets/ScriptableObjects/Weapons";
        private const string UpgradesDir = "Assets/ScriptableObjects/Upgrades";
        private const string SceneDir = "Assets/Scenes";
        private const string ScenePath = SceneDir + "/MainGame.unity";

        [MenuItem("VampireLike/Build Demo Scene")]
        public static void BuildScene()
        {
            EnsureFolders();

            var sprites = CreateSprites();
            var prefabs = CreatePrefabs(sprites);
            var enemyData = CreateEnemyData(prefabs.enemyPrefab);
            var weaponData = CreateWeaponData(prefabs.projectilePrefab);
            CreateUpgradeData(weaponData);

            BuildSceneHierarchy(enemyData, weaponData);

            Debug.Log("VampireLike demo scene built at " + ScenePath);
        }

        // ---------------------------------------------------------------
        // Folders
        // ---------------------------------------------------------------

        private static void EnsureFolders()
        {
            CreateFolderRecursive(SpritesDir);
            CreateFolderRecursive(PrefabsDir);
            CreateFolderRecursive(EnemiesDir);
            CreateFolderRecursive(WeaponsDir);
            CreateFolderRecursive(UpgradesDir);
            CreateFolderRecursive(SceneDir);
        }

        private static void CreateFolderRecursive(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string leaf = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                CreateFolderRecursive(parent);
            }
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // ---------------------------------------------------------------
        // Placeholder sprites
        // ---------------------------------------------------------------

        private struct SpriteSet
        {
            public Sprite player, enemy, toughEnemy, projectile, gem;
        }

        private static SpriteSet CreateSprites()
        {
            return new SpriteSet
            {
                player = CreateSolidSprite("PlayerSprite", new Color(0.25f, 0.55f, 1f), square: true),
                enemy = CreateSolidSprite("EnemySprite", new Color(0.85f, 0.2f, 0.2f), square: false),
                toughEnemy = CreateSolidSprite("ToughEnemySprite", new Color(0.5f, 0.05f, 0.05f), square: false),
                projectile = CreateSolidSprite("ProjectileSprite", new Color(1f, 0.9f, 0.2f), square: false, size: 16),
                gem = CreateSolidSprite("GemSprite", new Color(0.2f, 0.9f, 0.4f), square: false, size: 16),
            };
        }

        private static Sprite CreateSolidSprite(string name, Color color, bool square, int size = 32)
        {
            string path = $"{SpritesDir}/{name}.png";

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool fill = square || Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center) <= radius;
                    tex.SetPixel(x, y, fill ? color : clear);
                }
            }
            tex.Apply();

            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ---------------------------------------------------------------
        // Prefabs
        // ---------------------------------------------------------------

        private struct PrefabSet
        {
            public GameObject enemyPrefab, toughEnemyPrefab, projectilePrefab, gemPrefab;
        }

        private static PrefabSet CreatePrefabs(SpriteSet sprites)
        {
            GameObject gemPrefab = BuildAndSavePrefab("ExperienceGem", go =>
            {
                AddSprite(go, sprites.gem, "Sorting", 1);
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                go.AddComponent<ExperienceGem>();
            });

            GameObject projectilePrefab = BuildAndSavePrefab("Projectile", go =>
            {
                AddSprite(go, sprites.projectile, "Sorting", 2);
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.15f;
                go.AddComponent<Projectile>();
            });

            GameObject enemyPrefab = BuildAndSavePrefab("Enemy", go =>
            {
                AddSprite(go, sprites.enemy, "Sorting", 1);
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.4f;
                var enemy = go.AddComponent<EnemyController>();
                SetPrivateField(enemy, "experienceGemPrefab", gemPrefab);
            });

            GameObject toughEnemyPrefab = BuildAndSavePrefab("ToughEnemy", go =>
            {
                AddSprite(go, sprites.toughEnemy, "Sorting", 1);
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.5f;
                var enemy = go.AddComponent<EnemyController>();
                SetPrivateField(enemy, "experienceGemPrefab", gemPrefab);
            }, scale: 1.3f);

            return new PrefabSet
            {
                enemyPrefab = enemyPrefab,
                toughEnemyPrefab = toughEnemyPrefab,
                projectilePrefab = projectilePrefab,
                gemPrefab = gemPrefab
            };
        }

        private static GameObject BuildAndSavePrefab(string name, System.Action<GameObject> configure, float scale = 1f)
        {
            var go = new GameObject(name);
            configure(go);
            go.transform.localScale = Vector3.one * scale;

            string path = $"{PrefabsDir}/{name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void AddSprite(GameObject go, Sprite sprite, string sortingLayer, int order)
        {
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
        }

        // ---------------------------------------------------------------
        // ScriptableObject data
        // ---------------------------------------------------------------

        private struct EnemyDataSet
        {
            public EnemyData basic, tough;
        }

        private static EnemyDataSet CreateEnemyData(GameObject enemyPrefab)
        {
            var basic = CreateAsset<EnemyData>(EnemiesDir, "BasicEnemy", data =>
            {
                data.enemyName = "妖精"; // fairy - the series' iconic weakest generic mob, not a named character
                data.prefab = enemyPrefab;
                data.maxHealth = 10f;
                data.moveSpeed = 2.2f;
                data.contactDamage = 5f;
                data.contactDamageInterval = 1f;
                data.experienceValue = 1f;
            });

            var tough = CreateAsset<EnemyData>(EnemiesDir, "ToughEnemy", data =>
            {
                data.enemyName = "大妖精"; // big fairy - also a generic mob type, not a named character
                data.prefab = enemyPrefab; // swap prefab/sprite later if you want a distinct look
                data.maxHealth = 35f;
                data.moveSpeed = 1.5f;
                data.contactDamage = 10f;
                data.contactDamageInterval = 1f;
                data.experienceValue = 4f;
            });

            return new EnemyDataSet { basic = basic, tough = tough };
        }

        private struct WeaponDataSet
        {
            public WeaponData magicBolt, garlicAura;
        }

        private static WeaponDataSet CreateWeaponData(GameObject projectilePrefab)
        {
            var bolt = CreateAsset<WeaponData>(WeaponsDir, "MagicBolt", data =>
            {
                data.weaponName = "陰陽玉";
                data.weaponType = WeaponType.Projectile;
                data.maxLevel = 8;
                data.baseDamage = 8f;
                data.damagePerLevel = 2f;
                data.baseCooldown = 0.9f;
                data.cooldownReductionPerLevel = 0.05f;
                data.projectilePrefab = projectilePrefab;
                data.projectileSpeed = 9f;
                data.pierce = 1;
                data.projectileCount = 1;
                data.extraProjectileAtLevels = new[] { 3, 6 };
            });

            var garlic = CreateAsset<WeaponData>(WeaponsDir, "GarlicAura", data =>
            {
                data.weaponName = "結界";
                data.weaponType = WeaponType.Area;
                data.maxLevel = 6;
                data.baseDamage = 4f;
                data.damagePerLevel = 1.5f;
                data.baseCooldown = 0.5f;
                data.cooldownReductionPerLevel = 0.03f;
                data.baseAreaRadius = 1.8f;
                data.areaRadiusPerLevel = 0.2f;
            });

            return new WeaponDataSet { magicBolt = bolt, garlicAura = garlic };
        }

        private static void CreateUpgradeData(WeaponDataSet weapons)
        {
            CreateAsset<UpgradeData>(UpgradesDir, "Upgrade_MagicBolt", u =>
            {
                u.displayName = "陰陽玉";
                u.description = "一番近い妖怪に自動で追尾する陰陽玉を放つ。習得済みならレベルアップ。";
                u.category = UpgradeCategory.Weapon;
                u.weaponData = weapons.magicBolt;
            });

            CreateAsset<UpgradeData>(UpgradesDir, "Upgrade_GarlicAura", u =>
            {
                u.displayName = "結界";
                u.description = "周囲の妖怪に定期的にダメージを与える結界を展開する。習得済みならレベルアップ。";
                u.category = UpgradeCategory.Weapon;
                u.weaponData = weapons.garlicAura;
            });

            CreateAsset<UpgradeData>(UpgradesDir, "Upgrade_MaxHealth", u =>
            {
                u.displayName = "気合";
                u.description = "最大HP +20。";
                u.category = UpgradeCategory.Stat;
                u.statType = StatType.MaxHealth;
                u.isMultiplier = false;
                u.value = 20f;
            });

            CreateAsset<UpgradeData>(UpgradesDir, "Upgrade_MoveSpeed", u =>
            {
                u.displayName = "神速の歩法";
                u.description = "移動速度 +0.5。";
                u.category = UpgradeCategory.Stat;
                u.statType = StatType.MoveSpeed;
                u.isMultiplier = false;
                u.value = 0.5f;
            });

            CreateAsset<UpgradeData>(UpgradesDir, "Upgrade_Damage", u =>
            {
                u.displayName = "霊力強化";
                u.description = "攻撃力 +15%。";
                u.category = UpgradeCategory.Stat;
                u.statType = StatType.Damage;
                u.isMultiplier = true;
                u.value = 0.15f;
            });

            CreateAsset<UpgradeData>(UpgradesDir, "Upgrade_Cooldown", u =>
            {
                u.displayName = "高速詠唱";
                u.description = "攻撃間隔 -8%。";
                u.category = UpgradeCategory.Stat;
                u.statType = StatType.CooldownReduction;
                u.isMultiplier = true;
                u.value = 0.08f;
            });

            CreateAsset<UpgradeData>(UpgradesDir, "Upgrade_Area", u =>
            {
                u.displayName = "結界拡大";
                u.description = "エリア攻撃の範囲 +15%。";
                u.category = UpgradeCategory.Stat;
                u.statType = StatType.AreaSize;
                u.isMultiplier = true;
                u.value = 0.15f;
            });

            CreateAsset<UpgradeData>(UpgradesDir, "Upgrade_PickupRadius", u =>
            {
                u.displayName = "アイテム吸引";
                u.description = "アイテムを引き寄せる範囲 +0.75。";
                u.category = UpgradeCategory.Stat;
                u.statType = StatType.PickupRadius;
                u.isMultiplier = false;
                u.value = 0.75f;
            });

            CreateAsset<UpgradeData>(UpgradesDir, "Upgrade_Armor", u =>
            {
                u.displayName = "根性";
                u.description = "被ダメージ -1（固定軽減）。";
                u.category = UpgradeCategory.Stat;
                u.statType = StatType.Armor;
                u.isMultiplier = false;
                u.value = 1f;
            });

            CreateAsset<UpgradeData>(UpgradesDir, "Upgrade_XpGain", u =>
            {
                u.displayName = "スコア稼ぎ";
                u.description = "獲得ポイント +10%。";
                u.category = UpgradeCategory.Stat;
                u.statType = StatType.XpGain;
                u.isMultiplier = true;
                u.value = 0.1f;
            });
        }

        private static T CreateAsset<T>(string dir, string name, System.Action<T> configure) where T : ScriptableObject
        {
            string path = $"{dir}/{name}.asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            configure(asset);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        // ---------------------------------------------------------------
        // Scene hierarchy
        // ---------------------------------------------------------------

        private static void BuildSceneHierarchy(EnemyDataSet enemyData, WeaponDataSet weaponData)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Player ---
            var playerGo = new GameObject("Reimu");
            playerGo.transform.position = Vector3.zero;
            var playerSprite = playerGo.AddComponent<SpriteRenderer>();
            playerSprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/PlayerSprite.png");
            playerSprite.sortingOrder = 5;
            var playerCollider = playerGo.AddComponent<CircleCollider2D>();
            playerCollider.radius = 0.4f;
            playerGo.AddComponent<PlayerStats>();
            playerGo.AddComponent<PlayerHealth>();
            playerGo.AddComponent<PlayerLeveling>();
            var playerController = playerGo.AddComponent<PlayerController>();
            SetPrivateField(playerController, "spriteRenderer", playerSprite);
            playerGo.AddComponent<Player>();
            var weaponManager = playerGo.AddComponent<WeaponManager>();

            // --- Camera ---
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            cameraGo.AddComponent<AudioListener>();
            var camFollow = cameraGo.AddComponent<CameraFollow>();
            SetPrivateField(camFollow, "target", playerGo.transform);

            // --- Pool / GameManager / Spawner / Upgrades ---
            var systemsGo = new GameObject("GameSystems");
            systemsGo.AddComponent<ObjectPool>();
            systemsGo.AddComponent<GameManager>();

            var spawner = systemsGo.AddComponent<EnemySpawner>();
            var waves = new List<SpawnWave>
            {
                new SpawnWave { waveName = "妖精の群れ", startTime = 0f, enemyData = enemyData.basic, spawnInterval = 1.2f, spawnCountPerTick = 1 },
                new SpawnWave { waveName = "妖精増加", startTime = 30f, enemyData = enemyData.basic, spawnInterval = 0.6f, spawnCountPerTick = 2 },
                new SpawnWave { waveName = "大妖精出現", startTime = 60f, enemyData = enemyData.tough, spawnInterval = 4f, spawnCountPerTick = 1 },
                new SpawnWave { waveName = "弾幕地獄", startTime = 120f, enemyData = enemyData.basic, spawnInterval = 0.3f, spawnCountPerTick = 3 },
            };
            SetPrivateField(spawner, "waves", waves);

            // --- Canvas / UI ---
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();

            BuildHud(canvasGo.transform, out var healthSlider, out var xpSlider, out var levelText, out var timerText);
            var hud = canvasGo.AddComponent<HUDController>();
            SetPrivateField(hud, "healthBar", healthSlider);
            SetPrivateField(hud, "xpBar", xpSlider);
            SetPrivateField(hud, "levelText", levelText);
            SetPrivateField(hud, "timerText", timerText);

            var upgradeSystem = systemsGo.AddComponent<UpgradeSystem>();
            string[] upgradeGuids = AssetDatabase.FindAssets("t:UpgradeData");
            var allUpgrades = new List<UpgradeData>(System.Array.ConvertAll(upgradeGuids,
                guid => AssetDatabase.LoadAssetAtPath<UpgradeData>(AssetDatabase.GUIDToAssetPath(guid))));
            SetPrivateField(upgradeSystem, "allUpgrades", allUpgrades);
            SetPrivateField(upgradeSystem, "optionsPerLevelUp", 3);
            SetPrivateField(upgradeSystem, "weaponManager", weaponManager);
            SetPrivateField(upgradeSystem, "playerStats", playerGo.GetComponent<PlayerStats>());

            BuildLevelUpUI(canvasGo.transform, upgradeSystem);
            BuildGameOverUI(canvasGo.transform);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static void BuildHud(Transform canvas, out Slider healthBar, out Slider xpBar, out Text levelText, out Text timerText)
        {
            var hudRoot = CreateUIObject("HUD", canvas);
            var hudRect = hudRoot.GetComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;

            healthBar = CreateSlider(hudRoot.transform, "HealthBar", new Color(0.8f, 0.15f, 0.15f),
                anchorMin: new Vector2(0f, 1f), anchorMax: new Vector2(0f, 1f),
                anchoredPos: new Vector2(140f, -30f), size: new Vector2(240f, 24f));

            xpBar = CreateSlider(hudRoot.transform, "XpBar", new Color(0.2f, 0.55f, 0.9f),
                anchorMin: new Vector2(0.5f, 1f), anchorMax: new Vector2(0.5f, 1f),
                anchoredPos: new Vector2(0f, -8f), size: new Vector2(600f, 14f));

            levelText = CreateText(hudRoot.transform, "LevelText", "Lv. 1", 24,
                anchorMin: new Vector2(0f, 1f), anchorMax: new Vector2(0f, 1f),
                anchoredPos: new Vector2(30f, -60f), size: new Vector2(150f, 30f), alignment: TextAnchor.MiddleLeft);

            timerText = CreateText(hudRoot.transform, "TimerText", "00:00", 28,
                anchorMin: new Vector2(0.5f, 1f), anchorMax: new Vector2(0.5f, 1f),
                anchoredPos: new Vector2(0f, -34f), size: new Vector2(200f, 34f), alignment: TextAnchor.MiddleCenter);
        }

        private static void BuildLevelUpUI(Transform canvas, UpgradeSystem upgradeSystem)
        {
            var panel = CreateUIObject("LevelUpPanel", canvas);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.75f);

            var title = CreateText(panel.transform, "Title", "レベルアップ！ 強化するスペルを選んでください", 32,
                anchorMin: new Vector2(0.5f, 1f), anchorMax: new Vector2(0.5f, 1f),
                anchoredPos: new Vector2(0f, -80f), size: new Vector2(800f, 50f), alignment: TextAnchor.MiddleCenter);

            var cardSlots = new List<UpgradeCardUI>();
            float[] xPositions = { -350f, 0f, 350f };
            foreach (float x in xPositions)
            {
                cardSlots.Add(CreateUpgradeCard(panel.transform, x));
            }

            panel.SetActive(false);

            var levelUpUi = panel.AddComponent<LevelUpUI>();
            SetPrivateField(levelUpUi, "panelRoot", panel);
            SetPrivateField(levelUpUi, "cardSlots", cardSlots);
            SetPrivateField(levelUpUi, "upgradeSystem", upgradeSystem);
        }

        private static UpgradeCardUI CreateUpgradeCard(Transform parent, float xOffset)
        {
            var card = CreateUIObject("Card", parent);
            var rect = card.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(xOffset, -20f);
            rect.sizeDelta = new Vector2(300f, 380f);

            var bg = card.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            var icon = CreateUIObject("Icon", card.transform);
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.anchoredPosition = new Vector2(0f, -90f);
            iconRect.sizeDelta = new Vector2(96f, 96f);
            var iconImage = icon.AddComponent<Image>();
            iconImage.color = new Color(1f, 1f, 1f, 0.9f);

            var title = CreateText(card.transform, "Title", "Upgrade", 22,
                anchorMin: new Vector2(0.5f, 1f), anchorMax: new Vector2(0.5f, 1f),
                anchoredPos: new Vector2(0f, -160f), size: new Vector2(270f, 40f), alignment: TextAnchor.MiddleCenter);

            var description = CreateText(card.transform, "Description", "Description", 16,
                anchorMin: new Vector2(0.5f, 1f), anchorMax: new Vector2(0.5f, 1f),
                anchoredPos: new Vector2(0f, -210f), size: new Vector2(260f, 120f), alignment: TextAnchor.UpperCenter);

            var buttonGo = CreateUIObject("ChooseButton", card.transform);
            var buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.anchoredPosition = new Vector2(0f, 30f);
            buttonRect.sizeDelta = new Vector2(200f, 44f);
            var buttonImage = buttonGo.AddComponent<Image>();
            buttonImage.color = new Color(0.25f, 0.5f, 0.9f);
            var button = buttonGo.AddComponent<Button>();
            CreateText(buttonGo.transform, "Label", "選択", 20,
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                anchoredPos: Vector2.zero, size: Vector2.zero, alignment: TextAnchor.MiddleCenter);

            var cardUi = card.AddComponent<UpgradeCardUI>();
            SetPrivateField(cardUi, "icon", iconImage);
            SetPrivateField(cardUi, "titleText", title);
            SetPrivateField(cardUi, "descriptionText", description);
            SetPrivateField(cardUi, "button", button);

            return cardUi;
        }

        private static void BuildGameOverUI(Transform canvas)
        {
            var panel = CreateUIObject("GameOverPanel", canvas);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.85f);

            var survivedText = CreateText(panel.transform, "SurvivedText", "力尽きた…\n生存時間 00:00", 36,
                anchorMin: new Vector2(0.5f, 0.5f), anchorMax: new Vector2(0.5f, 0.5f),
                anchoredPos: new Vector2(0f, 60f), size: new Vector2(600f, 120f), alignment: TextAnchor.MiddleCenter);

            var buttonGo = CreateUIObject("RestartButton", panel.transform);
            var buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(0f, -40f);
            buttonRect.sizeDelta = new Vector2(220f, 50f);
            var buttonImage = buttonGo.AddComponent<Image>();
            buttonImage.color = new Color(0.25f, 0.5f, 0.9f);
            var button = buttonGo.AddComponent<Button>();
            CreateText(buttonGo.transform, "Label", "もう一度", 22,
                anchorMin: Vector2.zero, anchorMax: Vector2.one,
                anchoredPos: Vector2.zero, size: Vector2.zero, alignment: TextAnchor.MiddleCenter);

            panel.SetActive(false);

            var gameOverUi = panel.AddComponent<GameOverUI>();
            SetPrivateField(gameOverUi, "panelRoot", panel);
            SetPrivateField(gameOverUi, "survivedTimeText", survivedText);
            SetPrivateField(gameOverUi, "restartButton", button);
        }

        // ---------------------------------------------------------------
        // UI helpers
        // ---------------------------------------------------------------

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Text CreateText(Transform parent, string name, string text, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, TextAnchor alignment)
        {
            var go = CreateUIObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var textComponent = go.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = fontSize;
            textComponent.alignment = alignment;
            textComponent.color = Color.white;
            return textComponent;
        }

        private static Slider CreateSlider(Transform parent, string name, Color fillColor,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            var go = CreateUIObject(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var background = CreateUIObject("Background", go.transform);
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            var fillArea = CreateUIObject("Fill Area", go.transform);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            var fill = CreateUIObject("Fill", fillArea.transform);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = fillColor;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = null;
            slider.targetGraphic = fillImage;
            slider.transition = Selectable.Transition.None;
            slider.interactable = false;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            return slider;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

            if (field == null)
            {
                Debug.LogError($"SceneBootstrapper: field '{fieldName}' not found on {target.GetType().Name}");
                return;
            }
            field.SetValue(target, value);
        }
    }
}
