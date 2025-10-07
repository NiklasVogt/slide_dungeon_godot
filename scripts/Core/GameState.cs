using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Dungeon2048.Core
{
    public sealed partial class GameState : Node
    {
        public const int GridSize = 6;

        public Player Player = null!;
        public readonly List<Enemy> Enemies = new();
        public readonly List<Stone> Stones = new();
        public readonly List<SpellDrop> SpellDrops = new();
        public Door? Door;

        public int CurrentLevel = 1;
        public int TotalSwipes = 0;
        public int TotalEnemiesKilled = 0;
        public bool EnemiesFrozen = false;

        public LevelObjective CurrentObjective = null!;
        private bool _bossSpawned = false;
        private bool _bossKilled = false;
        public Random Rng = new();

        public override void _Ready()
        {
            Rng = new Random();
            Player = new Player(0,0);
            // Initialwerte konsistent setzen
            Player.MaxHp = Player.CalculatedMaxHp;
            Player.Hp = Player.MaxHp;
            Player.Atk = Player.CalculatedAtk;

            PlacePlayerRandomly();
            SpawnInitialStones();
            CurrentObjective = GenerateObjective();
        }

        public void RegisterSwipe()
        {
            TotalSwipes += 1;
            if (CurrentObjective.Type == LevelType.Survival || (CurrentObjective.Type == LevelType.Boss && !_bossSpawned))
            {
                CurrentObjective.Current += 1;
                if (CurrentObjective.Type == LevelType.Boss && CurrentObjective.Current >= CurrentObjective.Target && !_bossSpawned)
                    SpawnBoss();
            }
            CheckAndSpawnDoor();
        }

        public void SpawnEnemies()
        {
            int count = CalculateEnemySpawnCount();
            int elvl = CalculateEnemyLevel();
            for (int i=0;i<count;i++)
            {
                var c = RandomFreeCell();
                if (Rng.NextDouble() < 0.08) SpawnSpellDrop(c.X, c.Y);
                else Enemies.Add(new Enemy(c.X, c.Y, SelectEnemyType(), elvl, false));
            }
        }

        public void RegisterPlayerKill(Enemy e)
        {
            TotalEnemiesKilled++;
            if (CurrentObjective.Type == LevelType.Elimination) CurrentObjective.Current += 1;
            if (CurrentObjective.Type == LevelType.Boss && e.IsBoss) _bossKilled = true;
            CheckAndSpawnDoor();
        }

        public void RegisterEnemyKill(Enemy e)
        {
            TotalEnemiesKilled++;
            if (CurrentObjective.Type == LevelType.Boss && e.IsBoss) _bossKilled = true;
            CheckAndSpawnDoor();
        }

        public void RegisterSpellPickup(SpellDrop sd)
        {
            if (Player.AddSpell(sd.Spell))
                SpellDrops.Remove(sd);
        }

        public void InteractWithDoor()
        {
            if (Door == null || !Door.IsActive) return;
            NextLevel();
        }

        public void NextLevel()
        {
            CurrentLevel += 1;
            Enemies.Clear();
            Stones.Clear();
            SpellDrops.Clear();
            Door = null;
            EnemiesFrozen = false;
            _bossSpawned = false;
            _bossKilled = false;

            // Levelabhängig neu setzen
            Player.MaxHp = Player.CalculatedMaxHp;
            Player.Hp = Player.MaxHp;
            Player.Atk = Player.CalculatedAtk;

            PlacePlayerRandomly();
            SpawnInitialStones();
            CurrentObjective = GenerateObjective();
        }

        private void SpawnBoss()
        {
            if (_bossSpawned) return;
            var pos = RandomFreeCell();
            var boss = new Enemy(pos.X, pos.Y, EnemyType.Orc, CalcBossLevel(), true);
            Enemies.Add(boss);
            _bossSpawned = true;
        }

        private int CalcBossLevel() => Math.Max(1, (int)(CurrentLevel/3.0) + 1 + Rng.Next(0,2));

        private void CheckAndSpawnDoor()
        {
            if (Door != null && Door.IsActive) return;
            bool shouldSpawn = CurrentObjective.Type switch {
                LevelType.Survival or LevelType.Elimination => CurrentObjective.Current >= CurrentObjective.Target,
                LevelType.Boss => _bossKilled,
                _ => false
            };
            if (shouldSpawn) SpawnDoorAtEdge();
        }

        private void SpawnDoorAtEdge()
        {
            if (Door != null && Door.IsActive) return;
            var edges = new List<Vector2I>();
            for (int x=0;x<GridSize;x++){ edges.Add(new Vector2I(x,0)); edges.Add(new Vector2I(x,GridSize-1)); }
            for (int y=1;y<GridSize-1;y++){ edges.Add(new Vector2I(0,y)); edges.Add(new Vector2I(GridSize-1,y)); }
            var free = edges.Where(p => !IsOccupied(p.X, p.Y)).ToList();
            var pos = (free.Count>0 ? free[Rng.Next(free.Count)] : edges[Rng.Next(edges.Count)]);
            if (IsOccupied(pos.X, pos.Y)) ClearForDoor(pos.X, pos.Y);
            Door = new Door(pos.X, pos.Y) { IsActive = true };
        }

        private void ClearForDoor(int x, int y)
        {
            Stones.RemoveAll(s => s.X==x && s.Y==y);
            SpellDrops.RemoveAll(sd => sd.X==x && sd.Y==y);
            Enemies.RemoveAll(e => e.X==x && e.Y==y && !e.IsBoss);
            if (Player.X==x && Player.Y==y) RelocatePlayerAway();
        }

        private void RelocatePlayerAway()
        {
            var p = RandomFreeCell(ignorePlayer:true);
            Player.X = p.X; Player.Y = p.Y;
        }

        public bool IsObjectiveCompleted() => CurrentObjective.Type switch {
            LevelType.Survival or LevelType.Elimination => CurrentObjective.Current >= CurrentObjective.Target,
            LevelType.Boss => _bossKilled,
            _ => false
        };

        public double ObjectiveProgress() => CurrentObjective.Type switch {
            LevelType.Survival or LevelType.Elimination => CurrentObjective.Progress,
            LevelType.Boss => _bossKilled ? 1.0 : _bossSpawned ? 0.95 : CurrentObjective.Progress
        };

        public int CalculateEnemyLevel()
        {
            int baseLvl = Math.Max(1, (int)((CurrentLevel - 1)/2.0));
            int progressBonus = (int)(ObjectiveProgress() * 1.5 + 0.5);
            int typeBonus = CurrentObjective.Type == LevelType.Boss ? 1 : 0;
            return baseLvl + progressBonus + typeBonus;
        }

        public int CalculateEnemySpawnCount()
        {
            double doorMod = (Door != null && Door.IsActive) ? 0.3 : 1.0;
            return CurrentObjective.Type switch {
                LevelType.Survival => Math.Clamp((int)Math.Round((1 + (int)((CurrentLevel - 1)/4.0)) * doorMod), 0, 2),
                LevelType.Elimination => Math.Clamp((int)Math.Round((1 + (int)((CurrentLevel - 1)/3.0)) * doorMod), 0, 3),
                LevelType.Boss => Math.Clamp((int)Math.Round((1 + (int)((CurrentLevel - 1)/4.0)) * (_bossSpawned ? 0.8 : 1.0) * doorMod), 0, 2),
                _ => 1
            };
        }

        public EnemyType SelectEnemyType()
        {
            var avail = GetAvailableEnemyTypes();
            if (avail.Count == 1) return avail[0];
            int diff = CurrentLevel + (int)(TotalSwipes/15.0);
            var weights = new List<int>();
            foreach (var t in avail)
            {
                int baseW = t switch {
                    EnemyType.Goblin => Math.Max(10, 50 - diff * 6),
                    EnemyType.Orc => Math.Min(45, 15 + diff * 4),
                    EnemyType.Dragon => Math.Min(35, diff * 2),
                    _ => 10
                };
                weights.Add(baseW);
            }
            return WeightedChoice(avail, weights);
        }

        public List<EnemyType> GetAvailableEnemyTypes()
        {
            var a = new List<EnemyType>{ EnemyType.Goblin };
            if (CurrentLevel >= 2 || TotalSwipes >= 20) a.Add(EnemyType.Orc);
            if (CurrentLevel >= 4 || TotalSwipes >= 50) a.Add(EnemyType.Dragon);
            return a;
        }

        private T WeightedChoice<T>(IList<T> items, IList<int> weights)
        {
            int total = weights.Sum();
            if (total <= 0) return items[0];
            int r = Rng.Next(total);
            for (int i=0;i<items.Count;i++){ r -= weights[i]; if (r < 0) return items[i]; }
            return items[^1];
        }

        public void SpawnSpellDrop(int x, int y)
        {
            var s = Spell.CreateRandom(Player.Level, Rng);
            SpellDrops.Add(new SpellDrop(x,y,s));
        }

        public Vector2I RandomFreeCell(bool ignorePlayer=false)
        {
            for (int i=0;i<200;i++)
            {
                int x = Rng.Next(0, GridSize);
                int y = Rng.Next(0, GridSize);
                if (!IsOccupied(x, y, ignorePlayer)) return new Vector2I(x,y);
            }
            return new Vector2I(0,0);
        }

        public bool IsOccupied(int x, int y, bool ignorePlayer=false)
        {
            if (!ignorePlayer && Player.X==x && Player.Y==y) return true;
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
            for (int i=0;i<count;i++)
            {
                var c = RandomFreeCell();
                Stones.Add(new Stone(c.X, c.Y));
            }
        }

        public sealed class LevelObjective
        {
            public LevelType Type;
            public int Target;
            public int Current = 0;
            public LevelObjective(LevelType type, int target){ Type=type; Target=target; }
            public bool IsCompleted => Current >= Target;
            public double Progress => Math.Min(1.0, (double)Current / Target);
            public string Description => Type switch {
                LevelType.Survival => $"Überlebe {Target} Swipes",
                LevelType.Elimination => $"Töte {Target} Gegner",
                LevelType.Boss => $"Boss nach {Target} Swipes",
                _ => ""
            };
            public string ProgressText => Type switch {
                LevelType.Survival => $"{Current}/{Target} Swipes",
                LevelType.Elimination => $"{Current}/{Target} Gegner",
                LevelType.Boss => Current >= Target ? "Boss besiegen!" : $"{Current}/{Target} Swipes bis Boss",
                _ => ""
            };
            public string Icon => Type switch {
                LevelType.Survival => "⏱️",
                LevelType.Elimination => "⚔️",
                LevelType.Boss => "👑",
                _ => ""
            };
        }

        private LevelObjective GenerateObjective()
        {
            var weights = new Dictionary<LevelType,int> {
                { LevelType.Survival, 35 }, { LevelType.Elimination, 35 }, { LevelType.Boss, 30 }
            };
            var types = weights.Keys.ToList();
            var ws = types.Select(t => weights[t]).ToList();
            var tsel = WeightedChoice(types, ws);
            return new LevelObjective(tsel, CalcTarget(tsel));
        }

        private int CalcTarget(LevelType t) => t switch {
            LevelType.Survival or LevelType.Boss => 8 + CurrentLevel * 1 + Rng.Next(0,4),
            LevelType.Elimination => Math.Max(3, 2 + (int)(CurrentLevel/2.0) + Rng.Next(0,3)),
            _ => 5
        };
    }
}
