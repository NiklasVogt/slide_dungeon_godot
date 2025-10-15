// scripts/Core/Services/GameContext.cs - ERWEITERT
using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Objectives;
using Dungeon2048.Core.Spells;
using Dungeon2048.Core.Enemies;
using Dungeon2048.Core.World;
using Dungeon2048.Core.Tiles;

namespace Dungeon2048.Core.Services
{
    public sealed class GameContext
    {
        public const int GridSize = 6;

        public Player Player = null!;
        public readonly List<Enemy> Enemies = new();
        public readonly List<Stone> Stones = new();
        public readonly List<SpellDrop> SpellDrops = new();
        public Door? Door;
        
        // Neue Tile-Listen für Akt 1
        public readonly List<Gravestone> Gravestones = new();
        public readonly List<Torch> Torches = new();
        public readonly List<BonePile> BonePiles = new();

        public IObjective Objective = null!;
        public int CurrentLevel = 1;
        public int TotalSwipes = 0;
        public int TotalEnemiesKilled = 0;
        public bool EnemiesFrozen = false;
        
        // Boss-State
        public int GoblinKingSpawnCounter = 0; // Für Boss-Mechanik

        public Random Rng = new();
        
        // Biome System
        public BiomeSystem BiomeSystem { get; private set; }

        public GameContext()
        {
            Rng = new Random();
            Player = new Player(0, 0);
            Player.MaxHp = Player.CalculatedMaxHp;
            Player.Hp = Player.MaxHp;
            Player.Atk = Player.CalculatedAtk;
            
            BiomeSystem = new BiomeSystem(this);
            BiomeSystem.UpdateBiome(CurrentLevel);
            
            PlacePlayerRandomly();
            SpawnInitialStones();
            Objective = ObjectiveService.Generate(Rng, CurrentLevel);
        }

        public void RegisterSwipe()
        {
            TotalSwipes += 1;
            Objective.OnSwipe();
            
            // Boss-Mechanik: Goblin King spawnt Adds
            if (Enemies.Any(e => e.Type == EnemyType.GoblinKing))
            {
                GoblinKingSpawnCounter++;
                if (GoblinKingSpawnCounter >= 3)
                {
                    SpawnGoblinKingAdds();
                    GoblinKingSpawnCounter = 0;
                }
            }
            
            if (Objective is BossObjective bo && !bo.BossSpawned && bo.Current >= bo.Target)
            {
                SpawnBoss();
                bo.BossSpawned = true;
            }
            CheckAndSpawnDoor();
        }

        public void SpawnEnemies()
        {
            int count = CalculateEnemySpawnCount();
            var biome = BiomeSystem.CurrentBiome;
            
            for (int i = 0; i < count; i++)
            {
                var e = SpawnBiomeEnemy(biome);
                if (e != null) Enemies.Add(e);
            }
            
            // Spell Drop
            if (Rng.NextDouble() < 0.08)
            {
                var pos = RandomFreeCell();
                SpellDrops.Add(new SpellDrop(pos.X, pos.Y, SpellFactory.CreateRandom(Player.Level, Rng)));
            }
        }

        private Enemy SpawnBiomeEnemy(IBiome biome)
        {
            // Bestimme ob Standard oder Rare
            bool isRare = Rng.NextDouble() < 0.15; // 15% Rare Chance
            
            var pool = isRare ? biome.RareEnemies : biome.StandardEnemies;
            if (pool.Count == 0) return null;
            
            var selectedType = pool[Rng.Next(pool.Count)];
            var archetype = EnemyRegistry.Get(selectedType);
            
            int level = archetype.CalcLevel(this);
            var pos = RandomFreeCell();
            
            var enemy = archetype.Create(pos.X, pos.Y, level);
            
            // Biome Modifiers anwenden
            enemy.Hp = (int)(enemy.Hp * biome.EnemyHealthMultiplier);
            enemy.Atk = (int)(enemy.Atk * biome.EnemyDamageMultiplier);
            
            return enemy;
        }

        private void SpawnGoblinKingAdds()
        {
            // Goblin King spawnt 2 Goblins
            for (int i = 0; i < 2; i++)
            {
                var pos = RandomFreeCell();
                var goblin = EnemyRegistry.Get(EnemyType.Goblin).Create(pos.X, pos.Y, CalculateEnemyLevel());
                Enemies.Add(goblin);
            }
            Godot.GD.Print("Goblin-König ruft Verstärkung!");
        }

        public void RegisterPlayerKill(Enemy e)
        {
            TotalEnemiesKilled += 1;
            Objective.OnKillEnemy(e);
            
            // Skelett: 30% Chance auf Bone Pile
            if (e.Type == EnemyType.Skeleton && Rng.NextDouble() < 0.3)
            {
                BonePiles.Add(new BonePile(e.X, e.Y));
                Godot.GD.Print("Skelett hinterlässt Knochen!");
            }
            
            // Necrophage Healing
            foreach (var necro in Enemies.Where(en => en.Type == EnemyType.Necrophage))
            {
                necro.Hp += 3;
                necro.HealedThisRound += 3;
                Godot.GD.Print($"Necrophage heilt sich um 3 HP! (jetzt {necro.Hp} HP)");
            }
            
            if (e.IsBoss && Objective is BossObjective bo)
                bo.BossKilled = true;
        }

        public void RegisterEnemyKill(Enemy e)
        {
            Objective.OnKillEnemy(e);
            
            // Skelett Bones
            if (e.Type == EnemyType.Skeleton && Rng.NextDouble() < 0.3)
            {
                BonePiles.Add(new BonePile(e.X, e.Y));
            }
            
            // Necrophage
            foreach (var necro in Enemies.Where(en => en.Type == EnemyType.Necrophage))
            {
                necro.Hp += 3;
                necro.HealedThisRound += 3;
            }
        }

        public void RegisterSpellPickup(SpellDrop drop)
        {
            if (Player.AddSpell(drop.Spell))
            {
                SpellDrops.Remove(drop);
            }
        }

        public void InteractWithDoor()
        {
            if (Door == null || !Door.IsActive) return;
            
            BiomeSystem.OnLevelComplete();
            
            CurrentLevel += 1;
            TotalSwipes = 0;
            GoblinKingSpawnCounter = 0;
            
            Enemies.Clear();
            Stones.Clear();
            SpellDrops.Clear();
            Gravestones.Clear();
            Torches.Clear();
            BonePiles.Clear();
            Door = null;

            Player.Hp = Player.MaxHp;
            
            BiomeSystem.UpdateBiome(CurrentLevel);
            Objective = ObjectiveService.Generate(Rng, CurrentLevel);
            SpawnInitialStones();
        }

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
        public bool IsOccupied(int x, int y, bool ignorePlayer = false)
        {
            if (!ignorePlayer && Player.X == x && Player.Y == y) return true;
            if (Enemies.Any(e => e.X == x && e.Y == y)) return true;
            if (Stones.Any(s => s.X == x && s.Y == y)) return true;
            if (SpellDrops.Any(sd => sd.X == x && sd.Y == y)) return true;
            if (Gravestones.Any(g => g.X == x && g.Y == y)) return true;
            if (Torches.Any(t => t.X == x && t.Y == y)) return true;
            if (BonePiles.Any(b => b.X == x && b.Y == y)) return true;
            if (Door != null && Door.X == x && Door.Y == y) return true;
            return false;
        }

        private void PlacePlayerRandomly()
        {
            var p = RandomFreeCell(ignorePlayer: true);
            Player.X = p.X; Player.Y = p.Y;
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

        private void CheckAndSpawnDoor()
        {
            if (Door != null && Door.IsActive) return;
            bool shouldSpawn = Objective.Type switch
            {
                LevelType.Survival or LevelType.Elimination => Objective.IsCompleted,
                LevelType.Boss => (Objective as BossObjective)?.BossKilled ?? false,
                _ => false
            };
            if (shouldSpawn) SpawnDoorAtEdge();
        }

        private void SpawnDoorAtEdge()
        {
            if (Door != null && Door.IsActive) return;
            var edges = new List<(int X, int Y)>();
            for (int x = 0; x < GridSize; x++) { edges.Add((x, 0)); edges.Add((x, GridSize - 1)); }
            for (int y = 0; y < GridSize; y++) { edges.Add((0, y)); edges.Add((GridSize - 1, y)); }
            var free = edges.Where(p => !IsOccupied(p.X, p.Y)).ToList();
            var pos = (free.Count > 0 ? free[Rng.Next(free.Count)] : edges[Rng.Next(edges.Count)]);
            
            Stones.RemoveAll(s => s.X == pos.X && s.Y == pos.Y);
            Gravestones.RemoveAll(g => g.X == pos.X && g.Y == pos.Y);
            BonePiles.RemoveAll(b => b.X == pos.X && b.Y == pos.Y);
            SpellDrops.RemoveAll(sd => sd.X == pos.X && sd.Y == pos.Y);
            Enemies.RemoveAll(e => e.X == pos.X && e.Y == pos.Y && !e.IsBoss);
            
            if (Player.X == pos.X && Player.Y == pos.Y)
            {
                var alt = RandomFreeCell(ignorePlayer: true);
                Player.X = alt.X; Player.Y = alt.Y;
            }
            Door = new Door(pos.X, pos.Y) { IsActive = true };
        }

        public int CalculateEnemyLevel()
        {
            int baseLvl = System.Math.Max(1, (int)((CurrentLevel - 1) / 2.0));
            int progressBonus = (int)(Objective.Progress * 1.5 + 0.5);
            int typeBonus = Objective.Type == LevelType.Boss ? 1 : 0;
            return baseLvl + progressBonus + typeBonus;
        }

        public int CalculateEnemySpawnCount()
        {
            double doorMod = (Door != null && Door.IsActive) ? 0.3 : 1.0;
            var biome = BiomeSystem.CurrentBiome;
            double biomeMod = biome?.SpawnRateMultiplier ?? 1.0;
            
            int baseCount = Objective.Type switch
            {
                LevelType.Survival => System.Math.Clamp((int)System.Math.Round((1 + (int)((CurrentLevel - 1) / 4.0)) * doorMod * biomeMod), 0, 2),
                LevelType.Elimination => System.Math.Clamp((int)System.Math.Round((1 + (int)((CurrentLevel - 1) / 3.0)) * doorMod * biomeMod), 0, 3),
                LevelType.Boss => System.Math.Clamp((int)System.Math.Round((1 + (int)((CurrentLevel - 1) / 4.0)) * doorMod * biomeMod), 0, 2),
                _ => 1
            };
            
            return baseCount;
        }

        private void SpawnBoss()
        {
            var biome = BiomeSystem.CurrentBiome;
            
            if (biome.HasBoss(CurrentLevel))
            {
                var bossType = biome.GetBossType();
                var archetype = EnemyRegistry.Get(bossType);
                var pos = RandomFreeCell();
                var lvl = archetype.CalcLevel(this) + 2;
                var boss = archetype.Create(pos.X, pos.Y, lvl, true);
                Enemies.Add(boss);
                Godot.GD.Print($"Boss spawned: {boss.DisplayName}!");
            }
            else
            {
                // Fallback: Standard Boss
                var pos = RandomFreeCell();
                var lvl = CalculateEnemyLevel() + 1;
                Enemies.Add(new Enemy(pos.X, pos.Y, EnemyType.Boss, lvl, true));
            }
        }
    }
}