// scripts/Core/Managers/LevelManager.cs
using System;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Enemies;
using Dungeon2048.Core.Objectives;
using Dungeon2048.Core.Progression;
using Dungeon2048.Core.Spells;
using Dungeon2048.Core.World;
using Godot;

namespace Dungeon2048.Core.Managers
{
    /// <summary>
    /// Manages level progression, enemy spawning, and objectives
    /// Follows Single Responsibility Principle - only responsible for level lifecycle
    /// </summary>
    public sealed class LevelManager
    {
        private readonly Random _rng;

        public int CurrentLevel { get; private set; } = 1;
        public int TotalSwipes { get; private set; } = 0;
        public int TotalEnemiesKilled { get; private set; } = 0;
        public IObjective Objective { get; private set; } = null!;

        public BiomeSystem BiomeSystem { get; private set; }
        public SoulManager SoulManager { get; private set; }

        public LevelManager(Random rng)
        {
            _rng = rng;
            BiomeSystem = new BiomeSystem(null!); // Will be initialized properly
            SoulManager = new SoulManager();
            SoulManager.ResetRunSouls();
        }

        /// <summary>
        /// Initializes the level manager with starting objective
        /// </summary>
        public void Initialize()
        {
            Objective = ObjectiveService.Generate(_rng, CurrentLevel);
        }

        /// <summary>
        /// Registers a swipe and updates objective
        /// </summary>
        public void RegisterSwipe()
        {
            TotalSwipes++;
            Objective.OnSwipe();
        }

        /// <summary>
        /// Registers an enemy kill and updates objective
        /// </summary>
        public void RegisterEnemyKill(Enemy enemy)
        {
            TotalEnemiesKilled++;
            Objective.OnKillEnemy(enemy);

            // Soul reward
            int souls = SoulCurrency.GetSoulReward(enemy.Type, enemy.EnemyLevel, enemy.IsBoss);
            SoulManager.AddSouls(souls);
        }

        /// <summary>
        /// Advances to the next level
        /// </summary>
        public void AdvanceLevel()
        {
            // Soul bonus for level completion
            int levelBonus = SoulCurrency.GetLevelCompletionBonus(CurrentLevel);
            int objectiveBonus = SoulCurrency.GetObjectiveCompletionBonus(Objective.Type);
            int totalBonus = levelBonus + objectiveBonus;

            SoulManager.AddSouls(totalBonus);
            GD.Print($"✨ Level {CurrentLevel} abgeschlossen! Bonus: {totalBonus} Seelen");

            CurrentLevel++;
            TotalSwipes = 0;

            BiomeSystem.UpdateBiome(CurrentLevel);
            Objective = ObjectiveService.Generate(_rng, CurrentLevel);
        }

        /// <summary>
        /// Checks if door should spawn based on objective
        /// </summary>
        public bool ShouldSpawnDoor()
        {
            return Objective.Type switch
            {
                LevelType.Survival or LevelType.Elimination => Objective.IsCompleted,
                LevelType.Boss => (Objective as BossObjective)?.BossKilled ?? false,
                _ => false
            };
        }

        /// <summary>
        /// Checks if boss should spawn based on objective
        /// </summary>
        public bool ShouldSpawnBoss()
        {
            if (Objective is BossObjective bo && !bo.BossSpawned && bo.Current >= bo.Target)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Spawns a boss for the current level
        /// </summary>
        public Enemy? SpawnBoss(EntityManager entityManager)
        {
            var biome = BiomeSystem.CurrentBiome;

            // Check if current level is a boss level for this biome
            if (biome.HasBoss(CurrentLevel))
            {
                var bossType = biome.GetBossType();
                var archetype = EnemyRegistry.Get(bossType);
                var pos = FindFreePosition(entityManager);
                var lvl = archetype.CalcLevel(null!) + 2; // TODO: Pass proper context
                var boss = archetype.Create(pos.X, pos.Y, lvl, true);

                if (Objective is BossObjective bo)
                {
                    bo.BossSpawned = true;
                }

                GD.Print($"🔥 ACT BOSS SPAWNED: {boss.DisplayName}! 🔥");
                return boss;
            }

            return null;
        }

        /// <summary>
        /// Spawns enemies for the current turn
        /// </summary>
        public void SpawnEnemies(EntityManager entityManager, bool doorActive)
        {
            int count = CalculateEnemySpawnCount(doorActive);
            var biome = BiomeSystem.CurrentBiome;

            for (int i = 0; i < count; i++)
            {
                var enemy = SpawnBiomeEnemy(biome, entityManager);
                if (enemy != null)
                {
                    entityManager.AddEnemy(enemy);

                    // FEATURE: Goblin spawns always in pairs
                    if (enemy.Type == EnemyType.Goblin)
                    {
                        var pos2 = FindFreePosition(entityManager);
                        var goblin2 = EnemyRegistry.Get(EnemyType.Goblin).Create(pos2.X, pos2.Y, enemy.EnemyLevel);
                        goblin2.Hp = (int)(goblin2.Hp * biome.EnemyHealthMultiplier);
                        goblin2.Atk = (int)(goblin2.Atk * biome.EnemyDamageMultiplier);
                        entityManager.AddEnemy(goblin2);
                        GD.Print("Goblin-Paar spawnt!");
                    }
                }
            }

            // Spell Drop
            if (_rng.NextDouble() < 0.08)
            {
                var pos = FindFreePosition(entityManager);
                entityManager.SpellDrops.Add(new SpellDrop(pos.X, pos.Y, SpellFactory.CreateRandom(entityManager.Player.Level, _rng)));
            }
        }

        /// <summary>
        /// Calculates enemy level based on current progress
        /// </summary>
        public int CalculateEnemyLevel()
        {
            int baseLvl = System.Math.Max(1, (int)((CurrentLevel - 1) / 3.0));
            int progressBonus = (int)(Objective.Progress * 1.0 + 0.5);
            int typeBonus = Objective.Type == LevelType.Boss ? 1 : 0;
            return baseLvl + progressBonus + typeBonus;
        }

        private Enemy? SpawnBiomeEnemy(IBiome biome, EntityManager entityManager)
        {
            // Determine if standard or rare
            bool isRare = _rng.NextDouble() < 0.15; // 15% rare chance

            var pool = isRare ? biome.RareEnemies : biome.StandardEnemies;
            if (pool.Count == 0) return null;

            var selectedType = pool[_rng.Next(pool.Count)];
            var archetype = EnemyRegistry.Get(selectedType);

            int level = CalculateEnemyLevel();
            var pos = FindFreePosition(entityManager);

            var enemy = archetype.Create(pos.X, pos.Y, level);

            // Apply biome modifiers
            enemy.Hp = (int)(enemy.Hp * biome.EnemyHealthMultiplier);
            enemy.Atk = (int)(enemy.Atk * biome.EnemyDamageMultiplier);

            return enemy;
        }

        private int CalculateEnemySpawnCount(bool doorActive)
        {
            double doorMod = doorActive ? 0.3 : 1.0;
            var biome = BiomeSystem.CurrentBiome;
            double biomeMod = biome?.SpawnRateMultiplier ?? 1.0;

            // Always spawn only 1 enemy
            int baseCount = Objective.Type switch
            {
                LevelType.Survival => (int)System.Math.Round(1 * doorMod * biomeMod),
                LevelType.Elimination => (int)System.Math.Round(1 * doorMod * biomeMod),
                LevelType.Boss => (int)System.Math.Round(1 * doorMod * biomeMod),
                _ => 1
            };

            return System.Math.Clamp(baseCount, 0, 1);
        }

        private (int X, int Y) FindFreePosition(EntityManager entityManager)
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                int x = _rng.Next(0, 6);
                int y = _rng.Next(0, 6);

                if (!entityManager.IsOccupiedByEntity(x, y, false))
                {
                    return (x, y);
                }
            }
            return (0, 0); // Fallback
        }
    }
}
