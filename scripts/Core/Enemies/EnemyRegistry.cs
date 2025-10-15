// scripts/Core/Enemies/EnemyRegistry.cs
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
            // Biome-basiert
            var biome = ctx.BiomeSystem?.CurrentBiome;
            if (biome != null)
            {
                var available = new List<EnemyType>();
                available.AddRange(biome.StandardEnemies);
                available.AddRange(biome.RareEnemies);
                return available;
            }
            
            // Fallback: wenn kein Biome-System aktiv
            return new List<EnemyType> { EnemyType.Goblin };
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

        public static Enemy SpawnOne(GameContext ctx)
        {
            var avail = AvailableTypes(ctx).ToList();
            if (avail.Count == 0)
            {
                // Fallback wenn keine Gegner verfügbar
                var fallbackPos = ctx.RandomFreeCell();
                return new Enemy(fallbackPos.X, fallbackPos.Y, EnemyType.Goblin, 1);
            }
            
            var weights = avail.Select(t => Get(t).CalcSpawnWeight(ctx)).ToList();
            var sel = WeightedPick(avail, weights, ctx.Rng);

            int level = Get(sel).CalcLevel(ctx);
            var pos = ctx.RandomFreeCell();
            return Get(sel).Create(pos.X, pos.Y, level);
        }
    }
}