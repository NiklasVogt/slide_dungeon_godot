using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeon2048.Core.Enemies
{
    public static class EnemyRegistry
    {
        private static readonly Dictionary<Entities.EnemyType, IEnemyArchetype> _map = new();

        public static void Register(IEnemyArchetype archetype) => _map[archetype.Type] = archetype;

        public static IEnemyArchetype Get(Entities.EnemyType t)
        {
            if (_map.TryGetValue(t, out var a)) return a;
            throw new InvalidOperationException($"No archetype for {t}");
        }

        public static Entities.EnemyType WeightedPick(IEnumerable<Entities.EnemyType> list, IList<int> weights, System.Random rng)
        {
            int total = weights.Sum();
            if (total <= 0) return list.First();
            int r = rng.Next(total);
            int i = 0;
            foreach (var t in list)
            {
                int w = weights[i++];
                if ((r -= w) < 0) return t;
            }
            return list.First();
        }

        public static Entities.Enemy SpawnOne(Services.GameContext ctx)
        {
            var avail = AvailableTypes(ctx).ToList();
            var weights = avail.Select(t => Get(t).CalcSpawnWeight(ctx)).ToList();
            var sel = WeightedPick(avail, weights, ctx.Rng);
            var level = Get(sel).CalcLevel(ctx);
            var pos = ctx.RandomFreeCell();
            return Get(sel).Create(pos.X, pos.Y, level);
        }

        public static IEnumerable<Entities.EnemyType> AvailableTypes(Services.GameContext ctx)
        {
            // kompatibel mit bisheriger Logik
            var s = new List<Entities.EnemyType> { Entities.EnemyType.Goblin };
            if (ctx.CurrentLevel >= 2 || ctx.TotalSwipes >= 20) s.Add(Entities.EnemyType.Orc);
            if (ctx.CurrentLevel >= 4 || ctx.TotalSwipes >= 50) s.Add(Entities.EnemyType.Dragon);
            return s;
        }
    }

    // Default archetypes mapping current balance
    public sealed class GoblinArch : IEnemyArchetype
    {
        public Entities.EnemyType Type => Entities.EnemyType.Goblin;
        public int CalcSpawnWeight(Services.GameContext ctx)
        {
            int diff = ctx.CurrentLevel + (int)(ctx.TotalSwipes / 15.0);
            return Math.Max(10, 50 - diff * 6);
        }
        public int CalcLevel(Services.GameContext ctx) => ctx.CalculateEnemyLevel();
        public Entities.Enemy Create(int x, int y, int level, bool boss=false) => new Entities.Enemy(x,y,Type,level,boss);
        public bool IsBossEligible(Services.GameContext ctx) => false;
    }

    public sealed class OrcArch : IEnemyArchetype
    {
        public Entities.EnemyType Type => Entities.EnemyType.Orc;
        public int CalcSpawnWeight(Services.GameContext ctx)
        {
            int diff = ctx.CurrentLevel + (int)(ctx.TotalSwipes / 15.0);
            return Math.Min(45, 15 + diff * 4);
        }
        public int CalcLevel(Services.GameContext ctx) => ctx.CalculateEnemyLevel();
        public Entities.Enemy Create(int x, int y, int level, bool boss=false) => new Entities.Enemy(x,y,Type,level,boss);
        public bool IsBossEligible(Services.GameContext ctx) => false;
    }

    public sealed class DragonArch : IEnemyArchetype
    {
        public Entities.EnemyType Type => Entities.EnemyType.Dragon;
        public int CalcSpawnWeight(Services.GameContext ctx)
        {
            int diff = ctx.CurrentLevel + (int)(ctx.TotalSwipes / 15.0);
            return Math.Min(35, diff * 2);
        }
        public int CalcLevel(Services.GameContext ctx) => ctx.CalculateEnemyLevel();
        public Entities.Enemy Create(int x, int y, int level, bool boss=false) => new Entities.Enemy(x,y,Type,level,boss);
        public bool IsBossEligible(Services.GameContext ctx) => true;
    }
}
