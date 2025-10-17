// scripts/Core/Objectives/ObjectiveService.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeon2048.Core.Objectives
{
    public static class ObjectiveService
    {
        public static IObjective Generate(Random rng, int currentLevel)
        {
            // Boss-Level: Nur am Ende jedes Aktes (10, 20, 30, 40, 50)
            if (IsBossLevel(currentLevel))
            {
                return new BossObjective(12 + (currentLevel / 2) + rng.Next(0, 6));
            }
            
            // Normale Level: Nur Survival oder Elimination
            var weights = new Dictionary<LevelType, int> {
                { LevelType.Survival, 50 },      
                { LevelType.Elimination, 50 }   
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
                // Survival: Langsamer ansteigend, mehr Swipes benötigt
                LevelType.Survival => new SurvivalObjective(
                    10 + (currentLevel * 2) + rng.Next(0, 5)
                ),
                
                // Elimination: Deutlich mehr Kills benötigt
                LevelType.Elimination => new EliminationObjective(
                    Math.Max(4, 3 + (int)(currentLevel / 1.5) + rng.Next(0, 4))
                ),
                
                _ => new SurvivalObjective(10)
            };
        }
        
        // Prüft ob aktuelles Level ein Boss-Level ist
        public static bool IsBossLevel(int level)
        {
            return level % 10 == 0; // Level 10, 20, 30, 40, 50
        }
        
        // Gibt den Akt für ein Level zurück (1-5)
        public static int GetActForLevel(int level)
        {
            return ((level - 1) / 10) + 1;
        }
    }
}