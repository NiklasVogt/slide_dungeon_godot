using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Objectives;
using Dungeon2048.Core.Spells;
using Dungeon2048.Core.Enemies; // <- Wichtig: für EnemyRegistry

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

        public IObjective Objective = null!;
        public int CurrentLevel = 1;
        public int TotalSwipes = 0;
        public int TotalEnemiesKilled = 0;
        public bool EnemiesFrozen = false;

        public Random Rng = new();

        public GameContext()
        {
            Rng = new Random();
            Player = new Player(0,0);
            Player.MaxHp = Player.CalculatedMaxHp;
            Player.Hp = Player.MaxHp;
            Player.Atk = Player.CalculatedAtk;
            PlacePlayerRandomly();
            SpawnInitialStones();
            Objective = ObjectiveService.Generate(Rng, CurrentLevel);
        }

        public void RegisterSwipe()
        {
            TotalSwipes += 1;
            Objective.OnSwipe();
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
            for (int i=0; i<count; i++)
            {
                // Fix: EnemyRegistry direkt verwenden
                var e = EnemyRegistry.SpawnOne(this);
                Enemies.Add(e);
            }
            // 8% SpellDrop Chance
            if (Rng.NextDouble() < 0.08)
            {
                var pos = RandomFreeCell();
                SpellDrops.Add(new SpellDrop(pos.X, pos.Y, SpellFactory.CreateRandom(Player.Level, Rng)));
            }
        }

        public void RegisterPlayerKill(Enemy e)
        {
            TotalEnemiesKilled += 1;
            Objective.OnKillEnemy(e);
            if (e.IsBoss && Objective is BossObjective bo)
                bo.BossKilled = true;
        }

        public void RegisterEnemyKill(Enemy e)
        {
            Objective.OnKillEnemy(e);
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
            CurrentLevel += 1;
            TotalSwipes = 0;
            Enemies.Clear();
            Stones.Clear();
            SpellDrops.Clear();
            Door = null;

            Player.Hp = Player.MaxHp;
            Objective = ObjectiveService.Generate(Rng, CurrentLevel);
            SpawnInitialStones();
        }

        public (int X, int Y) RandomFreeCell(bool ignorePlayer=false)
        {
            var cells = new List<(int X,int Y)>();
            for (int x=0; x<GridSize; x++)
                for (int y=0; y<GridSize; y++)
                    cells.Add((x,y));
            var free = cells.Where(p => !IsOccupied(p.X,p.Y, ignorePlayer)).ToList();
            if (free.Count == 0) return (0,0);
            return free[Rng.Next(free.Count)];
        }

        public bool IsOccupied(int x, int y, bool ignorePlayer=false)
        {
            if (!ignorePlayer && Player.X == x && Player.Y == y) return true;
            if (Enemies.Any(e => e.X==x && e.Y==y)) return true;
            if (Stones.Any(s => s.X==x && s.Y==y)) return true;
            if (SpellDrops.Any(sd => sd.X==x && sd.Y==y)) return true;
            if (Door != null && Door.X==x && Door.Y==y) return true;
            return false;
        }

        private void PlacePlayerRandomly()
        {
            var p = RandomFreeCell(ignorePlayer:true);
            Player.X = p.X; Player.Y = p.Y;
        }

        private void SpawnInitialStones()
        {
            int count = Rng.Next(1,5);
            for (int i=0; i<count; i++)
            {
                var p = RandomFreeCell();
                Stones.Add(new Stone(p.X, p.Y));
            }
        }

        private void CheckAndSpawnDoor()
        {
            if (Door != null && Door.IsActive) return;
            bool shouldSpawn = Objective.Type switch {
                LevelType.Survival or LevelType.Elimination => Objective.IsCompleted,
                LevelType.Boss => (Objective as BossObjective)?.BossKilled ?? false,
                _ => false
            };
            if (shouldSpawn) SpawnDoorAtEdge();
        }

        private void SpawnDoorAtEdge()
        {
            if (Door != null && Door.IsActive) return;
            var edges = new System.Collections.Generic.List<(int X,int Y)>();
            for (int x=0; x<GridSize; x++) { edges.Add((x,0)); edges.Add((x,GridSize-1)); }
            for (int y=0; y<GridSize; y++) { edges.Add((0,y)); edges.Add((GridSize-1,y)); }
            var free = edges.Where(p => !IsOccupied(p.X,p.Y)).ToList();
            var pos = (free.Count>0 ? free[Rng.Next(free.Count)] : edges[Rng.Next(edges.Count)]);
            Stones.RemoveAll(s => s.X==pos.X && s.Y==pos.Y);
            SpellDrops.RemoveAll(sd => sd.X==pos.X && sd.Y==pos.Y);
            Enemies.RemoveAll(e => e.X==pos.X && e.Y==pos.Y && !e.IsBoss);
            if (Player.X==pos.X && Player.Y==pos.Y)
            {
                var alt = RandomFreeCell(ignorePlayer:true);
                Player.X = alt.X; Player.Y = alt.Y;
            }
            Door = new Door(pos.X, pos.Y) { IsActive = true };
        }

        public int CalculateEnemyLevel()
        {
            int baseLvl = System.Math.Max(1, (int)((CurrentLevel - 1)/2.0));
            int progressBonus = (int)(Objective.Progress * 1.5 + 0.5);
            int typeBonus = Objective.Type == LevelType.Boss ? 1 : 0;
            return baseLvl + progressBonus + typeBonus;
        }

        public int CalculateEnemySpawnCount()
        {
            double doorMod = (Door != null && Door.IsActive) ? 0.3 : 1.0;
            return Objective.Type switch {
                LevelType.Survival => System.Math.Clamp((int)System.Math.Round((1 + (int)((CurrentLevel - 1)/4.0)) * doorMod), 0, 2),
                LevelType.Elimination => System.Math.Clamp((int)System.Math.Round((1 + (int)((CurrentLevel - 1)/3.0)) * doorMod), 0, 3),
                LevelType.Boss => System.Math.Clamp((int)System.Math.Round((1 + (int)((CurrentLevel - 1)/4.0)) * doorMod), 0, 2),
                _ => 1
            };
        }

        private void SpawnBoss()
        {
            var pos = RandomFreeCell();
            var lvl = CalculateEnemyLevel() + 1;
            Enemies.Add(new Enemy(pos.X, pos.Y, EnemyType.Boss, lvl, true));
        }
    }
}
