// scripts/Core/Enemies/Act2Archetypes.cs
using System;
using Dungeon2048.Core.Entities;
using Dungeon2048.Core.Services;

namespace Dungeon2048.Core.Enemies
{
    // Orc - Standard Krieger
    public sealed class OrcArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Orc;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            return 40; // Häufig in Akt 2
        }
        
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.Orc, level, boss);
        }
        
        public bool IsBossEligible(GameContext ctx) => false;
    }
    
    // Kultist - Statisch, schießt Kreuz-Pattern
    public sealed class KultistArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Kultist;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            return 30; // Häufig, gefährlich
        }
        
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var kultist = new Enemy(x, y, EnemyType.Kultist, level, boss);
            // Kultist bewegt sich nie, attackiert aber fernkampf
            return kultist;
        }
        
        public bool IsBossEligible(GameContext ctx) => false;
    }
    
    // Gargoyle - Bewegt nur jeden 2. Zug
    public sealed class GargoyleArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.Gargoyle;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            return 25; // Mittlere Häufigkeit
        }
        
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var gargoyle = new Enemy(x, y, EnemyType.Gargoyle, level, boss);
            gargoyle.HasMoved = false; // Startet als Statue
            return gargoyle;
        }
        
        public bool IsBossEligible(GameContext ctx) => false;
    }
    
    // Soul Leech - Reduziert permanent Player ATK
    public sealed class SoulLeechArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.SoulLeech;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            return 8; // Rare, sehr gefährlich
        }
        
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.SoulLeech, level, boss);
        }
        
        public bool IsBossEligible(GameContext ctx) => false;
    }
    
    // Mirror Knight - Spiegelt Player Stats
    public sealed class MirrorKnightArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.MirrorKnight;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            return 5; // Rare, schwierig
        }
        
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.MirrorKnight, level, boss);
        }
        
        public bool IsBossEligible(GameContext ctx) => false;
    }
    
    // Hex Witch - Verflucht Heilung
    public sealed class HexWitchArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.HexWitch;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            return 6; // Rare
        }
        
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel();
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            return new Enemy(x, y, EnemyType.HexWitch, level, boss);
        }
        
        public bool IsBossEligible(GameContext ctx) => false;
    }
    
    // Lich Mage - Boss von Akt 2
    public sealed class LichMageArch : IEnemyArchetype
    {
        public EnemyType Type => EnemyType.LichMage;
        
        public int CalcSpawnWeight(GameContext ctx)
        {
            return 0; // Nur durch Boss-Spawn
        }
        
        public int CalcLevel(GameContext ctx) => ctx.CalculateEnemyLevel() + 3;
        
        public Enemy Create(int x, int y, int level, bool boss = false)
        {
            var lich = new Enemy(x, y, EnemyType.LichMage, level, true);
            lich.LichTeleportCounter = 0; // Tracking für Teleport
            return lich;
        }
        
        public bool IsBossEligible(GameContext ctx) => true;
    }
}