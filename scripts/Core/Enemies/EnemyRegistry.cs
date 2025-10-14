using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Services;

namespace Dungeon2048.Core.Enemies
{
    public static class EnemyRegistry
    {
        private static readonly Dictionary<EnemyType, IEnemyArchetype> map = new();

        public static void Register(IEnemyArchetype archetype) => map[archetype.Type] = archetype;

        public static IEnemyArchetype Get(EnemyType t)
        {
            if (map.TryGetValue(t, out var a)) return a;
            throw new InvalidOperationException($"No archetype for {t}");
        }

        public static IEnumerable<EnemyType> AvailableTypes(GameContext ctx)
        {
            // Basierend auf Progress freischalten (einfaches Beispiel)
            var s = new List<EnemyType> { EnemyType.Goblin };
            if (ctx.CurrentLevel >= 2 || ctx.TotalSwipes >= 20) s.Add(EnemyType.Orc);
            if (ctx.CurrentLevel >= 4 || ctx.TotalSwipes >= 50) s.Add(EnemyType.Dragon);
            return s;
        }

        public static EnemyType WeightedPick(IEnumerable<EnemyType> list, IList<int> weights, Random rng)
        {
            int total = weights.Sum();
            if (total == 0) return list.First();
            int r = rng.Next(total);
            int i = 0;
            foreach (var t in list)
            {
                int w = weights[i++];
                if ((r -= w) < 0) return t;
            }
            return list.First();
        }

        private static readonly EnemyType[] RarePool =
        {
            EnemyType.Masochist,
            EnemyType.Thorns
        };

        private static EnemyType PickRare(Random rng) => RarePool[rng.Next(RarePool.Length)];

        public static Enemy SpawnOne(GameContext ctx)
        {
            var avail = AvailableTypes(ctx).ToList();
            var weights = avail.Select(t => Get(t).CalcSpawnWeight(ctx)).ToList();
            var sel = WeightedPick(avail, weights, ctx.Rng);

            // 20% Chance auf Rare-Ersetzung
            if (ctx.Rng.NextDouble() < 0.20)
            {
                var rare = PickRare(ctx.Rng);
                if (map.ContainsKey(rare))
                    sel = rare;
            }

            int level = Get(sel).CalcLevel(ctx);
            var pos = ctx.RandomFreeCell();
            return Get(sel).Create(pos.X, pos.Y, level);
        }
    }

    // Standard-Archetypen
    public sealed class GoblinArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Goblin;
        public int CalcSpawnWeight(GameContext ctx)
        {
            int diff = ctx.CurrentLevel + (int)(ctx.TotalSwipes / 15.0);
            return Math.Max(10, 50 - diff / 6);
        }
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();
        public Enemy Create(int x, int y, int level, bool boss = false) => new Enemy(x, y, Type, level, boss);
        public bool IsBossEligible(GameContext ctx) => false;
    }

    public sealed class OrcArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Orc;
        public int CalcSpawnWeight(GameContext ctx)
        {
            int diff = ctx.CurrentLevel + (int)(ctx.TotalSwipes / 15.0);
            return Math.Min(45, 15 + diff / 4);
        }
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();
        public Enemy Create(int x, int y, int level, bool boss = false) => new Enemy(x, y, Type, level, boss);
        public bool IsBossEligible(GameContext ctx) => false;
    }

    public sealed class DragonArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Dragon;
        public int CalcSpawnWeight(GameContext ctx)
        {
            int diff = ctx.CurrentLevel + (int)(ctx.TotalSwipes / 15.0);
            return Math.Min(35, diff / 2);
        }
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();
        public Enemy Create(int x, int y, int level, bool boss = false) => new Enemy(x, y, Type, level, boss);
        public bool IsBossEligible(GameContext ctx) => true;
    }

    // Rare-Archetypen
    public sealed class MasochistArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Masochist;
        public int CalcSpawnWeight(GameContext ctx) => 1; // Rare via Ersetzung
        public int CalcLevel(GameContext ctx) => Math.Max(1, 1 + ctx.CurrentLevel / 2);
        public Enemy Create(int x, int y, int level, bool boss = false) => new Enemy(x, y, EnemyType.Masochist, level, boss);
        public bool IsBossEligible(GameContext ctx) => false;
    }

    public sealed class ThornsArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Thorns;
        public int CalcSpawnWeight(GameContext ctx) => 1; // Rare via Ersetzung
        public int CalcLevel(GameContext ctx) => Math.Max(1, 1 + ctx.CurrentLevel / 2);
        public Enemy Create(int x, int y, int level, bool boss = false) => new Enemy(x, y, EnemyType.Thorns, level, boss);
        public bool IsBossEligible(GameContext ctx) => false;
    }
}
