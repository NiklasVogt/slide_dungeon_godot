// scripts/Core/Services/GameContextRefactored.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Objectives;
using Dungeon2048.Core.Spells;
using Dungeon2048.Core.Enemies;
using Dungeon2048.Core.World;
using Dungeon2048.Core.Tiles;
using Dungeon2048.Core.Progression;
using Dungeon2048.Core.Managers;
using Dungeon2048.Core.Systems;
using Dungeon2048.Core.Boss;

namespace Dungeon2048.Core.Services
{
    /// <summary>
    /// Refactored GameContext using Facade Pattern
    /// Delegates to specialized managers while maintaining backward compatibility
    ///
    /// Architecture:
    /// - EntityManager: Manages all entity collections
    /// - TileManager: Manages all tile collections
    /// - StatusEffectManager: Coordinates status effect systems
    /// - BossStateManager: Manages boss state machines
    /// - LevelManager: Handles level progression and spawning
    ///
    /// Benefits:
    /// - Single Responsibility: Each manager has one clear purpose
    /// - Testability: Managers can be unit tested independently
    /// - Maintainability: Smaller, focused classes
    /// - Extensibility: Easy to add new systems
    /// </summary>
    public sealed class GameContextRefactored
    {
        public const int GridSize = 6;

        // Managers (Dependency Injection)
        private readonly EntityManager _entityManager;
        private readonly TileManager _tileManager;
        private readonly StatusEffectManager _statusEffectManager;
        private readonly BossStateManager _bossStateManager;
        private readonly LevelManager _levelManager;

        public Random Rng { get; private set; }

        // Backward Compatibility Properties (Facade)
        public Player Player
        {
            get => _entityManager.Player;
            set => _entityManager.Player = value;
        }

        public List<Enemy> Enemies => _entityManager.Enemies;
        public List<Stone> Stones => _entityManager.Stones;
        public List<SpellDrop> SpellDrops => _entityManager.SpellDrops;
        public List<Teleporter> Teleporters => _entityManager.Teleporters;
        public List<RuneTrap> RuneTraps => _entityManager.RuneTraps;
        public List<MagicBarrier> MagicBarriers => _entityManager.MagicBarriers;
        public Door? Door
        {
            get => _entityManager.Door;
            set => _entityManager.Door = value;
        }

        public List<Gravestone> Gravestones => _tileManager.Gravestones;
        public List<Torch> Torches => _tileManager.Torches;
        public List<BonePile> BonePiles => _tileManager.BonePiles;
        public List<FireTile> FireTiles => _tileManager.FireTiles;
        public List<FallingRock> FallingRocks => _tileManager.FallingRocks;
        public List<CampfireTile> Campfires => _tileManager.Campfires;

        public IObjective Objective => _levelManager.Objective;
        public int CurrentLevel => _levelManager.CurrentLevel;
        public int TotalSwipes => _levelManager.TotalSwipes;
        public int TotalEnemiesKilled => _levelManager.TotalEnemiesKilled;

        public bool EnemiesFrozen
        {
            get => _statusEffectManager.FreezeSystem.EnemiesFrozen;
            set => _statusEffectManager.FreezeSystem.EnemiesFrozen = value;
        }

        public int HexCurseTurnsRemaining => _statusEffectManager.HexCurseSystem.HexCurseTurnsRemaining;
        public bool IsHexCursed => _statusEffectManager.HexCurseSystem.IsHexCursed;

        public int TotalMovementTiles => _statusEffectManager.ColdSystem.TotalMovementTiles;
        public int FrostwindCounter => _statusEffectManager.ColdSystem.FrostwindCounter;
        public bool IsNightPhase => TotalSwipes >= 30;

        public BiomeSystem BiomeSystem => _levelManager.BiomeSystem;
        public SoulManager SoulManager => _levelManager.SoulManager;

        // Constructor with Dependency Injection
        public GameContextRefactored()
        {
            Rng = new Random();

            // Initialize Managers
            _entityManager = new EntityManager();
            _tileManager = new TileManager();
            _statusEffectManager = new StatusEffectManager();
            _bossStateManager = new BossStateManager(Rng);
            _levelManager = new LevelManager(Rng);

            // Initialize Player
            Player = new Player(0, 0);
            Player.MaxHp = Player.CalculatedMaxHp;
            Player.Hp = Player.MaxHp;
            Player.Atk = Player.CalculatedAtk;

            // Initialize BiomeSystem with proper context
            _levelManager.BiomeSystem = new BiomeSystem(this);
            _levelManager.BiomeSystem.UpdateBiome(CurrentLevel);

            PlacePlayerRandomly();
            SpawnInitialStones();
            _levelManager.Initialize();
        }

        /// <summary>
        /// Registers a swipe and processes all turn-based mechanics
        /// </summary>
        public void RegisterSwipe()
        {
            _levelManager.RegisterSwipe();

            // Boss mechanics
            _bossStateManager.UpdateBosses(_entityManager, _tileManager);

            // Check if boss should spawn
            if (_levelManager.ShouldSpawnBoss())
            {
                var boss = _levelManager.SpawnBoss(_entityManager);
                if (boss != null)
                {
                    _entityManager.AddEnemy(boss);
                    _bossStateManager.RegisterBoss(boss);
                }
            }

            // Tile mechanics
            ProcessTileEffects();

            // Status effects
            _statusEffectManager.ProcessEndOfTurnEffects(
                _entityManager,
                _tileManager,
                TotalSwipes,
                BiomeSystem.CurrentBiome?.Type
            );

            // Entity processing
            _entityManager.RegenerateMagicBarriers();
            _entityManager.ProcessTeleporters();
            UpdateMirrorKnights();

            // Bone pile revival
            var bonePilesToRevive = _tileManager.GetBonePilesToRevive();
            foreach (var pile in bonePilesToRevive)
            {
                _tileManager.RemoveBonePile(pile);
                ReviveSkeleton(pile.X, pile.Y);
            }

            // Door spawning
            if (_levelManager.ShouldSpawnDoor())
            {
                SpawnDoorAtEdge();
            }

            // Gargoyle update
            foreach (var gargoyle in Enemies.Where(e => e.Type == EnemyType.Gargoyle))
            {
                gargoyle.HasMoved = !gargoyle.HasMoved;
            }
        }

        /// <summary>
        /// Processes tile-related effects
        /// </summary>
        private void ProcessTileEffects()
        {
            _tileManager.AdvanceFallingRocks();
            _tileManager.ProcessFireTiles();
            _tileManager.EnsureSingleCampfire();

            // Falling rock spawning (Act 3)
            if (BiomeSystem.CurrentBiome?.Type == BiomeType.VolcanForge)
            {
                if (Rng.NextDouble() < 0.30) // 30% chance
                {
                    if (BiomeSystem.CurrentBiome is VolcanForgeBiome vfb)
                    {
                        vfb.SpawnFallingRock(this);
                    }
                }
            }
        }

        /// <summary>
        /// Processes falling rock damage (called after combat)
        /// </summary>
        public void ProcessFallingRockDamage()
        {
            var rocksToFall = _tileManager.GetFallingRocksReady();

            foreach (var rock in rocksToFall)
            {
                rock.Fall();

                // Damage Player
                if (Player.X == rock.X && Player.Y == rock.Y)
                {
                    Player.Hp -= FallingRock.FallDamage;
                    GD.Print($"💥 Fels fällt auf dich! {FallingRock.FallDamage} Schaden!");
                }

                // Damage Enemies
                var enemiesHit = Enemies.Where(e => e.X == rock.X && e.Y == rock.Y).ToList();
                foreach (var enemy in enemiesHit)
                {
                    enemy.Hp -= FallingRock.FallDamage;
                    GD.Print($"💥 Fels fällt auf {enemy.DisplayName}! {FallingRock.FallDamage} Schaden!");

                    if (enemy.Hp <= 0)
                    {
                        RegisterPlayerKill(enemy);
                        int xp = Player.CalculateXpReward(enemy.Type, enemy.EnemyLevel, enemy.IsBoss);
                        Player.GainExperience(xp);
                        GD.Print($"💎 +{xp} XP (Tod durch Felsen)");
                        Enemies.Remove(enemy);
                    }
                }

                _tileManager.RemoveFallingRock(rock);
            }
        }

        /// <summary>
        /// Processes cold damage (called after combat)
        /// </summary>
        public void ProcessColdDamage()
        {
            _statusEffectManager.ProcessColdDamage(_entityManager, BiomeSystem.CurrentBiome?.Type);
        }

        /// <summary>
        /// Spawns enemies for the current turn
        /// </summary>
        public void SpawnEnemies()
        {
            _levelManager.SpawnEnemies(_entityManager, Door?.IsActive ?? false);
        }

        /// <summary>
        /// Registers a player kill
        /// </summary>
        public void RegisterPlayerKill(Enemy e)
        {
            _levelManager.RegisterEnemyKill(e);

            // Cold system combat warmth
            _statusEffectManager.ColdSystem.ProcessCombatWarmth(Player, BiomeSystem.CurrentBiome?.Type);

            // Hex Witch curse
            if (e.Type == EnemyType.HexWitch)
            {
                _statusEffectManager.HexCurseSystem.ApplyHexCurse(5);
            }

            // Skeleton bone pile
            if (e.Type == EnemyType.Skeleton && Rng.NextDouble() < 0.3)
            {
                _tileManager.AddBonePile(e.X, e.Y);
                GD.Print($"💀 Skelett hinterlässt Knochenhaufen! (Revival in {BonePile.MaxSwipesAlive} Zügen)");
            }

            // Necrophage healing
            foreach (var necro in Enemies.Where(en => en.Type == EnemyType.Necrophage))
            {
                necro.Hp += 3;
                necro.HealedThisRound += 3;
                GD.Print($"Necrophage heilt sich um 3 HP! (jetzt {necro.Hp} HP)");
            }

            // Soul Leech
            if (e.Type == EnemyType.SoulLeech)
            {
                Player.Atk = System.Math.Max(1, Player.Atk - 1);
                GD.Print($"💀 Soul Leech saugt deine Kraft! ATK: {Player.Atk}");
            }

            // Pyromaniac explosion
            if (e.Type == EnemyType.Pyromaniac)
            {
                HandlePyromaniacExplosion(e.X, e.Y);
            }

            // Boss kill tracking
            if (e.IsBoss && Objective is BossObjective bo)
            {
                bo.BossKilled = true;
                _bossStateManager.UnregisterBoss(e.Id);
            }
        }

        /// <summary>
        /// Registers an enemy kill (not by player)
        /// </summary>
        public void RegisterEnemyKill(Enemy e)
        {
            _levelManager.Objective.OnKillEnemy(e);

            // Skeleton bones
            if (e.Type == EnemyType.Skeleton && Rng.NextDouble() < 0.3)
            {
                _tileManager.AddBonePile(e.X, e.Y);
            }

            // Necrophage
            foreach (var necro in Enemies.Where(en => en.Type == EnemyType.Necrophage))
            {
                necro.Hp += 3;
                necro.HealedThisRound += 3;
            }

            // Pyromaniac
            if (e.Type == EnemyType.Pyromaniac)
            {
                HandlePyromaniacExplosion(e.X, e.Y);
            }
        }

        /// <summary>
        /// Handles spell pickup
        /// </summary>
        public void RegisterSpellPickup(SpellDrop drop)
        {
            if (Player.AddSpell(drop.Spell))
            {
                _entityManager.RemoveSpellDrop(drop);
            }
        }

        /// <summary>
        /// Interacts with door and advances to next level
        /// </summary>
        public void InteractWithDoor()
        {
            if (Door == null || !Door.IsActive) return;

            BiomeSystem.OnLevelComplete();
            _levelManager.AdvanceLevel();

            // Clear all state
            _entityManager.ClearAllEntities();
            _tileManager.ClearAllTiles();
            _statusEffectManager.ClearAllEffects();
            _bossStateManager.ClearAllBossStates();

            Player.Hp = Player.MaxHp;

            SpawnInitialStones();
        }

        /// <summary>
        /// Checks if position is occupied
        /// </summary>
        public bool IsOccupied(int x, int y, bool ignorePlayer = false)
        {
            return _entityManager.IsOccupiedByEntity(x, y, ignorePlayer) ||
                   _tileManager.IsOccupiedByTile(x, y);
        }

        /// <summary>
        /// Finds a random free cell
        /// </summary>
        public (int X, int Y) RandomFreeCell(bool ignorePlayer = false)
        {
            var cells = new List<(int X, int Y)>();
            for (int x = 0; x < GridSize; x++)
                for (int y = 0; y < GridSize; y++)
                    cells.Add((x, y));
            var free = cells.Where(p => !IsOccupied(p.X, p.Y, ignorePlayer)).ToList();
            if (free.Count == 0) return (0, 0);
            return free[Rng.Next(free.Count)];
        }

        /// <summary>
        /// Calculates enemy level
        /// </summary>
        public int CalculateEnemyLevel()
        {
            return _levelManager.CalculateEnemyLevel();
        }

        /// <summary>
        /// Calculates enemy spawn count
        /// </summary>
        public int CalculateEnemySpawnCount()
        {
            return _levelManager.CalculateEnemySpawnCount();
        }

        // Private helper methods

        private void PlacePlayerRandomly()
        {
            var p = RandomFreeCell(ignorePlayer: true);
            Player.X = p.X;
            Player.Y = p.Y;
        }

        private void SpawnInitialStones()
        {
            int count = Rng.Next(1, 4);
            for (int i = 0; i < count; i++)
            {
                var p = RandomFreeCell();
                Stones.Add(new Stone(p.X, p.Y));
            }
        }

        private void SpawnDoorAtEdge()
        {
            if (Door != null && Door.IsActive) return;

            var edges = new List<(int X, int Y)>();
            for (int x = 0; x < GridSize; x++)
            {
                edges.Add((x, 0));
                edges.Add((x, GridSize - 1));
            }
            for (int y = 0; y < GridSize; y++)
            {
                edges.Add((0, y));
                edges.Add((GridSize - 1, y));
            }

            var free = edges.Where(p => !IsOccupied(p.X, p.Y)).ToList();
            var pos = (free.Count > 0 ? free[Rng.Next(free.Count)] : edges[Rng.Next(edges.Count)]);

            // Clear position
            Stones.RemoveAll(s => s.X == pos.X && s.Y == pos.Y);
            Gravestones.RemoveAll(g => g.X == pos.X && g.Y == pos.Y);
            BonePiles.RemoveAll(b => b.X == pos.X && b.Y == pos.Y);
            SpellDrops.RemoveAll(sd => sd.X == pos.X && sd.Y == pos.Y);
            Enemies.RemoveAll(e => e.X == pos.X && e.Y == pos.Y && !e.IsBoss);

            if (Player.X == pos.X && Player.Y == pos.Y)
            {
                var alt = RandomFreeCell(ignorePlayer: true);
                Player.X = alt.X;
                Player.Y = alt.Y;
            }

            Door = new Door(pos.X, pos.Y) { IsActive = true };
        }

        private void ReviveSkeleton(int x, int y)
        {
            var level = CalculateEnemyLevel();
            var skeleton = EnemyRegistry.Get(EnemyType.Skeleton).Create(x, y, level);

            var biome = BiomeSystem.CurrentBiome;
            skeleton.Hp = (int)(skeleton.Hp * biome.EnemyHealthMultiplier);
            skeleton.Atk = (int)(skeleton.Atk * biome.EnemyDamageMultiplier);

            Enemies.Add(skeleton);
            GD.Print($"💀 Knochenhaufen erwacht als Skelett! 💀");
        }

        private void UpdateMirrorKnights()
        {
            foreach (var mirror in Enemies.Where(e => e.Type == EnemyType.MirrorKnight))
            {
                mirror.SyncMirrorKnightStats(Player);
            }
        }

        private void HandlePyromaniacExplosion(int x, int y)
        {
            const int explosionDamage = 10;
            GD.Print($"💥 PYROMANIAC EXPLODIERT! ({x},{y})");

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int targetX = x + dx;
                    int targetY = y + dy;

                    if (targetX < 0 || targetX >= GridSize || targetY < 0 || targetY >= GridSize)
                        continue;

                    // Player damage
                    if (Player.X == targetX && Player.Y == targetY)
                    {
                        Player.Hp -= explosionDamage;
                        GD.Print($"💥 Explosion trifft Spieler! {explosionDamage} Schaden!");
                    }

                    // Enemy damage
                    var enemiesHit = Enemies.Where(e => e.X == targetX && e.Y == targetY).ToList();
                    foreach (var enemy in enemiesHit)
                    {
                        enemy.Hp -= explosionDamage;
                        GD.Print($"💥 Explosion trifft {enemy.DisplayName}! {explosionDamage} Schaden!");

                        if (enemy.Hp <= 0)
                        {
                            RegisterEnemyKill(enemy);
                            Enemies.Remove(enemy);
                        }
                    }
                }
            }

            _tileManager.AddFireTile(x, y);
            GD.Print($"🔥 Explosion hinterlässt Feuer bei ({x},{y})");
        }

        // Boss-specific methods (delegated to boss state manager)
        public void HandleFireGiantMechanics()
        {
            // Now handled by BossStateManager
        }

        public void HandleIceDragonMechanics()
        {
            // Now handled by BossStateManager
        }
    }
}
