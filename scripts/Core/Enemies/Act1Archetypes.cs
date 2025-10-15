// scripts/Core/Enemies/Act1Archetypes.cs
using System;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Services;

namespace Dungeon2048.Core.Enemies
{
    // Goblin - Standard Basis-Gegner
    public sealed class GoblinArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Goblin;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            int diff = ctx.CurrentLevel + (int)(ctx.TotalSwipes / 15.0);
            return Math.Max(10, 50 - diff / 6); // Häufig am Anfang, wird seltener
        }
        
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.Goblin, level, boss);
        }
        
        public bool IsBossEligible(GameContext ctx) => false;
    }
    
    // Skeleton - Hinterlässt Knochen beim Tod
    public sealed class SkeletonArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Skeleton;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            return 30; // Relativ häufig in Akt 1
        }
        
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.Skeleton, level, boss);
        }
        
        public bool IsBossEligible(GameContext ctx) => false;
    }
    
    // Rat - Schneller Schwarm-Gegner
    public sealed class RatArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Rat;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            return 25; // Häufig, schwarm-artig
        }
        
        public int CalcLevel(GameContext ctx) => Math.Max(1, ctx.CalculateEnemyLevel() - 1);
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.Rat, level, boss);
        }
        
        public bool IsBossEligible(GameContext ctx) => false;
    }
    
    // Necrophage - Heilt sich wenn Gegner sterben
    public sealed class NecrophageArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Necrophage;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            return 5; // Rare
        }
        
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.Necrophage, level, boss);
        }
        
        public bool IsBossEligible(GameContext ctx) => false;
    }
    
    // Mimic - Sieht aus wie Spell Drop
    public sealed class MimicArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Mimic;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            return 3; // Sehr rare
        }
        
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel() + 1;
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var mimic = new Enemy(x, y, EnemyType.Mimic, level, boss);
            mimic.IsDisguised = true; // Startet getarnt
            return mimic;
        }
        
        public bool IsBossEligible(GameContext ctx) => false;
    }
    
    // Goblin King - Boss von Akt 1
    public sealed class GoblinKingArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.GoblinKing;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            return 0; // Nur durch Boss-Spawn
        }
        
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel() + 2;
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.GoblinKing, level, true);
        }
        
        public bool IsBossEligible(GameContext ctx) => true;
    }
}