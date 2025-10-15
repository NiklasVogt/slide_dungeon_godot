// scripts/Core/World/IBiome.cs
using Godot;
using System.Collections.Generic;
using Dungeon2048.Core.Entities;

namespace Dungeon2048.Core.World
{
    public interface IBiome
    {
        BiomeType Type { get; }
        string Name { get; }
        string Description { get; }
        int StartLevel { get; }
        int EndLevel { get; }
        
        // Visuals
        Color BackgroundColor { get; }
        Color GridColor { get; }
        Color AmbientColor { get; }
        
        // Difficulty Modifiers
        float EnemyHealthMultiplier { get; }
        float EnemyDamageMultiplier { get; }
        float SpawnRateMultiplier { get; }
        
        // Content
        List<EnemyType> StandardEnemies { get; }
        List<EnemyType> RareEnemies { get; }
        string[] SpecialTileTypes { get; }
        
        // Callbacks
        void OnEnter(Services.GameContext ctx);
        void OnLevelStart(Services.GameContext ctx);
        void OnLevelComplete(Services.GameContext ctx);
        void OnExit(Services.GameContext ctx);
        
        // Boss
        bool HasBoss(int level);
        EnemyType GetBossType();
    }
}