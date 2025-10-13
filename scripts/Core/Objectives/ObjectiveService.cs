using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeon2048.Core.Objectives
{
    public static class ObjectiveService
    {
        public static IObjective Generate(Random rng, int currentLevel)
        {
            var weights = new Dictionary<LevelType, int> {
                { LevelType.Survival, 35 }, { LevelType.Elimination, 35 }, { LevelType.Boss, 30 }
            };
            var types = weights.Keys.ToList();
            int total = weights.Values.Sum();
            int r = rng.Next(total);
            LevelType pick = types[0];
            foreach (var kv in weights)
            {
                if ((r -= kv.Value) < 0) { pick = kv.Key; break; }
            }
            return pick switch
            {
                LevelType.Survival => new SurvivalObjective(8 + currentLevel * 1 + rng.Next(0,4)),
                LevelType.Elimination => new EliminationObjective(Math.Max(3, 2 + (int)(currentLevel/2.0) + rng.Next(0,3))),
                LevelType.Boss => new BossObjective(8 + currentLevel * 1 + rng.Next(0,4)),
                _ => new SurvivalObjective(6)
            };
        }
    }
}
